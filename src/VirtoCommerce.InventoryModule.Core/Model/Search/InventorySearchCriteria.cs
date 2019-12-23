using System.Collections.Generic;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.InventoryModule.Core.Model.Search
{
    public class InventorySearchCriteria : SearchCriteriaBase
    {
        //todo
        //public GeoPoint Location { get; set; }
        public IList<string> FulfillmentCenterIds { get; set; }
        public IList<string> ProductIds { get; set; }
    }
}
