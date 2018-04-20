﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using VirtoCommerce.Domain.Common.Events;
using VirtoCommerce.Domain.Inventory.Events;
using VirtoCommerce.Domain.Inventory.Model;
using VirtoCommerce.Domain.Inventory.Services;
using VirtoCommerce.InventoryModule.Data.Model;
using VirtoCommerce.InventoryModule.Data.Repositories;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Events;
using VirtoCommerce.Platform.Data.Infrastructure;

namespace VirtoCommerce.InventoryModule.Data.Services
{
    public class InventoryServiceImpl : ServiceBase, IInventoryService
    {
        private readonly Func<IInventoryRepository> _repositoryFactory;
        private readonly IEventPublisher _eventPublisher;
        public InventoryServiceImpl(Func<IInventoryRepository> repositoryFactory, IEventPublisher eventPublisher)
        {
            _repositoryFactory = repositoryFactory;
            _eventPublisher = eventPublisher;
        }

        public InventoryInfo GetById(string itemId)
        {
            var result = GetProductsInventoryInfos(new[] { itemId }).FirstOrDefault();
            return result;
        }

        #region IInventoryService Members


        [Obsolete]
        public IEnumerable<InventoryInfo> GetAllInventoryInfos()
        {
            using (var repository = _repositoryFactory())
            {
                repository.DisableChangesTracking();
                var result = repository.Inventories.ToArray().Select(x => x.ToModel(AbstractTypeFactory<InventoryInfo>.TryCreateInstance()));
                return result;
            }
        }

        public IEnumerable<InventoryInfo> GetProductsInventoryInfos(IEnumerable<string> productIds)
        {
            var retVal = new List<InventoryInfo>();
            using (var repository = _repositoryFactory())
            {
                repository.DisableChangesTracking();
                var entities = repository.GetProductsInventories(productIds.ToArray());
                retVal.AddRange(entities.Select(x => x.ToModel(AbstractTypeFactory<InventoryInfo>.TryCreateInstance())));
            }
            return retVal;
        }

        public void UpsertInventories(IEnumerable<InventoryInfo> inventoryInfos)
        {
            if (inventoryInfos == null)
            {
                throw new ArgumentNullException(nameof(inventoryInfos));
            }

            var changedEntries = new List<GenericChangedEntry<InventoryInfo>>();
            using (var repository = _repositoryFactory())
            {
                var dataExistInventories = repository.GetProductsInventories(inventoryInfos.Select(x=>x.ProductId));
                foreach (var changedInventory in inventoryInfos)
                {               
                    var originalEntity = dataExistInventories.FirstOrDefault(x => x.Sku == changedInventory.ProductId && x.FulfillmentCenterId == changedInventory.FulfillmentCenterId);
            
                    var modifiedEntity = AbstractTypeFactory<InventoryEntity>.TryCreateInstance().FromModel(changedInventory);
                    if (originalEntity != null)
                    {
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
                _eventPublisher.Publish(new InventoryChangingEvent(changedEntries));
                CommitChanges(repository);
                _eventPublisher.Publish(new InventoryChangedEvent(changedEntries));
            }
        }


        public InventoryInfo UpsertInventory(InventoryInfo inventoryInfo)
        {
            if (inventoryInfo == null)
            {
                throw new ArgumentNullException(nameof(inventoryInfo));
            }
            UpsertInventories(new[] { inventoryInfo });
            return inventoryInfo;
        }

        #endregion

    }
}
