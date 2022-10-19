using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.InventoryModule.Core.Model;
using VirtoCommerce.InventoryModule.Core.Model.Search;
using VirtoCommerce.InventoryModule.Core.Services;
using VirtoCommerce.InventoryModule.Data.Model;
using VirtoCommerce.InventoryModule.Data.Repositories;
using VirtoCommerce.Platform.Core.Caching;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Data.GenericCrud;

namespace VirtoCommerce.InventoryModule.Data.Services
{
    public class FulfillmentCenterSearchService : SearchService<FulfillmentCenterSearchCriteria, FulfillmentCenterSearchResult, FulfillmentCenter, FulfillmentCenterEntity>, IFulfillmentCenterSearchService
    {
        public FulfillmentCenterSearchService(Func<IInventoryRepository> repositoryFactory, IFulfillmentCenterService fulfillmentService, IPlatformMemoryCache platformMemoryCache)
             : base(repositoryFactory, platformMemoryCache, fulfillmentService)
        {
        }

        // TODO: Remove after 1 year (2023-08-02)
        [Obsolete("Use SearchAsync()")]
        public virtual Task<FulfillmentCenterSearchResult> SearchCentersAsync(FulfillmentCenterSearchCriteria criteria)
        {
            return SearchAsync(criteria);
        }

        protected override IQueryable<FulfillmentCenterEntity> BuildQuery(IRepository repository, FulfillmentCenterSearchCriteria criteria)
        {
            var query = ((IInventoryRepository)repository).FulfillmentCenters;

            if (criteria.ObjectIds?.Any() == true)
            {
                query = query.Where(x => criteria.ObjectIds.Contains(x.Id));
            }

            if (!string.IsNullOrEmpty(criteria.Keyword))
            {
                query = query.Where(x => x.Name.Contains(criteria.Keyword));
            }

            if (!string.IsNullOrEmpty(criteria.OuterId))
            {
                query = query.Where(x => x.OuterId == criteria.OuterId);
            }

            return query;
        }

        protected override IList<SortInfo> BuildSortExpression(FulfillmentCenterSearchCriteria criteria)
        {
            var sortInfos = criteria.SortInfos;
            if (sortInfos.IsNullOrEmpty())
            {
                sortInfos = new[] { new SortInfo { SortColumn = nameof(FulfillmentCenterEntity.Name) } };
            }

            return sortInfos;
        }
    }
}
