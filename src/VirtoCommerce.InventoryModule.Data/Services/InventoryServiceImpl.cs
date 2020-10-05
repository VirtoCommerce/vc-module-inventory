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
            var cacheKey = CacheKey.With(GetType(), nameof(GetByIdsAsync), string.Join("-", ids), responseGroup);
            return await _platformMemoryCache.GetOrCreateExclusiveAsync(cacheKey, async (cacheEntry) =>
            {
                using (var repository = _repositoryFactory())
                {
                    //It is so important to generate change tokens for all ids even for not existing objects to prevent an issue
                    //with caching of empty results for non - existing objects that have the infinitive lifetime in the cache
                    //and future unavailability to create objects with these ids.
                    cacheEntry.AddExpirationToken(InventoryCacheRegion.CreateChangeToken(ids));
    
                    repository.DisableChangesTracking();
                    var entries = await repository.GetByIdsAsync(ids, responseGroup);
                    if (entries.Any())
                    {
                        //Also need to add cancellation tokes for inventory productIds to be able evict from cache by productId.
                        cacheEntry.AddExpirationToken(InventoryCacheRegion.CreateChangeToken(entries.Select(x => x.Sku).ToArray()));
                    }

                    return entries.Select(e => e.ToModel(AbstractTypeFactory<InventoryInfo>.TryCreateInstance())).ToArray();
                }
            });
        }

        #region IInventoryService Members

        public virtual async Task<IEnumerable<InventoryInfo>> GetProductsInventoryInfosAsync(IEnumerable<string> productIds, string responseGroup = null)
        {
            var arrayProductsIds = productIds.ToArray();
            var cacheKey = CacheKey.With(GetType(), nameof(GetProductsInventoryInfosAsync), string.Join("-", arrayProductsIds), responseGroup);
            return await _platformMemoryCache.GetOrCreateExclusiveAsync(cacheKey, async (cacheEntry) =>
            {
                using (var repository = _repositoryFactory())
                {
                    repository.DisableChangesTracking();
                    var entities = await repository.GetProductsInventoriesAsync(arrayProductsIds, responseGroup);
                    var result = entities.Select(x => x.ToModel(AbstractTypeFactory<InventoryInfo>.TryCreateInstance())).ToList();
                    cacheEntry.AddExpirationToken(InventoryCacheRegion.CreateChangeToken(result));

                    return result;
                }
            });
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
                var dataExistInventories = await repository.GetProductsInventoriesAsync(inventoryInfos.Select(x => x.ProductId));
                foreach (var changedInventory in inventoryInfos)
                {
                    var originalEntity = dataExistInventories.FirstOrDefault(x => x.Sku == changedInventory.ProductId && x.FulfillmentCenterId == changedInventory.FulfillmentCenterId);

                    var modifiedEntity = AbstractTypeFactory<InventoryEntity>.TryCreateInstance().FromModel(changedInventory, pkMap);
                    if (originalEntity != null)
                    {
                        if (changedInventory.Id == null)
                        {
                            changedInventory.Id = originalEntity.Id;
                        }
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
                await _eventPublisher.Publish(new InventoryChangedEvent(changedEntries));

                ClearCache(inventoryInfos);
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

        #endregion

    }
}
