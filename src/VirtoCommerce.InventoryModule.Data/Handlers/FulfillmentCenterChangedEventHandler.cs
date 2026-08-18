using System;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.InventoryModule.Core.Events;
using VirtoCommerce.InventoryModule.Core.Services;
using VirtoCommerce.InventoryModule.Data.Caching;
using VirtoCommerce.InventoryModule.Data.Jobs;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Events;
using VirtoCommerce.Platform.Core.Jobs;

namespace VirtoCommerce.InventoryModule.Data.Handlers
{
    public class FulfillmentCenterChangedEventHandler : IEventHandler<FulfillmentCenterChangedEvent>
    {
        private readonly IFulfillmentCenterGeoService _fulfillmentCenterGeoService;

        public FulfillmentCenterChangedEventHandler(IFulfillmentCenterGeoService fulfillmentCenterGeoService)
        {
            _fulfillmentCenterGeoService = fulfillmentCenterGeoService;
        }

        public async Task Handle(FulfillmentCenterChangedEvent message)
        {
            var invalidate = message.ChangedEntries.Any(entry =>
                (entry.EntryState == EntryState.Modified && entry.NewEntry?.GeoLocation != entry.OldEntry?.GeoLocation) ||
                (entry.EntryState == EntryState.Added) ||
                (entry.EntryState == EntryState.Deleted));

            if (invalidate)
            {
                var payload = AbstractTypeFactory<RecalculateFulfillmentCenterDistanceJobPayload>.TryCreateInstance();

                // The static facade, not an injected IBackgroundJob: RegisterEventHandler resolves this handler once from
                // the root provider and holds it for the process lifetime, so it must not capture a Scoped dependency.
                await BackgroundJob.Enqueue<RecalculateFulfillmentCenterDistanceJobHandler>(payload);
            }
        }

        /// <summary>
        /// Kept for background jobs enqueued by an earlier version, which reference this method by name.
        /// New work goes through <see cref="RecalculateFulfillmentCenterDistanceJobHandler"/>; remove this once no
        /// such job can still be pending.
        /// </summary>
        // Signature is byte-identical on purpose: Hangfire persists a queued job as type name + method name +
        // parameter types + serialized args, so changing any of them would strand already-queued jobs as Failed.
        // The [DisableConcurrentExecution(10)] that guarded it is gone with the Hangfire reference and has no
        // equivalent in the engine-agnostic API. Overlapping runs only duplicate work: the body expires a cache
        // region and re-reads it, so a second run recomputes the same values rather than corrupting them.
        [Obsolete("Enqueued indirectly by legacy Hangfire jobs only; new work uses RecalculateFulfillmentCenterDistanceJobHandler.", DiagnosticId = "VC0015", UrlFormat = "https://docs.virtocommerce.org/products/products-virto3-versions")]
        public virtual async Task RecalculateFFDistance()
        {
            FulfillmentCenterGeoCacheRegion.ExpireRegion();

            _ = await _fulfillmentCenterGeoService.GetNearestAsync(string.Empty, default(int));
        }
    }
}
