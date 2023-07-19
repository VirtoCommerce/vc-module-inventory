using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.InventoryModule.Data.Model;
using VirtoCommerce.Platform.Core.Common;
namespace VirtoCommerce.InventoryModule.Data.Repositories
{
    public interface IInventoryRepository : IRepository
    {
        IQueryable<InventoryEntity> Inventories { get; }
        IQueryable<FulfillmentCenterEntity> FulfillmentCenters { get; }
        IQueryable<InventoryReservationTransactionEntity> InventoryReservationTransactions { get; }

        Task<IList<InventoryEntity>> GetProductsInventoriesAsync(IList<string> productIds, string responseGroup = null);
        Task<IList<FulfillmentCenterEntity>> GetFulfillmentCentersAsync(IList<string> ids);
        Task<IList<InventoryEntity>> GetByIdsAsync(IList<string> ids, string responseGroup = null);
        Task<IList<InventoryReservationTransactionEntity>> GetInventoryReservationTransactionsAsync(string transactionType, string itemType, IList<string> itemIds);
        Task SaveInventoryReservationTransactions(IList<InventoryReservationTransactionEntity> transactions, IList<InventoryEntity> inventories);
    }
}
