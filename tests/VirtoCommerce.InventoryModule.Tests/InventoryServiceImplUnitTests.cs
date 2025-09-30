using System;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.InventoryModule.Core.Model;
using Xunit;

namespace VirtoCommerce.InventoryModule.Tests;

[Trait("Category", "Unit")]
public class InventoryServiceImplUnitTests : InventoryTestsBase
{
    [Fact]
    public async Task GetByIdsAsync_ShouldReturnCachedResult()
    {
        // Arrange
        var id1 = NewId();
        var id2 = NewId();
        var service = GetInventoryService();

        // Act
        await service.SaveChangesAsync([
            new InventoryInfo { Id = id1, FulfillmentCenterId = NewId(), ProductId = NewId() },
            new InventoryInfo { Id = id2, FulfillmentCenterId = NewId(), ProductId = NewId() },
        ]);

        var result1 = await service.GetAsync([id1, id2]);
        var repositoryCallsCount1 = GetByIdsCallsCount;

        // Different order of IDs
        var result2 = await service.GetAsync([id2, id1]);
        var repositoryCallsCount2 = GetByIdsCallsCount;

        // One ID from previous calls
        var result3 = await service.GetAsync([id2]);
        var repositoryCallsCount3 = GetByIdsCallsCount;

        // Assert

        // Subsequent calls should not access repository
        Assert.NotEqual(0, repositoryCallsCount1);
        Assert.Equal(repositoryCallsCount1, repositoryCallsCount2);
        Assert.Equal(repositoryCallsCount1, repositoryCallsCount3);

        // Returned collections should be different instances with equal but not same objects
        AssertEqualButNotSame(result1, result2);
        AssertEqualButNotSame(result2.Take(1), result3);
    }

    [Fact]
    [Obsolete]
    public async Task GetProductsInventoryInfosAsync_ShouldReturnCachedResult()
    {
        // Arrange
        var productId1 = NewId();
        var productId2 = NewId();
        var service = GetInventoryService();

        // Act
        await service.SaveChangesAsync([
            new InventoryInfo { Id = NewId(), FulfillmentCenterId = NewId(), ProductId = productId1 },
            new InventoryInfo { Id = NewId(), FulfillmentCenterId = NewId(), ProductId = productId2 },
        ]);

        var result1 = await service.GetProductsInventoryInfosAsync([productId1, productId2]);
        var repositoryCallsCount1 = GetProductsInventoriesCallsCount;

        // Different order of IDs
        var result2 = await service.GetProductsInventoryInfosAsync([productId2, productId1]);
        var repositoryCallsCount2 = GetProductsInventoriesCallsCount;

        // One ID from previous calls
        var result3 = await service.GetProductsInventoryInfosAsync([productId2]);
        var repositoryCallsCount3 = GetProductsInventoriesCallsCount;

        // Assert

        // Subsequent calls should not access repository
        Assert.NotEqual(0, repositoryCallsCount1);
        Assert.Equal(repositoryCallsCount1, repositoryCallsCount2);
        Assert.Equal(repositoryCallsCount1, repositoryCallsCount3);

        // Returned collections should be different instances with equal but not same objects
        AssertEqualButNotSame(result1, result2);
        AssertEqualButNotSame(result2.Take(1), result3);
    }

    [Fact]
    public async Task GetByIdsAsync_SaveChangesAsync_ShouldClearCache()
    {
        // Arrange
        var id = NewId();
        var service = GetInventoryService();

        // Act
        var beforeSave = (await service.GetAsync([id])).ToList();

        await service.SaveChangesAsync([
            new InventoryInfo { Id = id, FulfillmentCenterId = NewId(), ProductId = NewId() },
            new InventoryInfo { Id = NewId(), FulfillmentCenterId = NewId(), ProductId = NewId() },
        ]);

        var afterSave = (await service.GetAsync([id])).ToList();

        // Assert
        Assert.Empty(beforeSave);
        Assert.NotEmpty(afterSave);
        Assert.Single(afterSave);
    }

    [Fact]
    [Obsolete]
    public async Task GetProductsInventoryInfosAsync_SaveChangesAsync_ShouldClearCache()
    {
        // Arrange
        var productId = NewId();
        var service = GetInventoryService();

        // Act
        var beforeSave = (await service.GetProductsInventoryInfosAsync([productId])).ToList();

        await service.SaveChangesAsync([
            new InventoryInfo { Id = NewId(), FulfillmentCenterId = NewId(), ProductId = productId },
            new InventoryInfo { Id = NewId(), FulfillmentCenterId = NewId(), ProductId = productId },
            new InventoryInfo { Id = NewId(), FulfillmentCenterId = NewId(), ProductId = NewId() },
        ]);

        var afterSave = (await service.GetProductsInventoryInfosAsync([productId])).ToList();

        // Assert
        Assert.Empty(beforeSave);
        Assert.NotEmpty(afterSave);
        Assert.Equal(2, afterSave.Count);
    }
}
