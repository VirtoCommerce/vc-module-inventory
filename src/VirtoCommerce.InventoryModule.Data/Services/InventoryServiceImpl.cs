using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.InventoryModule.Core.Events;
using VirtoCommerce.InventoryModule.Core.Model;
using VirtoCommerce.InventoryModule.Core.Services;
using VirtoCommerce.InventoryModule.Data.Model;
using VirtoCommerce.InventoryModule.Data.Repositories;
using VirtoCommerce.Platform.Core.Caching;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Events;
using VirtoCommerce.Platform.Data.GenericCrud;

namespace VirtoCommerce.InventoryModule.Data.Services
{
    public class InventoryServiceImpl : CrudService<InventoryInfo, InventoryEntity, InventoryChangingEvent, InventoryChangedEvent>, IInventoryService
    {
        private new readonly Func<IInventoryRepository> _repositoryFactory;

        public InventoryServiceImpl(Func<IInventoryRepository> repositoryFactory, IEventPublisher eventPublisher, IPlatformMemoryCache platformMemoryCache)
            : base(repositoryFactory, platformMemoryCache, eventPublisher)
        {
            _repositoryFactory = repositoryFactory;
        }

        public virtual async Task<IEnumerable<InventoryInfo>> GetByIdsAsync(string[] ids, string responseGroup = null)
        {
            return await base.GetByIdsAsync(ids);
        }

        public virtual async Task<IEnumerable<InventoryInfo>> GetProductsInventoryInfosAsync(IEnumerable<string> productIds, string responseGroup = null)
        {
            return await base.GetByIdsAsync(productIds);
        }

        public new async Task SaveChangesAsync(IEnumerable<InventoryInfo> inventoryInfos)
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

        protected async override Task<IEnumerable<InventoryEntity>> LoadEntities(IRepository repository, IEnumerable<string> ids, string responseGroup)
        {
            return await ((IInventoryRepository)repository).GetByIdsAsync(ids.ToArray());
        }
    }
}
