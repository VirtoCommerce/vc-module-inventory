using System;
using System.IO;
using System.Linq;
using VirtoCommerce.Domain.Inventory.Model;
using VirtoCommerce.Domain.Inventory.Model.Search;
using VirtoCommerce.Domain.Inventory.Services;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.ExportImport;

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

        public InventoryExportImport(
            IInventoryService inventoryService,
            IFulfillmentCenterSearchService fulfillmentCenterSearchService,
            IInventorySearchService inventorySearchService,
            IFulfillmentCenterService fulfillmentCenterService
            )
        {
            _inventoryService = inventoryService;
            _fulfillmentCenterSearchService = fulfillmentCenterSearchService;
            _fulfillmentCenterService = fulfillmentCenterService;
            _inventorySearchService = inventorySearchService;
        }

        public void DoExport(Stream backupStream, Action<ExportImportProgressInfo> progressCallback)
        {
            var backupObject = GetBackupObject(progressCallback);
            backupObject.SerializeJson(backupStream);
            progressCallback(new ExportImportProgressInfo("The inventory module data has been exported"));
        }

        public void DoImport(Stream backupStream, Action<ExportImportProgressInfo> progressCallback)
        {
            var backupObject = backupStream.DeserializeJson<BackupObject>();

            progressCallback(new ExportImportProgressInfo($"The {backupObject.FulfillmentCenters.Count()} fulfilmentCenters are importing"));
            _fulfillmentCenterService.SaveChanges(backupObject.FulfillmentCenters);
            progressCallback(new ExportImportProgressInfo($"The {backupObject.FulfillmentCenters.Count()} fulfilmentCenters has been imported"));

            const int batchSize = 50;
            var totalCount = backupObject.InventoryInfos.Count();
            progressCallback(new ExportImportProgressInfo($"The {totalCount} inventories are importing"));
            for (int i = 0; i < totalCount; i += batchSize)
            {               
                _inventoryService.UpsertInventories(backupObject.InventoryInfos.Skip(i).Take(batchSize));
                progressCallback(new ExportImportProgressInfo($"{i} of {totalCount} inventories records have been imported"));
            }
            progressCallback(new ExportImportProgressInfo("The inventory module data has been imported"));
        }

        private BackupObject GetBackupObject(Action<ExportImportProgressInfo> progressCallback)
        {
            progressCallback(new ExportImportProgressInfo("The fulfilmentCenters are loading"));
            var centers = _fulfillmentCenterSearchService.SearchCenters(new FulfillmentCenterSearchCriteria { Take = int.MaxValue }).Results;
            
            progressCallback(new ExportImportProgressInfo("Evaluation the number of inventory records"));

            const int batchSize = 50;
            var searchResult = _inventorySearchService.SearchInventories(new InventorySearchCriteria { Take = batchSize });
            var totalCount = searchResult.TotalCount;
            var inventories = searchResult.Results.ToList();
            for (int i = batchSize; i < totalCount; i += batchSize)
            {
                progressCallback(new ExportImportProgressInfo($"{i} of {totalCount} inventories have been loaded"));
                searchResult = _inventorySearchService.SearchInventories(new InventorySearchCriteria { Skip = i,  Take = batchSize });
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
