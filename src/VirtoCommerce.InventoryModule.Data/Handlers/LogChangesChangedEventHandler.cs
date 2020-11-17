using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using VirtoCommerce.InventoryModule.Core;
using VirtoCommerce.InventoryModule.Core.Events;
using VirtoCommerce.Platform.Core.ChangeLog;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Events;
using VirtoCommerce.Platform.Core.Settings;

namespace VirtoCommerce.InventoryModule.Data.Handlers
{
    public class LogChangesChangedEventHandler : IEventHandler<InventoryChangedEvent>
    {
        private readonly IChangeLogService _changeLogService;
        private readonly ILastModifiedDateTime _lastModifiedDateTime;
        private readonly ISettingsManager _settingsManager;

        public LogChangesChangedEventHandler(IChangeLogService changeLogService, ILastModifiedDateTime lastModifiedDateTime, ISettingsManager settingsManager)
        {
            _changeLogService = changeLogService;
            _lastModifiedDateTime = lastModifiedDateTime;
            _settingsManager = settingsManager;
        }

        public virtual async Task Handle(InventoryChangedEvent message)
        {
            await InnerHandle(message);
        }

        protected virtual async Task InnerHandle<T>(GenericChangedEntryEvent<T> @event) where T : IEntity
        {
            var logInventoryChangesEnabled = await _settingsManager.GetValueAsync(ModuleConstants.Settings.General.LogInventoryChanges.Name, (bool)ModuleConstants.Settings.General.LogInventoryChanges.DefaultValue);

            if (logInventoryChangesEnabled)
            {
                var logOperations = @event.ChangedEntries.Select(x => AbstractTypeFactory<OperationLog>.TryCreateInstance().FromChangedEntry(x)).ToArray();
                //Background task is used here for performance reasons
                BackgroundJob.Enqueue(() => LogEntityChangesInBackground(logOperations));
            }
            else
            {
                // Force reset the date of last data modifications, so that it would be reset even if the Inventory.LogInventoryChanges setting is inactive.
                _lastModifiedDateTime.Reset();
            }
        }

        public async Task LogEntityChangesInBackground(OperationLog[] operationLogs)
        {
            await _changeLogService.SaveChangesAsync(operationLogs);
        }
    }
}
