using System.Collections.Generic;
using System.Threading.Tasks;
using VirtoCommerce.InventoryModule.Core.Services;
using VirtoCommerce.InventoryModule.Data.Search.Indexing;
using VirtoCommerce.SearchModule.Core.Model;
using Xunit;

namespace VirtoCommerce.InventoryModule.Tests;

[Trait("Category", "Unit")]
public class ProductAvailabilityDocumentBuilderTests
{
    /// <summary>
    /// Integrators (VCST-5328 / VP-9216, Innovadis) need to subclass the availability
    /// document builder and override <see cref="ProductAvailabilityDocumentBuilder.GetDocumentsAsync"/>
    /// to customize the indexed availability documents. This derived class only compiles
    /// when the base method is declared <c>virtual</c>; it is the red→green reproduction
    /// for making the method overridable. The override does not call the base implementation,
    /// so the injected search service is unused here.
    /// </summary>
    private sealed class TestableProductAvailabilityDocumentBuilder(IInventorySearchService inventorySearchService)
        : ProductAvailabilityDocumentBuilder(inventorySearchService)
    {
        public bool OverrideInvoked { get; private set; }

        public override Task<IList<IndexDocument>> GetDocumentsAsync(IList<string> documentIds)
        {
            OverrideInvoked = true;

            IList<IndexDocument> documents = new List<IndexDocument>();
            foreach (var documentId in documentIds)
            {
                documents.Add(new IndexDocument(documentId));
            }

            return Task.FromResult(documents);
        }
    }

    [Fact]
    public async Task GetDocumentsAsync_CanBeOverridden_InvokesDerivedImplementation()
    {
        // Arrange
        var derived = new TestableProductAvailabilityDocumentBuilder(inventorySearchService: null);
        // Reference through the base type to prove the override is dispatched virtually.
        ProductAvailabilityDocumentBuilder builder = derived;
        var documentIds = new List<string> { "product-1", "product-2" };

        // Act
        var documents = await builder.GetDocumentsAsync(documentIds);

        // Assert — the derived implementation ran via virtual dispatch, not the base body.
        Assert.True(derived.OverrideInvoked);
        Assert.Equal(documentIds.Count, documents.Count);
    }
}
