namespace VirtoCommerce.InventoryModule.Core.Model
{
    public class InventoryReservationRequestItem
    {
        public string OuterType { get; set; }
        public string OuterId { get; set; }
        public string ProductId { get; set; }
        public long Quantity { get; set; }
    }
}
