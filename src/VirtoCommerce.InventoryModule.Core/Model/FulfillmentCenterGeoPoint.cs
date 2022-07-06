using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.InventoryModule.Core.Model
{
    public class FulfillmentCenterGeoPoint : ValueObject
    {
        public string FulfillmentCenterId { get; set; }

        public string GeoLocation { get; set; }
    }
}
