using Asp.Versioning;
using Azure.Core;
using Humanizer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RA.Core.PluginData.Inventory;
using RA.Inventory.Domain;
using RA.Inventory.Models;
using RA.Inventory.Services.InventoryReservationServices;
using RA.Inventory.Services.InventoryServices;
using RA.Inventory.Services.InventoryStockServices;
using RA.Inventory.Services.InventoryTransactionServices;
using RA.WebFramework.Extensions;
using RAerp.Controllers.Admin;
using RAerp.Helpers.UserHelper;
using RAerp.Security.AccessRightsControl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace RA.Inventory.Controllers
{
    [ApiController]
    [Route("api/v{version:ApiVersion}/inventory")]
    [ApiVersion("1.0")]
    public class InventoryAPIController : AdminApiController
    {
        #region Constants
        private readonly IInventoryManager _inventoryManager;
        private readonly IUserIdentity _userIdentity;
        private readonly IAccessControl _accessControl;
        private readonly IInventoryStockService _inventoryStockService;
        private readonly IInventoryReservationService _inventoryReservationService;
        private readonly IInventoryTransactionService _inventoryTransactionService;
        #endregion

        #region Ctor
        public InventoryAPIController(IInventoryManager inventoryManager,
            IUserIdentity userIdentity,
            IAccessControl accessControl,
            IInventoryStockService inventoryStockService,
            IInventoryReservationService inventoryReservationService,
            IInventoryTransactionService inventoryTransactionService)
        {
            _inventoryManager = inventoryManager;
            _userIdentity = userIdentity;
            _accessControl = accessControl;
            _inventoryStockService = inventoryStockService;
            _inventoryReservationService = inventoryReservationService;
            _inventoryTransactionService = inventoryTransactionService;
        }
        #endregion

        // GET api/v1/inventory/stock
        [HttpGet("stock"), MapToApiVersion("1.0")]
        public async Task<IActionResult> GetAllStockList([FromQuery] InventoryStockSearchModel searchModel)
        {
            await ValidateUserAccessAndCredentials<InventoryStock>(_accessControl, _userIdentity);
            if (searchModel.PageNumber == 0 && searchModel.PageSize == 0)
            {
                var stockList = await _inventoryStockService.GetStockListAsync(
                    catalogTypeId: searchModel.CatalogTypeId,
                    catalogIds: searchModel.CatalogIds, 
                    warehouseIds: searchModel.WarehouseIds,
                    searchCreatedOn: searchModel.CreatedOn,
                    showLowStockOnly: searchModel.ShowLowStockOnly,
                    sortByCatalogName: searchModel.SortByCatalogName);
                return Ok(GenerateListResponseModel<InventoryStock>(HttpStatusCode.OK, "Successfully get stock of catalog and warehouse", dataList: stockList));
            }
            else
            {
                var stockList = await _inventoryStockService.GetStockPagedResultListAsync(
                    catalogTypeId: searchModel.CatalogTypeId,
                    catalogIds: searchModel.CatalogIds,
                    warehouseIds: searchModel.WarehouseIds,
                    searchCreatedOn: searchModel.CreatedOn,
                    showLowStockOnly: searchModel.ShowLowStockOnly,
                    sortByCatalogName: searchModel.SortByCatalogName,
                    pageNumber: searchModel.PageNumber, pageSize: searchModel.PageSize);
                return Ok(GenerateListResponseModel<InventoryStock>(HttpStatusCode.OK, "Successfully get stock of catalog and warehouse", dataList: stockList.Items));
            }
        }

        // GET api/v1/inventory/stock/catalog/{catalogTypeId}
        [HttpGet("stock/catalog/{catalogTypeId:guid}"), MapToApiVersion("1.0")]
        public async Task<IActionResult> GetCatalogStockList(Guid catalogTypeId)
        {
            await ValidateUserAccessAndCredentials<InventoryStock>(_accessControl, _userIdentity);

            if (catalogTypeId.IsNullOrEmpty())
                return BadRequest(GenerateErrorResponseModel(HttpStatusCode.BadRequest, "CatalogId is invalid"));

            var stockList = await _inventoryStockService.GetStockListAsync(catalogTypeId);

            return Ok(GenerateListResponseModel<InventoryStock>(HttpStatusCode.OK, "Successfully get stock of catalog and warehouse", dataList: stockList));
        }

        // GET api/v1/inventory/stock/warehouse/{catalogTypeId}/{warehouseId}
        [HttpGet("stock/warehouse/{catalogTypeId:guid}/{warehouseId:guid}"), MapToApiVersion("1.0")]
        public async Task<IActionResult> GetWarehouseStockList(Guid catalogTypeId, Guid warehouseId)
        {
            await ValidateUserAccessAndCredentials<InventoryStock>(_accessControl, _userIdentity);

            if (catalogTypeId.IsNullOrEmpty())
                return BadRequest(GenerateErrorResponseModel(HttpStatusCode.BadRequest, "CatalogId is invalid"));

            if (warehouseId.IsNullOrEmpty())
                return BadRequest(GenerateErrorResponseModel(HttpStatusCode.BadRequest, "WarehouseId is invalid"));

            var stockList = await _inventoryStockService.GetStockListByWarehouseId(catalogTypeId, warehouseId);

            return Ok(GenerateListResponseModel<InventoryStock>(HttpStatusCode.OK, "Successfully get stock of catalog and warehouse", dataList: stockList));
        }

        // GET api/v1/inventory/stock/{catalogId}/{warehouseId}
        [HttpGet("stock/{catalogId:guid}/{warehouseId:guid}"), MapToApiVersion("1.0")]
        public async Task<IActionResult> GetStockByCatalogIdAndWarehouseId(Guid catalogId, Guid warehouseId)
        {
            await ValidateUserAccessAndCredentials<InventoryStock>(_accessControl, _userIdentity);

            var stock = await _inventoryStockService.GetByCatalogIdAndWarehouseIdAsync(catalogId, warehouseId);
            if (stock is null)
                return NotFound($"Stock for catalog {catalogId} and warehouse {warehouseId} not found.");

            return Ok(GenerateResponseModel<InventoryStock>(HttpStatusCode.OK, "Successfully get stock of catalog and warehouse", stock));
        }

        // POST api/v1/inventory/stock/initialize
        [HttpPost("stock/initialize"), MapToApiVersion("1.0")]
        public async Task<IActionResult> InitializeStock([FromBody] InventoryStock req)
        {
            await ValidateUserAccessAndCredentials<InventoryStock>(_accessControl, _userIdentity);

            if (req.QuantityOnHand <= 0)
                return BadRequest(GenerateErrorResponseModel(HttpStatusCode.BadRequest, "Cannot add stock with 0 qty or negative qty"));

            var currentUser = await _userIdentity.GetCurrentApiUserAsync(HttpContext.User);

            InventoryStock stock = new InventoryStock
            {
                CatalogId = req.CatalogId,
                WarehouseId = req.WarehouseId,
                QuantityOnHand = req.QuantityOnHand,
                QuantityReserved = 0,
                QuantityAvailable = req.QuantityOnHand,
                LowStockThreshold = req.LowStockThreshold,
                CreatedById = currentUser.Id,
                CreatedOn = DateTime.UtcNow
            };

            InventoryStock result = await _inventoryManager.InitializeStockAsync(stock);

            return Ok(GenerateResponseModel<InventoryStock>(HttpStatusCode.OK, "Successfully initialize stock", result));
        }

        // POST api/v1/inventory/stock/receive
        [HttpPost("stock/receive"), MapToApiVersion("1.0")]
        public async Task<IActionResult> ReceiveStock([FromBody] InventoryTransaction req)
        {
            await ValidateUserAccessAndCredentials<InventoryStock>(_accessControl, _userIdentity);

            if (req.Quantity <= 0)
                return BadRequest(GenerateErrorResponseModel(HttpStatusCode.BadRequest, "Cannot receive stock with 0 qty or negative qty"));

            if (req.ReferenceId.IsNullOrEmpty())
                return BadRequest(GenerateErrorResponseModel(HttpStatusCode.BadRequest, "ReferenceId cannot be empty"));

            var currentUser = await _userIdentity.GetCurrentApiUserAsync(HttpContext.User);

            await _inventoryManager.ReceiveStockAsync(
                req.ReferenceId.Value,
                req.ReferenceNbr,
                req.CatalogId,
                req.WarehouseId,
                req.Quantity,
                currentUser.Id,
                req.Note,
                req.TransactionType);

            return Created();
        }

        // POST api/inventory/stock/release
        [HttpPost("stock/release"), MapToApiVersion("1.0")]
        public async Task<IActionResult> ReleaseStock([FromBody] InventoryTransaction req)
        {
            await ValidateUserAccessAndCredentials<InventoryStock>(_accessControl, _userIdentity);

            if (req.Quantity <= 0)
                return BadRequest(GenerateErrorResponseModel(HttpStatusCode.BadRequest, "Cannot release stock with 0 qty or negative qty"));

            if (req.ReferenceId.IsNullOrEmpty())
                return BadRequest(GenerateErrorResponseModel(HttpStatusCode.BadRequest, "ReferenceId cannot be empty"));

            var currentUser = await _userIdentity.GetCurrentApiUserAsync(HttpContext.User);

            await _inventoryManager.ReleaseStockAsync(
                req.ReferenceId.Value,
                req.ReferenceNbr,
                req.CatalogId,
                req.WarehouseId,
                req.Quantity,
                currentUser.Id,
                req.Note,
                req.TransactionType);

            return Created();
        }

        // POST api/v1/inventory/stock/adjust
        [HttpPost("stock/adjust"), MapToApiVersion("1.0")]
        public async Task<IActionResult> ManualAdjustment([FromBody] InventoryTransaction req)
        {
            await ValidateUserAccessAndCredentials<InventoryStock>(_accessControl, _userIdentity);

            if (req.Quantity <= 0)
                return BadRequest(GenerateErrorResponseModel(HttpStatusCode.BadRequest, "Cannot adjust stock with 0 qty or negative qty"));

            var currentUser = await _userIdentity.GetCurrentApiUserAsync(HttpContext.User);

            await _inventoryManager.ManualAdjustmentAsync(
                req.CatalogId,
                req.WarehouseId,
                req.Quantity,
                currentUser.Id);

            return Created();
        }

        // GET api/v1/inventory/transaction
        [HttpGet("transaction"), MapToApiVersion("1.0")]
        public async Task<IActionResult> GetTransactionList([FromQuery] InventoryTransactionSearchModel searchModel)
        {
            await ValidateUserAccessAndCredentials<InventoryStock>(_accessControl, _userIdentity);

            if (searchModel.PageNumber == 0 && searchModel.PageSize == 0)
            {
                var transactions = await _inventoryTransactionService.GetTransactionListAsync(
                    referenceId: searchModel.ReferenceId,
                    catalogTypeId: searchModel.CatalogTypeId,
                    stockId: searchModel.StockId,
                    catalogIds: searchModel.CatalogIds,
                    warehouseIds: searchModel.WarehouseIds,
                    referenceNbr: searchModel.ReferenceNbr,
                    transactionTypeIds: searchModel.TransactionTypeIds,
                    searchCreatedOn: searchModel.CreatedOn,
                    showLowStockOnly: searchModel.ShowLowStockOnly,
                    sortByCatalogName: searchModel.SortByCatalogName);
                return Ok(GenerateListResponseModel<InventoryTransaction>(HttpStatusCode.OK, "Successfully get transactions", dataList: transactions));
            }
            else
            {
                var transactions = await _inventoryTransactionService.GetTransactionPagedResultListAsync(
                    referenceId: searchModel.ReferenceId,
                    catalogTypeId: searchModel.CatalogTypeId,
                    stockId: searchModel.StockId,
                    catalogIds: searchModel.CatalogIds,
                    warehouseIds: searchModel.WarehouseIds,
                    referenceNbr: searchModel.ReferenceNbr,
                    transactionTypeIds: searchModel.TransactionTypeIds,
                    searchCreatedOn: searchModel.CreatedOn,
                    showLowStockOnly: searchModel.ShowLowStockOnly,
                    sortByCatalogName: searchModel.SortByCatalogName,
                    pageNumber: searchModel.PageNumber, pageSize: searchModel.PageSize);
                return Ok(GenerateListResponseModel<InventoryTransaction>(HttpStatusCode.OK, "Successfully get transactions", dataList: transactions.Items));
            }
        }


        // GET api/v1/inventory/reservation
        [HttpGet("reservation"), MapToApiVersion("1.0")]
        public async Task<IActionResult> GetReservationList([FromQuery] InventoryReservationSearchModel searchModel)
        {
            await ValidateUserAccessAndCredentials<InventoryStock>(_accessControl, _userIdentity);
            if (searchModel.PageNumber == 0 && searchModel.PageSize == 0)
            {
                var reservations = await _inventoryReservationService.GetReservationListAsync(
                    referenceId: searchModel.ReferenceId,
                    catalogTypeId: searchModel.CatalogTypeId,
                    catalogIds: searchModel.CatalogIds,
                    referenceNbr: searchModel.ReferenceNbr,
                    searchReserveOn: searchModel.ReservedOn,
                    showLowStockOnly: searchModel.ShowLowStockOnly,
                    sortByCatalogName: searchModel.SortByCatalogName);
                return Ok(GenerateListResponseModel<InventoryReservation>(HttpStatusCode.OK, "Successfully get transactions", dataList: reservations));
            }
            else
            {
                var reservations = await _inventoryReservationService.GetReservationPagedResultListAsync(
                    referenceId: searchModel.ReferenceId,
                    catalogTypeId: searchModel.CatalogTypeId,
                    catalogIds: searchModel.CatalogIds,
                    referenceNbr: searchModel.ReferenceNbr,
                    searchReserveOn: searchModel.ReservedOn,
                    showLowStockOnly: searchModel.ShowLowStockOnly,
                    sortByCatalogName: searchModel.SortByCatalogName,
                    pageNumber: searchModel.PageNumber, pageSize: searchModel.PageSize);
                return Ok(GenerateListResponseModel<InventoryReservation>(HttpStatusCode.OK, "Successfully get transactions", dataList: reservations.Items));
            }
        }

        // POST api/inventory/reservation
        [HttpPost("reservation"), MapToApiVersion("1.0")]
        public async Task<IActionResult> ReserveStock([FromBody] InventoryReservation req)
        {
            await ValidateUserAccessAndCredentials<InventoryStock>(_accessControl, _userIdentity);

            if (req.Quantity <= 0)
                return BadRequest(GenerateErrorResponseModel(HttpStatusCode.BadRequest, "Cannot release stock with 0 qty or negative qty"));

            if (req.ReferenceId.IsNullOrEmpty())
                return BadRequest(GenerateErrorResponseModel(HttpStatusCode.BadRequest, "ReferenceId cannot be empty"));

            var currentUser = await _userIdentity.GetCurrentApiUserAsync(HttpContext.User);

            var reservation = await _inventoryManager.ReserveStockAsync(
                req.ReferenceId,
                req.ReferenceNbr,
                req.CatalogId,
                req.WarehouseId,
                req.Quantity,
                currentUser.Id);

            return Created();
        }

        // GET api/v1/inventory/reservation/{reservationId}
        [HttpGet("reservation/{reservationId:guid}"), MapToApiVersion("1.0")]
        public async Task<IActionResult> GetReservation(Guid reservationId)
        {
            await ValidateUserAccessAndCredentials<InventoryStock>(_accessControl, _userIdentity);

            if (reservationId.IsNullOrEmpty())
                return BadRequest(GenerateErrorResponseModel(HttpStatusCode.BadRequest, "ReservationId cannot be empty"));

            var reservation = await _inventoryReservationService.GetByIdAsync(reservationId);
            if (reservation is null)
                return NotFound($"Reservation {reservationId} not found.");

            return Ok(GenerateResponseModel<InventoryReservation>(HttpStatusCode.OK, "Successfully get reservation", reservation));
        }

        // POST api/v1/inventory/reservation/fulfill
        [HttpPost("reservation/fulfill")]
        public async Task<IActionResult> FulfillReservation([FromBody] InventoryReservation req)
        {
            await ValidateUserAccessAndCredentials<InventoryStock>(_accessControl, _userIdentity);

            if (req.Id.IsNullOrEmpty())
                return BadRequest(GenerateErrorResponseModel(HttpStatusCode.BadRequest, "ReservationId is invalid"));

            var reservation = await _inventoryReservationService.GetByIdAsync(req.Id);
            if (reservation == null)
                return NotFound($"Reservation {req.Id} not found.");

            var stock = await _inventoryStockService.GetByIdAsync(reservation.InventoryStockId);
            if (stock == null)
                return NotFound($"Stock {reservation.InventoryStockId} not found.");

            var currentUser = await _userIdentity.GetCurrentApiUserAsync(HttpContext.User);

            await _inventoryManager.FulfillReservationAsync(stock, reservation.Quantity, currentUser.Id);

            return NoContent();
        }
    }
}
