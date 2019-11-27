using System.Collections.Generic;

namespace VirtoCommerce.InventoryModule.Data.Model
{
    public class FulfillmentCenterInventories
    {
        public FulfillmentCenterEntity FulfillmentCenterEntity { get; set; }
        public IEnumerable<InventoryEntity> Inventories { get; set; }
    }
}
