using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.InventoryModule.Core.Model;
using VirtoCommerce.InventoryModule.Core.Model.Search;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.InventoryModule.Core.Services;

public static class ProductInventorySearchServiceExtensions
{
    public static async Task<IList<InventoryInfo>> SearchAllProductInventoriesNoCloneAsync(this IProductInventorySearchService searchService, ProductInventorySearchCriteria searchCriteria)
    {
        var result = new List<InventoryInfo>();

        await foreach (var item in searchService.SearchProductInventorBatchedAsync(searchCriteria))
        {
            result.AddRange(item.Results);
        }

        return result;
    }

    private static async IAsyncEnumerable<InventoryInfoSearchResult> SearchProductInventorBatchedAsync(this IProductInventorySearchService searchService, ProductInventorySearchCriteria searchCriteria)
    {
        searchCriteria = searchCriteria.CloneTyped();
        int totalCount;
        do
        {
            var searchResult = await searchService.SearchProductInventoriesAsync(searchCriteria);
            if (searchCriteria.Take == 0 || searchResult.Results.Any())
            {
                yield return searchResult;
            }

            if (searchCriteria.Take == 0)
            {
                break;
            }

            totalCount = searchResult.TotalCount;
            searchCriteria.Skip += searchCriteria.Take;
        }
        while (searchCriteria.Skip < totalCount);
    }
}
