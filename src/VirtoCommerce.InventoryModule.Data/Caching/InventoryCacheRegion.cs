using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Primitives;
using VirtoCommerce.InventoryModule.Core.Model;
using VirtoCommerce.Platform.Core.Caching;

namespace VirtoCommerce.InventoryModule.Data.Caching
{
    public class InventoryCacheRegion : CancellableCacheRegion<InventoryCacheRegion>
    {

        public static IChangeToken CreateChangeToken(List<InventoryInfo> inventoryInfos)
        {
            if (inventoryInfos == null)
            {
                throw new ArgumentNullException(nameof(inventoryInfos));
            }
            //generate the cancellation tokens for inventory.productId as well to be able evict from the cache the all inventories that are reference to the product id
            var keys = inventoryInfos.Select(x => x.Id).Concat(inventoryInfos.Select(x => x.ProductId))
                                     .Where(x => !string.IsNullOrEmpty(x))
                                     .Distinct()
                                     .ToArray();
            return CreateChangeToken(keys);
        }

        public static IChangeToken CreateChangeToken(string[] inventoryIds)
        {
            if (inventoryIds == null)
            {
                throw new ArgumentNullException(nameof(inventoryIds));
            }
            var changeTokens = new List<IChangeToken>() { CreateChangeToken() };
            foreach (var inventoryId in inventoryIds)
            {
                changeTokens.Add(CreateChangeTokenForKey(inventoryId));
            }
            return new CompositeChangeToken(changeTokens);
        }

        public static void ExpireInventory(InventoryInfo inventory)
        {
            if (inventory != null)
            {
                ExpireTokenForKey(inventory.IsTransient() ? inventory.ProductId : inventory.Id);
            }
        }
    }
}
