using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.InventoryModule.Core.Model.Search;
using VirtoCommerce.InventoryModule.Core.Services;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.SearchModule.Core.Extensions;
using VirtoCommerce.SearchModule.Core.Model;
using VirtoCommerce.SearchModule.Core.Services;

namespace VirtoCommerce.InventoryModule.Data.Search.Indexing;

/// <summary>
/// Extends product indexation process and provides available_in field for indexed products
/// </summary>
public class ProductAvailabilityDocumentBuilder(IInventorySearchService inventorySearchService) : IIndexSchemaBuilder, IIndexDocumentBuilder
{
    public Task BuildSchemaAsync(IndexDocument schema)
    {
        schema.AddFilterableCollection("available_in");
        schema.AddFilterableCollection("fulfillmentCenter_name");
        schema.AddFilterableInteger("inStock_quantity");
        schema.AddFilterableString("availability");

        return Task.CompletedTask;
    }

    public async Task<IList<IndexDocument>> GetDocumentsAsync(IList<string> documentIds)
    {
        var now = DateTime.UtcNow;
        var result = new List<IndexDocument>();

        var searchCriteria = AbstractTypeFactory<InventorySearchCriteria>.TryCreateInstance();
        searchCriteria.ProductIds = documentIds;
        searchCriteria.Take = Math.Max(searchCriteria.Take, documentIds.Count);

        var inventories = await inventorySearchService.SearchAllNoCloneAsync(searchCriteria);

        var inventoriesGroupByProduct = inventories
            .GroupBy(x => x.ProductId)
            .ToDictionary(g => g.Key, g => g.ToList());

        foreach (var documentId in documentIds)
        {
            var document = new IndexDocument(documentId);

            var isInStock = false;
            var totalInStockQuantity = 0L;

            if (inventoriesGroupByProduct.TryGetValue(documentId, out var productInventories))
            {
                foreach (var inventory in productInventories.Where(i => i.IsAvailableOn(now)))
                {
                    document.AddFilterableCollection("available_in", inventory.FulfillmentCenterId.ToLowerInvariant());
                    document.AddFilterableCollection("fulfillmentCenter_name", inventory.FulfillmentCenterName.ToLowerInvariant());
                    totalInStockQuantity += inventory.InStockQuantity;
                    isInStock = true;
                }
            }

            document.AddFilterableInteger("inStock_quantity", (int)totalInStockQuantity);
            document.AddFilterableString("availability", isInStock ? "InStock" : "OutOfStock");

            result.Add(document);
        }

        return await Task.FromResult(result);
    }
}
