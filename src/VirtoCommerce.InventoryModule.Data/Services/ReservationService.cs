using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using VirtoCommerce.InventoryModule.Core.Events;
using VirtoCommerce.InventoryModule.Core.Model;
using VirtoCommerce.InventoryModule.Core.Services;
using VirtoCommerce.InventoryModule.Data.Model;
using VirtoCommerce.InventoryModule.Data.Repositories;
using VirtoCommerce.Platform.Core.Caching;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Events;
using VirtoCommerce.Platform.Data.GenericCrud;

namespace VirtoCommerce.InventoryModule.Data.Services
{
    public class ReservationService : CrudService<InventoryReservationTransaction, InventoryReservationTransactionEntity, InventoryReservationTransactionChangingEvent, InventoryReservationTransactionChangedEvent>, IReservationService
    {
        private new readonly Func<IInventoryRepository> _repositoryFactory;
        private readonly ILogger<ReservationService> _logger;

        public ReservationService(
            Func<IInventoryRepository> repositoryFactory,
            IPlatformMemoryCache platformMemoryCache,
            IEventPublisher eventPublisher,
            ILogger<ReservationService> logger)
                : base(repositoryFactory, platformMemoryCache, eventPublisher)
        {
            _repositoryFactory = repositoryFactory;
            _logger = logger;
        }

        public virtual async Task ReserveStockAsync(ReserveStockRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (request.FulfillmentCenterIds.IsNullOrEmpty())
            {
                throw new ArgumentException(nameof(request.FulfillmentCenterIds));
            }

            if (request.Items.IsNullOrEmpty())
            {
                throw new ArgumentException(nameof(request.Items));
            }

            await ReserveStockInternalAsync(request);
        }

        public virtual async Task ReleaseStockAsync(ReleaseStockRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (request.Items.IsNullOrEmpty())
            {
                throw new ArgumentException(nameof(request.Items));
            }

            await ReleaseStockInternalAsync(request);
        }


        protected virtual async Task ReserveStockInternalAsync(ReserveStockRequest request)
        {
            try
            {
                await ProcessReserveRequest(request);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.Log(LogLevel.Error, ex, "ReserveStockAsync: Concurrent exception occur");
                //Try to process one more time
                await ProcessReserveRequest(request);
            }
        }

        protected virtual async Task ProcessReserveRequest(ReserveStockRequest request)
        {
            using var repository = _repositoryFactory();
            var productIds = request.Items.Select(x => x.ProductId);
            var productStocks = repository
                    .Inventories
                    .Where(x => productIds.Contains(x.Sku) &&
                                request.FulfillmentCenterIds.Contains(x.FulfillmentCenterId) &&
                                (x.FulfillmentCenterId == request.FulfillmentCenterIds.First() || x.InStockQuantity > 0))
                    .OrderByDescending(x => x.FulfillmentCenterId == request.FulfillmentCenterIds.First())
                    .ThenByDescending(x => x.InStockQuantity)
                    .ThenBy(x => x.Id)
                    .ToArray();

            var newTransactions = new List<InventoryReservationTransactionEntity>();
            var modifiedProductStocks = new List<InventoryEntity>();

            foreach (var item in request.Items)
            {
                var itemProductStocks = productStocks.Where(x => x.Sku == item.ProductId);
                if (productStocks.IsNullOrEmpty())
                {
                    _logger.LogInformation("ProcessReserveRequest: No stocks, item - {Item}, type - {Type}, parent - {Parent}", item.OuterId, request.OuterType, request.ParentId);
                    break;
                }

                decimal reserveQuantity = item.Quantity;
                var index = 0;

                do
                {
                    var productStock = productStocks[index];
                    index++;
                    var fulfillmentInStockQuantity = productStock.InStockQuantity;
                    var needToReserveQuantity = reserveQuantity;

                    if (index != productStocks.Length && fulfillmentInStockQuantity <= 0)
                    {
                        continue;
                    }

                    reserveQuantity = index == productStocks.Length ? 0 : reserveQuantity - fulfillmentInStockQuantity;
                    productStock.InStockQuantity = index == productStocks.Length || reserveQuantity <= 0
                        ? productStock.InStockQuantity - needToReserveQuantity
                        : 0;

                    newTransactions.Add(PrepareTransaction(productStock, request, item,
                        index == productStocks.Length || reserveQuantity <= 0
                            ? needToReserveQuantity
                            : fulfillmentInStockQuantity));
                    modifiedProductStocks.Add(productStock);

                } while (reserveQuantity > 0 && index < productStocks.Length);
            }

            await repository.StoreStockTransactions(newTransactions, modifiedProductStocks);
        }

