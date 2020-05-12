using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VirtoCommerce.InventoryModule.Core;
using VirtoCommerce.InventoryModule.Core.Model;
using VirtoCommerce.InventoryModule.Core.Model.Search;
using VirtoCommerce.InventoryModule.Core.Services;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.InventoryModule.Web.Controllers.Api
{
    [Route("api")]
    [Authorize]
    public class InventoryModuleController : Controller
    {
        private readonly IInventoryService _inventoryService;
        private readonly IInventorySearchService _inventorySearchService;
        private readonly IProductInventorySearchService _productInventorySearchService;
        private readonly IFulfillmentCenterSearchService _fulfillmentCenterSearchService;
        private readonly IFulfillmentCenterService _fulfillmentCenterService;

        public InventoryModuleController(IInventoryService inventoryService,
            IInventorySearchService inventorySearchService,
            IProductInventorySearchService fulfillmentCenterInventorySearchService,
            IFulfillmentCenterSearchService fulfillmentCenterSearchService,
            IFulfillmentCenterService fulfillmentCenterService)
        {
            _inventoryService = inventoryService;
            _inventorySearchService = inventorySearchService;
            _productInventorySearchService = fulfillmentCenterInventorySearchService;
            _fulfillmentCenterSearchService = fulfillmentCenterSearchService;
            _fulfillmentCenterService = fulfillmentCenterService;
        }

        /// <summary>
        /// Search inventories by given criteria
        /// </summary>
        [HttpPost]
        [Route("inventories/search")]
        public async Task<ActionResult<InventoryInfoSearchResult>> SearchInventories([FromBody] InventorySearchCriteria searchCriteria)
        {
            var result = await _inventorySearchService.SearchInventoriesAsync(searchCriteria);
            return Ok(result);
        }

        //TODO remove the enpoint but It needs for swagger validation
        [HttpPost]
        [Route("inventory/search")]
        public Task<ActionResult<InventoryInfoSearchResult>> SearchInventory([FromBody] InventorySearchCriteria searchCriteria)
        {
            return SearchInventories(searchCriteria);
        }

        /// <summary>
        /// Search inventories by given criteria
        /// </summary>
        [HttpPost]
        [Route("inventory/product/inventories/search")]
        public async Task<ActionResult<InventoryInfoSearchResult>> SearchProductInventories([FromBody] ProductInventorySearchCriteria searchCriteria)
        {
            var result = await _productInventorySearchService.SearchProductInventoriesAsync(searchCriteria);
            return Ok(result);
        }

        //TODO remove the enpoint but It needs for swagger validation
        [HttpPost]
        [Route("inventory/product/search")]
        public Task<ActionResult<InventoryInfoSearchResult>> SearchProductInventory([FromBody] ProductInventorySearchCriteria searchCriteria)
        {
            return SearchProductInventories(searchCriteria);
        }

        /// <summary>
        /// Search fulfillment centers registered in the system
        /// </summary>
        [HttpPost]
        [Route("inventory/fulfillmentcenters/search")]
        public async Task<ActionResult<FulfillmentCenterSearchResult>> SearchFulfillmentCenters([FromBody] FulfillmentCenterSearchCriteria searchCriteria)
        {
            var retVal = await _fulfillmentCenterSearchService.SearchCentersAsync(searchCriteria);
            return Ok(retVal);
        }

        /// <summary>
        /// Get fulfillment center by id
        /// </summary>
        /// <param name="id">fulfillment center id</param>
        [HttpGet]
        [Route("inventory/fulfillmentcenters/{id}")]
        public async Task<ActionResult<FulfillmentCenter>> GetFulfillmentCenter([FromRoute]string id)
        {
            var retVal = await _fulfillmentCenterService.GetByIdsAsync(new[] { id });
            return Ok(retVal.FirstOrDefault());
        }

        /// <summary>
        /// Get fulfillment centers by ids
        /// </summary>
        /// <param name="ids">fulfillment center ids</param>
        [HttpPost]
        [Route("inventory/fulfillmentcenters/plenty")]
        public async Task<ActionResult<IEnumerable<FulfillmentCenter>>> GetFulfillmentCenters([FromBody] string[] ids)
        {
            var retVal = await _fulfillmentCenterService.GetByIdsAsync(ids);
            return Ok(retVal);
        }

        /// <summary>
        ///  Save fulfillment center 
        /// </summary>
        /// <param name="center">fulfillment center</param>
        [HttpPut]
        [Route("inventory/fulfillmentcenters")]
        [Authorize(ModuleConstants.Security.Permissions.FulfillmentEdit)]
        public async Task<ActionResult<FulfillmentCenter>> SaveFulfillmentCenter([FromBody]FulfillmentCenter center)
        {
            await _fulfillmentCenterService.SaveChangesAsync(new[] { center });
            return Ok(center);
        }


        /// <summary>
        ///  Save fulfillment centers 
        /// </summary>
        /// <param name="centers">fulfillment centers</param>
        [HttpPost]
        [Route("inventory/fulfillmentcenters/batch")]
        [Authorize(ModuleConstants.Security.Permissions.FulfillmentEdit)]
        public async Task<ActionResult<FulfillmentCenter[]>> SaveFulfillmentCenters([FromBody] FulfillmentCenter[] centers)
        {
            await _fulfillmentCenterService.SaveChangesAsync(centers);
            return Ok(centers);
        }

        /// <summary>
        /// Delete fulfillment centers registered in the system
        /// </summary>
        [HttpDelete]
        [Route("inventory/fulfillmentcenters")]
        [Authorize(ModuleConstants.Security.Permissions.FulfillmentDelete)]
        [ProducesResponseType(typeof(void), StatusCodes.Status204NoContent)]
        public async Task<ActionResult> DeleteInventoryFulfillmentCenters([FromQuery] string[] ids)
        {
            await _fulfillmentCenterService.DeleteAsync(ids);
            return NoContent();
        }

        //TODO remove the enpoint but It needs for swagger validation
        [HttpDelete]
        [Route("fulfillment/centers")]
        [Authorize(ModuleConstants.Security.Permissions.FulfillmentDelete)]
        [ProducesResponseType(typeof(void), StatusCodes.Status204NoContent)]
        public Task<ActionResult> DeleteFulfillmentCenters([FromQuery] string[] ids)
        {
            return DeleteInventoryFulfillmentCenters(ids);
        }

        /// <summary>
        /// Get inventories of products
        /// </summary>
        /// <remarks>Get inventory of products for each fulfillment center.</remarks>
        /// <param name="ids">Products ids</param>
        /// <param name="fulfillmentCenterIds">The fulfillment centers that will be used to filter product inventories</param>
        [HttpGet]
        [Route("inventory/products")]
        public async Task<ActionResult<InventoryInfo[]>> GetProductsInventories([FromQuery] string[] ids, [FromQuery] string[] fulfillmentCenterIds = null)
        {
            if (ids.IsNullOrEmpty() && fulfillmentCenterIds.IsNullOrEmpty())
            {
                throw new ArgumentNullException($"the products ids or fulfillmentCenters ids must be set");
            }
            var criteria = AbstractTypeFactory<InventorySearchCriteria>.TryCreateInstance();
            criteria.FulfillmentCenterIds = fulfillmentCenterIds;
            criteria.ProductIds = ids;
            criteria.Take = int.MaxValue;

            var result = await _inventorySearchService.SearchInventoriesAsync(criteria);
            return Ok(result.Results);
        }

        /// <summary>
        /// Get inventories of products
        /// </summary>
        /// <remarks>Get inventory of products for each fulfillment center.</remarks>
        /// <param name="ids">Products ids</param>
        /// <param name="fulfillmentCenterIds">The fulfillment centers that will be used to filter product inventories</param>
        [HttpPost]
        [Route("inventory/products/plenty")]
        public async Task<ActionResult<InventoryInfo[]>> GetProductsInventoriesByPlentyIds([FromBody] string[] ids, [FromQuery] string[] fulfillmentCenterIds = null)
        {
            return await GetProductsInventories(ids, fulfillmentCenterIds);
        }

        /// <summary>
        /// Get inventories of product
        /// </summary>
        /// <remarks>Get inventories of product for each fulfillment center.</remarks>
        /// <param name="productId">Product id</param>
        [HttpGet]
        [Route("inventory/products/{productId}")]
        public async Task<ActionResult<InventoryInfo[]>> GetProductInventories([FromRoute]string productId)
        {
            return await GetProductsInventories(new[] { productId });
        }

        /// <summary>
        /// Update inventory
        /// </summary>
        /// <remarks>Update given inventory of product.</remarks>
        /// <param name="inventory">Inventory to update</param>
        [HttpPut]
        [Route("inventory/products/{productId}")]
        [Authorize(ModuleConstants.Security.Permissions.Update)]
        public async Task<ActionResult<InventoryInfo>> UpdateProductInventory([FromBody]InventoryInfo inventory)
        {
            await _inventoryService.SaveChangesAsync(new[] { inventory });
            return Ok(inventory);
        }

        /// <summary>
        /// Upsert inventories
        /// </summary>
        /// <remarks>Upsert (add or update) given inventories.</remarks>
        /// <param name="inventories">Inventories to upsert</param>
        [HttpPut]
        [Route("inventory/plenty")]
        [Authorize(ModuleConstants.Security.Permissions.Update)]
        public async Task<ActionResult> UpsertProductInventories([FromBody]InventoryInfo[] inventories)
        {
            await _inventoryService.SaveChangesAsync(inventories);
            return Ok();
        }
    }
}
