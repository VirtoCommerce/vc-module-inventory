using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using VirtoCommerce.CatalogModule.Core.Events;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.InventoryModule.Core.Model;
using VirtoCommerce.InventoryModule.Core.Services;
using VirtoCommerce.InventoryModule.Data.Handlers;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Events;
using Xunit;

namespace VirtoCommerce.InventoryModule.Tests;

[Trait("Category", "Unit")]
public class ProductChangedEventHandlerTests : InventoryTestsBase
{
    private const string _productId1 = "1";
    private const string _productId2 = "2";

    [Fact]
    public async Task Handle_ProductDeleted_ShouldDeleteItsInventories()
    {
        // Arrange
        var inventoryId1 = NewId();
        var inventoryId2 = NewId();

        await GetInventoryService().SaveChangesAsync([
            new InventoryInfo { Id = inventoryId1, FulfillmentCenterId = NewId(), ProductId = _productId1 },
            new InventoryInfo { Id = inventoryId2, FulfillmentCenterId = NewId(), ProductId = _productId1 },
            new InventoryInfo { Id = NewId(), FulfillmentCenterId = NewId(), ProductId = _productId2 },
        ]);

        var inventoryServiceMock = new Mock<IInventoryService>();
        var handler = new ProductChangedEventHandler(GetInventorySearchService(), inventoryServiceMock.Object);

        // Act
        await handler.Handle(GetEvent(EntryState.Deleted, _productId1));

        // Assert
        inventoryServiceMock.Verify(x => x.DeleteAsync(
            It.Is<IList<string>>(ids => ids.Count == 2 && ids.Contains(inventoryId1) && ids.Contains(inventoryId2)),
            It.IsAny<bool>()));
    }

    [Theory]
    [InlineData(EntryState.Added)]
    [InlineData(EntryState.Modified)]
    [InlineData(EntryState.Unchanged)]
    public async Task Handle_ProductNotDeleted_ShouldKeepItsInventories(EntryState entryState)
    {
        // Arrange
        await GetInventoryService().SaveChangesAsync([
            new InventoryInfo { Id = NewId(), FulfillmentCenterId = NewId(), ProductId = _productId1 },
        ]);

        var inventoryServiceMock = new Mock<IInventoryService>();
        var handler = new ProductChangedEventHandler(GetInventorySearchService(), inventoryServiceMock.Object);

        // Act
        await handler.Handle(GetEvent(entryState, _productId1));

        // Assert
        inventoryServiceMock.Verify(x => x.DeleteAsync(It.IsAny<IList<string>>(), It.IsAny<bool>()), Times.Never);
    }

    private static ProductChangedEvent GetEvent(EntryState entryState, string productId)
    {
        return new ProductChangedEvent([new GenericChangedEntry<CatalogProduct>(new CatalogProduct { Id = productId }, entryState)]);
    }
}
