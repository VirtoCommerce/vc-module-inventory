using System;
using System.Collections.Generic;
using System.Text;
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
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Domain;
using VirtoCommerce.Platform.Core.Events;
using Xunit;

namespace VirtoCommerce.InventoryModule.Tests
{
    public class InventoryServiceImplUnitTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IInventoryRepository> _repositoryFactoryMock;
        private readonly Mock<IEventPublisher> _eventPublisherMock;

        public InventoryServiceImplUnitTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _repositoryFactoryMock = new Mock<IInventoryRepository>();
            _eventPublisherMock = new Mock<IEventPublisher>();
        }

        [Fact]
        public async Task GetByIdsAsync_GetThenSaveInventoryInfo_ReturnCachedInventoryInfo()
        {
            //Arrange
            var id = Guid.NewGuid().ToString();
            var newInventoryInfo = new InventoryInfo { Id = id, ProductId = Guid.NewGuid().ToString() };
            var newInventoryInfoEntity = AbstractTypeFactory<InventoryEntity>.TryCreateInstance().FromModel(newInventoryInfo);
            var service = GetInventoryInfoServiceWithPlatformMemoryCache();
            _repositoryFactoryMock.Setup(x => x.Add(newInventoryInfoEntity))
                .Callback(() =>
                {
                    _repositoryFactoryMock.Setup(o => o.GetByIdsAsync(new[] { id }, null))
                        .ReturnsAsync(new[] { newInventoryInfoEntity });
                });

            //Act
            var nullInventoryInfo = await service.GetByIdsAsync(new[] { id }, null);
            await service.SaveChangesAsync(new[] { newInventoryInfo });
            var inventoryInfo = await service.GetByIdsAsync(new[] { id }, null);

            //Assert
            Assert.NotEqual(nullInventoryInfo, inventoryInfo);
        }

        private InventoryServiceImpl GetInventoryInfoServiceWithPlatformMemoryCache()
        {
            var memoryCache = new MemoryCache(Options.Create(new MemoryCacheOptions()));
            var platformMemoryCache = new PlatformMemoryCache(memoryCache, Options.Create(new CachingOptions()), new Mock<ILogger<PlatformMemoryCache>>().Object);
            _repositoryFactoryMock.Setup(ss => ss.UnitOfWork).Returns(_unitOfWorkMock.Object);

            return GetInventoryServiceImpl(platformMemoryCache, _repositoryFactoryMock.Object);
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
