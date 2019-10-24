using System;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using VirtoCommerce.Domain.Inventory.Events;
using VirtoCommerce.Domain.Search;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Events;

namespace VirtoCommerce.InventoryModule.Data.Handlers
{
    public class IndexInventoryChangedEventHandler : IEventHandler<InventoryChangedEvent>
    {
        private readonly IIndexingManager _indexingManager;
        private static readonly EntryState[] _entityStates = new[] { EntryState.Added, EntryState.Modified, EntryState.Deleted };

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

            var indexProductIds = message.ChangedEntries.Where(x => _entityStates.Any(s => s == x.EntryState)
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
        public Task TryIndexInventoryBackgroundJob(string[] indexProductIds)
        {
            return TryIndexInventory(indexProductIds);
        }


        protected virtual Task TryIndexInventory(string[] indexProductIds)
        {
            return _indexingManager.IndexDocumentsAsync(KnownDocumentTypes.Product, indexProductIds);
        }
    }
}
