using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.InventoryModule.Core.Events;
using VirtoCommerce.InventoryModule.Data.Search.Indexing;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Events;
using VirtoCommerce.Platform.Core.Jobs;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.SearchModule.Core.Model;
using VirtoCommerce.SearchModule.Data.BackgroundJobs;
using VirtoCommerce.SearchModule.Data.Services;

namespace VirtoCommerce.InventoryModule.Data.Handlers
{
    public class IndexInventoryChangedEventHandler : IEventHandler<InventoryChangedEvent>
    {
        private readonly ISettingsManager _settingsManager;
        private readonly IEnumerable<IndexDocumentConfiguration> _configurations;

        public IndexInventoryChangedEventHandler(ISettingsManager settingsManager,
            IEnumerable<IndexDocumentConfiguration> configurations)
        {
            _settingsManager = settingsManager;
            _configurations = configurations;
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
                    .Select(x => new IndexEntry
                    {
                        Id = x.OldEntry.ProductId,
                        EntryState = EntryState.Modified,
                        Type = KnownDocumentTypes.Product
                    })
                    .ToArray();

                IndexingJobs.EnqueueIndexAndDeleteDocuments(indexEntries, JobPriority.Normal,
                    _configurations.GetBuildersForProvider(typeof(ProductAvailabilityChangesProvider)).ToList());
            }

            return Task.CompletedTask;
        }
    }
}
