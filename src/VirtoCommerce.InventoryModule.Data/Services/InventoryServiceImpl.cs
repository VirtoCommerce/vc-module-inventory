using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using VirtoCommerce.InventoryModule.Core.Events;
using VirtoCommerce.InventoryModule.Core.Model;
using VirtoCommerce.InventoryModule.Core.Services;
using VirtoCommerce.InventoryModule.Data.Caching;
using VirtoCommerce.InventoryModule.Data.Model;
using VirtoCommerce.InventoryModule.Data.Repositories;
using VirtoCommerce.Platform.Core.Caching;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Events;
using VirtoCommerce.Platform.Data.GenericCrud;
using VirtoCommerce.Platform.Data.Infrastructure;

namespace VirtoCommerce.InventoryModule.Data.Services;

public class InventoryServiceImpl(
    Func<IInventoryRepository> repositoryFactory,
    IPlatformMemoryCache platformMemoryCache,
    IEventPublisher eventPublisher)
    : CrudService<InventoryInfo, InventoryEntity, InventoryChangingEvent, InventoryChangedEvent>
        (repositoryFactory, platformMemoryCache, eventPublisher),
        IInventoryService
{
    private readonly IPlatformMemoryCache _platformMemoryCache = platformMemoryCache;

    [Obsolete("Use GetAsync()", DiagnosticId = "VC0011", UrlFormat = "https://docs.virtocommerce.org/products/products-virto3-versions")]
    public virtual async Task<IEnumerable<InventoryInfo>> GetByIdsAsync(string[] ids, string responseGroup = null)
    {
        return await GetAsync(ids, responseGroup);
    }

    [Obsolete("Use InventorySearchService.SearchAsync()", DiagnosticId = "VC0011", UrlFormat = "https://docs.virtocommerce.org/products/products-virto3-versions")]
    public virtual async Task<IEnumerable<InventoryInfo>> GetProductsInventoryInfosAsync(IEnumerable<string> productIds, string responseGroup = null)
    {
        var productIdsList = productIds.ToList();
        var cacheKeyPrefix = CacheKey.With(GetType(), nameof(GetProductsInventoryInfosAsync), responseGroup);

        var models = await _platformMemoryCache.GetOrLoadByIdsAsync(cacheKeyPrefix, productIdsList,
            async missingIds =>
            {
                using var repository = repositoryFactory();
                repository.DisableChangesTracking();
                var entities = await repository.GetProductsInventoriesAsync(missingIds, responseGroup);

                IList<CacheEntity> models = entities
                    .Select(x => x.ToModel(AbstractTypeFactory<InventoryInfo>.TryCreateInstance()))
                    .GroupBy(x => x.ProductId)
                    .Select(g => new CacheEntity(g.Key, g))
                    .ToList();

                return models;
            },
            (cacheOptions, id, model) =>
            {
                var tokenIds = new HashSet<string> { id };

                if (model != null)
                {
                    foreach (var inventory in model.Inventories)
                    {
                        tokenIds.Add(inventory.Id);
                        tokenIds.Add(inventory.ProductId);
                    }
                }

                cacheOptions.AddExpirationToken(InventoryCacheRegion.CreateChangeToken(tokenIds));
            });

        return models
            .OrderBy(x => productIdsList.IndexOf(x.Id))
            .SelectMany(x => x.Inventories)
            .Select(x => x.CloneTyped())
            .ToList();
    }

    protected override Task<IList<InventoryEntity>> LoadEntities(IRepository repository, IList<string> ids, string responseGroup)
    {
        return ((IInventoryRepository)repository).GetByIdsAsync(ids, responseGroup);
    }

    protected override Task<IList<InventoryEntity>> LoadExistingEntities(IRepository repository, IList<InventoryInfo> models)
    {
        var productIds = models
            .Select(x => x.ProductId)
            .Where(x => !x.IsNullOrEmpty())
            .ToList();

        return productIds.Count > 0
            ? ((IInventoryRepository)repository).GetProductsInventoriesAsync(productIds)
            : Task.FromResult<IList<InventoryEntity>>(Array.Empty<InventoryEntity>());
    }

    protected override InventoryEntity FindExistingEntity(IList<InventoryEntity> existingEntities, InventoryInfo model)
    {
        InventoryEntity existingEntity = null;

        // If current inventory has an ID, let's attempt to find matching existing entity by ID. This should always take precedence over other properties
        // to make sure that we update the exact inventory record requested by calling code.
        if (!model.Id.IsNullOrEmpty())
        {
            existingEntity = existingEntities.FirstOrDefault(x => x.Id == model.Id);
        }

        // If there is no such entity (or if current inventory is transient), look it up by (ProductId, FulfillmentCenterId).
        if (existingEntity == null)
        {
            existingEntity = existingEntities.FirstOrDefault(x =>
                x.Sku.EqualsIgnoreCase(model.ProductId) &&
                x.FulfillmentCenterId.EqualsIgnoreCase(model.FulfillmentCenterId));
        }

        if (existingEntity != null && model.Id.IsNullOrEmpty())
        {
            model.Id = existingEntity.Id;
        }

        return existingEntity;
    }

    protected override void ConfigureCache(MemoryCacheEntryOptions cacheOptions, string id, InventoryInfo model)
    {
        var tokenIds = new HashSet<string> { id };

        if (model != null)
        {
            tokenIds.Add(model.ProductId);
        }

        cacheOptions.AddExpirationToken(InventoryCacheRegion.CreateChangeToken(tokenIds));
    }

    protected override void ClearCache(IList<InventoryInfo> models)
    {
        InventorySearchCacheRegion.ExpireRegion();

        foreach (var inventory in models)
        {
            InventoryCacheRegion.ExpireInventory(inventory);
        }
    }

    private sealed class CacheEntity : Entity
    {
        public CacheEntity(string productId, IEnumerable<InventoryInfo> inventories)
        {
            Id = productId;
            Inventories = inventories;
        }

        public IEnumerable<InventoryInfo> Inventories { get; }
    }
}
