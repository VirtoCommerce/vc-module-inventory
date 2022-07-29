using System;
using System.Collections.Generic;
using System.Linq;
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
            var result = await base.GetAsync(ids.ToList());
            return result;
        }

        public virtual Task DeleteAsync(IEnumerable<string> ids)
        {
            return base.DeleteAsync(ids);
        }

        protected override Task<IEnumerable<FulfillmentCenterEntity>> LoadEntities(IRepository repository, IEnumerable<string> ids, string responseGroup)
        {
            return ((IInventoryRepository)repository).GetFulfillmentCentersAsync(ids);
        }
    }
}
