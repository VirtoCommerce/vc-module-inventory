using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using VirtoCommerce.InventoryModule.Core.Model;
using VirtoCommerce.InventoryModule.Core.Model.Search;
using VirtoCommerce.InventoryModule.Core.Services;
using VirtoCommerce.InventoryModule.Data.Caching;
using VirtoCommerce.InventoryModule.Data.Model;
using VirtoCommerce.InventoryModule.Data.Repositories;
using VirtoCommerce.Platform.Core.Caching;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.GenericCrud;
using VirtoCommerce.Platform.Data.GenericCrud;

namespace VirtoCommerce.InventoryModule.Data.Services;

public class InventorySearchService(
    Func<IInventoryRepository> repositoryFactory,
    IPlatformMemoryCache platformMemoryCache,
    IInventoryService crudService,
    IOptions<CrudOptions> crudOptions)
    : SearchService<InventorySearchCriteria, InventoryInfoSearchResult, InventoryInfo, InventoryEntity>
        (repositoryFactory, platformMemoryCache, crudService, crudOptions),
        IInventorySearchService
{
    [Obsolete("Use SearchAsync()", DiagnosticId = "VC0011", UrlFormat = "https://docs.virtocommerce.org/products/products-virto3-versions")]
    public virtual Task<InventoryInfoSearchResult> SearchInventoriesAsync(InventorySearchCriteria criteria)
    {
        return SearchAsync(criteria);
    }

    protected override IQueryable<InventoryEntity> BuildQuery(IRepository repository, InventorySearchCriteria criteria)
    {
#pragma warning disable VC0011 // Type or member is obsolete
        return BuildQuery((IInventoryRepository)repository, criteria);
#pragma warning restore VC0011 // Type or member is obsolete
    }

    [Obsolete("Use BuildQuery(IRepository repository, InventorySearchCriteria criteria)", DiagnosticId = "VC0011", UrlFormat = "https://docs.virtocommerce.org/products/products-virto3-versions")]
    protected virtual IQueryable<InventoryEntity> BuildQuery(IInventoryRepository repository, InventorySearchCriteria criteria)
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

    protected override IList<SortInfo> BuildSortExpression(InventorySearchCriteria criteria)
    {
        var sortInfos = criteria.SortInfos;

        if (sortInfos.IsNullOrEmpty())
        {
            sortInfos =
            [
                new SortInfo {
                    SortColumn = nameof(InventoryEntity.ModifiedDate),
                    SortDirection = SortDirection.Descending,
                },
            ];
        }

        return sortInfos;
    }

    protected override IChangeToken CreateCacheToken(InventorySearchCriteria criteria)
    {
        return InventorySearchCacheRegion.CreateChangeToken();
    }
}
