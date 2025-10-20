using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VirtoCommerce.InventoryModule.Core.Model;
using VirtoCommerce.Platform.Core.GenericCrud;

namespace VirtoCommerce.InventoryModule.Core.Services;

public interface IInventoryService : ICrudService<InventoryInfo>
{
    [Obsolete("Use IInventorySearchService.SearchAsync()", DiagnosticId = "VC0011", UrlFormat = "https://docs.virtocommerce.org/products/products-virto3-versions")]
    Task<IEnumerable<InventoryInfo>> GetProductsInventoryInfosAsync(IEnumerable<string> productIds, string responseGroup = null);

    [Obsolete("Use GetAsync()", DiagnosticId = "VC0011", UrlFormat = "https://docs.virtocommerce.org/products/products-virto3-versions")]
    Task<IEnumerable<InventoryInfo>> GetByIdsAsync(string[] ids, string responseGroup = null);
}
