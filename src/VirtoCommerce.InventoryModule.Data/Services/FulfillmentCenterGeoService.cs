using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using VirtoCommerce.InventoryModule.Core.Model;
using VirtoCommerce.InventoryModule.Core.Model.Search;
using VirtoCommerce.InventoryModule.Core.Services;
using VirtoCommerce.InventoryModule.Data.Caching;
using VirtoCommerce.InventoryModule.Data.Extensions;
using VirtoCommerce.Platform.Core.Caching;

namespace VirtoCommerce.InventoryModule.Data.Services
{
    public class FulfillmentCenterGeoService : IFulfillmentCenterGeoService
    {
        private readonly IFulfillmentCenterSearchService _searchService;
        private readonly IPlatformMemoryCache _platformMemoryCache;

        private readonly int _limit = 10;

        public FulfillmentCenterGeoService(
            IFulfillmentCenterSearchService fulfillmentCenterSearchService,
            IPlatformMemoryCache platformMemoryCache)
        {
            _searchService = fulfillmentCenterSearchService;
            _platformMemoryCache = platformMemoryCache;
        }

        public async Task<IList<FulfillmentCenter>> GetNearestAsync(string ffId, int take)
        {
            if (take > _limit)
            {
                take = _limit;
            }

            var ffcsByNearest = await GetFirstLevelCacheAsync();

            if (!ffcsByNearest.TryGetValue(ffId, out var nearestCenters))
            {
                return new List<FulfillmentCenter>();
            }

            var searchResult = await _searchService.SearchAsync(new FulfillmentCenterSearchCriteria
            {
                ObjectIds = nearestCenters,
                Take = take,
            });

            return searchResult.Results.OrderBy(x => nearestCenters.IndexOf(x.Id)).ToList();
        }

        private Task<Dictionary<string, List<string>>> GetFirstLevelCacheAsync()
        {
            var cacheKey = CacheKey.With(GetType(), nameof(GetFirstLevelCacheAsync));
            return _platformMemoryCache.GetOrCreateExclusiveAsync(cacheKey, async cacheEntry =>
            {
                cacheEntry.AddExpirationToken(FulfillmentCenterGeoCacheRegion.CreateChangeToken());

                var geoPoints = await LoadFulfillmentCentersGeoPointsAsync();
                return await CalculateNearestAsync(geoPoints);
            });
        }

        private async Task<List<FulfillmentCenterGeoPoint>> LoadFulfillmentCentersGeoPointsAsync()
        {
            var result = new List<FulfillmentCenterGeoPoint>();

            var countResult = await _searchService.SearchAsync(new FulfillmentCenterSearchCriteria());

            var pageSize = 20;

            for (var i = 0; i < countResult.TotalCount; i += pageSize)
            {
                var searchResult = await _searchService.SearchAsync(new FulfillmentCenterSearchCriteria
                {
                    Skip = i,
                    Take = pageSize,
                });

                var geoPoints = searchResult.Results.Select(x => new FulfillmentCenterGeoPoint { FulfillmentCenterId = x.Id, GeoLocation = x.GeoLocation });
                result.AddRange(geoPoints);
            }

            return result;
        }

        private Task<Dictionary<string, List<string>>> CalculateNearestAsync(List<FulfillmentCenterGeoPoint> ffcGeoPoints)
        {
            var nearestByFFCId = new Dictionary<string, List<string>>();

            var withLocations = ffcGeoPoints.Where(x => !string.IsNullOrEmpty(x.GeoLocation)).ToList();

            foreach (var ffc in withLocations)
            {
                nearestByFFCId.Add(ffc.FulfillmentCenterId, new List<string>());

                var sorted = new Dictionary<string, int>();

                foreach (var ffc2 in withLocations)
                {
                    if (ffc.FulfillmentCenterId == ffc2.FulfillmentCenterId)
                    {
                        continue;
                    }

                    var distance = ffc.GeoLocation.CalculateDistance(ffc2.GeoLocation);

                    if (distance is not null)
                    {
                        sorted.Add(ffc2.FulfillmentCenterId, distance.Value);
                    }
                }

                var resultList = sorted.OrderBy(x => x.Value).Take(_limit).Select(x => x.Key).ToList();
                nearestByFFCId[ffc.FulfillmentCenterId] = resultList;
            }

            // fill the rest ffcs
            var emptyLocations = ffcGeoPoints.Where(x => string.IsNullOrEmpty(x.GeoLocation)).Select(x => x.FulfillmentCenterId);
            foreach (var ffcIds in emptyLocations)
            {
                var list = ffcGeoPoints
                    .Where(x => x.FulfillmentCenterId != ffcIds)
                    .Take(_limit)
                    .Select(x => x.FulfillmentCenterId)
                    .ToList();

                nearestByFFCId.Add(ffcIds, list);
            }

            return Task.FromResult(nearestByFFCId);
        }
    }
}
