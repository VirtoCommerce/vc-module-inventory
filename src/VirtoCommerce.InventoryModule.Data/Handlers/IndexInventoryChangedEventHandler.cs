using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.InventoryModule.Core;
using VirtoCommerce.InventoryModule.Core.Events;
using VirtoCommerce.InventoryModule.Data.Search.Indexing;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Events;
using VirtoCommerce.Platform.Core.Jobs;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.SearchModule.Core.BackgroundJobs;
using VirtoCommerce.SearchModule.Core.Extensions;
using VirtoCommerce.SearchModule.Core.Model;

namespace VirtoCommerce.InventoryModule.Data.Handlers
{
    public class IndexInventoryChangedEventHandler : IEventHandler<InventoryChangedEvent>
    {
        private readonly ISettingsManager _settingsManager;
        private readonly IIndexingJobService _indexingJobService;
        private readonly IEnumerable<IndexDocumentConfiguration> _configurations;

        public IndexInventoryChangedEventHandler(
            ISettingsManager settingsManager,
            IIndexingJobService indexingJobService,
            IEnumerable<IndexDocumentConfiguration> configurations)
        {
            _settingsManager = settingsManager;
            _indexingJobService = indexingJobService;
            _configurations = configurations;
        }

        public async Task Handle(InventoryChangedEvent message)
        {
            if (await _settingsManager.GetValueAsync<bool>(ModuleConstants.Settings.Search.EventBasedIndexationEnable))
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

                _indexingJobService.EnqueueIndexAndDeleteDocuments(indexEntries, JobPriority.Normal,
                    _configurations.GetDocumentBuilders(KnownDocumentTypes.Product, typeof(ProductAvailabilityChangesProvider)).ToList());
            }
        }
    }
}
