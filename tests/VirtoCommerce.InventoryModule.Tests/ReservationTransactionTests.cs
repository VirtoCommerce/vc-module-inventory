using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.Logging;
using Moq;
using VirtoCommerce.InventoryModule.Core.Model;
using VirtoCommerce.InventoryModule.Data.Model;
using VirtoCommerce.InventoryModule.Data.Repositories;
using VirtoCommerce.InventoryModule.Data.Services;
using VirtoCommerce.Platform.Core.Events;
using Xunit;

namespace VirtoCommerce.InventoryModule.Tests
{
    [Trait("Category", "Unit")]
    public class ReservationTransactionTests
    {
        private readonly Mock<IInventoryRepository> _repositoryMock;
        private readonly Mock<ILogger<InventoryReservationService>> _loggerMock;
        private readonly Mock<IEventPublisher> _eventPublisherMock;

        private readonly List<InventoryEntity> _initialInventories = new();
        private readonly List<InventoryReservationTransactionEntity> _initialReservationTransactions = new();

        private readonly List<InventoryEntity> _modifiedInventoryEntities = new();

        private readonly List<InventoryReservationTransactionEntity> _newTransactions = new();

        public ReservationTransactionTests()
        {
            _repositoryMock = new Mock<IInventoryRepository>();
            _loggerMock = new Mock<ILogger<InventoryReservationService>>();
            _eventPublisherMock = new Mock<IEventPublisher>();

            _repositoryMock
                .Setup(x => x.SaveInventoryReservationTransactions(It.IsAny<IList<InventoryReservationTransactionEntity>>(),
                    It.IsAny<IList<InventoryEntity>>()))
                .Callback((IEnumerable<InventoryReservationTransactionEntity> transactions,
                    IEnumerable<InventoryEntity> inventories) =>
                {
                    _modifiedInventoryEntities.AddRange(inventories.ToList());
                    _newTransactions.AddRange(transactions.ToList());
                });

            _repositoryMock
                .Setup(x => x.GetInventoryReservationTransactionsAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IList<string>>()))
                .ReturnsAsync((string type, string itemType, IList<string> ids) => _initialReservationTransactions);

            _repositoryMock.Setup(x => x.Inventories).Returns(_initialInventories.AsAsyncQueryable());
            _repositoryMock.Setup(x => x.InventoryReservationTransactions).Returns(_initialReservationTransactions.AsAsyncQueryable());
        }

        [Theory]
        [MemberData(nameof(ReserveStockTestData))]
        public async Task ReserveStockTest(InventoryEntity[] inventoryEntities, InventoryReserveRequest request, dynamic assert)
        {
            //Arrange
            _initialInventories.AddRange(inventoryEntities);
            var service = new InventoryReservationService(() => _repositoryMock.Object, _loggerMock.Object, _eventPublisherMock.Object);

            //Act
            await service.ReserveAsync(request);

            //Assert
            Assert.Equal(assert.ModifiedInventoriesCount, _modifiedInventoryEntities.Count);
            Assert.Equal(assert.NewTransactionsCount, _newTransactions.Count);
            Assert.Equal(assert.ModifiedInventoryQuantitySumLeft, _modifiedInventoryEntities.Sum(x => x.InStockQuantity));
            Assert.Equal(assert.FirstModifiedInventoryFulfillmentCenterId, _modifiedInventoryEntities.First().FulfillmentCenterId);
            Assert.Equal(request.Items.Sum(x => x.Quantity), _newTransactions.Sum(x => x.Quantity));
        }

