using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using VirtoCommerce.InventoryModule.Core.Model;
using VirtoCommerce.InventoryModule.Core.Model.Search;
using VirtoCommerce.InventoryModule.Core.Services;
using VirtoCommerce.InventoryModule.Data.Services;
using VirtoCommerce.Platform.Caching;
using VirtoCommerce.Platform.Core.Common;
using Xunit;

namespace VirtoCommerce.InventoryModule.Tests
{
    public class FulfillmentCenterGeoServiceTests
    {
        [Fact]
        public async Task GetNearest_HasFFCs_TakeLimited()
        {
            // Arrange
            var knownLimit = 10;
            var fulfillmentCenters = GetFFCs();
            var target = GetGeoServiceMock(fulfillmentCenters);
            var ffc = fulfillmentCenters.FirstOrDefault();

            // Act
            var result = await target.GetNearestAsync(ffc.Id, 20);

            // Assert
            Assert.Equal(knownLimit, result.Count);
        }

        [Fact]
        public async Task GetNearest_HasFFCs_FFCWithoutLocationsReturned()
        {
            // Arrange
            var fulfillmentCenters = GetFFCs();
            var target = GetGeoServiceMock(fulfillmentCenters);
            var ffc = fulfillmentCenters.FirstOrDefault(x => x.GeoLocation == null);

            // Act
            var result = await target.GetNearestAsync(ffc.Id, 5);

            // Assert
            Assert.Equal(5, result.Count);
        }

        [Fact]
        public async Task GetNearest_HasFFCs_FFCWithLocationsReturned()
        {
            // Arrange
            var fulfillmentCenters = GetFFCs();
            var target = GetGeoServiceMock(fulfillmentCenters);
            var ffc = fulfillmentCenters.FirstOrDefault(x => x.GeoLocation != null);

            // Act
            var result = await target.GetNearestAsync(ffc.Id, 5);

            // Assert
            Assert.Equal(5, result.Count);
        }


        private static FulfillmentCenterGeoService GetGeoServiceMock(List<FulfillmentCenter> fulfillmentCenters)
        {
            var searchServiceMock = new FulfillmentCenterSearchServiceMock(fulfillmentCenters);

            var memoryCache = new MemoryCache(Options.Create(new MemoryCacheOptions()));
            var platformMemoryCache = new PlatformMemoryCache(memoryCache, Options.Create(new CachingOptions()), new Mock<ILogger<PlatformMemoryCache>>().Object);

            var target = new FulfillmentCenterGeoService(searchServiceMock, platformMemoryCache);
            return target;
        }

        private static List<FulfillmentCenter> GetFFCs()
        {
            var fixture = new Fixture();
            var list = new List<FulfillmentCenter>();

            for (int i = 0; i < 200; i++)
            {
                var ff = fixture.Create<FulfillmentCenter>();
                ff.GeoLocation = i % 2 == 0 ? "10.0,-10.0" : null;
                list.Add(ff);
            }

            return list;
        }
    }

    /// <summary>
    /// Custom search service mock 
    /// </summary>
    public class FulfillmentCenterSearchServiceMock : IFulfillmentCenterSearchService
    {
        private readonly List<FulfillmentCenter> _fulfillmentCenters;

        public FulfillmentCenterSearchServiceMock(List<FulfillmentCenter> fulfillmentCenters)
        {
            _fulfillmentCenters = fulfillmentCenters;
        }

        public Task<FulfillmentCenterSearchResult> SearchAsync(FulfillmentCenterSearchCriteria criteria)
        {
            var querable = _fulfillmentCenters.AsQueryable();

            if (!criteria.ObjectIds.IsNullOrEmpty())
            {
                querable = querable.Where(x => criteria.ObjectIds.Contains(x.Id));
            }

            var ffcs = querable.Skip(criteria.Skip).Take(criteria.Take).ToList();

            var result = new FulfillmentCenterSearchResult
            {
                Results = ffcs,
                TotalCount = _fulfillmentCenters.Count,
            };

            return Task.FromResult(result);
        }

        public Task<FulfillmentCenterSearchResult> SearchCentersAsync(FulfillmentCenterSearchCriteria criteria)
        {
            throw new NotImplementedException();
        }
    }
}
