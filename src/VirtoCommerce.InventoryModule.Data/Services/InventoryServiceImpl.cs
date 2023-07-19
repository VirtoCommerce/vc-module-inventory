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
using VirtoCommerce.Platform.Data.Infrastructure;

namespace VirtoCommerce.InventoryModule.Data.Services
{
    public class InventoryServiceImpl : IInventoryService
    {
        private readonly Func<IInventoryRepository> _repositoryFactory;
        private readonly IEventPublisher _eventPublisher;
        private readonly IPlatformMemoryCache _platformMemoryCache;

        public InventoryServiceImpl(Func<IInventoryRepository> repositoryFactory, IEventPublisher eventPublisher, IPlatformMemoryCache platformMemoryCache)
        {
            _repositoryFactory = repositoryFactory;
            _eventPublisher = eventPublisher;
            _platformMemoryCache = platformMemoryCache;
        }

        public virtual async Task<IEnumerable<InventoryInfo>> GetByIdsAsync(string[] ids, string responseGroup = null)
        {
            var cacheKeyPrefix = CacheKey.With(GetType(), nameof(GetByIdsAsync), responseGroup);

            var models = await _platformMemoryCache.GetOrLoadByIdsAsync(cacheKeyPrefix, ids,
                async missingIds =>
                {
                    using var repository = _repositoryFactory();
                    repository.DisableChangesTracking();
                    var entities = await repository.GetByIdsAsync(missingIds.ToArray(), responseGroup);

                    IList<InventoryInfo> models = entities
                        .Select(x => x.ToModel(AbstractTypeFactory<InventoryInfo>.TryCreateInstance()))
                        .ToList();

                    return models;
                },
                (cacheOptions, id, model) =>
                {
                    var tokenIds = new HashSet<string> { id };

                    if (model != null)
                    {
                        tokenIds.Add(model.ProductId);
                    }

                    cacheOptions.AddExpirationToken(InventoryCacheRegion.CreateChangeToken(tokenIds.ToArray()));
                });

            return models
                .OrderBy(x => Array.IndexOf(ids, x.Id))
                .Select(x => x.CloneTyped())
                .ToList();
        }

        public virtual async Task<IEnumerable<InventoryInfo>> GetProductsInventoryInfosAsync(IEnumerable<string> productIds, string responseGroup = null)
        {
            var productIdsList = productIds.ToList();
            var cacheKeyPrefix = CacheKey.With(GetType(), nameof(GetProductsInventoryInfosAsync), responseGroup);

            var models = await _platformMemoryCache.GetOrLoadByIdsAsync(cacheKeyPrefix, productIdsList,
                async missingIds =>
                {
                    using var repository = _repositoryFactory();
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

                    cacheOptions.AddExpirationToken(InventoryCacheRegion.CreateChangeToken(tokenIds.ToArray()));
                });

            return models
                .OrderBy(x => productIdsList.IndexOf(x.Id))
                .SelectMany(x => x.Inventories)
                .Select(x => x.CloneTyped())
                .ToList();
        }

        public virtual async Task SaveChangesAsync(IEnumerable<InventoryInfo> inventoryInfos)
        {
            if (inventoryInfos == null)
            {
                throw new ArgumentNullException(nameof(inventoryInfos));
            }

            var pkMap = new PrimaryKeyResolvingMap();
            var changedEntries = new List<GenericChangedEntry<InventoryInfo>>();
            using (var repository = _repositoryFactory())
            {
                var dataExistInventories = await repository.GetProductsInventoriesAsync(inventoryInfos.Select(x => x.ProductId).ToList());
                foreach (var changedInventory in inventoryInfos)
                {
                    InventoryEntity originalEntity = null;

                    // If current inventory has an ID, let's attempt to find matching existing entity by ID. This should always take precedence over other properties
                    // to make sure that we update the exact inventory record requested by calling code.
                    if (!changedInventory.IsTransient())
                    {
                        originalEntity = dataExistInventories.FirstOrDefault(x => x.Id == changedInventory.Id);
                    }

                    // If there is no such entity (or if current inventory is transient), look it up by (ProductId, FulfillmentCenterId).
                    if (originalEntity == null)
                    {
                        originalEntity = dataExistInventories.FirstOrDefault(x => x.Sku == changedInventory.ProductId && x.FulfillmentCenterId == changedInventory.FulfillmentCenterId);
                    }

                    var modifiedEntity = AbstractTypeFactory<InventoryEntity>.TryCreateInstance().FromModel(changedInventory, pkMap);

                    if (originalEntity != null)
                    {
                        changedInventory.Id ??= originalEntity.Id;

                        changedEntries.Add(new GenericChangedEntry<InventoryInfo>(changedInventory, originalEntity.ToModel(AbstractTypeFactory<InventoryInfo>.TryCreateInstance()), EntryState.Modified));
                        modifiedEntity?.Patch(originalEntity);
                    }
                    else
                    {
                        repository.Add(modifiedEntity);
                        changedEntries.Add(new GenericChangedEntry<InventoryInfo>(changedInventory, EntryState.Added));
                    }
                }

                //Raise domain events
                await _eventPublisher.Publish(new InventoryChangingEvent(changedEntries));

                await repository.UnitOfWork.CommitAsync();
                pkMap.ResolvePrimaryKeys();
                ClearCache(inventoryInfos);

                await _eventPublisher.Publish(new InventoryChangedEvent(changedEntries));
            }
        }

        protected virtual void ClearCache(IEnumerable<InventoryInfo> inventories)
        {
            InventorySearchCacheRegion.ExpireRegion();

            foreach (var inventory in inventories)
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
}
