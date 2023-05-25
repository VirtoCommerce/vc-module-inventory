using System.Threading.Tasks;
using VirtoCommerce.InventoryModule.Core.Model;

namespace VirtoCommerce.InventoryModule.Core.Services
{
    public interface IReservationService
    {
        Task ReserveStockAsync(ReserveStockRequest request);
        Task ReleaseStockAsync(ReleaseStockRequest request);
        //IList<InventoryReservationTransaction> GetReservationTransactions();
    }
}
