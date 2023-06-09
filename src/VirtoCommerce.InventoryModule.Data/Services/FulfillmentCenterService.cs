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


        protected override Task<IList<FulfillmentCenterEntity>> LoadEntities(IRepository repository, IList<string> ids, string responseGroup)
        {
            return ((IInventoryRepository)repository).GetFulfillmentCentersAsync(ids);
        }
    }
}
