using System.Collections.Generic;
using System.Linq;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.InventoryModule.Core.Model.Search
{
    public class ProductInventorySearchCriteria : SearchCriteriaBase
    {
        public string ProductId
        {
            get
            {
                if (ProductIds.IsNullOrEmpty())
                {
                    return null;
                }

                return ProductIds.First();
            }
            set
            {
                if (ProductIds == null)
                {
                    ProductIds = new List<string>();
                }

                if (!ProductIds.Contains(value))
                {
                    ProductIds.Insert(0, value);
                }
            }
        }

        public IList<string> ProductIds { get; set; }

        public bool WithInventoryOnly { get; set; }
    }
}
