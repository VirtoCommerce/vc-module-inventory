using System.Linq;
using System.Web.Http;
using System.Web.Http.Description;
using VirtoCommerce.Domain.Commerce.Model.Search;
using VirtoCommerce.Domain.Inventory.Model;
using VirtoCommerce.Domain.Inventory.Model.Search;
using VirtoCommerce.Domain.Inventory.Services;
using VirtoCommerce.InventoryModule.Web.Security;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Web.Security;

namespace VirtoCommerce.InventoryModule.Web.Controllers.Api
{
    [RoutePrefix("api/inventory")]
    public class InventoryModuleController : ApiController
    {
        private readonly IInventoryService _inventoryService;
        private readonly IInventorySearchService _inventorySearchService;
        private readonly IFulfillmentCenterSearchService _fulfillmentCenterSearchService;
        private readonly IFulfillmentCenterService _fulfillmentCenterService;

        public InventoryModuleController(IInventoryService inventoryService,
            IInventorySearchService inventorySearchService,
            IFulfillmentCenterSearchService fulfillmentCenterSearchService,
            IFulfillmentCenterService fulfillmentCenterService)
        {
            _inventoryService = inventoryService;
            _inventorySearchService = inventorySearchService;
            _fulfillmentCenterSearchService = fulfillmentCenterSearchService;
            _fulfillmentCenterService = fulfillmentCenterService;
        }

        /// <summary>
        /// Search inventories by given criteria
        /// </summary>
        [HttpPost]
        [Route("inventories/search")]
        [ResponseType(typeof(GenericSearchResult<InventoryInfo>))]
        public IHttpActionResult SearchInventories([FromBody] InventorySearchCriteria searchCriteria)
        {
            var result = _inventorySearchService.SearchInventories(searchCriteria);
            return Ok(result);
        }

        /// <summary>
        /// Search inventories by given criteria
        /// </summary>
        [HttpPost]
        [Route("fulfillmentcenterinventories/search")]
        [ResponseType(typeof(GenericSearchResult<InventoryInfo>))]
        public IHttpActionResult SearchFulfillmentCenterInventory([FromBody] InventorySearchCriteria searchCriteria)
        {
            var result = _inventorySearchService.SearchInventories(searchCriteria);
            return Ok(result);
        }

        /// <summary>
        /// Search fulfillment centers registered in the system
        /// </summary>
        [HttpPost]
        [Route("fulfillmentcenters/search")]
        [ResponseType(typeof(GenericSearchResult<FulfillmentCenter>))]
        public IHttpActionResult SearchFulfillmentCenters([FromBody] FulfillmentCenterSearchCriteria searchCriteria)
        {
            var result = _fulfillmentCenterSearchService.SearchCenters(searchCriteria);
            return Ok(result);
        }

        /// <summary>
        /// Get fulfillment center by id
        /// </summary>
        /// <param name="id">fulfillment center id</param>
        [HttpGet]
        [Route("fulfillmentcenters/{id}")]
        [ResponseType(typeof(FulfillmentCenter))]
        public IHttpActionResult GetFulfillmentCenter(string id)
        {
            var retVal = _fulfillmentCenterService.GetByIds(new[] { id }).FirstOrDefault();
            return Ok(retVal);
        }

        /// <summary>
        /// Get fulfillment centers by ids
        /// </summary>
        /// <param name="ids">fulfillment center ids</param>
        [HttpPost]
        [ResponseType(typeof(FulfillmentCenter[]))]
        [Route("fulfillmentcenters/plenty")]
        public IHttpActionResult GetFulfillmentCenters([FromBody] string[] ids)
        {
            var retVal = _fulfillmentCenterService.GetByIds(ids);
            return Ok(retVal);
        }

