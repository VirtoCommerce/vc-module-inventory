using System;
using System.Threading.Tasks;
using VirtoCommerce.InventoryModule.Core.Model;
using VirtoCommerce.InventoryModule.Core.Model.Search;
using VirtoCommerce.Platform.Core.GenericCrud;

namespace VirtoCommerce.InventoryModule.Core.Services;

/// <summary>
/// This interface is intended to search  the product inventories within all fulfillment centers, do a LeftJoin search with fulfillment centers and return  InventoryInfo
/// object for each Fulfillment center registered in the system even if it won't have an inventory record for a given product 
/// </summary>
public interface IProductInventorySearchService : ISearchService<ProductInventorySearchCriteria, InventoryInfoSearchResult, InventoryInfo>
{
    [Obsolete("Use SearchAsync()", DiagnosticId = "VC0011", UrlFormat = "https://docs.virtocommerce.org/products/products-virto3-versions")]
    Task<InventoryInfoSearchResult> SearchProductInventoriesAsync(ProductInventorySearchCriteria criteria);
}
