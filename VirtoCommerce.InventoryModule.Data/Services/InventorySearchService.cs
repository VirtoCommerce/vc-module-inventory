using System;
using System.Collections.Generic;
using System.Linq;
using VirtoCommerce.Domain.Commerce.Model.Search;
using VirtoCommerce.Domain.Inventory.Model;
using VirtoCommerce.Domain.Inventory.Model.Search;
using VirtoCommerce.Domain.Inventory.Services;
using VirtoCommerce.InventoryModule.Data.Model;
using VirtoCommerce.InventoryModule.Data.Repositories;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Data.Infrastructure;

namespace VirtoCommerce.InventoryModule.Data.Services
{
    public class InventorySearchService : IInventorySearchService
    {
        private readonly Func<IInventoryRepository> _repositoryFactory;
        private readonly Dictionary<string, string> _sortingAliases = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        public InventorySearchService(Func<IInventoryRepository> repositoryFactory)
        {
            _repositoryFactory = repositoryFactory;
            InitSortingAliases();
        }

        public virtual GenericSearchResult<InventoryInfo> SearchInventories(InventorySearchCriteria criteria)
        {
            var inventoryResponseGroup = EnumUtility.SafeParse(criteria.ResponseGroup, InventoryResponseGroup.OnlyInventory);
            var result = new GenericSearchResult<InventoryInfo>();

            if (inventoryResponseGroup == InventoryResponseGroup.OnlyInventory)
            {
                result = GetInventories(criteria);
            }

            else if (inventoryResponseGroup == InventoryResponseGroup.WithAllFulfillmentCenters)
            {
                result = GetAllFulfillmentCentersWithInventory(criteria);
            }

            return result;
        }

        private GenericSearchResult<InventoryInfo> GetAllFulfillmentCentersWithInventory(InventorySearchCriteria criteria)
        {
            var result = new GenericSearchResult<InventoryInfo>();
            using (var repository = _repositoryFactory())
            {
                repository.DisableChangesTracking();

                var query = GetFulfillmentCenterInventoryQuery(repository, criteria);
                result.TotalCount = query.Count();

                var sortInfos = criteria.SortInfos;
                if (sortInfos.IsNullOrEmpty())
                {
                    sortInfos = new[]
                    {
                        new SortInfo {SortColumn = $"{nameof(InventoryInfo.InStockQuantity)}", SortDirection = SortDirection.Descending},
                        new SortInfo {SortColumn = $"{nameof(InventoryInfo.FulfillmentCenterName)}", SortDirection = SortDirection.Ascending}
                    };
                }

                TryTransformSortingInfoColumnNames(_sortingAliases, sortInfos);

                query = query.OrderBySortInfos(sortInfos).ThenBy(x => x.FulfillmentCenter.Id);

                result.Results = query.Skip(criteria.Skip)
                    .Take(criteria.Take)
                    .AsEnumerable()
                    .Select(x => x.ToModel(AbstractTypeFactory<InventoryInfo>.TryCreateInstance()))
                    .ToList();
            }

            return result;
        }

        private GenericSearchResult<InventoryInfo> GetInventories(InventorySearchCriteria criteria)
        {
            var result = new GenericSearchResult<InventoryInfo>();
            using (var repository = _repositoryFactory())
            {
                repository.DisableChangesTracking();

                var query = GetInventoriesQuery(repository, criteria);
                result.TotalCount = query.Count();

                var sortInfos = criteria.SortInfos;
                if (sortInfos.IsNullOrEmpty())
                {
                    sortInfos = new[] { new SortInfo { SortColumn = "Id" } };
                }

                query = query.OrderBySortInfos(sortInfos).ThenBy(x => x.Id);

                result.Results = query.Skip(criteria.Skip)
                    .Take(criteria.Take)
                    .AsEnumerable()
                    .Select(x => x.ToModel(AbstractTypeFactory<InventoryInfo>.TryCreateInstance()))
                    .ToList();

            }

            return result;
        }


        /// <summary>
        /// Build column name map from resulting FulfillmentCenterInventoryInfo to FulfillmentCenterInventory object fields by which we queue the data
        /// </summary>
        protected virtual void InitSortingAliases()
        {
            var inventoryInfoRealType = AbstractTypeFactory<InventoryInfo>.TryCreateInstance().GetType();

            foreach (var property in inventoryInfoRealType.GetProperties())
            {
                _sortingAliases[property.Name] = $"{nameof(FulfillmentCenterInventory.Inventory)}.{property.Name}";
            }

            _sortingAliases["FulfillmentCenterName"] = $"{nameof(FulfillmentCenterInventory.FulfillmentCenter)}.{nameof(FulfillmentCenterEntity.Name)}";
        }

        protected virtual IQueryable<FulfillmentCenterInventory> GetFulfillmentCenterInventoryQuery(IInventoryRepository repository, InventorySearchCriteria criteria)
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
                    (x, y) => new
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

        protected virtual IQueryable<InventoryEntity> GetInventoriesQuery(IInventoryRepository repository, InventorySearchCriteria criteria)
        {
            var query = repository.Inventories;

            if (!criteria.ProductIds.IsNullOrEmpty())
            {
                query = query.Where(x => criteria.ProductIds.Contains(x.Sku));
            }

            if (!criteria.FulfillmentCenterIds.IsNullOrEmpty())
            {
                query = query.Where(x => criteria.FulfillmentCenterIds.Contains(x.FulfillmentCenterId));
            }

            return query;
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

        protected class FulfillmentCenterInventory
        {
            public FulfillmentCenterEntity FulfillmentCenter { get; set; }
            public InventoryEntity Inventory { get; set; }

            public virtual InventoryInfo ToModel(InventoryInfo inventoryInfo)
            {
                if (inventoryInfo == null)
                {
                    throw new ArgumentNullException(nameof(inventoryInfo));
                }

                Inventory?.ToModel(inventoryInfo);

                inventoryInfo.FulfillmentCenterName = FulfillmentCenter?.Name;
                inventoryInfo.FulfillmentCenterId = FulfillmentCenter?.Id;
                inventoryInfo.FulfillmentCenter = FulfillmentCenter?.ToModel(AbstractTypeFactory<FulfillmentCenter>.TryCreateInstance());

                return inventoryInfo;
            }
        }

    }
}
