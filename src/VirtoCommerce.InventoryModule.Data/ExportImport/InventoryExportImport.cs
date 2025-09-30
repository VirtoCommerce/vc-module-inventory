using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using VirtoCommerce.InventoryModule.Core;
using VirtoCommerce.InventoryModule.Core.Model;
using VirtoCommerce.InventoryModule.Core.Model.Search;
using VirtoCommerce.InventoryModule.Core.Services;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.ExportImport;
using VirtoCommerce.Platform.Core.Settings;

namespace VirtoCommerce.InventoryModule.Data.ExportImport;

public sealed class InventoryExportImport(
    IInventoryService inventoryService,
    IInventorySearchService inventorySearchService,
    IFulfillmentCenterSearchService fulfillmentCenterSearchService,
    IFulfillmentCenterService fulfillmentCenterService,
    ISettingsManager settingsManager,
    JsonSerializer jsonSerializer)
{
    private int? _batchSize;

    private int BatchSize
    {
        get
        {
            _batchSize ??= settingsManager.GetValue<int>(ModuleConstants.Settings.General.PageSize);

            return (int)_batchSize;
        }
    }

    public async Task DoExportAsync(Stream outStream, Action<ExportImportProgressInfo> progressCallback, ICancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var progressInfo = new ExportImportProgressInfo { Description = "The fulfilmentCenters are loading" };
        progressCallback(progressInfo);

        await using var streamWriter = new StreamWriter(outStream, Encoding.UTF8);
        await using var writer = new JsonTextWriter(streamWriter);
        await writer.WriteStartObjectAsync();

        await writer.WritePropertyNameAsync("FulfillmentCenters");
        await writer.SerializeArrayWithPagingAsync(jsonSerializer, BatchSize, async (skip, take) =>
        {
            var searchCriteria = AbstractTypeFactory<FulfillmentCenterSearchCriteria>.TryCreateInstance();
            searchCriteria.Take = take;
            searchCriteria.Skip = skip;
            var searchResult = await fulfillmentCenterSearchService.SearchNoCloneAsync(searchCriteria);
            return (GenericSearchResult<FulfillmentCenter>)searchResult;
        }, (processedCount, totalCount) =>
        {
            progressInfo.Description = $"{processedCount} of {totalCount} FulfillmentCenters have been exported";
            progressCallback(progressInfo);
        }, cancellationToken);

        progressInfo.Description = "The Inventories are loading";
        progressCallback(progressInfo);

        await writer.WritePropertyNameAsync("Inventories");
        await writer.SerializeArrayWithPagingAsync(jsonSerializer, BatchSize, async (skip, take) =>
        {
            var searchCriteria = AbstractTypeFactory<InventorySearchCriteria>.TryCreateInstance();
            searchCriteria.Take = take;
            searchCriteria.Skip = skip;
            var searchResult = await inventorySearchService.SearchAsync(searchCriteria);
            return (GenericSearchResult<InventoryInfo>)searchResult;
        }, (processedCount, totalCount) =>
        {
            progressInfo.Description = $"{processedCount} of {totalCount} inventories have been exported";
            progressCallback(progressInfo);
        }, cancellationToken);

        await writer.WriteEndObjectAsync();
        await writer.FlushAsync();
    }

    public async Task DoImportAsync(Stream inputStream, Action<ExportImportProgressInfo> progressCallback, ICancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var progressInfo = new ExportImportProgressInfo();

        using var streamReader = new StreamReader(inputStream);
        await using var reader = new JsonTextReader(streamReader);
        while (await reader.ReadAsync())
        {
            if (reader.TokenType == JsonToken.PropertyName && reader.Value != null)
            {
                if (reader.Value.ToString() == "FulfillmentCenters")
                {
                    await reader.DeserializeArrayWithPagingAsync<FulfillmentCenter>(jsonSerializer, BatchSize,
                        fulfillmentCenterService.SaveChangesAsync, processedCount =>
                        {
                            progressInfo.Description = $"{processedCount} FulfillmentCenters have been imported";
                            progressCallback(progressInfo);
                        }, cancellationToken);
                }
                else if (reader.Value.ToString() == "Inventories")
                {
                    await reader.DeserializeArrayWithPagingAsync<InventoryInfo>(jsonSerializer, BatchSize,
                        inventoryService.SaveChangesAsync, processedCount =>
                        {
                            progressInfo.Description = $"{processedCount} Inventories have been imported";
                            progressCallback(progressInfo);
                        }, cancellationToken);
                }
            }
        }
    }
}
