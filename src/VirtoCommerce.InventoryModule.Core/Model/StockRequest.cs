using System.Collections.Generic;

namespace VirtoCommerce.InventoryModule.Core.Model
{
    public class StockRequest
    {
        public string OuterType { get; set; }
        public string ParentId { get; set; }
        public IList<StockRequestItem> Items { get; set; }
    }
}
