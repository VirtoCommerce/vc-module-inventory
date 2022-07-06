using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using VirtoCommerce.InventoryModule.Core.Events;
using VirtoCommerce.InventoryModule.Core.Services;
using VirtoCommerce.InventoryModule.Data.Caching;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Events;

namespace VirtoCommerce.InventoryModule.Data.Handlers
{
    public class FulfillmentCenterChangedEventHandler : IEventHandler<FulfillmentCenterChangedEvent>
    {
        private readonly IFulfillmentCenterGeoService _fulfillmentCenterGeoService;

        public FulfillmentCenterChangedEventHandler(IFulfillmentCenterGeoService fulfillmentCenterGeoService)
        {
            _fulfillmentCenterGeoService = fulfillmentCenterGeoService;
        }

        public Task Handle(FulfillmentCenterChangedEvent message)
        {
            var invalidate = message.ChangedEntries.Any(entry =>
                (entry.EntryState == EntryState.Modified && entry.NewEntry?.GeoLocation != entry.OldEntry?.GeoLocation) ||
                (entry.EntryState == EntryState.Added) ||
                (entry.EntryState == EntryState.Deleted));

            if (invalidate)
            {
                BackgroundJob.Enqueue(() => RecalculateFFDistance());
            }

            return Task.CompletedTask;
        }

        [DisableConcurrentExecution(10)]
        public virtual async Task RecalculateFFDistance()
        {
            FulfillmentCenterGeoCacheRegion.ExpireRegion();

            _ = await _fulfillmentCenterGeoService.GetNearest(string.Empty, default(int));
        }
    }
}
