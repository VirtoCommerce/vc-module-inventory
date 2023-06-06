using System;
using System.Collections.Generic;

namespace VirtoCommerce.InventoryModule.Core.Model
{
    public class InventoryReserveRequest
    {
        public string ParentId { get; set; }
        public IList<InventoryReservationRequestItem> Items { get; set; }
        public IList<string> FulfillmentCenterIds { get; set; }
        public DateTime? ExpirationDate { get; set; }
    }
}
