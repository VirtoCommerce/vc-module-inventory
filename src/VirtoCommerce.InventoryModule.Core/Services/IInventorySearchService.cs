using System;
using System.Threading.Tasks;
using VirtoCommerce.InventoryModule.Core.Model.Search;

namespace VirtoCommerce.InventoryModule.Core.Services
{
    /// <summary>
    /// This interface should implement <![CDATA[<see cref="SearchService<InventoryInfo>"/>]]> without methods.
    /// Methods left for compatibility and should be removed after upgrade to inheritance
    /// </summary>
    public interface IInventorySearchService
    {
        [Obsolete(@"Need to remove after inherit IInventorySearchService from SearchService<InventoryInfo>")]
        Task<InventoryInfoSearchResult> SearchInventoriesAsync(InventorySearchCriteria criteria);
    }
}
