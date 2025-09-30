using System;
using System.Threading.Tasks;
using VirtoCommerce.InventoryModule.Core.Model;
using VirtoCommerce.InventoryModule.Core.Model.Search;
using VirtoCommerce.Platform.Core.GenericCrud;

namespace VirtoCommerce.InventoryModule.Core.Services;

public interface IInventorySearchService : ISearchService<InventorySearchCriteria, InventoryInfoSearchResult, InventoryInfo>
{
    [Obsolete("Use SearchAsync()", DiagnosticId = "VC0011", UrlFormat = "https://docs.virtocommerce.org/products/products-virto3-versions")]
    Task<InventoryInfoSearchResult> SearchInventoriesAsync(InventorySearchCriteria criteria);
}
