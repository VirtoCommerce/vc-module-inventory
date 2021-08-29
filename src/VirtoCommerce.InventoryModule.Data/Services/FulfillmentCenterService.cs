using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VirtoCommerce.InventoryModule.Core.Events;
using VirtoCommerce.InventoryModule.Core.Model;
using VirtoCommerce.InventoryModule.Core.Services;
using VirtoCommerce.InventoryModule.Data.Model;
using VirtoCommerce.InventoryModule.Data.Repositories;
using VirtoCommerce.Platform.Core.Caching;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Events;
using VirtoCommerce.Platform.Data.GenericCrud;

namespace VirtoCommerce.InventoryModule.Data.Services
{
    public class FulfillmentCenterService : CrudService<FulfillmentCenter, FulfillmentCenterEntity, FulfillmentCenterChangingEvent, FulfillmentCenterChangedEvent>, IFulfillmentCenterService
    {

        public FulfillmentCenterService(Func<IInventoryRepository> repositoryFactory, IEventPublisher eventPublisher, IPlatformMemoryCache platformMemoryCache)
          : base(repositoryFactory, platformMemoryCache, eventPublisher)
        {
        }

        public virtual async Task<IEnumerable<FulfillmentCenter>> GetByIdsAsync(IEnumerable<string> ids)
        {
            return await base.GetByIdsAsync(ids);
        }

        public virtual async Task DeleteAsync(IEnumerable<string> ids)
        {
            await base.DeleteAsync(ids);
        }

        protected async override Task<IEnumerable<FulfillmentCenterEntity>> LoadEntities(IRepository repository, IEnumerable<string> ids, string responseGroup)
        {
            return await ((IInventoryRepository)repository).GetFulfillmentCentersAsync(ids);
        }
    }
}