        /// <summary>
        ///  Save fulfillment center 
        /// </summary>
        /// <param name="center">fulfillment center</param>
        [HttpPut]
        [Route("fulfillmentcenters")]
        [ResponseType(typeof(FulfillmentCenter))]
        [CheckPermission(Permission = InventoryPredefinedPermissions.FulfillmentEdit)]
        public IHttpActionResult SaveFulfillmentCenter(FulfillmentCenter center)
        {
            _fulfillmentCenterService.SaveChanges(new[] { center });
            return Ok(center);
        }


        /// <summary>
        ///  Save fulfillment centers 
        /// </summary>
        /// <param name="centers">fulfillment centers</param>
        [HttpPost]
        [ResponseType(typeof(FulfillmentCenter[]))]
        [Route("fulfillmentcenters/batch")]
        [CheckPermission(Permission = InventoryPredefinedPermissions.FulfillmentEdit)]
        public IHttpActionResult SaveFulfillmentCenters([FromBody] FulfillmentCenter[] centers)
        {
            _fulfillmentCenterService.SaveChanges(centers);
            return Ok(centers);
        }

        /// <summary>
        /// Delete fulfillment centers registered in the system
        /// </summary>
        [HttpDelete]
        [Route("fulfillmentcenters")]
        [ResponseType(typeof(void))]
        [CheckPermission(Permission = InventoryPredefinedPermissions.FulfillmentDelete)]
        public IHttpActionResult DeleteFulfillmentCenters([FromUri] string[] ids)
        {
            _fulfillmentCenterService.Delete(ids);
            return Ok();
        }


        /// <summary>
        /// Get inventories of products
        /// </summary>
        /// <remarks>Get inventory of products for each fulfillment center.</remarks>
        /// <param name="ids">Products ids</param>
        /// <param name="fulfillmentCenterIds">The fulfillment centers that will be used to filter product inventories</param>
        [HttpGet]
        [Route("products")]
        [ResponseType(typeof(InventoryInfo[]))]
        public IHttpActionResult GetProductsInventories([FromUri] string[] ids, [FromUri] string[] fulfillmentCenterIds = null)
        {
            var criteria = AbstractTypeFactory<InventorySearchCriteria>.TryCreateInstance();
            criteria.FulfillmentCenterIds = fulfillmentCenterIds;
            criteria.ProductIds = ids;
            criteria.Take = int.MaxValue;

            var result = _inventorySearchService.SearchInventories(criteria);
            return Ok(result.Results);
        }

        /// <summary>
        /// Get inventories of products
        /// </summary>
        /// <remarks>Get inventory of products for each fulfillment center.</remarks>
        /// <param name="ids">Products ids</param>
        /// <param name="fulfillmentCenterIds">The fulfillment centers that will be used to filter product inventories</param>
        [HttpPost]
        [Route("products/plenty")]
        [ResponseType(typeof(InventoryInfo[]))]
        public IHttpActionResult GetProductsInventoriesByPlentyIds([FromBody] string[] ids, [FromUri] string[] fulfillmentCenterIds = null)
        {
            return GetProductsInventories(ids, fulfillmentCenterIds);
        }

        /// <summary>
        /// Get inventories of product
        /// </summary>
        /// <remarks>Get inventories of product for each fulfillment center.</remarks>
        /// <param name="productId">Product id</param>
        [HttpGet]
        [Route("products/{productId}")]
        [ResponseType(typeof(InventoryInfo[]))]
        public IHttpActionResult GetProductInventories(string productId)
        {
            return GetProductsInventories(new[] { productId });
        }

        /// <summary>
        /// Upsert inventory
        /// </summary>
        /// <remarks>Upsert (add or update) given inventory of product.</remarks>
        /// <param name="inventory">Inventory to upsert</param>
        [HttpPut]
        [Route("products/{productId}")]
        [ResponseType(typeof(InventoryInfo))]
        [CheckPermission(Permission = InventoryPredefinedPermissions.Update)]
        public IHttpActionResult UpsertProductInventory(InventoryInfo inventory)
        {
            var result = _inventoryService.UpsertInventory(inventory);
            return Ok(result);
        }
    }
}
