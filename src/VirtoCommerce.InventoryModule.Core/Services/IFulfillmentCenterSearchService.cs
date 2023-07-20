using VirtoCommerce.InventoryModule.Core.Model;
using VirtoCommerce.InventoryModule.Core.Model.Search;
using VirtoCommerce.Platform.Core.GenericCrud;

namespace VirtoCommerce.InventoryModule.Core.Services
{
    public interface IFulfillmentCenterSearchService : ISearchService<FulfillmentCenterSearchCriteria, FulfillmentCenterSearchResult, FulfillmentCenter>
    {
    }
}
