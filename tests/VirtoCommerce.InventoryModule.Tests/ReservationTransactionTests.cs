using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using VirtoCommerce.InventoryModule.Core.Model;
using VirtoCommerce.InventoryModule.Data.Model;
using VirtoCommerce.InventoryModule.Data.Repositories;
using VirtoCommerce.InventoryModule.Data.Services;
using VirtoCommerce.Platform.Core.Caching;
using VirtoCommerce.Platform.Core.Events;
using Xunit;

namespace VirtoCommerce.InventoryModule.Tests
{
    [Trait("Category", "Unit")]
    public class ReservationTransactionTests
    {
        private readonly Mock<IInventoryRepository> _repositoryMock;
        private readonly Mock<IPlatformMemoryCache> _platformMemoryCacheMock;
        private readonly Mock<IEventPublisher> _eventPublisherMock;
        private readonly Mock<ILogger<ReservationService>> _loggerMock;

        private readonly List<InventoryEntity> _initialStocks = new List<InventoryEntity>();

        private readonly List<InventoryEntity> _newStocks = new List<InventoryEntity>();

        private readonly List<InventoryReservationTransactionEntity> _newTransactions =
            new List<InventoryReservationTransactionEntity>();

        public ReservationTransactionTests()
        {
            _repositoryMock = new Mock<IInventoryRepository>();
            _platformMemoryCacheMock = new Mock<IPlatformMemoryCache>();
            _eventPublisherMock = new Mock<IEventPublisher>();
            _loggerMock = new Mock<ILogger<ReservationService>>();

            _repositoryMock
                .Setup(x => x.StoreStockTransactions(It.IsAny<IEnumerable<InventoryReservationTransactionEntity>>(),
                    It.IsAny<IEnumerable<InventoryEntity>>()))
                .Callback((IEnumerable<InventoryReservationTransactionEntity> transactions,
                    IEnumerable<InventoryEntity> inventories) =>
                {
                    _newStocks.AddRange(inventories.ToList());
                    _newTransactions.AddRange(transactions.ToList());
                });

            _repositoryMock.Setup(x => x.Inventories).Returns(_initialStocks.AsQueryable());

            //_repositoryMock
            //    .Setup(x => x.Inventories)
            //    .Callback()
        }

        [Theory]
        [MemberData(nameof(TestData))]
        public async Task ReserveStockTest(InventoryEntity[] stocks, ReserveStockRequest request, dynamic assert)
        {
            //Arrange
            _initialStocks.AddRange(stocks);

            var service = new ReservationService(() => _repositoryMock.Object, _platformMemoryCacheMock.Object,
                _eventPublisherMock.Object, _loggerMock.Object);

            //Act
            await service.ReserveStockAsync(request);

            //Assert
            Assert.Equal(assert.NewStocksCount, _newStocks.Count);
            Assert.Equal(assert.NewTransactionsCount, _newTransactions.Count);
            Assert.Equal(assert.UpdatedStockQuantitySumLeft, _newStocks.Sum(x => x.InStockQuantity));
            Assert.Equal(assert.FirstStockChangeFulfillmentCenterId, _newStocks.First().FulfillmentCenterId);
            Assert.Equal(request.Items.Sum(x => x.Quantity), _newTransactions.Sum(x => x.Quantity));

        }

        public static readonly IList<object[]> TestData = new List<object[]>
        {
            new object[]
            {
                new[]
                {
                    new InventoryEntity { Id = "1", InStockQuantity = 10, FulfillmentCenterId = "1", Sku = "1" },
                },
                new ReserveStockRequest
                {
                    FulfillmentCenterIds = new[] { "1" },
                    Items = new List<StockRequestItem>
                    {
                        new() { OuterId = "1", ProductId = "1", Quantity = 15 }
                    }
                },
                new
                {
                    NewStocksCount = 1,
                    NewTransactionsCount = 1,
                    UpdatedStockQuantitySumLeft = -5,
                    FirstStockChangeFulfillmentCenterId = "1",
                }
            },
            new object[]
            {
                new[]
                {
                    new InventoryEntity { Id = "1", InStockQuantity = 10, FulfillmentCenterId = "1", Sku = "1" },
                    new InventoryEntity { Id = "2", InStockQuantity = -5, FulfillmentCenterId = "2", Sku = "1" },
                },
                new ReserveStockRequest
                {
                    FulfillmentCenterIds = new[] { "1", "2" },
                    Items = new List<StockRequestItem>
                    {
                        new() { OuterId = "1", ProductId = "1", Quantity = 15 }
                    }
                },
                new
                {
                    NewStocksCount = 1,
                    NewTransactionsCount = 1,
                    UpdatedStockQuantitySumLeft = -5,
                    FirstStockChangeFulfillmentCenterId = "1",
                }
            },
            new object[]
            {
                new[]
                {
                    new InventoryEntity { Id = "1", InStockQuantity = -10, FulfillmentCenterId = "1", Sku = "1" },
                    new InventoryEntity { Id = "2", InStockQuantity = 5, FulfillmentCenterId = "2", Sku = "1" },
                },
                new ReserveStockRequest
                {
                    FulfillmentCenterIds = new[] { "1", "2" },
                    Items = new List<StockRequestItem>
                    {
                        new() { OuterId = "1", ProductId = "1", Quantity = 15 }
                    }
                },
                new
                {
                    NewStocksCount = 1,
                    NewTransactionsCount = 1,
                    UpdatedStockQuantitySumLeft = -10,
                    FirstStockChangeFulfillmentCenterId = "2",
                }
            },
            new object[]
            {
                new[]
                {
                    new InventoryEntity { Id = "1", InStockQuantity = -10, FulfillmentCenterId = "1", Sku = "1" },
                    new InventoryEntity { Id = "2", InStockQuantity = -15, FulfillmentCenterId = "2", Sku = "1" },
                },
                new ReserveStockRequest
                {
                    FulfillmentCenterIds = new[] { "1", "2" },
                    Items = new List<StockRequestItem>
                    {
                        new() { OuterId = "1", ProductId = "1", Quantity = 25 }
                    },
                },
                new
                {
                    NewStocksCount = 1,
                    NewTransactionsCount = 1,
                    UpdatedStockQuantitySumLeft = -35,
                    FirstStockChangeFulfillmentCenterId = "1",
                }
            },
            new object[]
            {
                new[]
                {
                    new InventoryEntity { Id = "1", InStockQuantity = 0, FulfillmentCenterId = "1", Sku = "1" },
                    new InventoryEntity { Id = "2", InStockQuantity = 15, FulfillmentCenterId = "2", Sku = "1" },
                    new InventoryEntity { Id = "3", InStockQuantity = 30, FulfillmentCenterId = "3", Sku = "1" },
                    new InventoryEntity { Id = "4", InStockQuantity = 45, FulfillmentCenterId = "4", Sku = "1" },
                },
                new ReserveStockRequest
                {
                    FulfillmentCenterIds = new[] { "1", "2", "3", "4" },
                    Items = new List<StockRequestItem>
                    {
                        new() { OuterId = "1", ProductId = "1", Quantity = 60 }
                    },
                },
                new
                {
                    NewStocksCount = 2,
                    NewTransactionsCount = 2,
                    UpdatedStockQuantitySumLeft = 15,
                    FirstStockChangeFulfillmentCenterId = "4",
                }
            }
        };
    }
}
