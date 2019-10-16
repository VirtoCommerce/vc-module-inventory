using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using VirtoCommerce.InventoryModule.Core.Events;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Events;
using VirtoCommerce.SearchModule.Core.Model;
using VirtoCommerce.SearchModule.Core.Services;

namespace VirtoCommerce.InventoryModule.Data.Handlers
{
    public class IndexInventoryChangedEventHandler : IEventHandler<InventoryChangedEvent>
    {
        private readonly IIndexingManager _indexingManager;

        public IndexInventoryChangedEventHandler(IIndexingManager indexingManager)
        {
            _indexingManager = indexingManager;
        }

        public Task Handle(InventoryChangedEvent message)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            var indexProductIds = message.ChangedEntries.Where(x => (x.EntryState == EntryState.Modified || x.EntryState == EntryState.Added)
                                                                    && x.OldEntry.ProductId != null)
                                                          .Select(x => x.OldEntry.ProductId)
                                                          .Distinct().ToArray();

            if (!indexProductIds.IsNullOrEmpty())
            {
                BackgroundJob.Enqueue(() => TryIndexInventoryBackgroundJob(indexProductIds));
            }

            return Task.CompletedTask;
        }


        [DisableConcurrentExecution(60 * 60 * 24)]
        public async Task TryIndexInventoryBackgroundJob(string[] indexProductIds)
        {
            await TryIndexInventory(indexProductIds);
        }


        protected virtual async Task TryIndexInventory(string[] indexProductIds)
        {
            await _indexingManager.IndexDocumentsAsync(KnownDocumentTypes.Product, indexProductIds);
        }
    }
}
