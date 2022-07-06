using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VirtoCommerce.InventoryModule.Core.Model;
using VirtoCommerce.InventoryModule.Core.Services;
using VirtoCommerce.InventoryModule.Data.Repositories;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.InventoryModule.Data.Services
{
    public class FulfillmentCenterGeoHashService : IFulfillmentCenterGeoHashService
    {
        private readonly Func<IInventoryRepository> _repositoryFactory;

        public FulfillmentCenterGeoHashService(Func<IInventoryRepository> repositoryFactory)
        {
            _repositoryFactory = repositoryFactory;
        }

        public async Task<string> GetGeoHashAsync()
        {
            using (var repository = _repositoryFactory())
            {
                var points = await repository.FulfillmentCenters
                    .Where(x => x.GeoLocation != null)
                    .OrderBy(x => x.Id)
                    .Select(x => new FulfillmentCenterGeoPoint
                    {
                        FulfillmentCenterId = x.Id,
                        GeoLocation = x.GeoLocation
                    })
                    .ToListAsync();

                var code = points.GetMD5Hash();

                return code.ToString();
            }
        }
    }
}
