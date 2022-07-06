using System.Threading.Tasks;

namespace VirtoCommerce.InventoryModule.Core.Services
{
    public interface IFulfillmentCenterGeoHashService
    {
        Task<string> GetGeoHashAsync();
    }
}
