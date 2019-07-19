using System;
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
        public InventorySearchService(Func<IInventoryRepository> repositoryFactory)
        {
            _repositoryFactory = repositoryFactory;
        }

        public virtual GenericSearchResult<InventoryInfo> SearchInventories(InventorySearchCriteria criteria)
        {
            var result = new GenericSearchResult<InventoryInfo>();
            using (var repository = _repositoryFactory())
            {
                repository.DisableChangesTracking();

                var query = GetInventoriesQuery(repository, criteria);

                var sortInfos = criteria.SortInfos;
                if (sortInfos.IsNullOrEmpty())
                {
                    sortInfos = new[] { new SortInfo { SortColumn = "ModifiedDate" } };
                }

                query = query.OrderBySortInfos(sortInfos).ThenBy(x => x.Id);

                result.TotalCount = query.Count();
                result.Results = query.Skip(criteria.Skip)
                                 .Take(criteria.Take)
                                 .AsEnumerable()
                                 .Select(x => x.ToModel(AbstractTypeFactory<InventoryInfo>.TryCreateInstance()))
                                 .ToList();
            }

            return result;
        }

        protected virtual IQueryable<InventoryEntity> GetInventoriesQuery(IInventoryRepository repository,
            InventorySearchCriteria criteria)
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
    }
}
