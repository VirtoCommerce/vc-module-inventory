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
    public class InventorySearchService : SearchService<InventorySearchCriteria, InventoryInfoSearchResult, InventoryInfo, InventoryEntity>, IInventorySearchService
    {
        public InventorySearchService(Func<IInventoryRepository> repositoryFactory, IPlatformMemoryCache platformMemoryCache, IInventoryService inventoryService)
            : base(repositoryFactory, platformMemoryCache, (ICrudService<InventoryInfo>)inventoryService)
        {
        }

        public virtual Task<InventoryInfoSearchResult> SearchInventoriesAsync(InventorySearchCriteria criteria)
        {
            return SearchAsync(criteria);
        }

        protected override IQueryable<InventoryEntity> BuildQuery(IRepository repository, InventorySearchCriteria criteria)
        {
            var query = ((IInventoryRepository)repository).Inventories;
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

        protected override IList<SortInfo> BuildSortExpression(InventorySearchCriteria criteria)
        {
            var sortInfos = criteria.SortInfos;
            if (sortInfos.IsNullOrEmpty())
            {
                sortInfos = new[]
                {
                    new SortInfo {
                        SortColumn = nameof(InventoryEntity.ModifiedDate),
                        SortDirection = SortDirection.Descending
                    }
                };
            }

            return sortInfos;
        }
    }
}
