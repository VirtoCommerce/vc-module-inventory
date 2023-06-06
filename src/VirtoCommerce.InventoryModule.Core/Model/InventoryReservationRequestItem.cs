namespace VirtoCommerce.InventoryModule.Core.Model
{
    public class InventoryReservationRequestItem
    {
        public string ItemType { get; set; }
        public string ItemId { get; set; }
        public string ProductId { get; set; }
        public long Quantity { get; set; }
    }
}
