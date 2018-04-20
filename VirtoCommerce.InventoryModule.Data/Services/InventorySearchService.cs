﻿using System;
using System.Linq;
using VirtoCommerce.Domain.Commerce.Model.Search;
using VirtoCommerce.Domain.Inventory.Model;
using VirtoCommerce.Domain.Inventory.Model.Search;
using VirtoCommerce.Domain.Inventory.Services;
using VirtoCommerce.InventoryModule.Data.Repositories;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Data.Infrastructure;

namespace VirtoCommerce.InventoryModule.Data.Services
{
    public class InventorySearchService : IInventorySearchService
    {
        private readonly Func<IInventoryRepository> _repositoryFactory;
        public InventorySearchService(Func<IInventoryRepository> repositoryFactory)
        {
            _repositoryFactory = repositoryFactory;
        }

        public GenericSearchResult<InventoryInfo> SearchInventories(InventorySearchCriteria criteria)
        {
            var result = new GenericSearchResult<InventoryInfo>();
            using (var repository = _repositoryFactory())
            {
                repository.DisableChangesTracking();

                var query = repository.Inventories;
                if (!criteria.ProductIds.IsNullOrEmpty())
                {
                    query = query.Where(x => criteria.ProductIds.Contains(x.Sku));
                }
                if (!criteria.FulfillmentCenterIds.IsNullOrEmpty())
                {
                    query = query.Where(x => criteria.FulfillmentCenterIds.Contains(x.FulfillmentCenterId));
                }
                var sortInfos = criteria.SortInfos;
                if (sortInfos.IsNullOrEmpty())
                {
                    sortInfos = new[] { new SortInfo { SortColumn = "ModifiedDate" } };
                }

                query = query.OrderBySortInfos(sortInfos);

                result.TotalCount = query.Count();
                result.Results = query.Skip(criteria.Skip)
                                 .Take(criteria.Take)
                                 .ToArray()
                                 .Select(x => x.ToModel(AbstractTypeFactory<InventoryInfo>.TryCreateInstance()))
                                 .ToList();
            }
            return result;
        }
    }
}
