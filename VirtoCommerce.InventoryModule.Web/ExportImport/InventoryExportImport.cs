using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using VirtoCommerce.Domain.Inventory.Model;
using VirtoCommerce.Domain.Inventory.Model.Search;
using VirtoCommerce.Domain.Inventory.Services;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.ExportImport;
using VirtoCommerce.Platform.Core.Settings;

namespace VirtoCommerce.InventoryModule.Web.ExportImport
{
    public sealed class BackupObject
    {
        public BackupObject()
        {
            InventoryInfos = Array.Empty<InventoryInfo>();
            FulfillmentCenters = Array.Empty<FulfillmentCenter>();
        }
        public InventoryInfo[] InventoryInfos { get; set; }
        public FulfillmentCenter[] FulfillmentCenters { get; set; }
    }

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
                if (_batchSize == null)
                {
                    _batchSize = _settingsManager.GetValue("Inventory.ExportImport.PageSize", 50);
                }

                return (int)_batchSize;
            }
        }

        public InventoryExportImport(
            IInventoryService inventoryService,
            IFulfillmentCenterSearchService fulfillmentCenterSearchService,
            IInventorySearchService inventorySearchService,
            IFulfillmentCenterService fulfillmentCenterService,
            ISettingsManager settingsManager
            )
        {
            _inventoryService = inventoryService;
            _fulfillmentCenterSearchService = fulfillmentCenterSearchService;
            _fulfillmentCenterService = fulfillmentCenterService;
            _inventorySearchService = inventorySearchService;
            _settingsManager = settingsManager;

            _jsonSerializer = new JsonSerializer
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                Formatting = Formatting.Indented,
                NullValueHandling = NullValueHandling.Ignore
            };
        }

        public void DoExport(Stream backupStream, Action<ExportImportProgressInfo> progressCallback)
        {
            var backupObject = GetBackupObject(progressCallback);
            backupObject.SerializeJson(backupStream);
            progressCallback(new ExportImportProgressInfo("The inventory module data has been exported"));
        }

        public void DoImport(Stream backupStream, Action<ExportImportProgressInfo> progressCallback)
        {
            var progressInfo = new ExportImportProgressInfo();

            using (var streamReader = new StreamReader(backupStream))
            using (var reader = new JsonTextReader(streamReader))
            {
                while (reader.Read())
                {
                    if (reader.TokenType == JsonToken.PropertyName)
                    {
                        var readerValue = reader.Value.ToString();

                        if (readerValue == "InventoryInfos")
                        {
                            reader.Read();

                            if (reader.TokenType == JsonToken.StartArray)
                            {
                                reader.Read();

                                var inventoryInfoChunk = new List<InventoryInfo>();

                                while (reader.TokenType != JsonToken.EndArray)
                                {
                                    var inventoryInfo = AbstractTypeFactory<InventoryInfo>.TryCreateInstance();

                                    inventoryInfo = _jsonSerializer.Deserialize(reader, inventoryInfo.GetType()) as InventoryInfo;

                                    inventoryInfoChunk.Add(inventoryInfo);

                                    reader.Read();

                                    if (inventoryInfoChunk.Count >= BatchSize || reader.TokenType == JsonToken.EndArray)
                                    {

                                        progressInfo.ProcessedCount += inventoryInfoChunk.Count;
                                        progressInfo.Description = $"{progressInfo.ProcessedCount} inventories records have been imported";
                                        progressCallback(progressInfo);

                                        _inventoryService.UpsertInventories(inventoryInfoChunk);

                                        inventoryInfoChunk.Clear();
                                    }
                                }
                            }

                        }
                        else if (readerValue == "FulfillmentCenters")
                        {
                            reader.Read();

                            var fulfillmentCentersType = AbstractTypeFactory<FulfillmentCenter>.TryCreateInstance().GetType().MakeArrayType();
                            var fulfillmentCenters = _jsonSerializer.Deserialize(reader, fulfillmentCentersType) as FulfillmentCenter[];

                            progressInfo.Description = $"The {fulfillmentCenters.Count()} fulfillmentCenters has been imported";
                            progressCallback(progressInfo);

                            _fulfillmentCenterService.SaveChanges(fulfillmentCenters);
                        }
                    }
                }
            }
        }

        private BackupObject GetBackupObject(Action<ExportImportProgressInfo> progressCallback)
        {
            progressCallback(new ExportImportProgressInfo("The fulfilmentCenters are loading"));
            var centers = _fulfillmentCenterSearchService.SearchCenters(new FulfillmentCenterSearchCriteria { Take = int.MaxValue }).Results;

            progressCallback(new ExportImportProgressInfo("Evaluation the number of inventory records"));

            var searchResult = _inventorySearchService.SearchInventories(new InventorySearchCriteria { Take = BatchSize });
            var totalCount = searchResult.TotalCount;
            var inventories = searchResult.Results.ToList();
            for (int i = BatchSize; i < totalCount; i += BatchSize)
            {
                progressCallback(new ExportImportProgressInfo($"{i} of {totalCount} inventories have been loaded"));
                searchResult = _inventorySearchService.SearchInventories(new InventorySearchCriteria { Skip = i, Take = BatchSize });
                inventories.AddRange(searchResult.Results);
            }

            return new BackupObject()
            {
                InventoryInfos = inventories.ToArray(),
                FulfillmentCenters = centers.ToArray()
            };
        }
    }
}
