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

        Task<IEnumerable<InventoryEntity>> GetProductsInventoriesAsync(IEnumerable<string> productIds, string responseGroup = null);
        Task<IEnumerable<FulfillmentCenterEntity>> GetFulfillmentCentersAsync(IEnumerable<string> ids);
        Task<IEnumerable<InventoryEntity>> GetByIdsAsync(string[] ids, string responseGroup = null);
        Task<IEnumerable<InventoryReservationTransactionEntity>> GetInventoryReservationTransactionsAsync(IEnumerable<string> ids, string responseGroup = null);
        Task<IList<InventoryReservationTransactionEntity>> GetItemInventoryReservationTransactionsAsync(IList<string> itemIds, string itemType, int transactionType);
        Task<IEnumerable<InventoryReservationTransactionEntity>> GetParentInventoryReservationTransactionsAsync(string parentId);
        Task StoreStockTransactions(IEnumerable<InventoryReservationTransactionEntity> transactions, IEnumerable<InventoryEntity> inventories);
    }
}