        protected virtual async Task ReleaseStockInternalAsync(ReleaseStockRequest request)
        {
            try
            {
                await ProcessReleaseRequest(request);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.Log(LogLevel.Error, ex, "ReleaseStockAsync: Concurrent exception occur");
                //Try to process one more time
                await ProcessReleaseRequest(request);
            }
        }

        protected virtual async Task ProcessReleaseRequest(ReleaseStockRequest request)
        {
            using var repository = _repositoryFactory();
            var outerIds = request.Items.Select(x => x.OuterId).ToList();
            var itemTransactionsEntities = await repository.GetItemInventoryReservationTransactionsAsync(outerIds, request.OuterType, (int)TransactionType.Reservation);

            if (itemTransactionsEntities == null || !itemTransactionsEntities.Any())
            {
                _logger.LogInformation("ProcessReleaseRequest: No reserve transactions, parent - {Parent}, type - {Type}", request.ParentId, request.OuterType);
                return;
            }

            var fulfillmentCenterIds = itemTransactionsEntities.Select(x => x.FulfillmentCenterId);
            var productIds = request.Items.Select(x => x.ProductId);
            var productStocks = repository
                .Inventories
                .Where(x => productIds.Contains(x.Sku) && fulfillmentCenterIds.Contains(x.FulfillmentCenterId))
                .ToList();

            if (productStocks.IsNullOrEmpty())
            {
                _logger.LogInformation("ProcessReleaseRequest: No stocks, parent - {Parent}, type - {Type}", request.ParentId, request.OuterType);
                return;
            }

            var newTransactions = new List<InventoryReservationTransactionEntity>();
            var modifiedProductStocks = new List<InventoryEntity>();

            foreach (var itemTransactionsEntity in itemTransactionsEntities)
            {
                var productStock = productStocks.FirstOrDefault(x => x.FulfillmentCenterId == itemTransactionsEntity.FulfillmentCenterId);
                if (productStock == null)
                {
                    continue;
                }

                productStock.InStockQuantity += itemTransactionsEntity.Quantity;

                newTransactions.Add(PrepareTransaction(productStock, itemTransactionsEntity));
                modifiedProductStocks.Add(productStock);
            }

            await repository.StoreStockTransactions(newTransactions, modifiedProductStocks);
        }

        protected virtual InventoryReservationTransactionEntity PrepareTransaction(InventoryEntity productStock, ReserveStockRequest request, StockRequestItem item, decimal quantity)
        {
            var transaction = AbstractTypeFactory<InventoryReservationTransactionEntity>.TryCreateInstance();

            transaction.FulfillmentCenterId = productStock.FulfillmentCenterId;
            transaction.ExpirationDate = request.ExpirationDate;
            transaction.Quantity = quantity;
            transaction.OuterId = item.OuterId;
            transaction.OuterType = request.OuterType;
            transaction.ParentId = request.ParentId;
            transaction.ProductId = item.ProductId;
            transaction.Type = (int)TransactionType.Reservation;

            return transaction;
        }

        protected virtual InventoryReservationTransactionEntity PrepareTransaction(InventoryEntity productStock, InventoryReservationTransactionEntity transactionEntity)
        {
            var transaction = AbstractTypeFactory<InventoryReservationTransactionEntity>.TryCreateInstance();

            transaction.FulfillmentCenterId = productStock.FulfillmentCenterId;
            transaction.ExpirationDate = transactionEntity.ExpirationDate;
            transaction.Quantity = -transactionEntity.Quantity;
            transaction.OuterId = transactionEntity.OuterId;
            transaction.OuterType = transactionEntity.OuterType;
            transaction.ParentId = transactionEntity.ParentId;
            transaction.ProductId = transactionEntity.ProductId;
            transaction.Type = (int)TransactionType.Release;

            return transaction;
        }

        protected override async Task<IEnumerable<InventoryReservationTransactionEntity>> LoadEntities(IRepository repository, IEnumerable<string> ids, string responseGroup)
        {
            return await ((IInventoryRepository)repository).GetInventoryReservationTransactionsAsync(ids.ToArray(), responseGroup);
        }
    }
}
