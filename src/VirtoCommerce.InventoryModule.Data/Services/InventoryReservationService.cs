using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using VirtoCommerce.InventoryModule.Core.Model;
using VirtoCommerce.InventoryModule.Core.Services;
using VirtoCommerce.InventoryModule.Data.Model;
using VirtoCommerce.InventoryModule.Data.Repositories;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.InventoryModule.Data.Services
{
    public class InventoryReservationService : IInventoryReservationService
    {
        private readonly Func<IInventoryRepository> _repositoryFactory;
        private readonly ILogger<InventoryReservationService> _logger;

        public InventoryReservationService(Func<IInventoryRepository> repositoryFactory, ILogger<InventoryReservationService> logger)
        {
            _repositoryFactory = repositoryFactory;
            _logger = logger;
        }

        public virtual Task ReserveAsync(InventoryReserveRequest request)
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

            return ReserveInternalAsync(request);
        }

        public virtual Task ReleaseAsync(InventoryReleaseRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (request.Items.IsNullOrEmpty())
            {
                throw new ArgumentException(nameof(request.Items));
            }

            return ReleaseInternalAsync(request);
        }


        protected virtual async Task ReserveInternalAsync(InventoryReserveRequest request)
        {
            try
            {
                await ProcessReserveRequest(request);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "{MethodName}: Concurrent exception occur", nameof(ReserveInternalAsync));
                //Try to process one more time
                await ProcessReserveRequest(request);
            }
        }

        protected virtual async Task ProcessReserveRequest(InventoryReserveRequest request)
        {
            using var repository = _repositoryFactory();
            var productIds = request.Items.Select(x => x.ProductId).ToList();
            var inventoryEntities = await GetInventoryEntities(repository, request.FulfillmentCenterIds, productIds);

            var newTransactions = new List<InventoryReservationTransactionEntity>();
            var modifiedInventoryEntities = new List<InventoryEntity>();

            foreach (var item in request.Items)
            {
                var itemInventoryEntities = inventoryEntities.Where(x => x.Sku == item.ProductId).ToArray();
                if (itemInventoryEntities.IsNullOrEmpty())
                {
                    _logger.LogInformation("{MethodName}: No inventory, parent: {Parent}, type: {Type}, item: {Item}", nameof(ProcessReserveRequest), request.ParentId, item.ItemType, item.ItemId);
                    break;
                }

                decimal reserveQuantityLeft = item.Quantity;
                var index = 0;

                do
                {
                    var inventoryEntity = itemInventoryEntities[index];
                    var fulfillmentInStockQuantity = inventoryEntity.InStockQuantity;
                    var needToReserveQuantity = reserveQuantityLeft;
                    index++;
                    var isLastElement = index == itemInventoryEntities.Length;

                    if (!isLastElement && fulfillmentInStockQuantity <= 0)
                    {
                        continue;
                    }

                    reserveQuantityLeft = Math.Max(0, reserveQuantityLeft - fulfillmentInStockQuantity);

                    if (isLastElement || reserveQuantityLeft == 0)
                    {
                        inventoryEntity.InStockQuantity -= needToReserveQuantity;
                        newTransactions.Add(BuildReservationTransaction(inventoryEntity, request, item, needToReserveQuantity));
                    }
                    else
                    {
                        inventoryEntity.InStockQuantity = 0;
                        newTransactions.Add(BuildReservationTransaction(inventoryEntity, request, item, fulfillmentInStockQuantity));
                    }

                    modifiedInventoryEntities.Add(inventoryEntity);
                }
                while (reserveQuantityLeft > 0 && index < itemInventoryEntities.Length);
            }

            await repository.SaveInventoryReservationTransactions(newTransactions, modifiedInventoryEntities);
        }

        protected virtual async Task ReleaseInternalAsync(InventoryReleaseRequest request)
        {
            try
            {
                await ProcessReleaseRequest(request);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "{MethodName}: Concurrent exception occur", nameof(ReleaseInternalAsync));
                //Try to process one more time
                await ProcessReleaseRequest(request);
            }
        }

        protected virtual async Task ProcessReleaseRequest(InventoryReleaseRequest request)
        {
            using var repository = _repositoryFactory();
            var outerIds = request.Items.Select(x => x.ItemId).ToList();
            var outerType = request.Items.FirstOrDefault()?.ItemType;
            var itemTransactionsEntities = await repository.GetInventoryReservationTransactionsAsync((int)TransactionType.Reservation, outerType, outerIds);

            if (itemTransactionsEntities == null || !itemTransactionsEntities.Any())
            {
                _logger.LogInformation("{MethodName}: No reserve transactions, parent: {Parent}, type: {Type}", nameof(ProcessReleaseRequest), request.ParentId, outerType);
                return;
            }

            var fulfillmentCenterIds = itemTransactionsEntities.Select(x => x.FulfillmentCenterId);
            var productIds = request.Items.Select(x => x.ProductId);

            var inventoryEntities = await repository
                .Inventories
                .Where(x => productIds.Contains(x.Sku) && fulfillmentCenterIds.Contains(x.FulfillmentCenterId))
                .ToListAsync();

            if (inventoryEntities.IsNullOrEmpty())
            {
                _logger.LogInformation("{MethodName}: No stocks, parent: {Parent}, type: {Type}", nameof(ProcessReleaseRequest), request.ParentId, outerType);
                return;
            }

            var newTransactions = new List<InventoryReservationTransactionEntity>();
            var modifiedInventoryEntities = new List<InventoryEntity>();

            foreach (var itemTransactionsEntity in itemTransactionsEntities)
            {
                var inventoryEntity = inventoryEntities.FirstOrDefault(x => x.FulfillmentCenterId == itemTransactionsEntity.FulfillmentCenterId);
                if (inventoryEntity == null)
                {
                    continue;
                }

                inventoryEntity.InStockQuantity += itemTransactionsEntity.Quantity;

                newTransactions.Add(BuildReleaseTransaction(inventoryEntity, itemTransactionsEntity));
                modifiedInventoryEntities.Add(inventoryEntity);
            }

            await repository.SaveInventoryReservationTransactions(newTransactions, modifiedInventoryEntities);
        }

        protected virtual async Task<IList<InventoryEntity>> GetInventoryEntities(IInventoryRepository repository, IList<string> fulfillmentCenterIds, IList<string> productIds)
        {
            //Will return all inventories for all products in first fulfillment center list and
            //all inventories for product-fulfillment pairs with InStockQuantity > 0
            return await repository
                .Inventories
                .Where(x => productIds.Contains(x.Sku) &&
                            fulfillmentCenterIds.Contains(x.FulfillmentCenterId) &&
                            (x.InStockQuantity > 0 || x.FulfillmentCenterId == fulfillmentCenterIds.First()))
                .OrderByDescending(x => x.FulfillmentCenterId == fulfillmentCenterIds.First())
                .ThenByDescending(x => x.InStockQuantity)
                .ThenBy(x => x.Id)
                .ToListAsync();
        }

        protected virtual InventoryReservationTransactionEntity BuildReservationTransaction(InventoryEntity inventoryEntity, InventoryReserveRequest request, InventoryReservationRequestItem item, decimal quantity)
        {
            var transaction = AbstractTypeFactory<InventoryReservationTransactionEntity>.TryCreateInstance();

            transaction.FulfillmentCenterId = inventoryEntity.FulfillmentCenterId;
            transaction.ExpirationDate = request.ExpirationDate;
            transaction.Quantity = quantity;
            transaction.ItemId = item.ItemId;
            transaction.ItemType = item.ItemType;
            transaction.ParentId = request.ParentId;
            transaction.ProductId = item.ProductId;
            transaction.Type = (int)TransactionType.Reservation;

            return transaction;
        }

        protected virtual InventoryReservationTransactionEntity BuildReleaseTransaction(InventoryEntity inventoryEntity, InventoryReservationTransactionEntity transactionEntity)
        {
            var transaction = AbstractTypeFactory<InventoryReservationTransactionEntity>.TryCreateInstance();

            transaction.FulfillmentCenterId = inventoryEntity.FulfillmentCenterId;
            transaction.ExpirationDate = transactionEntity.ExpirationDate;
            transaction.Quantity = -transactionEntity.Quantity;
            transaction.ItemId = transactionEntity.ItemId;
            transaction.ItemType = transactionEntity.ItemType;
            transaction.ParentId = transactionEntity.ParentId;
            transaction.ProductId = transactionEntity.ProductId;
            transaction.Type = (int)TransactionType.Release;

            return transaction;
        }
    }
}
