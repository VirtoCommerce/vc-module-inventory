using System.Collections.Generic;
using VirtoCommerce.InventoryModule.Core.Model;
using VirtoCommerce.Platform.Core.Events;

namespace VirtoCommerce.InventoryModule.Core.Events
{
    public class InventoryReservationTransactionChangingEvent : GenericChangedEntryEvent<InventoryReservationTransaction>
    {
        public InventoryReservationTransactionChangingEvent(IEnumerable<GenericChangedEntry<InventoryReservationTransaction>> changedEntries)
            : base(changedEntries)
        {
        }
    }
}
