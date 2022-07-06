using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.Extensions.Caching.Memory;
using VirtoCommerce.InventoryModule.Core.Events;
using VirtoCommerce.InventoryModule.Core.Services;
using VirtoCommerce.InventoryModule.Data.Caching;
using VirtoCommerce.Platform.Core.Caching;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Events;

namespace VirtoCommerce.InventoryModule.Data.Handlers
{
    public class FulfillmentCenterChangedEventHandler : IEventHandler<FulfillmentCenterChangedEvent>
    {
        private readonly IFulfillmentCenterGeoHashService _fulfillmentCenterGeoHashService;
        private readonly IFulfillmentCenterGeoService _fulfillmentCenterGeoService;
        private readonly IPlatformMemoryCache _platformMemoryCache;

        public FulfillmentCenterChangedEventHandler(
            IFulfillmentCenterGeoHashService fulfillmentCenterGeoHashService,
            IFulfillmentCenterGeoService fulfillmentCenterGeoService,
            IPlatformMemoryCache platformMemoryCache)
        {
            _fulfillmentCenterGeoHashService = fulfillmentCenterGeoHashService;
            _fulfillmentCenterGeoService = fulfillmentCenterGeoService;
            _platformMemoryCache = platformMemoryCache;
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
            var hash = await _fulfillmentCenterGeoHashService.GetGeoHashAsync();
            var savedHash = await GetGeohashCached();

            if (hash != savedHash)
            {
                FulfillmentCenterGeoCacheRegion.ExpireRegion();

                _ = await _fulfillmentCenterGeoService.GetNearest(string.Empty, default(int));
            }
        }

        private Task<string> GetGeohashCached()
        {
            var cacheKey = CacheKey.With(GetType(), nameof(GetGeohashCached));
            return _platformMemoryCache.GetOrCreateExclusiveAsync(cacheKey, async cacheEntry =>
            {
                cacheEntry.AddExpirationToken(FulfillmentCenterGeoCacheRegion.CreateChangeToken());

                return await _fulfillmentCenterGeoHashService.GetGeoHashAsync();
            });
        }
    }
}
