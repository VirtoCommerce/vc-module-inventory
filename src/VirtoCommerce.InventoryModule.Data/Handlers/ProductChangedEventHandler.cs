using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.CatalogModule.Core.Events;
using VirtoCommerce.InventoryModule.Core.Model.Search;
using VirtoCommerce.InventoryModule.Core.Services;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Events;

namespace VirtoCommerce.InventoryModule.Data.Handlers
{
    /// <summary>
    /// Deletes inventories of the products deleted from the catalog, otherwise they stay forever
    /// and are counted in every inventory search.
    /// </summary>
    public class ProductChangedEventHandler(
        IInventorySearchService inventorySearchService,
        IInventoryService inventoryService)
        : IEventHandler<ProductChangedEvent>
    {
        public virtual async Task Handle(ProductChangedEvent message)
        {
            var deletedProductIds = message.ChangedEntries
                .Where(x => x.EntryState == EntryState.Deleted && x.NewEntry != null)
                .Select(x => x.NewEntry.Id)
                .ToList();

            if (deletedProductIds.Count == 0)
            {
                return;
            }

            var criteria = AbstractTypeFactory<InventorySearchCriteria>.TryCreateInstance();
            criteria.ProductIds = deletedProductIds;

            var inventories = await inventorySearchService.SearchAllNoCloneAsync(criteria);

            if (inventories.Count > 0)
            {
                await inventoryService.DeleteAsync(inventories.Select(x => x.Id).ToList());
            }
        }
    }
}
