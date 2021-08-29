using System;
using System.Threading.Tasks;
using VirtoCommerce.InventoryModule.Core.Model.Search;

namespace VirtoCommerce.InventoryModule.Core.Services
{
    /// <summary>
    /// This interface should implement <see cref="SearchService<FulfillmentCenter>"/> without methods.
    /// Methods left for compatibility and should be removed after upgrade to inheritance
    /// </summary>
    public interface IFulfillmentCenterSearchService
    {
        [Obsolete(@"Need to remove after inherit IFulfillmentCenterSearchService from SearchService<FulfillmentCenter>.")]
        Task<FulfillmentCenterSearchResult> SearchCentersAsync(FulfillmentCenterSearchCriteria criteria);
    }
}
