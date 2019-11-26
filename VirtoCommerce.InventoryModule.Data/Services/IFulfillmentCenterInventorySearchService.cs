using VirtoCommerce.Domain.Commerce.Model.Search;
using VirtoCommerce.Domain.Inventory.Model.Search;
using VirtoCommerce.InventoryModule.Data.Model;

namespace VirtoCommerce.InventoryModule.Data.Services
{
    public interface IFulfillmentCenterInventorySearchService
    {
        GenericSearchResult<FulfillmentCenterInventoryInfo> Search(InventorySearchCriteria criteria);
    }
}
