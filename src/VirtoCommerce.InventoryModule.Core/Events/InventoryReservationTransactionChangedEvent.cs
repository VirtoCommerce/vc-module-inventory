using System.Collections.Generic;
using VirtoCommerce.InventoryModule.Core.Model;
using VirtoCommerce.Platform.Core.Events;

namespace VirtoCommerce.InventoryModule.Core.Events
{
    public class InventoryReservationTransactionChangedEvent : GenericChangedEntryEvent<InventoryReservationTransaction>
    {
        public InventoryReservationTransactionChangedEvent(IEnumerable<GenericChangedEntry<InventoryReservationTransaction>> changedEntries)
            : base(changedEntries)
        {
        }
    }
}