        [Theory]
        [MemberData(nameof(ReleaseStockTestData))]
        public async Task ReleaseStockTest(InventoryEntity[] inventoryEntities, InventoryReservationTransactionEntity[] transactions, InventoryReleaseRequest request, dynamic assert)
        {
            //Arrange
            _initialInventories.AddRange(inventoryEntities);
            _initialReservationTransactions.AddRange(transactions);
            var service = new InventoryReservationService(() => _repositoryMock.Object, _loggerMock.Object, _eventPublisherMock.Object);

            //Act
            await service.ReleaseAsync(request);

            //Assert
            Assert.Equal(assert.ModifiedInventoriesCount, _modifiedInventoryEntities.Count);
            Assert.Equal(assert.NewTransactionsCount, _newTransactions.Count);
            Assert.Equal(assert.ModifiedInventoryQuantitySumLeft, _modifiedInventoryEntities.Sum(x => x.InStockQuantity));
            Assert.Equal(assert.FirstModifiedInventoryFulfillmentCenterId, _modifiedInventoryEntities.First().FulfillmentCenterId);
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
                new InventoryReserveRequest
                {
                    FulfillmentCenterIds = new[] { "1" },
                    Items = new List<InventoryReservationRequestItem>
                    {
                        new() { ItemId = "1", ProductId = "1", Quantity = 15 }
                    }
                },
                new
                {
                    ModifiedInventoriesCount = 1,
                    NewTransactionsCount = 1,
                    ModifiedInventoryQuantitySumLeft = -5,
                    FirstModifiedInventoryFulfillmentCenterId = "1",
                }
            },
            new object[]
            {
                new[]
                {
                    new InventoryEntity { Id = "1", InStockQuantity = 10, FulfillmentCenterId = "1", Sku = "1" },
                    new InventoryEntity { Id = "2", InStockQuantity = -5, FulfillmentCenterId = "2", Sku = "1" },
                },
                new InventoryReserveRequest
                {
                    FulfillmentCenterIds = new[] { "1", "2" },
                    Items = new List<InventoryReservationRequestItem>
                    {
                        new() { ItemId = "1", ProductId = "1", Quantity = 15 }
                    }
                },
                new
                {
                    ModifiedInventoriesCount = 1,
                    NewTransactionsCount = 1,
                    ModifiedInventoryQuantitySumLeft = -5,
                    FirstModifiedInventoryFulfillmentCenterId = "1",
                }
            },
            new object[]
            {
                new[]
                {
                    new InventoryEntity { Id = "1", InStockQuantity = -10, FulfillmentCenterId = "1", Sku = "1" },
                    new InventoryEntity { Id = "2", InStockQuantity = 5, FulfillmentCenterId = "2", Sku = "1" },
                },
                new InventoryReserveRequest
                {
                    FulfillmentCenterIds = new[] { "1", "2" },
                    Items = new List<InventoryReservationRequestItem>
                    {
                        new() { ItemId = "1", ProductId = "1", Quantity = 15 }
                    }
                },
                new
                {
                    ModifiedInventoriesCount = 1,
                    NewTransactionsCount = 1,
                    ModifiedInventoryQuantitySumLeft = -10,
                    FirstModifiedInventoryFulfillmentCenterId = "2",
                }
            },
            new object[]
            {
                new[]
                {
                    new InventoryEntity { Id = "1", InStockQuantity = -10, FulfillmentCenterId = "1", Sku = "1" },
                    new InventoryEntity { Id = "2", InStockQuantity = -15, FulfillmentCenterId = "2", Sku = "1" },
                },
                new InventoryReserveRequest
                {
                    FulfillmentCenterIds = new[] { "1", "2" },
                    Items = new List<InventoryReservationRequestItem>
                    {
                        new() { ItemId = "1", ProductId = "1", Quantity = 25 }
                    },
                },
                new
                {
                    ModifiedInventoriesCount = 1,
                    NewTransactionsCount = 1,
                    ModifiedInventoryQuantitySumLeft = -35,
                    FirstModifiedInventoryFulfillmentCenterId = "1",
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
                new InventoryReserveRequest
                {
                    FulfillmentCenterIds = new[] { "1", "2", "3", "4" },
                    Items = new List<InventoryReservationRequestItem>
                    {
                        new() { ItemId = "1", ProductId = "1", Quantity = 60 }
                    },
                },
                new
                {
                    ModifiedInventoriesCount = 2,
                    NewTransactionsCount = 2,
                    ModifiedInventoryQuantitySumLeft = 15,
                    FirstModifiedInventoryFulfillmentCenterId = "4",
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
                        Id = "1", Quantity = 10, FulfillmentCenterId = "1", ProductId = "1", ItemId = "1", ItemType = "LineItem", Type = TransactionType.Reserve.ToString()
                    },
                },
                new InventoryReleaseRequest
                {
                    Items = new List<InventoryReservationRequestItem>
                    {
                        new() { ItemId = "1", ProductId = "1", ItemType = "LineItem" }
                    }
                },
                new
                {
                    ModifiedInventoriesCount = 1,
                    NewTransactionsCount = 1,
                    ModifiedInventoryQuantitySumLeft = 20,
                    FirstModifiedInventoryFulfillmentCenterId = "1",
                    NewTransactionsSum = -10
                }
            }
        };
    }

    public static class AsyncQueryable
    {
        /// <summary>
        /// Returns the input typed as IQueryable that can be queried asynchronously
        /// </summary>
        /// <typeparam name="TEntity">The item type</typeparam>
        /// <param name="source">The input</param>
        public static IQueryable<TEntity> AsAsyncQueryable<TEntity>(this IEnumerable<TEntity> source)
            => new AsyncQueryable<TEntity>(source ?? throw new ArgumentNullException(nameof(source)));
    }

    public class AsyncQueryable<TEntity> : EnumerableQuery<TEntity>, IAsyncEnumerable<TEntity>, IQueryable<TEntity>
    {
        public AsyncQueryable(IEnumerable<TEntity> enumerable) : base(enumerable) { }
        public AsyncQueryable(Expression expression) : base(expression) { }
        public IAsyncEnumerator<TEntity> GetAsyncEnumerator(CancellationToken cancellationToken = default) => new AsyncEnumerator(this.AsEnumerable().GetEnumerator());
        IQueryProvider IQueryable.Provider => new AsyncQueryProvider(this);

        private class AsyncEnumerator : IAsyncEnumerator<TEntity>
        {
            private readonly IEnumerator<TEntity> _inner;
            public AsyncEnumerator(IEnumerator<TEntity> inner) => _inner = inner;
            public TEntity Current => _inner.Current;
            public ValueTask<bool> MoveNextAsync() => new(_inner.MoveNext());
#pragma warning disable CS1998 // Nothing to await
            public async ValueTask DisposeAsync() => _inner.Dispose();
#pragma warning restore CS1998
        }

        private class AsyncQueryProvider : IAsyncQueryProvider
        {
            private readonly IQueryProvider _inner;
            internal AsyncQueryProvider(IQueryProvider inner) => _inner = inner;
            public IQueryable CreateQuery(Expression expression) => new AsyncQueryable<TEntity>(expression);
            public IQueryable<TElement> CreateQuery<TElement>(Expression expression) => new AsyncQueryable<TElement>(expression);
            public object Execute(Expression expression) => _inner.Execute(expression);
            public TResult Execute<TResult>(Expression expression) => _inner.Execute<TResult>(expression);
            TResult IAsyncQueryProvider.ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken) => Execute<TResult>(expression);
        }
    }
}
