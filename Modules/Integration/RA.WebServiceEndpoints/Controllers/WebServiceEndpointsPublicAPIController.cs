using Microsoft.AspNetCore.Mvc;
using RA.Catalogs.Domain;
using RA.Catalogs.DTO;
using RA.Catalogs.Services;
using RA.Categories.Domain;
using RA.Inventory.Domain;
using RA.Inventory.Services.InventoryStockServices;
using RA.WebFramework.Extensions;
using RA.WebServiceEndpoints.Domain;
using RA.WebServiceEndpoints.Services;
using RAerp.Controllers.Admin;
using RAerp.Services.EntityAttributeServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RA.WebServiceEndpoints.Controllers
{
    [ApiController]
    [Route("api/public")]
    public class WebServiceEndpointsPublicAPIController : AdminApiController
    {
        private readonly ICatalogService _catalogService;
        private readonly IInventoryStockService _inventoryStockService;
        private readonly IWebServiceEndpointService _webServiceEndpointService;
        private readonly IEntityAttributeService _entityAttributeService;

        public WebServiceEndpointsPublicAPIController(ICatalogService catalogService,
            IInventoryStockService inventoryStockService,
            IWebServiceEndpointService webServiceEndpointService,
            IEntityAttributeService entityAttributeService)
        {
            _catalogService = catalogService;
            _inventoryStockService = inventoryStockService;
            _webServiceEndpointService = webServiceEndpointService;
            _entityAttributeService = entityAttributeService;
        }

        [HttpGet("catalog/detail/{id:guid}")]
        public async Task<IActionResult> GetCatalogDetail(Guid id)
        {
            if (id.IsNullOrEmpty())
                return NotFound();

            Catalog catalog = await _catalogService.GetByIdAsync(id);
            if (catalog == null)
                return NotFound();

            InventoryStock stock = await _inventoryStockService.GetByCatalogIdAsync(catalog.Id);
            if (stock == null)
                return NotFound();

            CatalogDetailResponseDto catalogDetail = new CatalogDetailResponseDto()
            {
                Name = catalog.Name,
                Description = catalog.Description,
                Price = catalog.Price,
                ImagePath = catalog.ImagePath,
                AvailableStock = stock.QuantityAvailable,
                IsLowStock = stock.IsLowStock,
                Attributes = _entityAttributeService.GetEntityAttributeValueListAsync(entityId: catalog.Id).Result.ToList()
            };

            return Ok(catalogDetail);
        }
    }
}
