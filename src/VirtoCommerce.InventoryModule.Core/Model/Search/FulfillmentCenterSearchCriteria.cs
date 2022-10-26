using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.InventoryModule.Core.Model.Search
{
    public class FulfillmentCenterSearchCriteria : SearchCriteriaBase
    {
        public string OuterId { get; set; }
        public string OrganizationId { get; set; }
    }
}
