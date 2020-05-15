using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.Primitives;
using VirtoCommerce.InventoryModule.Core.Model;
using VirtoCommerce.Platform.Core.Caching;

namespace VirtoCommerce.InventoryModule.Data.Caching
{
    public class InventoryCacheRegion : CancellableCacheRegion<InventoryCacheRegion>
    {
        private static readonly ConcurrentDictionary<string, CancellationTokenSource> _inventoryRegionTokenLookup = new ConcurrentDictionary<string, CancellationTokenSource>();
        
        public static IChangeToken CreateChangeToken(List<InventoryInfo> inventoryInfos)
        {
            if (inventoryInfos == null)
            {
                throw new ArgumentNullException(nameof(inventoryInfos));
            }
            return CreateChangeToken(inventoryInfos.Select(x => x.Id).ToArray());
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
            if (_inventoryRegionTokenLookup.TryRemove(inventory.Id, out var token))
            {
                token.Cancel();
            }
        }
    }
}
