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
    public class FulfillmentCenterSearchService : IFulfillmentCenterSearchService
    {
        private readonly Func<IInventoryRepository> _repositoryFactory;
        public FulfillmentCenterSearchService(Func<IInventoryRepository> repositoryFactory)
        {
            _repositoryFactory = repositoryFactory;
        }

        public virtual GenericSearchResult<FulfillmentCenter> SearchCenters(FulfillmentCenterSearchCriteria criteria)
        {
            var result = new GenericSearchResult<FulfillmentCenter>();
            using (var repository = _repositoryFactory())
            {
                repository.DisableChangesTracking();

                var query = GetFulfillmentCentersQuery(repository, criteria);

                var sortInfos = criteria.SortInfos;
                if (sortInfos.IsNullOrEmpty())
                {
                    sortInfos = new[] { new SortInfo { SortColumn = "Name" } };
                }

                query = query.OrderBySortInfos(sortInfos).ThenBy(x => x.Id);

                result.TotalCount = query.Count();
                result.Results = query.Skip(criteria.Skip)
                                 .Take(criteria.Take)
                                 .AsEnumerable()
                                 .Select(x => x.ToModel(AbstractTypeFactory<FulfillmentCenter>.TryCreateInstance()))
                                 .ToList();
            }

            return result;
        }

        protected virtual IQueryable<FulfillmentCenterEntity> GetFulfillmentCentersQuery(IInventoryRepository repository,
            FulfillmentCenterSearchCriteria criteria)
        {
            var query = repository.FulfillmentCenters;

            if (!string.IsNullOrEmpty(criteria.SearchPhrase))
            {
                query = query.Where(x => x.Name.Contains(criteria.SearchPhrase));
            }

            return query;
        }
    }
}
