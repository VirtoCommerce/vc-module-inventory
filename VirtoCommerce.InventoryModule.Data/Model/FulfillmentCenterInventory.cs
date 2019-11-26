using System;
using VirtoCommerce.Domain.Inventory.Model;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.InventoryModule.Data.Model
{
    public class FulfillmentCenterInventory
    {
        public FulfillmentCenterEntity FulfillmentCenter { get; set; }
        public InventoryEntity Inventory { get; set; }

        public virtual FulfillmentCenterInventoryInfo ToModel(FulfillmentCenterInventoryInfo fulfillmentCenterInventoryInfo)
        {
            if (fulfillmentCenterInventoryInfo == null)
            {
                throw new ArgumentNullException(nameof(fulfillmentCenterInventoryInfo));
            }

            Inventory?.ToModel(fulfillmentCenterInventoryInfo);

            fulfillmentCenterInventoryInfo.FulfillmentCenterName = FulfillmentCenter?.Name;
            fulfillmentCenterInventoryInfo.FulfillmentCenterId = FulfillmentCenter?.Id;
            fulfillmentCenterInventoryInfo.FulfillmentCenter = FulfillmentCenter.ToModel(AbstractTypeFactory<FulfillmentCenter>.TryCreateInstance());

            return fulfillmentCenterInventoryInfo;
        }
    }
}
