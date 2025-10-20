using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VirtoCommerce.InventoryModule.Core.Model;
using VirtoCommerce.InventoryModule.Core.Model.Search;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.InventoryModule.Core.Services;

public static class ProductInventorySearchServiceExtensions
{
    [Obsolete("Use SearchAllNoCloneAsync()", DiagnosticId = "VC0011", UrlFormat = "https://docs.virtocommerce.org/products/products-virto3-versions")]
    public static Task<IList<InventoryInfo>> SearchAllProductInventoriesNoCloneAsync(this IProductInventorySearchService searchService, ProductInventorySearchCriteria searchCriteria)
    {
        return searchService.SearchAllNoCloneAsync(searchCriteria);
    }
}
