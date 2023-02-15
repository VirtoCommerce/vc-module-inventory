using VirtoCommerce.Platform.Data.Model;

namespace VirtoCommerce.InventoryModule.Data.Model
{
    public class FulfillmentCenterDynamicPropertyObjectValueEntity : DynamicPropertyObjectValueEntity
    {
        public virtual FulfillmentCenterEntity FulfillmentCenter { get; set; }
    }
}
