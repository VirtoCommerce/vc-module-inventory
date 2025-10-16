using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.InventoryModule.Core.Model;
using VirtoCommerce.InventoryModule.Core.Services;
using VirtoCommerce.InventoryModule.Data.Model;
using VirtoCommerce.Platform.Core.ChangeLog;
using VirtoCommerce.SearchModule.Core.Model;
using VirtoCommerce.SearchModule.Core.Services;

namespace VirtoCommerce.InventoryModule.Data.Search.Indexing;

/// <summary>
/// Extend product indexation process. Invalidate products as changed when products availability in fulfillment centers updated.
/// </summary>
public class ProductAvailabilityChangesProvider(IChangeLogSearchService changeLogSearchService, IInventoryService inventoryService) : IIndexDocumentChangesProvider
{
    public const string ChangeLogObjectType = nameof(InventoryInfo);

    public async Task<long> GetTotalChangesCountAsync(DateTime? startDate, DateTime? endDate)
    {
        var criteria = new ChangeLogSearchCriteria
        {
            ObjectType = ChangeLogObjectType,
            StartDate = startDate,
            EndDate = endDate,
            Take = 0,
        };

        // Get changes count from operation log
        var searchResult = await changeLogSearchService.SearchAsync(criteria);

        return searchResult.TotalCount;
    }

    public virtual async Task<IList<IndexDocumentChange>> GetChangesAsync(DateTime? startDate, DateTime? endDate, long skip, long take)
    {
        var criteria = new ChangeLogSearchCriteria
        {
            ObjectType = ChangeLogObjectType,
            StartDate = startDate,
            EndDate = endDate,
            Skip = (int)skip,
            Take = (int)take,
        };

        // Get changes from operation log
        var operations = (await changeLogSearchService.SearchAsync(criteria)).Results;

        var inventories = await inventoryService.GetAsync(operations.Select(o => o.ObjectId).ToArray(), nameof(InventoryResponseGroup.Default));

        var result = operations.Join(inventories, o => o.ObjectId, i => i.Id, (o, i) => new IndexDocumentChange
        {
            DocumentId = i.ProductId,
            ChangeType = IndexDocumentChangeType.Modified,
            ChangeDate = o.ModifiedDate ?? o.CreatedDate,
        }).ToList();


        return await Task.FromResult(result);
    }
}
