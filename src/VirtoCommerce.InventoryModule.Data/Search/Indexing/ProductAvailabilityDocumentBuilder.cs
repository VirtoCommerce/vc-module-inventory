using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.InventoryModule.Core.Services;
using VirtoCommerce.SearchModule.Core.Model;
using VirtoCommerce.SearchModule.Core.Services;
using VirtoCommerce.Platform.Core.GenericCrud;
using VirtoCommerce.InventoryModule.Core.Model;

namespace VirtoCommerce.InventoryModule.Data.Search.Indexing
{
    /// <summary>
    /// Extend product indexation process and provides available_in field for indexed products
    /// </summary>
    public class ProductAvailabilityDocumentBuilder : IIndexDocumentBuilder
    {
        private readonly ICrudService<InventoryInfo> _inventoryService;

        public ProductAvailabilityDocumentBuilder(IInventoryService inventoryService)
        {
            _inventoryService = (ICrudService<InventoryInfo>)inventoryService;
        }

        public async Task<IList<IndexDocument>> GetDocumentsAsync(IList<string> documentIds)
        {
            var now = DateTime.UtcNow;
            var result = new List<IndexDocument>();
            var inventoriesGroupByProduct = await _inventoryService.GetByIdsAsync(documentIds.ToArray());
            foreach (var productInventories in inventoriesGroupByProduct.GroupBy(x => x.ProductId))
            {
                var document = new IndexDocument(productInventories.Key);
                foreach (var inventory in productInventories)
                {                   
                    if (inventory.IsAvailableOn(now))
                    {
                        document.Add(new IndexDocumentField("available_in", inventory.FulfillmentCenterId.ToLowerInvariant()) { IsRetrievable = true, IsFilterable = true, IsCollection = true });
                    }
                    result.Add(document);
                }
            }
            return await Task.FromResult(result);
        }
    }
}
