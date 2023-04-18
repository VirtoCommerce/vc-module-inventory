using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.InventoryModule.Core.Services;
using VirtoCommerce.SearchModule.Core.Model;
using VirtoCommerce.SearchModule.Core.Services;

namespace VirtoCommerce.InventoryModule.Data.Search.Indexing
{
    /// <summary>
    /// Extend product indexation process and provides available_in field for indexed products
    /// </summary>
    public class ProductAvailabilityDocumentBuilder : IIndexDocumentBuilder
    {
        private readonly IInventoryService _inventoryService;

        public ProductAvailabilityDocumentBuilder(IInventoryService inventoryService)
        {
            _inventoryService = inventoryService;
        }

        public async Task<IList<IndexDocument>> GetDocumentsAsync(IList<string> documentIds)
        {
            var now = DateTime.UtcNow;
            var result = new List<IndexDocument>();
            var inventoriesGroupByProduct = await _inventoryService.GetProductsInventoryInfosAsync(documentIds.ToArray());
            foreach (var productInventories in inventoriesGroupByProduct.GroupBy(x => x.ProductId))
            {
                var document = new IndexDocument(productInventories.Key);
                var isInStock = false;
                var totalInStockQuantity = 0L;

                foreach (var inventory in productInventories)
                {
                    if (inventory.IsAvailableOn(now))
                    {
                        document.Add(new IndexDocumentField("available_in", inventory.FulfillmentCenterId.ToLowerInvariant()) { IsRetrievable = true, IsFilterable = true, IsCollection = true });
                        document.Add(new IndexDocumentField("fulfillmentCenter_name", inventory.FulfillmentCenterName.ToLowerInvariant()) { IsRetrievable = true, IsFilterable = true, IsCollection = true });
                        totalInStockQuantity += inventory.InStockQuantity;
                        isInStock = true;
                    }
                }

                document.Add(new IndexDocumentField("inStock_quantity", totalInStockQuantity) { IsRetrievable = true, IsFilterable = true, IsCollection = false });

                if (isInStock)
                {
                    document.Add(new IndexDocumentField("availability", "InStock") { IsRetrievable = true, IsFilterable = true, IsCollection = false });
                }
                else
                {
                    document.Add(new IndexDocumentField("availability", "OutOfStock") { IsRetrievable = true, IsFilterable = true, IsCollection = false });
                }

                result.Add(document);
            }
            return await Task.FromResult(result);
        }
    }
}
