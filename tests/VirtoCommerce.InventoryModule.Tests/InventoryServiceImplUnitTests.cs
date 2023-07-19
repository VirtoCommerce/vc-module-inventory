using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using VirtoCommerce.InventoryModule.Core.Model;
using VirtoCommerce.InventoryModule.Data.Model;
using VirtoCommerce.InventoryModule.Data.Repositories;
using VirtoCommerce.InventoryModule.Data.Services;
using VirtoCommerce.Platform.Caching;
using VirtoCommerce.Platform.Core.Caching;
using VirtoCommerce.Platform.Core.Domain;
using VirtoCommerce.Platform.Core.Events;
using Xunit;

namespace VirtoCommerce.InventoryModule.Tests
{
    [Trait("Category", "Unit")]
    public class InventoryServiceImplUnitTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IInventoryRepository> _repositoryMock;
        private readonly Mock<IEventPublisher> _eventPublisherMock;
        private readonly List<InventoryEntity> _inventories = new List<InventoryEntity>();
        private int _getByIdsCallsCount;
        private int _getProductsInventoriesCallsCount;

        public InventoryServiceImplUnitTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _repositoryMock = new Mock<IInventoryRepository>();
            _eventPublisherMock = new Mock<IEventPublisher>();

            _repositoryMock
                .Setup(x => x.Add(It.IsAny<InventoryEntity>()))
                .Callback((InventoryEntity entity) => _inventories.Add(entity));

            _repositoryMock
                .Setup(x => x.GetByIdsAsync(It.IsAny<string[]>(), It.IsAny<string>()))
                .Callback(() => _getByIdsCallsCount++)
                .ReturnsAsync((string[] ids, string _) => _inventories.Where(x => ids.Contains(x.Id)).ToList());

            _repositoryMock
                .Setup(x => x.GetProductsInventoriesAsync(It.IsAny<IList<string>>(), It.IsAny<string>()))
                .Callback(() => _getProductsInventoriesCallsCount++)
                .ReturnsAsync((IList<string> productIds, string _) => _inventories.Where(x => productIds.Contains(x.Sku)).ToList());
        }

        [Fact]
        public async Task GetByIdsAsync_ShouldReturnCachedResult()
        {
            // Arrange
            var id1 = NewId();
            var id2 = NewId();
            var service = GetInventoryInfoServiceWithPlatformMemoryCache();

            // Act
            await service.SaveChangesAsync(new[]
            {
                new InventoryInfo { Id = id1, FulfillmentCenterId = NewId(), ProductId = NewId() },
                new InventoryInfo { Id = id2, FulfillmentCenterId = NewId(), ProductId = NewId() },
            });

            var result1 = await service.GetByIdsAsync(new[] { id1, id2 });
            var repositoryCallsCount1 = _getByIdsCallsCount;

            // Different order of IDs
            var result2 = await service.GetByIdsAsync(new[] { id2, id1 });
            var repositoryCallsCount2 = _getByIdsCallsCount;

            // One ID from previous calls
            var result3 = await service.GetByIdsAsync(new[] { id2 });
            var repositoryCallsCount3 = _getByIdsCallsCount;

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
        public async Task GetProductsInventoryInfosAsync_ShouldReturnCachedResult()
        {
            // Arrange
            var productId1 = NewId();
            var productId2 = NewId();
            var service = GetInventoryInfoServiceWithPlatformMemoryCache();

            // Act
            await service.SaveChangesAsync(new[]
            {
                new InventoryInfo { Id = NewId(), FulfillmentCenterId = NewId(), ProductId = productId1 },
                new InventoryInfo { Id = NewId(), FulfillmentCenterId = NewId(), ProductId = productId2 },
            });

            var result1 = await service.GetProductsInventoryInfosAsync(new[] { productId1, productId2 });
            var repositoryCallsCount1 = _getProductsInventoriesCallsCount;

            // Different order of IDs
            var result2 = await service.GetProductsInventoryInfosAsync(new[] { productId2, productId1 });
            var repositoryCallsCount2 = _getProductsInventoriesCallsCount;

            // One ID from previous calls
            var result3 = await service.GetProductsInventoryInfosAsync(new[] { productId2 });
            var repositoryCallsCount3 = _getProductsInventoriesCallsCount;

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
            var service = GetInventoryInfoServiceWithPlatformMemoryCache();

            // Act
            var beforeSave = (await service.GetByIdsAsync(new[] { id })).ToList();

            await service.SaveChangesAsync(new[]
            {
                new InventoryInfo { Id = id, FulfillmentCenterId = NewId(), ProductId = NewId() },
                new InventoryInfo { Id = NewId(), FulfillmentCenterId = NewId(), ProductId = NewId() },
            });

            var afterSave = (await service.GetByIdsAsync(new[] { id })).ToList();

            // Assert
            Assert.Empty(beforeSave);
            Assert.NotEmpty(afterSave);
            Assert.Single(afterSave);
        }

        [Fact]
        public async Task GetProductsInventoryInfosAsync_SaveChangesAsync_ShouldClearCache()
        {
            // Arrange
            var productId = NewId();
            var service = GetInventoryInfoServiceWithPlatformMemoryCache();

            // Act
            var beforeSave = (await service.GetProductsInventoryInfosAsync(new[] { productId })).ToList();

            await service.SaveChangesAsync(new[]
            {
                new InventoryInfo { Id = NewId(), FulfillmentCenterId = NewId(), ProductId = productId },
                new InventoryInfo { Id = NewId(), FulfillmentCenterId = NewId(), ProductId = productId },
                new InventoryInfo { Id = NewId(), FulfillmentCenterId = NewId(), ProductId = NewId() },
            });

            var afterSave = (await service.GetProductsInventoryInfosAsync(new[] { productId })).ToList();

            // Assert
            Assert.Empty(beforeSave);
            Assert.NotEmpty(afterSave);
            Assert.Equal(2, afterSave.Count);
        }


        private static void AssertEqualButNotSame(IEnumerable<InventoryInfo> enumerable1, IEnumerable<InventoryInfo> enumerable2)
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

        private static void AssertEqualButNotSame(InventoryInfo item1, InventoryInfo item2)
        {
            Assert.NotSame(item1, item2);
            Assert.Equal(item1, item2);
        }

        private static string NewId()
        {
            return Guid.NewGuid().ToString();
        }

        private InventoryServiceImpl GetInventoryInfoServiceWithPlatformMemoryCache()
        {
            var memoryCache = new MemoryCache(Options.Create(new MemoryCacheOptions()));
            var platformMemoryCache = new PlatformMemoryCache(memoryCache, Options.Create(new CachingOptions()), new Mock<ILogger<PlatformMemoryCache>>().Object);
            _repositoryMock.Setup(ss => ss.UnitOfWork).Returns(_unitOfWorkMock.Object);

            return GetInventoryServiceImpl(platformMemoryCache, _repositoryMock.Object);
        }

        private InventoryServiceImpl GetInventoryServiceImpl(IPlatformMemoryCache platformMemoryCache, IInventoryRepository repository)
        {
            return new InventoryServiceImpl(
                () => repository,
                _eventPublisherMock.Object,
                platformMemoryCache
                );
        }
    }
}
