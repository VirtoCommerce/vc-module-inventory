using System;
using System.Collections.Generic;

namespace VirtoCommerce.InventoryModule.Core.Model
{
    public class ReserveStockRequest : StockRequest
    {
        public IList<string> FulfillmentCenterIds { get; set; }
        public DateTime? ExpirationDate { get; set; }
    }
}
