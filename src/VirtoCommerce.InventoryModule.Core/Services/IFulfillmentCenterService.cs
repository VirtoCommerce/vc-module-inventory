using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VirtoCommerce.InventoryModule.Core.Model;

namespace VirtoCommerce.InventoryModule.Core.Services
{
    /// <summary>
    /// This interface should implement <![CDATA[<see cref="ICrudService<FulfillmentCenter>"/>]]> without methods.
    /// Methods left for compatibility and should be removed after upgrade to inheritance
    /// </summary>
    public interface IFulfillmentCenterService
    {
        [Obsolete(@"Need to remove after inherit IFulfillmentCenterService from ICrudService<FulfillmentCenter>")]
        Task SaveChangesAsync(IEnumerable<FulfillmentCenter> fulfillmentCenters);
        [Obsolete(@"Need to remove after inherit IFulfillmentCenterService from ICrudService<FulfillmentCenter>")]
        Task<IEnumerable<FulfillmentCenter>> GetByIdsAsync(IEnumerable<string> ids);
        [Obsolete(@"Need to remove after inherit IFulfillmentCenterService from ICrudService<FulfillmentCenter>")]
        Task DeleteAsync(IEnumerable<string> ids);
    }
}
