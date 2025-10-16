using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MockQueryable.Moq;
using Moq;
using VirtoCommerce.InventoryModule.Core.Model;
using VirtoCommerce.InventoryModule.Core.Services;
using VirtoCommerce.InventoryModule.Data.Model;
using VirtoCommerce.InventoryModule.Data.Repositories;
using VirtoCommerce.InventoryModule.Data.Services;
using VirtoCommerce.Platform.Caching;
using VirtoCommerce.Platform.Core.Domain;
using VirtoCommerce.Platform.Core.Events;
using VirtoCommerce.Platform.Core.GenericCrud;
using Xunit;

namespace VirtoCommerce.InventoryModule.Tests;

public abstract class InventoryTestsBase
{
    private readonly Mock<IInventoryRepository> _repositoryMock;
    private readonly PlatformMemoryCache _platformMemoryCache;
    private readonly List<InventoryEntity> _inventories = [];

    protected int GetByIdsCallsCount;
    protected int GetProductsInventoriesCallsCount;

    protected InventoryTestsBase()
    {
        var memoryCache = new MemoryCache(Options.Create(new MemoryCacheOptions()));
        _platformMemoryCache = new PlatformMemoryCache(memoryCache, Options.Create(new CachingOptions()), new Mock<ILogger<PlatformMemoryCache>>().Object);

        var unitOfWorkMock = new Mock<IUnitOfWork>();

        _repositoryMock = new Mock<IInventoryRepository>();

        _repositoryMock
            .Setup(x => x.Add(It.IsAny<InventoryEntity>()))
            .Callback((InventoryEntity entity) => _inventories.Add(entity));

        _repositoryMock
            .Setup(x => x.GetByIdsAsync(It.IsAny<IList<string>>(), It.IsAny<string>()))
            .Callback(() => GetByIdsCallsCount++)
            .ReturnsAsync((IList<string> ids, string _) => _inventories.Where(x => ids.Contains(x.Id)).ToList());

        _repositoryMock
            .Setup(x => x.GetProductsInventoriesAsync(It.IsAny<IList<string>>(), It.IsAny<string>()))
            .Callback(() => GetProductsInventoriesCallsCount++)
            .ReturnsAsync((IList<string> productIds, string _) => _inventories.Where(x => productIds.Contains(x.Sku)).ToList());

        _repositoryMock
            .Setup(ss => ss.UnitOfWork)
            .Returns(unitOfWorkMock.Object);

        var itemsDbSetMock = _inventories.BuildMockDbSet();

        _repositoryMock
            .Setup(x => x.Inventories)
            .Returns(itemsDbSetMock.Object);
    }

    protected static void AssertEqualButNotSame(IEnumerable<InventoryInfo> enumerable1, IEnumerable<InventoryInfo> enumerable2)
    {
        Assert.NotSame(enumerable1, enumerable2);

        var list1 = enumerable1.OrderBy(x => x.Id).ToList();
        var list2 = enumerable2.OrderBy(x => x.Id).ToList();

        Assert.Equal(list1.Count, list2.Count);

        for (var i = 0; i < list1.Count; i++)
        {
            AssertEqualButNotSame(list1[i], list2[i]);
        }
    }

    protected static void AssertEqualButNotSame(InventoryInfo item1, InventoryInfo item2)
    {
        Assert.NotSame(item1, item2);
        Assert.Equal(item1, item2);
    }

    protected static string NewId()
    {
        return Guid.NewGuid().ToString();
    }

    protected IInventorySearchService GetInventorySearchService()
    {
        return new InventorySearchService(
            () => _repositoryMock.Object,
            _platformMemoryCache,
            GetInventoryService(),
            Options.Create(new CrudOptions())
        );
    }

    protected IInventoryService GetInventoryService()
    {
        return new InventoryServiceImpl(
            () => _repositoryMock.Object,
            _platformMemoryCache,
            Mock.Of<IEventPublisher>()
        );
    }
}
