using System.Threading.Tasks;
using GenFu;
using VirtoCommerce.InventoryModule.Core.Model;
using VirtoCommerce.Testing;
using Xunit;

namespace VirtoCommerce.CatalogModule.Test
{
    [Trait("Category", "Unit")]
    public class EntityCloningTests
    {

        [Fact]
        public async Task CloneFullfillmentCenter()
        {
            A.Configure<FulfillmentCenter>()
                .Fill(x => x.Address, x => A.New<Address>());
            var fulfillmentCenter = A.New<FulfillmentCenter>();
            await fulfillmentCenter.AssertCloneIndependency();
        }

        [Fact]
        public async Task CloneInventoryInfo()
        {
            A.Configure<FulfillmentCenter>()
                .Fill(x => x.Address, x => A.New<Address>());
            A.Configure<InventoryInfo>()
                .Fill(x => x.FulfillmentCenter, x => A.New<FulfillmentCenter>());

            var inventoryInfo = A.New<InventoryInfo>();
            await inventoryInfo.AssertCloneIndependency();
        }

    }
}
