using VirtoCommerce.CatalogModule.Core.Model;

namespace VirtoCommerce.InventoryModule.Core.Model
{
    /// <summary>
    /// Inventory of a product along with the product information resolved from the catalog.
    /// </summary>
    public class ProductInventoryInfo
    {
        public string ProductId { get; set; }

        /// <summary>
        /// Null when the product no longer exists in the catalog.
        /// </summary>
        public CatalogProduct Product { get; set; }

        public InventoryInfo Inventory { get; set; }
    }
}
