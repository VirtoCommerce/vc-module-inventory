using System.Threading.Tasks;
using VirtoCommerce.InventoryModule.Core.Model;

namespace VirtoCommerce.InventoryModule.Core.Services
{
    public interface IInventoryReservationService
    {
        Task ReserveAsync(InventoryReserveRequest request);
        Task ReleaseAsync(InventoryReleaseRequest request);
    }
}
