using System;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.InventoryModule.Core.Model;
using VirtoCommerce.InventoryModule.Core.Model.Search;
using VirtoCommerce.Platform.Core.Common;
using Xunit;

namespace VirtoCommerce.InventoryModule.Tests;

[Trait("Category", "Unit")]
public class InventorySearchServiceTests : InventoryTestsBase
{
    private const string _productId1 = "1";
    private const string _productId2 = "2";
    private const string _productId3 = "3";

    [Theory]
    [InlineData(_productId1, _productId2)]
    [InlineData(_productId2, _productId1)]
    [InlineData(_productId2)]
    [Obsolete("To be removed", DiagnosticId = "VC0011", UrlFormat = "https://docs.virtocommerce.org/products/products-virto3-versions")]
    public async Task SearchAsync_ShouldReturnTheSameResultAs_GetProductsInventoryInfosAsync(params string[] productIds)
    {
        // Arrange
        var crudService = GetInventoryService();
        var searchService = GetInventorySearchService();

        await crudService.SaveChangesAsync([
            new InventoryInfo { Id = NewId(), FulfillmentCenterId = NewId(), ProductId = _productId1 },
            new InventoryInfo { Id = NewId(), FulfillmentCenterId = NewId(), ProductId = _productId2 },
            new InventoryInfo { Id = NewId(), FulfillmentCenterId = NewId(), ProductId = _productId3 },
        ]);

        // Act
        var result1 = (await crudService.GetProductsInventoryInfosAsync(productIds))?.ToList();
        var result2 = await searchService.SearchAllAsync(new InventorySearchCriteria { ProductIds = productIds });

        // Assert
        Assert.NotNull(result1);
        Assert.NotNull(result2);

        // The number of returned objects should be equal to the number of requested product IDs
        Assert.Equal(productIds.Length, result1.Count);
        Assert.Equal(productIds.Length, result2.Count);

        // Returned collections should be different instances with equal but not the same objects
        AssertEqualButNotSame(result1, result2);
    }
}
