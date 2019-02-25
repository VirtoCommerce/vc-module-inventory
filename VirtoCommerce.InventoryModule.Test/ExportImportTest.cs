using System.Collections.Generic;
using System.IO;
using Moq;
using VirtoCommerce.Domain.Inventory.Model;
using VirtoCommerce.Domain.Inventory.Services;
using VirtoCommerce.InventoryModule.Web.ExportImport;
using VirtoCommerce.Platform.Core.ExportImport;
using VirtoCommerce.Platform.Core.Settings;
using Xunit;

namespace VirtoCommerce.InventoryModule.Test
{
    public class ExportImportTest
    {

        [Fact]
        public void TestArbitraryImport()
        {
            var inventoryService = GetInventoryService();

            var fulfillmentCenterService = GetFulfillmentCenterService();

            var settingsManager = GetSettingsManager();
            settingsManager
                .Setup(s => s.GetValue(It.IsAny<string>(), It.IsAny<int>()))
                .Returns(2);

            var exportImportProcessor = GetExportImportProcessor(inventoryService.Object, fulfillmentCenterService.Object, settingsManager.Object);

            exportImportProcessor.DoImport(GetStreamFromString(GetSampleData()), GetProgressInfoCallback);

            inventoryService.Verify(i => i.UpsertInventories(It.IsAny<IEnumerable<InventoryInfo>>()), Times.Exactly(2));
            fulfillmentCenterService.Verify(f => f.SaveChanges(It.IsAny<IEnumerable<FulfillmentCenter>>()), Times.Exactly(1));
        }

        private Mock<IInventoryService> GetInventoryService()
        {
            return new Mock<IInventoryService>();
        }

        private IFulfillmentCenterSearchService GetFulfillmentcenterSearchService()
        {
            return new Mock<IFulfillmentCenterSearchService>().Object;
        }

        private IInventorySearchService GetInventorySearchService()
        {
            return new Mock<IInventorySearchService>().Object;
        }

        private Mock<IFulfillmentCenterService> GetFulfillmentCenterService()
        {
            return new Mock<IFulfillmentCenterService>();
        }

        private Mock<ISettingsManager> GetSettingsManager()
        {
            return new Mock<ISettingsManager>();
        }

        private void GetProgressInfoCallback(ExportImportProgressInfo exportImportProgressInfo)
        {
        }

        private InventoryExportImport GetExportImportProcessor(IInventoryService inventoryService, IFulfillmentCenterService fulfillmentCenterService, ISettingsManager settingsManager)
        {
            return new InventoryExportImport(inventoryService, GetFulfillmentcenterSearchService(), GetInventorySearchService(), fulfillmentCenterService, settingsManager);
        }

        private Stream GetStreamFromString(string value)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(value);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }

        private string GetSampleData()
        {
            var data = @"{
            ""FulfillmentCenters"": [
            {
            ""Name"": ""Vendor Fulfillment Center"",
            ""Address"": {
                ""AddressType"": 0,
                ""CountryCode"": ""USA"",
                ""CountryName"": ""United States"",
                ""City"": ""Los Angeles"",
                ""PostalCode"": ""90234"",
                ""Line1"": ""1232 Wilshire Blvd"",
                ""RegionName"": ""California"",
                ""Phone"": ""3232323232""
            },
            ""CreatedDate"": ""2018-05-03T14:49:23.307"",
            ""ModifiedDate"": ""2018-05-03T14:49:23.307"",
            ""CreatedBy"": ""unknown"",
            ""ModifiedBy"": ""unknown"",
            ""Id"": ""vendor-fulfillment""
            }], ""InventoryInfos"": [
            {
              ""CreatedDate"": ""2015-10-06T22:39:44.533"",
              ""CreatedBy"": ""unknown"",
              ""ModifiedDate"": ""2015-10-06T22:39:44.533"",
              ""ModifiedBy"": ""unknown"",
              ""FulfillmentCenterId"": ""vendor-fulfillment"",
              ""ProductId"": ""7384ecc9cba84f2eb755c5136736bb9f"",
              ""InStockQuantity"": 6,
              ""ReservedQuantity"": 0,
              ""ReorderMinQuantity"": 0,
              ""PreorderQuantity"": 0,
              ""BackorderQuantity"": 0,
              ""AllowBackorder"": false,
              ""AllowPreorder"": false,
              ""InTransit"": 0,
              ""Status"": 0
            },
            {
              ""CreatedDate"": ""2015-10-06T22:39:44.533"",
              ""CreatedBy"": ""unknown"",
              ""ModifiedDate"": ""2015-10-06T22:39:44.533"",
              ""ModifiedBy"": ""unknown"",
              ""FulfillmentCenterId"": ""vendor-fulfillment"",
              ""ProductId"": ""1f8b0048ce2d4377813b2e8060afed59"",
              ""InStockQuantity"": 44,
              ""ReservedQuantity"": 0,
              ""ReorderMinQuantity"": 0,
              ""PreorderQuantity"": 0,
              ""BackorderQuantity"": 0,
              ""AllowBackorder"": false,
              ""AllowPreorder"": false,
              ""InTransit"": 0,
              ""Status"": 0
            },
            {
              ""CreatedDate"": ""2015-10-06T22:39:44.533"",
              ""CreatedBy"": ""unknown"",
              ""ModifiedDate"": ""2015-10-06T22:39:44.533"",
              ""ModifiedBy"": ""unknown"",
              ""FulfillmentCenterId"": ""vendor-fulfillment"",
              ""ProductId"": ""7c835a9b1c8e4445aa118dae659231c3"",
              ""InStockQuantity"": 6,
              ""ReservedQuantity"": 0,
              ""ReorderMinQuantity"": 0,
              ""PreorderQuantity"": 0,
              ""BackorderQuantity"": 0,
              ""AllowBackorder"": false,
              ""AllowPreorder"": false,
              ""InTransit"": 0,
              ""Status"": 0
            }
            ]}";

            return data;
        }
    }
}
