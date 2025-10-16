using System.Collections.Generic;
using System.Linq;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.InventoryModule.Core.Model.Search;

public class ProductInventorySearchCriteria : SearchCriteriaBase
{
    public string ProductId
    {
        get
        {
            return ProductIds?.FirstOrDefault();
        }
        set
        {
            ProductIds = !value.IsNullOrEmpty() ? [value] : null;
        }
    }

    public IList<string> ProductIds { get; set; }

    public bool WithInventoryOnly { get; set; }
}
