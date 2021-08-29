using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.InventoryModule.Core.Model.Search;
using VirtoCommerce.InventoryModule.Core.Services;
using VirtoCommerce.InventoryModule.Data.Model;
using VirtoCommerce.InventoryModule.Data.Repositories;
using VirtoCommerce.Platform.Core.Caching;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.GenericCrud;
using VirtoCommerce.Platform.Data.GenericCrud;
using VirtoCommerce.InventoryModule.Core.Model;

namespace VirtoCommerce.InventoryModule.Data.Services
{
    public class FulfillmentCenterSearchService : SearchService<FulfillmentCenterSearchCriteria, FulfillmentCenterSearchResult, FulfillmentCenter, FulfillmentCenterEntity>, IFulfillmentCenterSearchService
    {
        public FulfillmentCenterSearchService(Func<IInventoryRepository> repositoryFactory, IFulfillmentCenterService fulfillmentService, IPlatformMemoryCache platformMemoryCache)
             : base(repositoryFactory, platformMemoryCache, (ICrudService<FulfillmentCenter>)fulfillmentService)
        {
        }

        public virtual async Task<FulfillmentCenterSearchResult> SearchCentersAsync(FulfillmentCenterSearchCriteria criteria)
        {
            return await SearchAsync(criteria);
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
