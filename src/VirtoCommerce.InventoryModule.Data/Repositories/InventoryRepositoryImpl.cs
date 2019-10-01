using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VirtoCommerce.InventoryModule.Data.Model;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Domain;
using VirtoCommerce.Platform.Data.Infrastructure;

namespace VirtoCommerce.InventoryModule.Data.Repositories
{
    public class InventoryRepositoryImpl : DbContextRepositoryBase<InventoryDbContext>, IInventoryRepository
    {
        public InventoryRepositoryImpl(InventoryDbContext dbContext, IUnitOfWork unitOfWork = null) : base(dbContext, unitOfWork)
        {
        }

        #region IFoundationInventoryRepository Members

        public IQueryable<InventoryEntity> Inventories => DbContext.Set<InventoryEntity>();

        public IQueryable<FulfillmentCenterEntity> FulfillmentCenters => DbContext.Set<FulfillmentCenterEntity>();

        public virtual async Task<IEnumerable<InventoryEntity>> GetProductsInventoriesAsync(IEnumerable<string> productIds, string responseGroup = null)
        {
            var query = Inventories.Where(x => productIds.Contains(x.Sku));
            var inventoryResponseGroup = EnumUtility.SafeParseFlags(responseGroup, InventoryResponseGroup.Full);
            if (inventoryResponseGroup.HasFlag(InventoryResponseGroup.WithFulfillmentCenter))
            {
                query = query.Include(x => x.FulfillmentCenter);
            }
            var inventories = await query.ToListAsync();
            return inventories;
        }

        public virtual async Task<IEnumerable<FulfillmentCenterEntity>> GetFulfillmentCentersAsync(IEnumerable<string> ids)
        {
            return await FulfillmentCenters.Where(x => ids.Contains(x.Id)).ToListAsync();
        }

        public async Task<IEnumerable<InventoryEntity>> GetByIdsAsync(string[] ids, string responseGroup = null)
        {
            var query = Inventories.Where(x => ids.Contains(x.Id));

            var inventoryResponseGroup = EnumUtility.SafeParseFlags(responseGroup, InventoryResponseGroup.Full);
            if (inventoryResponseGroup.HasFlag(InventoryResponseGroup.WithFulfillmentCenter))
            {
                query = query.Include(x => x.FulfillmentCenter);
            }
            var inventories = await query.ToListAsync();
            return inventories;
        }

        #endregion
    }
}
