using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Extensions.Primitives;
using VirtoCommerce.InventoryModule.Core.Model;
using VirtoCommerce.Platform.Core.Caching;

namespace VirtoCommerce.InventoryModule.Data.Caching
{
    public class InventoryCacheRegion : CancellableCacheRegion<InventoryCacheRegion>
    {
        private static readonly ConcurrentDictionary<string, CancellationTokenSource> _inventoryRegionTokenLookup = new ConcurrentDictionary<string, CancellationTokenSource>();


        public static IChangeToken CreateChangeToken(InventoryInfo inventory)
        {
            if (inventory == null)
            {
                throw new ArgumentNullException(nameof(inventory));
            }

            return CreateChangeToken(new[] { inventory.Id });
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
                changeTokens.Add(new CancellationChangeToken(_inventoryRegionTokenLookup.GetOrAdd(inventoryId, new CancellationTokenSource()).Token));
            }
            return new CompositeChangeToken(changeTokens);
        }

        public static void ExpireInventory(InventoryInfo inventory)
        {
            if (_inventoryRegionTokenLookup.TryRemove(inventory.ProductId, out var token))
            {
                token.Cancel();
            }
        }
    }
}
