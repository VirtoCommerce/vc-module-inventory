using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Primitives;
using VirtoCommerce.InventoryModule.Core.Model;
using VirtoCommerce.Platform.Core.Caching;

namespace VirtoCommerce.InventoryModule.Data.Caching;

public class InventoryCacheRegion : CancellableCacheRegion<InventoryCacheRegion>
{
    public static IChangeToken CreateChangeToken(List<InventoryInfo> inventoryInfos)
    {
        if (inventoryInfos == null)
        {
            throw new ArgumentNullException(nameof(inventoryInfos));
        }

        // Generate the cancellation tokens for inventory.productId as well to be able to evict from the cache all inventories that have this product id
        var keys = inventoryInfos
            .Select(x => x.Id)
            .Concat(inventoryInfos.Select(x => x.ProductId))
            .Where(x => !string.IsNullOrEmpty(x))
            .Distinct()
            .ToArray();

        return CreateChangeToken(keys);
    }

    public static IChangeToken CreateChangeToken(ICollection<string> ids)
    {
        if (ids == null)
        {
            throw new ArgumentNullException(nameof(ids));
        }

        var changeTokens = new List<IChangeToken> { CreateChangeToken() };

        foreach (var id in ids)
        {
            changeTokens.Add(CreateChangeTokenForKey(id));
        }

        return new CompositeChangeToken(changeTokens);
    }

    public static void ExpireInventory(InventoryInfo inventory)
    {
        if (inventory != null)
        {
            ExpireTokenForKey(inventory.Id);
            ExpireTokenForKey(inventory.ProductId);
        }
    }
}
