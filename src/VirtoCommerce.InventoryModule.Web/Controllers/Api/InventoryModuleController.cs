using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using VirtoCommerce.InventoryModule.Core.Model;
using VirtoCommerce.InventoryModule.Core.Model.Search;
using VirtoCommerce.InventoryModule.Core.Services;
using VirtoCommerce.Platform.Core.Common;
using Permissions = VirtoCommerce.InventoryModule.Core.ModuleConstants.Security.Permissions;

namespace VirtoCommerce.InventoryModule.Web.Controllers.Api
{
    [Route("api")]
    [Authorize]
    public class InventoryModuleController(
        IInventoryService inventoryService,
        IInventorySearchService inventorySearchService,
        IProductInventorySearchService productInventorySearchService,
        IFulfillmentCenterSearchService fulfillmentCenterSearchService,
        IFulfillmentCenterService fulfillmentCenterService)
        : Controller
    {
        /// <summary>
        /// Search inventories by given criteria
        /// </summary>
        [HttpPost]
        [Route("inventories/search")]
        [Authorize(Permissions.Read)]
        [Obsolete("Use 'inventory/search'", DiagnosticId = "VC0010", UrlFormat = "https://docs.virtocommerce.org/products/products-virto3-versions")]
        public Task<ActionResult<InventoryInfoSearchResult>> SearchInventoriesObsolete([FromBody] InventorySearchCriteria searchCriteria)
        {
            return SearchInventories(searchCriteria);
        }

        /// <summary>
        /// Search inventories by given criteria
        /// </summary>
        [HttpPost]
        [Route("inventory/search")]
        [Authorize(Permissions.Read)]
        public async Task<ActionResult<InventoryInfoSearchResult>> SearchInventories([FromBody] InventorySearchCriteria searchCriteria)
        {
            var result = await inventorySearchService.SearchNoCloneAsync(searchCriteria);
            return Ok(result);
        }

        /// <summary>
        /// Search inventories by given criteria
        /// </summary>
        [HttpPost]
        [Route("inventory/product/inventories/search")]
        [Authorize(Permissions.Read)]
        [Obsolete("Use 'inventory/product/search'", DiagnosticId = "VC0010", UrlFormat = "https://docs.virtocommerce.org/products/products-virto3-versions")]
        public Task<ActionResult<InventoryInfoSearchResult>> SearchProductInventoriesObsolete([FromBody] ProductInventorySearchCriteria searchCriteria)
        {
            return SearchProductInventories(searchCriteria);
        }

        /// <summary>
        /// Search inventories by given criteria
        /// </summary>
        [HttpPost]
        [Route("inventory/product/search")]
        [Authorize(Permissions.Read)]
        public async Task<ActionResult<InventoryInfoSearchResult>> SearchProductInventories([FromBody] ProductInventorySearchCriteria searchCriteria)
        {
            var result = await productInventorySearchService.SearchNoCloneAsync(searchCriteria);
            return Ok(result);
        }

        /// <summary>
        /// Search fulfillment centers registered in the system
        /// </summary>
        [HttpPost]
        [Route("inventory/fulfillmentcenters/search")]
        [Authorize(Permissions.FulfillmentRead)]
        public async Task<ActionResult<FulfillmentCenterSearchResult>> SearchFulfillmentCenters([FromBody] FulfillmentCenterSearchCriteria searchCriteria)
        {
            var retVal = await fulfillmentCenterSearchService.SearchNoCloneAsync(searchCriteria);
            return Ok(retVal);
        }

        /// <summary>
        /// Get fulfillment center by id
        /// </summary>
        /// <param name="id">fulfillment center id</param>
        [HttpGet]
        [Route("inventory/fulfillmentcenters/{id}")]
        [Authorize(Permissions.FulfillmentRead)]
        public async Task<ActionResult<FulfillmentCenter>> GetFulfillmentCenter([FromRoute] string id)
        {
            var retVal = await fulfillmentCenterService.GetNoCloneAsync(id);
            return Ok(retVal);
        }

        /// <summary>
        /// Get fulfillment center by outer id
        /// </summary>
        /// <param name="outerId">fulfillment center outer id</param>
        [HttpGet]
        [Route("inventory/fulfillmentcenters/outer/{outerId}")]
        [Authorize(Permissions.FulfillmentRead)]
        public async Task<ActionResult<FulfillmentCenter>> GetFulfillmentCenterByOuterId([FromRoute] string outerId)
        {
            var fulfillmentCenter = await fulfillmentCenterService.GetByOuterIdAsync(outerId);
            return Ok(fulfillmentCenter);
        }

        /// <summary>
        /// Get fulfillment centers by ids
        /// </summary>
        /// <param name="ids">fulfillment center ids</param>
        [HttpPost]
        [Route("inventory/fulfillmentcenters/plenty")]
        [Authorize(Permissions.FulfillmentRead)]
        public async Task<ActionResult<FulfillmentCenter[]>> GetFulfillmentCenters([FromBody] string[] ids)
        {
            var retVal = await fulfillmentCenterService.GetNoCloneAsync(ids);
            return Ok(retVal);
        }

        /// <summary>
        ///  Save fulfillment center 
        /// </summary>
        /// <param name="center">fulfillment center</param>
        [HttpPut]
        [Route("inventory/fulfillmentcenters")]
        [Authorize(Permissions.FulfillmentEdit)]
        public async Task<ActionResult<FulfillmentCenter>> SaveFulfillmentCenter([FromBody] FulfillmentCenter center)
        {
            await fulfillmentCenterService.SaveChangesAsync([center]);
            return Ok(center);
        }

        /// <summary>
        ///  Save fulfillment centers 
        /// </summary>
        /// <param name="centers">fulfillment centers</param>
        [HttpPost]
        [Route("inventory/fulfillmentcenters/batch")]
        [Authorize(Permissions.FulfillmentEdit)]
        public async Task<ActionResult<FulfillmentCenter[]>> SaveFulfillmentCenters([FromBody] FulfillmentCenter[] centers)
        {
            await fulfillmentCenterService.SaveChangesAsync(centers);
            return Ok(centers);
        }

        /// <summary>
        /// Delete fulfillment centers registered in the system
        /// </summary>
        [HttpDelete]
        [Route("inventory/fulfillmentcenters")]
        [Authorize(Permissions.FulfillmentDelete)]
        [ProducesResponseType(typeof(void), StatusCodes.Status204NoContent)]
        public async Task<ActionResult> DeleteFulfillmentCenters([FromQuery] string[] ids)
        {
            await fulfillmentCenterService.DeleteAsync(ids);
            return NoContent();
        }

        /// <summary>
        /// Get inventories of product
        /// </summary>
        /// <remarks>Get inventories of product for each fulfillment center.</remarks>
        /// <param name="productId">Product id</param>
        [HttpGet]
        [Route("inventory/products/{productId}")]
        [Authorize(Permissions.Read)]
        public Task<ActionResult<InventoryInfo[]>> GetProductInventories([FromRoute] string productId)
        {
            return GetProductInventories([productId]);
        }

        /// <summary>
        /// Get inventories of products
        /// </summary>
        /// <remarks>Get inventory of products for each fulfillment center.</remarks>
        /// <param name="ids">Products ids</param>
        /// <param name="fulfillmentCenterIds">The fulfillment centers that will be used to filter product inventories</param>
        [HttpGet]
        [Route("inventory/products")]
        [Authorize(Permissions.Read)]
        public async Task<ActionResult<InventoryInfo[]>> GetProductInventories([FromQuery] string[] ids, [FromQuery] string[] fulfillmentCenterIds = null)
        {
            if (ids.IsNullOrEmpty() && fulfillmentCenterIds.IsNullOrEmpty())
            {
                return BadRequest($"{nameof(ids)} or {nameof(fulfillmentCenterIds)} must be set");
            }

            var criteria = AbstractTypeFactory<InventorySearchCriteria>.TryCreateInstance();
            criteria.FulfillmentCenterIds = fulfillmentCenterIds;
            criteria.ProductIds = ids;

            var result = await inventorySearchService.SearchAllNoCloneAsync(criteria);

            return Ok(result);
        }

        /// <summary>
        /// Get inventories of products
        /// </summary>
        /// <remarks>Get inventory of products for each fulfillment center.</remarks>
        /// <param name="ids">Products ids</param>
        /// <param name="fulfillmentCenterIds">The fulfillment centers that will be used to filter product inventories</param>
        [HttpPost]
        [Route("inventory/products/plenty")]
        [Authorize(Permissions.Read)]
        public Task<ActionResult<InventoryInfo[]>> GetProductsInventoriesByPlentyIds([FromBody] string[] ids, [FromQuery] string[] fulfillmentCenterIds = null)
        {
            return GetProductInventories(ids, fulfillmentCenterIds);
        }

        /// <summary>
        /// Update inventory
        /// </summary>
        /// <remarks>Update given inventory of product.</remarks>
        /// <param name="inventory">Inventory to update</param>
        [HttpPut]
        [Route("inventory/products/{productId}")]
        [Authorize(Permissions.Update)]
        public async Task<ActionResult<InventoryInfo>> UpdateProductInventory([FromBody] InventoryInfo inventory)
        {
            await inventoryService.SaveChangesAsync([inventory]);
            return Ok(inventory);
        }

        /// <summary>
        /// Upsert inventories
        /// </summary>
        /// <remarks>Upsert (add or update) given inventories.</remarks>
        /// <param name="inventories">Inventories to upsert</param>
        [HttpPut]
        [Route("inventory/plenty")]
        [Authorize(Permissions.Update)]
        public async Task<ActionResult> UpsertProductInventories([FromBody] InventoryInfo[] inventories)
        {
            await inventoryService.SaveChangesAsync(inventories);
            return Ok();
        }

        /// <summary>
        /// Partial update for the specified FulfillmentCenter by id
        /// </summary>
        /// <param name="id">FulfillmentCenter id</param>
        /// <param name="patchDocument">JsonPatchDocument object with fields to update</param>
        [HttpPatch]
        [Route("inventory/fulfillmentcenters/{id}")]
        [Authorize(Permissions.FulfillmentEdit)]
        [ProducesResponseType(typeof(void), StatusCodes.Status204NoContent)]
        public async Task<ActionResult> PatchFulfillmentCenter(string id, [FromBody] JsonPatchDocument<FulfillmentCenter> patchDocument)
        {
            if (patchDocument == null)
            {
                return BadRequest();
            }

            var fulfillment = await fulfillmentCenterService.GetByIdAsync(id);
            if (fulfillment == null)
            {
                return NotFound();
            }

            patchDocument.ApplyTo(fulfillment, ModelState);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await fulfillmentCenterService.SaveChangesAsync([fulfillment]);

            return NoContent();
        }

        /// <summary>
        /// Partial update for the specified inventory of product by id
        /// </summary>
        /// <param name="id">Inventory id</param>
        /// <param name="patchDocument">JsonPatchDocument object with fields to update</param>
        [HttpPatch]
        [Route("inventory/{id}")]
        [Authorize(Permissions.Update)]
        [ProducesResponseType(typeof(void), StatusCodes.Status204NoContent)]
        public async Task<ActionResult> PatchProductInventory(string id, [FromBody] JsonPatchDocument<InventoryInfo> patchDocument)
        {
            if (patchDocument == null)
            {
                return BadRequest();
            }

            var inventory = await inventoryService.GetByIdAsync(id);
            if (inventory == null)
            {
                return NotFound();
            }

            patchDocument.ApplyTo(inventory, ModelState);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await inventoryService.SaveChangesAsync([inventory]);

            return NoContent();
        }
    }
}
