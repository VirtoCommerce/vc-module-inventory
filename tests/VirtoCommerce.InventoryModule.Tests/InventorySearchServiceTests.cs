using System;
using System.Collections.Generic;
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

    [Fact]
    public async Task SearchAsync_WithPositiveQuantityOnly_ShouldReturnInventoriesInStockOnly()
    {
        // Arrange
        var fulfillmentCenterId = NewId();
        var crudService = GetInventoryService();
        var searchService = GetInventorySearchService();

        await crudService.SaveChangesAsync([
            new InventoryInfo { Id = NewId(), FulfillmentCenterId = fulfillmentCenterId, ProductId = _productId1, InStockQuantity = 5 },
            new InventoryInfo { Id = NewId(), FulfillmentCenterId = fulfillmentCenterId, ProductId = _productId2, InStockQuantity = 0 },
            new InventoryInfo { Id = NewId(), FulfillmentCenterId = fulfillmentCenterId, ProductId = _productId3, InStockQuantity = -3 },
        ]);

        var criteria = new InventorySearchCriteria
        {
            FulfillmentCenterIds = [fulfillmentCenterId],
            WithPositiveQuantityOnly = true,
        };

        // Act
        var result = await searchService.SearchAsync(criteria);

        // Assert
        Assert.Equal(1, result.TotalCount);
        Assert.Equal(new List<string> { _productId1 }, result.Results.Select(x => x.ProductId).ToList());
    }
}
