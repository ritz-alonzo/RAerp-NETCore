using Asp.Versioning;
using AutoMapper;
using Azure.Core;
using Humanizer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using RA.Core.PluginData.Inventory;
using RA.Inventory.Domain;
using RA.Inventory.DTO.InventoryReservation;
using RA.Inventory.DTO.InventoryStock;
using RA.Inventory.DTO.InventoryTransaction;
using RA.Inventory.Models;
using RA.Inventory.Services.InventoryReservationServices;
using RA.Inventory.Services.InventoryServices;
using RA.Inventory.Services.InventoryStockServices;
using RA.Inventory.Services.InventoryTransactionServices;
using RA.WebFramework.Extensions;
using RAerp.Controllers.Admin;
using RAerp.Domain.Application;
using RAerp.Helpers.UserHelper;
using RAerp.Models.ApiModel;
using RAerp.Security.AccessRightsControl;
using RAerp.Services.ApplicationSettingServices;
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
    [Authorize]
    public class InventoryAPIController : AdminApiController
    {
        #region Constants
        private readonly IInventoryManager _inventoryManager;
        private readonly IUserIdentity _userIdentity;
        private readonly IAccessControl _accessControl;
        private readonly IInventoryStockService _inventoryStockService;
        private readonly IInventoryReservationService _inventoryReservationService;
        private readonly IInventoryTransactionService _inventoryTransactionService;
        private readonly IApplicationSettingService _applicationSettingService;
        private readonly ApplicationSetting _applicationSetting;
        private readonly IMapper _mapper;
        #endregion

        #region Ctor
        public InventoryAPIController(IInventoryManager inventoryManager,
            IUserIdentity userIdentity,
            IAccessControl accessControl,
            IInventoryStockService inventoryStockService,
            IInventoryReservationService inventoryReservationService,
            IInventoryTransactionService inventoryTransactionService,
            IApplicationSettingService applicationSettingService,
            IMapper mapper)
        {
            _inventoryManager = inventoryManager;
            _userIdentity = userIdentity;
            _accessControl = accessControl;
            _inventoryStockService = inventoryStockService;
            _inventoryReservationService = inventoryReservationService;
            _inventoryTransactionService = inventoryTransactionService;
            _applicationSettingService = applicationSettingService;
            _applicationSetting = _applicationSettingService.GetCurrentApplicationSettingAsync()?.Result;
            _mapper = mapper;
        }
        #endregion

        #region Inventory Stock
        // GET api/v1/inventory/stock
        [HttpGet("stock"), MapToApiVersion("1.0")]
        public async Task<IActionResult> GetAllStockList([FromQuery] InventoryStockQueryRequestDto searchModel)
        {
            ApiValidationModel validationModel = await ValidateUserAccessAndCredentials<InventoryStock>(_accessControl, _userIdentity, _applicationSetting);
            if (validationModel != null && validationModel.IsPassed == false)
                return StatusCode((int)validationModel.StatusCode, GenerateErrorResponseModel(validationModel.StatusCode, validationModel.Message));

            var stockList = await _inventoryStockService.GetStockPagedResultListAsync(
                    catalogTypeId: searchModel.CatalogTypeId,
                    catalogIds: searchModel.CatalogIds,
                    warehouseIds: searchModel.WarehouseIds,
                    searchCreatedOn: searchModel.CreatedOn,
                    showLowStockOnly: searchModel.ShowLowStockOnly,
                    sortByCatalogName: searchModel.SortByCatalogName,
                    pageNumber: searchModel.PageNumber, pageSize: searchModel.PageSize);

            List<InventoryStockResponseDto> stockResponseList = new List<InventoryStockResponseDto>();
            if (stockList.Items.Any())
            {
                stockResponseList = stockList.Items.Select(stock =>
                {
                    InventoryStockResponseDto stockResponse = _mapper.Map<InventoryStockResponseDto>(stock);
                    return stockResponse;

                }).ToList();
            }
            return Ok(GenerateListResponseModel(HttpStatusCode.OK, "Successfully get stock of catalog and warehouse", dataList: stockResponseList));
        }

        // GET api/v1/inventory/stock/catalog/{catalogTypeId}
        [HttpGet("stock/catalog/{catalogTypeId:guid}"), MapToApiVersion("1.0")]
        public async Task<IActionResult> GetCatalogStockList(Guid catalogTypeId)
        {
            ApiValidationModel validationModel = await ValidateUserAccessAndCredentials<InventoryStock>(_accessControl, _userIdentity, _applicationSetting);
            if (validationModel != null && validationModel.IsPassed == false)
                return StatusCode((int)validationModel.StatusCode, GenerateErrorResponseModel(validationModel.StatusCode, validationModel.Message));

            if (catalogTypeId.IsNullOrEmpty())
                return BadRequest(GenerateErrorResponseModel(HttpStatusCode.BadRequest, "CatalogId is invalid"));

            var stockList = await _inventoryStockService.GetStockListAsync(catalogTypeId);

            List<InventoryStockResponseDto> stockResponseList = new List<InventoryStockResponseDto>();
            if (stockList.Any())
            {
                stockResponseList = stockList.Select(stock =>
                {
                    InventoryStockResponseDto stockResponse = _mapper.Map<InventoryStockResponseDto>(stock);
                    return stockResponse;

                }).ToList();
            }

            return Ok(GenerateListResponseModel(HttpStatusCode.OK, "Successfully get stock of catalog and warehouse", dataList: stockResponseList));
        }

        // GET api/v1/inventory/stock/warehouse/{catalogTypeId}/{warehouseId}
        [HttpGet("stock/warehouse/{catalogTypeId:guid}/{warehouseId:guid}"), MapToApiVersion("1.0")]
        public async Task<IActionResult> GetWarehouseStockList(Guid catalogTypeId, Guid warehouseId)
        {
            ApiValidationModel validationModel = await ValidateUserAccessAndCredentials<InventoryStock>(_accessControl, _userIdentity, _applicationSetting);
            if (validationModel != null && validationModel.IsPassed == false)
                return StatusCode((int)validationModel.StatusCode, GenerateErrorResponseModel(validationModel.StatusCode, validationModel.Message));

            if (catalogTypeId.IsNullOrEmpty())
                return BadRequest(GenerateErrorResponseModel(HttpStatusCode.BadRequest, "CatalogId is invalid"));

            if (warehouseId.IsNullOrEmpty())
                return BadRequest(GenerateErrorResponseModel(HttpStatusCode.BadRequest, "WarehouseId is invalid"));

            var stockList = await _inventoryStockService.GetStockListByWarehouseId(catalogTypeId, warehouseId);

            List<InventoryStockResponseDto> stockResponseList = new List<InventoryStockResponseDto>();
            if (stockList.Any())
            {
                stockResponseList = stockList.Select(stock =>
                {
                    InventoryStockResponseDto stockResponse = _mapper.Map<InventoryStockResponseDto>(stock);
                    return stockResponse;

                }).ToList();
            }

            return Ok(GenerateListResponseModel(HttpStatusCode.OK, "Successfully get stock of warehouse", dataList: stockResponseList));
        }

        // GET api/v1/inventory/stock/{catalogId}
        [HttpGet("stock/{catalogId:guid}"), MapToApiVersion("1.0")]
        public async Task<IActionResult> GetStockByCatalogId(Guid catalogId)
        {
            ApiValidationModel validationModel = await ValidateUserAccessAndCredentials<InventoryStock>(_accessControl, _userIdentity, _applicationSetting);
            if (validationModel != null && validationModel.IsPassed == false)
                return StatusCode((int)validationModel.StatusCode, GenerateErrorResponseModel(validationModel.StatusCode, validationModel.Message));

            var stock = await _inventoryStockService.GetByCatalogIdAsync(catalogId);
            if (stock is null)
                return NotFound($"Stock for catalog {catalogId} not found.");

            InventoryStockResponseDto stockResponse = _mapper.Map<InventoryStockResponseDto>(stock);

            return Ok(GenerateResponseModel(HttpStatusCode.OK, "Successfully get stock of catalog", stockResponse));
        }

        // GET api/v1/inventory/stock/{catalogId}/{warehouseId}
        [HttpGet("stock/{catalogId:guid}/{warehouseId:guid}"), MapToApiVersion("1.0")]
        public async Task<IActionResult> GetStockByCatalogIdAndWarehouseId(Guid catalogId, Guid warehouseId)
        {
            ApiValidationModel validationModel = await ValidateUserAccessAndCredentials<InventoryStock>(_accessControl, _userIdentity, _applicationSetting);
            if (validationModel != null && validationModel.IsPassed == false)
                return StatusCode((int)validationModel.StatusCode, GenerateErrorResponseModel(validationModel.StatusCode, validationModel.Message));

            var stock = await _inventoryStockService.GetByCatalogIdAndWarehouseIdAsync(catalogId, warehouseId);
            if (stock is null)
                return NotFound($"Stock for catalog {catalogId} and warehouse {warehouseId} not found.");

            InventoryStockResponseDto stockResponse = _mapper.Map<InventoryStockResponseDto>(stock);

            return Ok(GenerateResponseModel(HttpStatusCode.OK, "Successfully get stock of catalog and warehouse", stockResponse));
        }


        #endregion

        #region Inventory Transactions
        // POST api/v1/inventory/stock/initialize
        [HttpPost("stock/initialize"), MapToApiVersion("1.0")]
        public async Task<IActionResult> InitializeStock([FromBody] InventoryStockRequestDto req)
        {
            ApiValidationModel validationModel = await ValidateUserAccessAndCredentials<InventoryStock>(_accessControl, _userIdentity, _applicationSetting);
            if (validationModel != null && validationModel.IsPassed == false)
                return StatusCode((int)validationModel.StatusCode, GenerateErrorResponseModel(validationModel.StatusCode, validationModel.Message));

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

            InventoryStockResponseDto stockResponse = _mapper.Map<InventoryStockResponseDto>(result);

            return Ok(GenerateResponseModel(HttpStatusCode.OK, "Successfully initialize stock", stockResponse));
        }

        // POST api/v1/inventory/stock/receive
        [HttpPost("stock/receive"), MapToApiVersion("1.0")]
        public async Task<IActionResult> ReceiveStock([FromBody] InventoryTransactionRequestDto req)
        {
            ApiValidationModel validationModel = await ValidateUserAccessAndCredentials<InventoryStock>(_accessControl, _userIdentity, _applicationSetting);
            if (validationModel != null && validationModel.IsPassed == false)
                return StatusCode((int)validationModel.StatusCode, GenerateErrorResponseModel(validationModel.StatusCode, validationModel.Message));

            if (req.Quantity <= 0)
                return BadRequest(GenerateErrorResponseModel(HttpStatusCode.BadRequest, "Cannot receive stock with 0 qty or negative qty"));

            if (req.ReferenceId.IsNullOrEmpty())
                return BadRequest(GenerateErrorResponseModel(HttpStatusCode.BadRequest, "ReferenceId cannot be empty"));

            var currentUser = await _userIdentity.GetCurrentApiUserAsync(HttpContext.User);

            if (!Enum.IsDefined(typeof(TransactionType), req.TransactionTypeId))
                return BadRequest("Transaction type is invalid");

            await _inventoryManager.ReceiveStockAsync(
                req.ReferenceId.Value,
                req.ReferenceNbr,
                req.CatalogId,
                req.WarehouseId,
                req.Quantity,
                currentUser.Id,
                req.Note,
                ((TransactionType)req.TransactionTypeId));

            return Created();
        }

        // POST api/inventory/stock/release
        [HttpPost("stock/release"), MapToApiVersion("1.0")]
        public async Task<IActionResult> ReleaseStock([FromBody] InventoryTransactionRequestDto req)
        {
            ApiValidationModel validationModel = await ValidateUserAccessAndCredentials<InventoryStock>(_accessControl, _userIdentity, _applicationSetting);
            if (validationModel != null && validationModel.IsPassed == false)
                return StatusCode((int)validationModel.StatusCode, GenerateErrorResponseModel(validationModel.StatusCode, validationModel.Message));

            if (req.Quantity <= 0)
                return BadRequest(GenerateErrorResponseModel(HttpStatusCode.BadRequest, "Cannot release stock with 0 qty or negative qty"));

            if (req.ReferenceId.IsNullOrEmpty())
                return BadRequest(GenerateErrorResponseModel(HttpStatusCode.BadRequest, "ReferenceId cannot be empty"));

            var currentUser = await _userIdentity.GetCurrentApiUserAsync(HttpContext.User);

            if (!Enum.IsDefined(typeof(TransactionType), req.TransactionTypeId))
                return BadRequest("Transaction type is invalid");

            await _inventoryManager.ReleaseStockAsync(
                req.ReferenceId.Value,
                req.ReferenceNbr,
                req.CatalogId,
                req.WarehouseId,
                req.Quantity,
                currentUser.Id,
                req.Note,
                ((TransactionType)req.TransactionTypeId));

            return Created();
        }

        // POST api/v1/inventory/stock/adjust
        [HttpPost("stock/adjust"), MapToApiVersion("1.0")]
        public async Task<IActionResult> ManualAdjustment([FromBody] InventoryTransactionRequestDto req)
        {
            ApiValidationModel validationModel = await ValidateUserAccessAndCredentials<InventoryStock>(_accessControl, _userIdentity, _applicationSetting);
            if (validationModel != null && validationModel.IsPassed == false)
                return StatusCode((int)validationModel.StatusCode, GenerateErrorResponseModel(validationModel.StatusCode, validationModel.Message));

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
        public async Task<IActionResult> GetTransactionList([FromQuery] InventoryTransactionQueryRequestDto searchModel)
        {
            ApiValidationModel validationModel = await ValidateUserAccessAndCredentials<InventoryStock>(_accessControl, _userIdentity, _applicationSetting);
            if (validationModel != null && validationModel.IsPassed == false)
                return StatusCode((int)validationModel.StatusCode, GenerateErrorResponseModel(validationModel.StatusCode, validationModel.Message));

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

            List<InventoryTransactionResponseDto> transactionResponseList = new List<InventoryTransactionResponseDto>();
            if (transactions.Items.Any())
            {
                transactionResponseList = transactions.Items.Select(tran =>
                {
                    InventoryTransactionResponseDto transactionResponse = _mapper.Map<InventoryTransactionResponseDto>(tran);
                    return transactionResponse;

                }).ToList();
            }

            return Ok(GenerateListResponseModel(HttpStatusCode.OK, "Successfully get transactions", dataList: transactionResponseList));
        }


        // GET api/v1/inventory/reservation
        [HttpGet("reservation"), MapToApiVersion("1.0")]
        public async Task<IActionResult> GetReservationList([FromQuery] InventoryReservationQueryRequestDto searchModel)
        {
            ApiValidationModel validationModel = await ValidateUserAccessAndCredentials<InventoryStock>(_accessControl, _userIdentity, _applicationSetting);
            if (validationModel != null && validationModel.IsPassed == false)
                return StatusCode((int)validationModel.StatusCode, GenerateErrorResponseModel(validationModel.StatusCode, validationModel.Message));

            var reservations = await _inventoryReservationService.GetReservationPagedResultListAsync(
                    referenceId: searchModel.ReferenceId,
                    catalogTypeId: searchModel.CatalogTypeId,
                    catalogIds: searchModel.CatalogIds,
                    referenceNbr: searchModel.ReferenceNbr,
                    searchReserveOn: searchModel.ReservedOn,
                    showLowStockOnly: searchModel.ShowLowStockOnly,
                    sortByCatalogName: searchModel.SortByCatalogName,
                    pageNumber: searchModel.PageNumber, pageSize: searchModel.PageSize);

            List<InventoryReservationResponseDto> reservationResponseList = new List<InventoryReservationResponseDto>();
            if (reservations.Items.Any())
            {
                reservationResponseList = reservations.Items.Select(reservation =>
                {
                    InventoryReservationResponseDto reservationResponse = _mapper.Map<InventoryReservationResponseDto>(reservation);
                    return reservationResponse;

                }).ToList();
            }

            return Ok(GenerateListResponseModel(HttpStatusCode.OK, "Successfully get transactions", dataList: reservationResponseList));
        }

        // POST api/inventory/reservation
        [HttpPost("reservation"), MapToApiVersion("1.0")]
        public async Task<IActionResult> ReserveStock([FromBody] InventoryReservationRequestDto req)
        {
            ApiValidationModel validationModel = await ValidateUserAccessAndCredentials<InventoryStock>(_accessControl, _userIdentity, _applicationSetting);
            if (validationModel != null && validationModel.IsPassed == false)
                return StatusCode((int)validationModel.StatusCode, GenerateErrorResponseModel(validationModel.StatusCode, validationModel.Message));

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
            ApiValidationModel validationModel = await ValidateUserAccessAndCredentials<InventoryStock>(_accessControl, _userIdentity, _applicationSetting);
            if (validationModel != null && validationModel.IsPassed == false)
                return StatusCode((int)validationModel.StatusCode, GenerateErrorResponseModel(validationModel.StatusCode, validationModel.Message));

            if (reservationId.IsNullOrEmpty())
                return BadRequest(GenerateErrorResponseModel(HttpStatusCode.BadRequest, "ReservationId cannot be empty"));

            var reservation = await _inventoryReservationService.GetByIdAsync(reservationId);
            if (reservation is null)
                return NotFound($"Reservation {reservationId} not found.");

            InventoryReservationResponseDto reservationResponse = _mapper.Map<InventoryReservationResponseDto>(reservation);

            return Ok(GenerateResponseModel(HttpStatusCode.OK, "Successfully get reservation", reservationResponse));
        }

        // POST api/v1/inventory/reservation/fulfill
        [HttpPost("reservation/fulfill")]
        public async Task<IActionResult> FulfillReservation([FromBody] InventoryReservationRequestDto req)
        {
            ApiValidationModel validationModel = await ValidateUserAccessAndCredentials<InventoryStock>(_accessControl, _userIdentity, _applicationSetting);
            if (validationModel != null && validationModel.IsPassed == false)
                return StatusCode((int)validationModel.StatusCode, GenerateErrorResponseModel(validationModel.StatusCode, validationModel.Message));

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

        #endregion
    }
}
