using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using VirtoCommerce.InventoryModule.Data.Model;
using VirtoCommerce.InventoryModule.Data.Repositories;
using VirtoCommerce.InventoryModule.Data.Services;
using VirtoCommerce.Platform.Caching;
using VirtoCommerce.Platform.Core.Caching;
using VirtoCommerce.Platform.Core.Domain;
using VirtoCommerce.Platform.Core.Events;
using Xunit;

namespace VirtoCommerce.InventoryModule.Tests;

public class InventoryServiceDeleteTests
{
    private readonly Mock<IInventoryRepository> _repositoryMock = new();
    private readonly List<InventoryEntity> _removedEntities = [];

    /// <summary>
    /// InventoryEntity has a concurrency token (<see cref="InventoryEntity.RowVersion"/>), and InventoryInfo does not
    /// carry it. Deleting an entity built out of the model therefore leaves the token unset, EF adds it to the DELETE
    /// predicate as `RowVersion IS NULL`, no row matches, and the commit throws DbUpdateConcurrencyException instead of
    /// deleting the row. Deleting the entity loaded from the repository keeps the token intact.
    /// The exception itself only shows up against a real rowversion column, so what is asserted here is the invariant
    /// that prevents it: the entity handed to the repository for removal is the loaded one.
    /// </summary>
    [Fact]
    public async Task DeleteAsync_RemovesEntityLoadedFromRepository_KeepingConcurrencyToken()
    {
        // Arrange
        var entity = new InventoryEntity
        {
            Id = "inventory-1",
            Sku = "product-1",
            FulfillmentCenterId = "fulfillment-center-1",
            InStockQuantity = 10,
            RowVersion = [0, 0, 0, 0, 0, 0, 7, 209],
        };

        var service = GetInventoryService([entity]);

        // Act
        await service.DeleteAsync([entity.Id]);

        // Assert
        var removedEntity = Assert.Single(_removedEntities);
        Assert.Same(entity, removedEntity);
        Assert.NotNull(removedEntity.RowVersion);
    }

    private InventoryServiceImpl GetInventoryService(IList<InventoryEntity> entities)
    {
        _repositoryMock
            .Setup(x => x.GetByIdsAsync(It.IsAny<IList<string>>(), It.IsAny<string>()))
            .ReturnsAsync((IList<string> ids, string _) => entities.Where(x => ids.Contains(x.Id)).ToList());

        _repositoryMock
            .Setup(x => x.Remove(It.IsAny<InventoryEntity>()))
            .Callback((InventoryEntity entity) => _removedEntities.Add(entity));

        _repositoryMock
            .Setup(x => x.UnitOfWork)
            .Returns(Mock.Of<IUnitOfWork>());

        var memoryCache = new MemoryCache(Options.Create(new MemoryCacheOptions()));
        var platformMemoryCache = new PlatformMemoryCache(memoryCache, Options.Create(new CachingOptions()),
            new Mock<ILogger<PlatformMemoryCache>>().Object);

        return new InventoryServiceImpl(() => _repositoryMock.Object, platformMemoryCache, Mock.Of<IEventPublisher>());
    }
}
