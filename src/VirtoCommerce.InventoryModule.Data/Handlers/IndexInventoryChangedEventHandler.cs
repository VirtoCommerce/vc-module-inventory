using System;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.InventoryModule.Core.Events;
using VirtoCommerce.Platform.Core.Events;
using VirtoCommerce.SearchModule.Core.Model;
using VirtoCommerce.SearchModule.Data.BackgroundJobs;

namespace VirtoCommerce.InventoryModule.Data.Handlers
{
    public class IndexInventoryChangedEventHandler : IEventHandler<InventoryChangedEvent>
    {
        public Task Handle(InventoryChangedEvent message)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            var indexEntries = message.ChangedEntries
                .Select(x => new IndexEntry { Id = x.OldEntry.ProductId, EntryState = x.EntryState, Type = KnownDocumentTypes.Product })
                .ToArray();

            IndexingJobs.EnqueueIndexAndDeleteDocuments(indexEntries);

            return Task.CompletedTask;
        }
    }
}
