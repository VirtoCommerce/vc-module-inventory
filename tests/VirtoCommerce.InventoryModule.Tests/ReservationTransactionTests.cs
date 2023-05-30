using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using VirtoCommerce.InventoryModule.Core.Model;
using VirtoCommerce.InventoryModule.Data.Model;
using VirtoCommerce.InventoryModule.Data.Repositories;
using VirtoCommerce.InventoryModule.Data.Services;
using Xunit;

namespace VirtoCommerce.InventoryModule.Tests
{
    [Trait("Category", "Unit")]
    public class ReservationTransactionTests
    {
        private readonly Mock<IInventoryRepository> _repositoryMock;
        private readonly Mock<ILogger<ReservationService>> _loggerMock;

        private readonly List<InventoryEntity> _initialStocks = new();
        private readonly List<InventoryReservationTransactionEntity> _initialReservationTransactions = new();

        private readonly List<InventoryEntity> _newStocks = new();

        private readonly List<InventoryReservationTransactionEntity> _newTransactions = new();

        public ReservationTransactionTests()
        {
            _repositoryMock = new Mock<IInventoryRepository>();
            _loggerMock = new Mock<ILogger<ReservationService>>();

            _repositoryMock
                .Setup(x => x.StoreStockTransactions(It.IsAny<IList<InventoryReservationTransactionEntity>>(),
                    It.IsAny<IList<InventoryEntity>>()))
                .Callback((IEnumerable<InventoryReservationTransactionEntity> transactions,
                    IEnumerable<InventoryEntity> inventories) =>
                {
                    _newStocks.AddRange(inventories.ToList());
                    _newTransactions.AddRange(transactions.ToList());
                });

            _repositoryMock
                .Setup(x => x.GetInventoryReservationTransactionsAsync(It.IsAny<IList<string>>(), It.IsAny<string>(), It.IsAny<int>()))
                .ReturnsAsync((IList<string> ids, string itemType, int type) => _initialReservationTransactions);

            _repositoryMock.Setup(x => x.Inventories).Returns(_initialStocks.AsQueryable());
            _repositoryMock.Setup(x => x.InventoryReservationTransactions).Returns(_initialReservationTransactions.AsQueryable());
        }

        [Theory]
        [MemberData(nameof(ReserveStockTestData))]
        public async Task ReserveStockTest(InventoryEntity[] stocks, ReserveStockRequest request, dynamic assert)
        {
            //Arrange
            _initialStocks.AddRange(stocks);
            var service = new ReservationService(() => _repositoryMock.Object, _loggerMock.Object);

            //Act
            await service.ReserveStockAsync(request);

            //Assert
            Assert.Equal(assert.NewStocksCount, _newStocks.Count);
            Assert.Equal(assert.NewTransactionsCount, _newTransactions.Count);
            Assert.Equal(assert.UpdatedStockQuantitySumLeft, _newStocks.Sum(x => x.InStockQuantity));
            Assert.Equal(assert.FirstStockChangeFulfillmentCenterId, _newStocks.First().FulfillmentCenterId);
            Assert.Equal(request.Items.Sum(x => x.Quantity), _newTransactions.Sum(x => x.Quantity));
        }

        [Theory]
        [MemberData(nameof(ReleaseStockTestData))]
        public async Task ReleaseStockTest(InventoryEntity[] stocks, InventoryReservationTransactionEntity[] transactions, ReleaseStockRequest request, dynamic assert)
        {
            //Arrange
            _initialStocks.AddRange(stocks);
            _initialReservationTransactions.AddRange(transactions);
            var service = new ReservationService(() => _repositoryMock.Object, _loggerMock.Object);

            //Act
            await service.ReleaseStockAsync(request);

            //Assert
            Assert.Equal(assert.NewStocksCount, _newStocks.Count);
            Assert.Equal(assert.NewTransactionsCount, _newTransactions.Count);
            Assert.Equal(assert.UpdatedStockQuantitySumLeft, _newStocks.Sum(x => x.InStockQuantity));
            Assert.Equal(assert.FirstStockChangeFulfillmentCenterId, _newStocks.First().FulfillmentCenterId);
            Assert.Equal(assert.NewTransactionsSum, _newTransactions.Sum(x => x.Quantity));
        }

        public static readonly IList<object[]> ReserveStockTestData = new List<object[]>
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

        public static readonly IList<object[]> ReleaseStockTestData = new List<object[]>
        {
            new object[]
            {
                new[]
                {
                    new InventoryEntity { Id = "1", InStockQuantity = 10, FulfillmentCenterId = "1", Sku = "1" },
                },
                new[]
                {
                    new InventoryReservationTransactionEntity
                    {
                        Id = "1", Quantity = 10, FulfillmentCenterId = "1", ProductId = "1", OuterId = "1", OuterType = "LineItem", Type = 0
                    },
                },
                new ReleaseStockRequest
                {
                    OuterType = "LineItem",
                    Items = new List<StockRequestItem>
                    {
                        new() { OuterId = "1", ProductId = "1" }
                    }
                },
                new
                {
                    NewStocksCount = 1,
                    NewTransactionsCount = 1,
                    UpdatedStockQuantitySumLeft = 20,
                    FirstStockChangeFulfillmentCenterId = "1",
                    NewTransactionsSum = -10
                }
            }
        };
    }
}
