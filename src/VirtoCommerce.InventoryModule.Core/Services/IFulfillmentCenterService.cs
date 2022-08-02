using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VirtoCommerce.InventoryModule.Core.Model;
using VirtoCommerce.Platform.Core.GenericCrud;

namespace VirtoCommerce.InventoryModule.Core.Services
{
    public interface IFulfillmentCenterService : ICrudService<FulfillmentCenter>
    {
        // TODO: Remove after 1 year (2023-08-02)
        [Obsolete("Use GetAsync()")]
        Task<IEnumerable<FulfillmentCenter>> GetByIdsAsync(IEnumerable<string> ids);

        // TODO: Remove after 1 year (2023-08-02)
        Task DeleteAsync(IEnumerable<string> ids);
    }
}
