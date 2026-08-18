using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using VirtoCommerce.InventoryModule.Core.Events;
using VirtoCommerce.InventoryModule.Core.Model;
using VirtoCommerce.InventoryModule.Core.Services;
using VirtoCommerce.InventoryModule.Data.Handlers;
using VirtoCommerce.InventoryModule.Data.Jobs;
using VirtoCommerce.Platform.Core.ChangeLog;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Events;
using VirtoCommerce.Platform.Core.Jobs;
using VirtoCommerce.Platform.Core.Settings;
using Xunit;

namespace VirtoCommerce.InventoryModule.Tests
{
    // Any other test class that enqueues through the static BackgroundJob facade must join this collection:
    // the facade has no reset API (Initialize rejects null), so Dispose leaves a DISPOSED provider behind in
    // the static, and a class racing this one would see ObjectDisposedException from it.
    [Collection(nameof(BackgroundJobEnqueueTests))]
    [Trait("Category", "Unit")]
    public class BackgroundJobEnqueueTests
    {
        [Fact]
        public async Task LogChanges_LoggingEnabled_EnqueuesOneJobWithOneLogPerChangedEntry()
        {
            //Arrange
            using var capture = new EnqueueCapture();
            var lastModifiedDateTime = new Mock<ILastModifiedDateTime>();
            var handler = new LogChangesChangedEventHandler(Mock.Of<IChangeLogService>(), lastModifiedDateTime.Object, CreateSettingsManager(logInventoryChanges: true));

            var message = new InventoryChangedEvent(
            [
                new GenericChangedEntry<InventoryInfo>(new InventoryInfo { Id = "inv1" }, new InventoryInfo { Id = "inv1" }, EntryState.Modified),
                new GenericChangedEntry<InventoryInfo>(new InventoryInfo { Id = "inv2" }, new InventoryInfo { Id = "inv2" }, EntryState.Added),
            ]);

            //Act
            await handler.Handle(message);

            //Assert
            var payload = Assert.IsType<LogEntityChangesJobPayload>(capture.Payload);
            Assert.Equal(1, capture.EnqueueCount);
            Assert.Equal(["inv1", "inv2"], payload.OperationLogs.Select(x => x.ObjectId));
            Assert.Equal([EntryState.Modified, EntryState.Added], payload.OperationLogs.Select(x => x.OperationType));

            // The job's SaveChangesAsync ends in Reset(), so the handler must not reset the date itself as well.
            lastModifiedDateTime.Verify(x => x.Reset(), Times.Never);
        }

        [Fact]
        public async Task LogChanges_LoggingDisabled_ResetsLastModifiedInsteadOfEnqueuing()
        {
            //Arrange
            using var capture = new EnqueueCapture();
            var lastModifiedDateTime = new Mock<ILastModifiedDateTime>();
            var handler = new LogChangesChangedEventHandler(Mock.Of<IChangeLogService>(), lastModifiedDateTime.Object, CreateSettingsManager(logInventoryChanges: false));

            var inventory = new InventoryInfo { Id = "inv1" };

            //Act
            await handler.Handle(new InventoryChangedEvent([new GenericChangedEntry<InventoryInfo>(inventory, inventory, EntryState.Modified)]));

            //Assert
            Assert.Equal(0, capture.EnqueueCount);
            lastModifiedDateTime.Verify(x => x.Reset(), Times.Once);
        }

        [Fact]
        public async Task LogEntityChangesJobHandler_SavesThePayloadLogs()
        {
            //Arrange
            var operationLogs = new[] { AbstractTypeFactory<OperationLog>.TryCreateInstance() };
            var changeLogServiceMock = new Mock<IChangeLogService>();

            var handler = new LogEntityChangesJobHandler(changeLogServiceMock.Object);

            //Act
            await handler.Execute(new LogEntityChangesJobPayload { OperationLogs = operationLogs }, context: null,
                TestContext.Current.CancellationToken);

            //Assert
            changeLogServiceMock.Verify(x => x.SaveChangesAsync(operationLogs), Times.Once);
        }

