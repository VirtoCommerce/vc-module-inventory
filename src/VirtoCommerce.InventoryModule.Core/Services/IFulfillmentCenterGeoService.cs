using System.Collections.Generic;
using System.Threading.Tasks;
using VirtoCommerce.InventoryModule.Core.Model;

namespace VirtoCommerce.InventoryModule.Core.Services
{
    public interface IFulfillmentCenterGeoService
    {
        Task<IList<FulfillmentCenter>> GetNearest(string ffId, int take);
    }
}
