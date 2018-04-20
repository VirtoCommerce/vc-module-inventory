using System.Collections.Generic;
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

        public InventoryModuleController(IInventoryService inventoryService, IFulfillmentCenterSearchService fulfillmentCenterSearchService, 
                                          IInventorySearchService inventorySearchService, IFulfillmentCenterService fulfillmentCenterService)
        {
            _inventoryService = inventoryService;
            _fulfillmentCenterSearchService = fulfillmentCenterSearchService;
            _fulfillmentCenterService = fulfillmentCenterService;
            _inventorySearchService = inventorySearchService;
        }

        /// <summary>
        /// Search fulfillment centers registered in the system
        /// </summary>
        [HttpPost]
        [AllowAnonymous]
        [ResponseType(typeof(GenericSearchResult<FulfillmentCenter>))]
        [Route("fulfillmentcenters/search")]
        public IHttpActionResult SearchFulfillmentCenters([FromBody] FulfillmentCenterSearchCriteria searchCriteria)
        {
            var retVal = _fulfillmentCenterSearchService.SearchCenters(searchCriteria);
            return Ok(retVal);
        }
      

        /// <summary>
        /// Find fulfillment center by id
        /// </summary>
        /// <param name="id">fulfillment center id</param>
        [HttpGet]
        [ResponseType(typeof(FulfillmentCenter))]
        [Route("fulfillmentcenters/{id}")]
        public IHttpActionResult GetFulfillmentCenter(string id)
        {
            var retVal = _fulfillmentCenterService.GetByIds(new[] { id }).FirstOrDefault();
            return Ok(retVal);
        }

        /// <summary>
        ///  Save fulfillment center 
        /// </summary>
        /// <param name="center">fulfillment center</param>
        [HttpPut]
        [ResponseType(typeof(FulfillmentCenter))]
        [Route("fulfillmentcenters")]
        [CheckPermission(Permission = InventoryPredefinedPermissions.FulfillmentEdit)]
        public IHttpActionResult SaveFulfillmentCenter(FulfillmentCenter center)
        {
            _fulfillmentCenterService.SaveChanges(new[] { center });
            return Ok(center);
        }

        /// <summary>
        /// Delete  fulfillment centers registered in the system
        /// </summary>
        [HttpDelete]
        [ResponseType(typeof(void))]
        [Route("fulfillmentcenters")]
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
		[HttpGet]
        [Route("products")]
        [ResponseType(typeof(InventoryInfo[]))]
        public IHttpActionResult GetProductsInventories([FromUri] string[] ids)
        {
            var result = new List<InventoryInfo>();
            var allFulfillments = _fulfillmentCenterSearchService.SearchCenters(new FulfillmentCenterSearchCriteria { Take = int.MaxValue }).Results;
            var inventories = _inventoryService.GetProductsInventoryInfos(ids);
            foreach (var productId in ids)
            {
                foreach (var fulfillment in allFulfillments)
                {
                    var inventory = inventories.FirstOrDefault(x => x.ProductId == productId && x.FulfillmentCenterId == fulfillment.Id);
                    if(inventory == null)
                    {
                        inventory = AbstractTypeFactory<InventoryInfo>.TryCreateInstance();
                        inventory.ProductId = productId;
                        inventory.FulfillmentCenter = fulfillment;
                        inventory.FulfillmentCenterId = fulfillment.Id;
                    }
                    result.Add(inventory);
                }
            }

            return Ok(result.ToArray());
        }

        /// <summary>
        /// Get inventories of products
        /// </summary>
        /// <remarks>Get inventory of products for each fulfillment center.</remarks>
        /// <param name="ids">Products ids</param>
		[HttpPost]
        [Route("products/plenty")]
        [ResponseType(typeof(InventoryInfo[]))]
        public IHttpActionResult GetProductsInventoriesByPlentyIds([FromBody] string[] ids)
        {
            return GetProductsInventories(ids);
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
