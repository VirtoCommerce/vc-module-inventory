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

namespace VirtoCommerce.InventoryModule.Data.ExportImport
{
    public sealed class InventoryExportImport
    {
        private readonly IInventoryService _inventoryService;
        private readonly IInventorySearchService _inventorySearchService;
        private readonly IFulfillmentCenterSearchService _fulfillmentCenterSearchService;
        private readonly IFulfillmentCenterService _fulfillmentCenterService;
        private readonly ISettingsManager _settingsManager;
        private readonly JsonSerializer _jsonSerializer;

        private int? _batchSize;

        private int BatchSize
        {
            get
            {
                _batchSize ??= _settingsManager.GetValue<int>(ModuleConstants.Settings.General.PageSize);

                return (int)_batchSize;
            }
        }

        public InventoryExportImport(IInventoryService inventoryService,
            IInventorySearchService inventorySearchService,
            IFulfillmentCenterSearchService fulfillmentCenterSearchService,
            IFulfillmentCenterService fulfillmentCenterService,
            ISettingsManager settingsManager,
            JsonSerializer jsonSerializer)
        {
            _inventoryService = inventoryService;
            _inventorySearchService = inventorySearchService;
            _fulfillmentCenterSearchService = fulfillmentCenterSearchService;
            _fulfillmentCenterService = fulfillmentCenterService;
            _settingsManager = settingsManager;
            _jsonSerializer = jsonSerializer;
        }

        public async Task DoExportAsync(Stream outStream, Action<ExportImportProgressInfo> progressCallback, ICancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var progressInfo = new ExportImportProgressInfo { Description = "The fulfilmentCenters are loading" };
            progressCallback(progressInfo);

            using (var sw = new StreamWriter(outStream, Encoding.UTF8))
            using (var writer = new JsonTextWriter(sw))
            {
                await writer.WriteStartObjectAsync();

                await writer.WritePropertyNameAsync("FulfillmentCenters");
                await writer.SerializeArrayWithPagingAsync(_jsonSerializer, BatchSize, async (skip, take) =>
                {
                    var searchCriteria = AbstractTypeFactory<FulfillmentCenterSearchCriteria>.TryCreateInstance();
                    searchCriteria.Take = take;
                    searchCriteria.Skip = skip;
                    var searchResult = await _fulfillmentCenterSearchService.SearchNoCloneAsync(searchCriteria);
                    return (GenericSearchResult<FulfillmentCenter>)searchResult;
                }, (processedCount, totalCount) =>
                {
                    progressInfo.Description = $"{processedCount} of {totalCount} FulfillmentCenters have been exported";
                    progressCallback(progressInfo);
                }, cancellationToken);

                progressInfo.Description = "The Inventories are loading";
                progressCallback(progressInfo);

                await writer.WritePropertyNameAsync("Inventories");
                await writer.SerializeArrayWithPagingAsync(_jsonSerializer, BatchSize, async (skip, take) =>
                {
                    var searchCriteria = AbstractTypeFactory<InventorySearchCriteria>.TryCreateInstance();
                    searchCriteria.Take = take;
                    searchCriteria.Skip = skip;
                    var searchResult = await _inventorySearchService.SearchInventoriesAsync(searchCriteria);
                    return (GenericSearchResult<InventoryInfo>)searchResult;
                }, (processedCount, totalCount) =>
                {
                    progressInfo.Description = $"{processedCount} of {totalCount} inventories have been exported";
                    progressCallback(progressInfo);
                }, cancellationToken);

                await writer.WriteEndObjectAsync();
                await writer.FlushAsync();
            }
        }

        public async Task DoImportAsync(Stream inputStream, Action<ExportImportProgressInfo> progressCallback, ICancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var progressInfo = new ExportImportProgressInfo();

            using (var streamReader = new StreamReader(inputStream))
            using (var reader = new JsonTextReader(streamReader))
            {
                while (await reader.ReadAsync())
                {
                    if (reader.TokenType == JsonToken.PropertyName)
                    {
                        if (reader.Value.ToString() == "FulfillmentCenters")
                        {
                            await reader.DeserializeArrayWithPagingAsync<FulfillmentCenter>(_jsonSerializer, BatchSize,
                                items => _fulfillmentCenterService.SaveChangesAsync(items), processedCount =>
                                {
                                    progressInfo.Description = $"{processedCount} FulfillmentCenters have been imported";
                                    progressCallback(progressInfo);
                                }, cancellationToken);
                        }
                        else if (reader.Value.ToString() == "Inventories")
                        {
                            await reader.DeserializeArrayWithPagingAsync<InventoryInfo>(_jsonSerializer, BatchSize,
                                items => _inventoryService.SaveChangesAsync(items), processedCount =>
                                {
                                    progressInfo.Description = $"{processedCount} Inventories have been imported";
                                    progressCallback(progressInfo);
                                }, cancellationToken);
                        }
                    }
                }
            }
        }
    }
}