        [Fact]
        public async Task FulfillmentCenterChanged_GeoLocationChanged_EnqueuesRecalculation()
        {
            //Arrange
            using var capture = new EnqueueCapture();
            var handler = new FulfillmentCenterChangedEventHandler(Mock.Of<IFulfillmentCenterGeoService>());

            var message = new FulfillmentCenterChangedEvent(
            [
                new GenericChangedEntry<FulfillmentCenter>(
                    new FulfillmentCenter { Id = "ffc1", GeoLocation = "1,1" },
                    new FulfillmentCenter { Id = "ffc1", GeoLocation = "2,2" },
                    EntryState.Modified),
            ]);

            //Act
            await handler.Handle(message);

            //Assert
            Assert.Equal(1, capture.EnqueueCount);
            Assert.IsType<RecalculateFulfillmentCenterDistanceJobPayload>(capture.Payload);
        }

        [Fact]
        public async Task FulfillmentCenterChanged_GeoLocationUnchanged_EnqueuesNothing()
        {
            //Arrange
            using var capture = new EnqueueCapture();
            var handler = new FulfillmentCenterChangedEventHandler(Mock.Of<IFulfillmentCenterGeoService>());

            var message = new FulfillmentCenterChangedEvent(
            [
                new GenericChangedEntry<FulfillmentCenter>(
                    new FulfillmentCenter { Id = "ffc1", GeoLocation = "1,1", Name = "new name" },
                    new FulfillmentCenter { Id = "ffc1", GeoLocation = "1,1", Name = "old name" },
                    EntryState.Modified),
            ]);

            //Act
            await handler.Handle(message);

            //Assert
            Assert.Equal(0, capture.EnqueueCount);
        }

        [Fact]
        public async Task RecalculateFulfillmentCenterDistanceJobHandler_WarmsTheGeoCache()
        {
            //Arrange
            var geoServiceMock = new Mock<IFulfillmentCenterGeoService>();
            var handler = new RecalculateFulfillmentCenterDistanceJobHandler(geoServiceMock.Object);

            //Act
            await handler.Execute(new RecalculateFulfillmentCenterDistanceJobPayload(), context: null,
                TestContext.Current.CancellationToken);

            //Assert
            geoServiceMock.Verify(x => x.GetNearestAsync(string.Empty, 0), Times.Once);
        }

        private static ISettingsManager CreateSettingsManager(bool logInventoryChanges)
        {
            var settingsManager = new Mock<ISettingsManager>();
            settingsManager
                .Setup(x => x.GetObjectSettingAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new ObjectSettingEntry { Value = logInventoryChanges });

            return settingsManager.Object;
        }

        // Captures what a handler enqueued through the static BackgroundJob facade. IBackgroundJob is registered
        // Scoped here exactly as the engine module registers it, so this also proves the facade's per-call scope
        // resolves it - the handlers themselves are root-resolved and must never hold it.
        private sealed class EnqueueCapture : IDisposable
        {
            private readonly ServiceProvider _provider;

            public EnqueueCapture()
            {
                BackgroundJobMock
                    .Setup(x => x.Enqueue<LogEntityChangesJobHandler>(It.IsAny<object>(), It.IsAny<EnqueueOptions>(), It.IsAny<CancellationToken>()))
                    .Callback<object, EnqueueOptions, CancellationToken>((payload, _, _) => Capture(payload))
                    .ReturnsAsync("job-id");

                BackgroundJobMock
                    .Setup(x => x.Enqueue<RecalculateFulfillmentCenterDistanceJobHandler>(It.IsAny<object>(), It.IsAny<EnqueueOptions>(), It.IsAny<CancellationToken>()))
                    .Callback<object, EnqueueOptions, CancellationToken>((payload, _, _) => Capture(payload))
                    .ReturnsAsync("job-id");

                var services = new ServiceCollection();
                services.AddScoped(_ => BackgroundJobMock.Object);
                _provider = services.BuildServiceProvider(validateScopes: true);

                BackgroundJob.Initialize(_provider);
            }

            public Mock<IBackgroundJob> BackgroundJobMock { get; } = new();

            public object Payload { get; private set; }

            public int EnqueueCount { get; private set; }

            public void Dispose()
            {
                _provider.Dispose();
            }

            private void Capture(object payload)
            {
                Payload = payload;
                EnqueueCount++;
            }
        }
    }
}
