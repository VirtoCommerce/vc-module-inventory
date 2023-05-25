using System;
using System.Collections.Generic;

namespace VirtoCommerce.InventoryModule.Core.Model
{
    public class ReserveStockRequest
    {
        public string OuterId { get; set; }
        public string OuterType { get; set; }
        public IList<string> FulfillmentCenterIds { get; set; }
        public string ProductId { get; set; }
        public string ParentId { get; set; }
        public long Quantity { get; set; }
        public DateTime? ExpirationDate { get; set; }
    }
}
