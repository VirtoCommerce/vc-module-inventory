using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VirtoCommerce.InventoryModule.Core.Model;
using VirtoCommerce.InventoryModule.Core.Model.Search;
using VirtoCommerce.InventoryModule.Core.Services;
using VirtoCommerce.InventoryModule.Data.Repositories;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Data.Infrastructure;

namespace VirtoCommerce.InventoryModule.Data.Services
{
    public class ProductInventorySearchService(Func<IInventoryRepository> repositoryFactory) : IProductInventorySearchService
    {
        private static readonly Dictionary<string, string> _sortingAliases = new(StringComparer.OrdinalIgnoreCase);

        static ProductInventorySearchService()
        {
            var inventoryInfoRealType = AbstractTypeFactory<InventoryInfo>.TryCreateInstance().GetType();

            foreach (var property in inventoryInfoRealType.GetProperties())
            {
                _sortingAliases[property.Name] = $"Inventory.{property.Name}";
            }

            // Build column name map from resulting FulfillmentCenterInventoryInfo to FulfillmentCenterInventory object fields by which we queue the data            
            _sortingAliases["FulfillmentCenterName"] = "FulfillmentCenter.Name";
        }

        [Obsolete("Use SearchAsync()", DiagnosticId = "VC0011", UrlFormat = "https://docs.virtocommerce.org/products/products-virto3-versions")]
        public virtual Task<InventoryInfoSearchResult> SearchProductInventoriesAsync(ProductInventorySearchCriteria criteria)
        {
            return SearchAsync(criteria);
        }

        public virtual async Task<InventoryInfoSearchResult> SearchAsync(ProductInventorySearchCriteria criteria, bool clone = true)
        {
            var result = AbstractTypeFactory<InventoryInfoSearchResult>.TryCreateInstance();

            using var repository = repositoryFactory();
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

            result.TotalCount = await query.CountAsync();

            var sortInfos = criteria.SortInfos;
            if (sortInfos.IsNullOrEmpty())
            {
                sortInfos =
                [
                    new SortInfo { SortColumn = nameof(InventoryInfo.InStockQuantity), SortDirection = SortDirection.Descending },
                    new SortInfo { SortColumn = "FulfillmentCenterName", SortDirection = SortDirection.Ascending },
                ];
            }

            TryTransformSortingInfoColumnNames(_sortingAliases, sortInfos);

            query = query.OrderBySortInfos(sortInfos).ThenBy(x => x.FulfillmentCenter.Id);

            var queryResult = await query.AsNoTracking().Skip(criteria.Skip).Take(criteria.Take)
                .ToListAsync();

            result.Results = queryResult.Select(x =>
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

            return result;
        }

        protected virtual void TryTransformSortingInfoColumnNames(Dictionary<string, string> sortingAliases, IEnumerable<SortInfo> sortInfos)
        {
            foreach (var sortInfo in sortInfos)
            {
                if (sortingAliases.TryGetValue(sortInfo.SortColumn, out var newColumnName))
                {
                    sortInfo.SortColumn = newColumnName;
                }
            }
        }
    }
}
