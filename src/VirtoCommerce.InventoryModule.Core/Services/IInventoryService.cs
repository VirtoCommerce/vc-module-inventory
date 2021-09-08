using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VirtoCommerce.InventoryModule.Core.Model;

namespace VirtoCommerce.InventoryModule.Core.Services
{
	public interface IInventoryService
    {
        [Obsolete(@"Need to remove after inherit IInventoryService from ICrudService<InventoryInfo>")]
        Task<IEnumerable<InventoryInfo>> GetProductsInventoryInfosAsync(IEnumerable<string> productIds, string responseGroup = null);
        Task SaveChangesAsync(IEnumerable<InventoryInfo> inventoryInfos);
        [Obsolete(@"Need to remove after inherit IInventoryService from ICrudService<InventoryInfo>")]
        Task<IEnumerable<InventoryInfo>> GetByIdsAsync(string[] ids, string responseGroup = null);
    }
}
