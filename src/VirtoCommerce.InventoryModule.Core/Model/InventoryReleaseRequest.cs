using System.Collections.Generic;

namespace VirtoCommerce.InventoryModule.Core.Model
{
    public class InventoryReleaseRequest
    {
        public string ParentId { get; set; }
        public IList<InventoryReservationRequestItem> Items { get; set; }
    }
}
