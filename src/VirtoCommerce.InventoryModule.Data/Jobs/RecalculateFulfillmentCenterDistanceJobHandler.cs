using System.Threading;
using System.Threading.Tasks;
using VirtoCommerce.InventoryModule.Core.Services;
using VirtoCommerce.InventoryModule.Data.Caching;
using VirtoCommerce.Platform.Core.Jobs;

namespace VirtoCommerce.InventoryModule.Data.Jobs
{
    /// <summary>
    /// Drops the cached fulfillment center distances and warms them again, off the request thread.
    /// </summary>
    public class RecalculateFulfillmentCenterDistanceJobHandler(IFulfillmentCenterGeoService fulfillmentCenterGeoService)
        : IBackgroundJobHandler<RecalculateFulfillmentCenterDistanceJobPayload>
    {
        public virtual async Task Execute(RecalculateFulfillmentCenterDistanceJobPayload payload, IJobExecutionContext context, CancellationToken cancellationToken = default)
        {
            FulfillmentCenterGeoCacheRegion.ExpireRegion();

            _ = await fulfillmentCenterGeoService.GetNearestAsync(string.Empty, default(int));
        }
    }
}
