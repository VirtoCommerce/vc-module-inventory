using System;
using System.Collections.Generic;
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
    public class ProductInventorySearchService : IProductInventorySearchService
    {
        private readonly Func<IInventoryRepository> _repositoryFactory;
        private static Dictionary<string, string> _sortingAliases = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        static ProductInventorySearchService()
        {
            var inventoryInfoRealType = AbstractTypeFactory<InventoryInfo>.TryCreateInstance().GetType();
            foreach (var property in inventoryInfoRealType.GetProperties())
            {
                _sortingAliases[property.Name] = $"Inventory.{property.Name}";
            }
            /// <summary>
            /// Build column name map from resulting FulfillmentCenterInventoryInfo to FulfillmentCenterInventory object fields by which we queue the data
            /// </summary>
            _sortingAliases["FulfillmentCenterName"] = "FulfillmentCenter.Name";
        }

        public ProductInventorySearchService(Func<IInventoryRepository> repositoryFactory)
        {
            _repositoryFactory = repositoryFactory;
        }

        public virtual GenericSearchResult<InventoryInfo> SearchProductInventories(ProductInventorySearchCriteria criteria)
        {
            if (criteria == null)
            {
                throw new ArgumentNullException(nameof(criteria));
            }
            if (string.IsNullOrEmpty(criteria.ProductId))
            {
                throw new ArgumentNullException($"{nameof(criteria.ProductId)} must be set", nameof(criteria.ProductId));
            }

            var result = new GenericSearchResult<InventoryInfo>();
            using (var repository = _repositoryFactory())
            {
                repository.DisableChangesTracking();

                var fulfillmentCenters = repository.FulfillmentCenters;

                if (!string.IsNullOrEmpty(criteria.SearchPhrase))
                {
                    fulfillmentCenters = fulfillmentCenters.Where(x => x.Name.Contains(criteria.SearchPhrase));
                }

                var inventories = repository.Inventories.Where(x => criteria.ProductId == x.Sku);

                // SELECT FFC.*, I.* FROM FulfillmentCenter as FFC
                // LEFT JOIN Inventory as I on FFC.Id = i.FulfillmentCenterId AND (FFC + PRODUCT conditions)
                var query = fulfillmentCenters
                    .GroupJoin(
                        inventories,
                        x => x.Id,
                        x => x.FulfillmentCenterId,
                        (x, y) => new
                        {
                            FulfillmentCenterEntity = x,
                            Inventories = y,
                        })
                    .SelectMany(
                        x => x.Inventories.DefaultIfEmpty(),
                        (x, y) => new
                        {
                            FulfillmentCenter = x.FulfillmentCenterEntity,
                            Inventory = y,
                        });

                result.TotalCount = query.Count();

                var sortInfos = criteria.SortInfos;
                if (sortInfos.IsNullOrEmpty())
                {
                    sortInfos = new[]
                    {
                        new SortInfo { SortColumn = $"{nameof(InventoryInfo.InStockQuantity)}", SortDirection = SortDirection.Descending },
                        new SortInfo { SortColumn = $"FulfillmentCenterName", SortDirection = SortDirection.Ascending }
                    };
                }

                TryTransformSortingInfoColumnNames(_sortingAliases, sortInfos);

                query = query.OrderBySortInfos(sortInfos).ThenBy(x => x.FulfillmentCenter.Id);

                result.Results = query.Skip(criteria.Skip).Take(criteria.Take)
                                 .AsEnumerable()
                                 .Select(x =>
                                 {
                                     var inventory = AbstractTypeFactory<InventoryInfo>.TryCreateInstance();
                                     inventory.FulfillmentCenter = x.FulfillmentCenter.ToModel(AbstractTypeFactory<FulfillmentCenter>.TryCreateInstance());
                                     inventory.FulfillmentCenterId = x.FulfillmentCenter.Id;
                                     inventory.FulfillmentCenterName = inventory.FulfillmentCenter.Name;
                                     inventory.ProductId = criteria.ProductId;
                                     if (x.Inventory != null)
                                     {
                                         x.Inventory.ToModel(inventory);
                                     }
                                     return inventory;
                                 }).ToList();
            }

            return result;
        }

      
     
        protected virtual void TryTransformSortingInfoColumnNames(Dictionary<string, string> sortingAliases, SortInfo[] sortInfos)
        {
            foreach (var sortInfo in sortInfos)
            {
                string newColumnName;
                if (sortingAliases.TryGetValue(sortInfo.SortColumn, out newColumnName))
                {
                    sortInfo.SortColumn = newColumnName;
                }
            }
        }
    }
}
