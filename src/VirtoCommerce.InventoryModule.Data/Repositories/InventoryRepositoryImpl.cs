using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using Microsoft.EntityFrameworkCore;
using VirtoCommerce.InventoryModule.Data.Model;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Domain;
using VirtoCommerce.Platform.Data.Infrastructure;

namespace VirtoCommerce.InventoryModule.Data.Repositories
{
    public class InventoryRepositoryImpl : DbContextRepositoryBase<InventoryDbContext>, IInventoryRepository
    {
        public InventoryRepositoryImpl(InventoryDbContext dbContext, IUnitOfWork unitOfWork = null) : base(dbContext, unitOfWork)
        {
        }

        #region IFoundationInventoryRepository Members

        public IQueryable<InventoryEntity> Inventories => DbContext.Set<InventoryEntity>();

        public IQueryable<FulfillmentCenterEntity> FulfillmentCenters => DbContext.Set<FulfillmentCenterEntity>();

        public IQueryable<InventoryReservationTransactionEntity> InventoryReservationTransactions => DbContext.Set<InventoryReservationTransactionEntity>();

        public IQueryable<FulfillmentCenterDynamicPropertyObjectValueEntity> DynamicPropertyObjectValues => DbContext.Set<FulfillmentCenterDynamicPropertyObjectValueEntity>();

        public virtual async Task<IEnumerable<InventoryEntity>> GetProductsInventoriesAsync(IEnumerable<string> productIds, string responseGroup = null)
        {
            var query = Inventories.Where(x => productIds.Contains(x.Sku));
            var inventoryResponseGroup = EnumUtility.SafeParseFlags(responseGroup, InventoryResponseGroup.Full);
            if (inventoryResponseGroup.HasFlag(InventoryResponseGroup.WithFulfillmentCenter))
            {
                query = query.Include(x => x.FulfillmentCenter);
            }
            var inventories = await query.ToListAsync();
            return inventories;
        }

        public virtual async Task<IEnumerable<FulfillmentCenterEntity>> GetFulfillmentCentersAsync(IEnumerable<string> ids)
        {
            var centers = await FulfillmentCenters.Where(x => ids.Contains(x.Id)).ToListAsync();

            var centersIds = centers.Select(x => x.Id).ToList();
            await DynamicPropertyObjectValues.Where(x => centersIds.Contains(x.ObjectId)).LoadAsync();

            return centers;
        }

        public async Task<IEnumerable<InventoryEntity>> GetByIdsAsync(string[] ids, string responseGroup = null)
        {
            var query = Inventories.Where(x => ids.Contains(x.Id));

            var inventoryResponseGroup = EnumUtility.SafeParseFlags(responseGroup, InventoryResponseGroup.Full);
            if (inventoryResponseGroup.HasFlag(InventoryResponseGroup.WithFulfillmentCenter))
            {
                query = query.Include(x => x.FulfillmentCenter);
            }
            var inventories = await query.ToListAsync();
            return inventories;
        }

        public virtual async Task<IList<InventoryReservationTransactionEntity>> GetInventoryReservationTransactionsAsync(int transactionType, string itemType, IList<string> itemIds)
        {
            var query = InventoryReservationTransactions.Where(x => x.Type == transactionType && x.OuterType == itemType && itemIds.Contains(x.OuterId));

            var result = await query.ToListAsync();
            return result;
        }

        public virtual async Task SaveInventoryReservationTransactions(IList<InventoryReservationTransactionEntity> transactions, IList<InventoryEntity> inventories)
        {
            using var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

            foreach (var transaction in transactions)
            {
                Add(transaction);
            }

            foreach (var inventory in inventories)
            {
                Update(inventory);
            }

            await UnitOfWork.CommitAsync();
            transactionScope.Complete();
        }

        #endregion
    }
}
