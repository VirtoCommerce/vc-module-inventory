namespace VirtoCommerce.InventoryModule.Core.Model
{
    public class ReleaseStockRequest
    {
        public string OuterId { get; set; }
        public string OuterType { get; set; }
        public string ProductId { get; set; }
    }
}
