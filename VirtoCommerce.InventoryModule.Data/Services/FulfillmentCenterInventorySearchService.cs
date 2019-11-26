using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using VirtoCommerce.Domain.Commerce.Model.Search;
using VirtoCommerce.Domain.Inventory.Model;
using VirtoCommerce.Domain.Inventory.Model.Search;
using VirtoCommerce.InventoryModule.Data.Model;
using VirtoCommerce.InventoryModule.Data.Repositories;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Data.Infrastructure;

namespace VirtoCommerce.InventoryModule.Data.Services
{
    public class FulfillmentCenterInventorySearchService : IFulfillmentCenterInventorySearchService
    {
        private readonly Func<IInventoryRepository> _repositoryFactory;
        private readonly Dictionary<string, string> _sortingAliases = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        public FulfillmentCenterInventorySearchService(Func<IInventoryRepository> repositoryFactory)
        {
            _repositoryFactory = repositoryFactory;
            InitSortingAliases();
        }

        public virtual GenericSearchResult<FulfillmentCenterInventoryInfo> Search(InventorySearchCriteria criteria)
        {
            var result = new GenericSearchResult<FulfillmentCenterInventoryInfo>();
            using (var repository = _repositoryFactory())
            {
                repository.DisableChangesTracking();

                var query = GetQuery(repository, criteria);

                result.TotalCount = query.Count();

                var sortInfos = criteria.SortInfos;
                if (sortInfos.IsNullOrEmpty())
                {
                    sortInfos = new[]
                    {
                        new SortInfo { SortColumn = $"{nameof(FulfillmentCenterInventoryInfo.InStockQuantity)}", SortDirection = SortDirection.Descending },
                        new SortInfo { SortColumn = $"{nameof(FulfillmentCenterInventoryInfo.FulfillmentCenterName)}", SortDirection = SortDirection.Ascending }
                    };
                }

                TryTransformSortingInfoColumnNames(_sortingAliases, sortInfos);

                query = query.OrderBySortInfos(sortInfos).ThenBy(x => x.FulfillmentCenter.Id);
                var fulfilmentCenterids = query.Select(x => x.FulfillmentCenter.Id).ToList();

                result.Results = query.Skip(criteria.Skip)
                                 .Take(criteria.Take)
                                 .AsEnumerable()
                                 .Select(x => x.ToModel(AbstractTypeFactory<FulfillmentCenterInventoryInfo>.TryCreateInstance()))
                                 .OrderBy(x => fulfilmentCenterids.IndexOf(x.FulfillmentCenterId))
                                 .ToList();
            }

            return result;
        }

        protected virtual IQueryable<FulfillmentCenterInventory> GetQuery(IInventoryRepository repository, InventorySearchCriteria criteria)
        {
            var fulfillmentCenters = repository.FulfillmentCenters;

            // Important! Need to make filtering here. If we make it after, we will get INNER JOIN instead of LEFT JOIN
            if (!criteria.FulfillmentCenterIds.IsNullOrEmpty())
            {
                fulfillmentCenters = fulfillmentCenters.Where(x => criteria.FulfillmentCenterIds.Contains(x.Id));
            }

            if (!string.IsNullOrEmpty(criteria.SearchPhrase))
            {
                fulfillmentCenters = fulfillmentCenters.Where(x => x.Name.Contains(criteria.SearchPhrase));
            }

            var inventories = repository.Inventories;

            if (!criteria.ProductIds.IsNullOrEmpty())
            {
                inventories = inventories.Where(x => criteria.ProductIds.Contains(x.Sku));
            }

            // SELECT FFC.*, I.* FROM FulfillmentCenter as FFC
            // LEFT JOIN Inventory as I on FFC.Id = i.FulfillmentCenterId AND (FFC + PRODUCT conditions)
            var query = fulfillmentCenters
                .GroupJoin(
                    inventories,
                    x => x.Id,
                    x => x.FulfillmentCenterId,
                    (x, y) => new FulfillmentCenterInventories()
                    {
                        FulfillmentCenterEntity = x,
                        Inventories = y,
                    })
                .SelectMany(
                    x => x.Inventories.DefaultIfEmpty(),
                    (x, y) => new FulfillmentCenterInventory()
                    {
                        FulfillmentCenter = x.FulfillmentCenterEntity,
                        Inventory = y,
                    });


            return query;
        }

        /// <summary>
        /// Build column name map from resulting FulfillmentCenterInventoryInfo to FulfillmentCenterInventory object fields by which we queue the data
        /// </summary>
        private void InitSortingAliases()
        {
            _sortingAliases["FulfillmentCenterName"] = $"{nameof(FulfillmentCenterInventory.FulfillmentCenter)}.{nameof(FulfillmentCenterEntity.Name)}";

            var inventoryInfoRealType = AbstractTypeFactory<InventoryInfo>.TryCreateInstance().GetType();

            foreach (var property in inventoryInfoRealType.GetProperties())
            {
                _sortingAliases[property.Name] = $"{nameof(FulfillmentCenterInventory.Inventory)}.{property.Name}";
            }
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
