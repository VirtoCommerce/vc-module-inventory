using System;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.InventoryModule.Core.Model
{
    public class InventoryReservationTransaction : AuditableEntity, ICloneable
    {
        public string ItemType { get; set; }
        public string ItemId { get; set; }
        public string ProductId { get; set; }
        public string FulfillmentCenterId { get; set; }
        public string ParentId { get; set; }
        public TransactionType Type { get; set; }
        public long Quantity { get; set; }
        public DateTime? ExpirationDate { get; set; }

        public object Clone()
        {
            var result = (InventoryReservationTransaction)MemberwiseClone();

            return result;
        }
    }
}
