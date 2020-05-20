using System;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.InventoryModule.Core.Events;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Events;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.SearchModule.Core.Model;
using VirtoCommerce.SearchModule.Data.BackgroundJobs;

namespace VirtoCommerce.InventoryModule.Data.Handlers
{
    public class IndexInventoryChangedEventHandler : IEventHandler<InventoryChangedEvent>
    {
        private readonly ISettingsManager _settingsManager;

        public IndexInventoryChangedEventHandler(ISettingsManager settingsManager)
        {
            _settingsManager = settingsManager;
        }

        public Task Handle(InventoryChangedEvent message)
        {
            if (_settingsManager.GetValue(Core.ModuleConstants.Settings.Search.EventBasedIndexationEnable.Name, false))
            {
                if (message == null)
                {
                    throw new ArgumentNullException(nameof(message));
                }

                var indexEntries = message.ChangedEntries
                    .Select(x => new IndexEntry { Id = x.OldEntry.ProductId, EntryState = EntryState.Modified, Type = KnownDocumentTypes.Product })
                    .ToArray();

                IndexingJobs.EnqueueIndexAndDeleteDocuments(indexEntries);
            }

            return Task.CompletedTask;
        }
    }
}
