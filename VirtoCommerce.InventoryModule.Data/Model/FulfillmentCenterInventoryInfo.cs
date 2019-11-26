using System;
using VirtoCommerce.Domain.Inventory.Model;

namespace VirtoCommerce.InventoryModule.Data.Model
{
    public class FulfillmentCenterInventoryInfo : InventoryInfo
    {
        public string FulfillmentCenterName { get; set; }

        public FulfillmentCenterInventoryInfo FromEntity(InventoryEntity inventoryEntity)
        {
            if (inventoryEntity == null)
            {
                throw new ArgumentNullException(nameof(inventoryEntity));
            }

            inventoryEntity.ToModel(this);

            FulfillmentCenterName = inventoryEntity.FulfillmentCenter?.Name;

            return this;
        }
    }
}
