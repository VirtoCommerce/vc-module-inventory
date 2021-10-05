using System.Threading.Tasks;
using VirtoCommerce.InventoryModule.Core.Model.Search;

namespace VirtoCommerce.InventoryModule.Core.Services
{
    public interface IInventorySearchService
    {
        Task<InventoryInfoSearchResult> SearchInventoriesAsync(InventorySearchCriteria criteria);
    }
}
