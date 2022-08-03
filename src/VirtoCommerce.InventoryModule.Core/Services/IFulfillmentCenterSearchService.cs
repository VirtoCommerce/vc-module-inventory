using System;
using System.Threading.Tasks;
using VirtoCommerce.InventoryModule.Core.Model;
using VirtoCommerce.InventoryModule.Core.Model.Search;
using VirtoCommerce.Platform.Core.GenericCrud;

namespace VirtoCommerce.InventoryModule.Core.Services
{
    public interface IFulfillmentCenterSearchService : ISearchService<FulfillmentCenterSearchCriteria, FulfillmentCenterSearchResult, FulfillmentCenter>
    {
        // TODO: Remove after 1 year (2023-08-02)
        [Obsolete("Use SearchAsync()")]
        Task<FulfillmentCenterSearchResult> SearchCentersAsync(FulfillmentCenterSearchCriteria criteria);
    }
}
