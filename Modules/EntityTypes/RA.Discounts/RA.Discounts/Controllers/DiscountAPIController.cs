using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RA.Discounts.Domain;
using RA.Discounts.DTO.DiscountRedemptions;
using RA.Discounts.DTO.Discounts;
using RA.Discounts.Models;
using RA.Discounts.Services;
using RA.EntityTypes.Services;
using RA.WebFramework.Extensions;
using RAerp.Controllers.Admin;
using RAerp.Domain.Application;
using RAerp.Domain.EntityTypes;
using RAerp.Domain.Users;
using RAerp.Helpers.UserHelper;
using RAerp.Models.ApiModel;
using RAerp.Security.AccessRightsControl;
using RAerp.Services.ApplicationSettingServices;
using RAerp.Services.EntityAttributeServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace RA.Discounts.Controllers
{
    [ApiController]
    [Route("api/v{version:apiVersion}/discounts")]
    [Authorize]
    [ApiVersion("1.0")]
    [ApiVersion("2.0")]
    public class DiscountAPIController : AdminApiController
    {
        #region Constants
        private readonly IDiscountService _discountService;
        private readonly IMapper _mapper;
        private readonly IEntityTypeManager _entityTypeManager;
        private readonly IAccessControl _accessControl;
        private readonly IUserIdentity _userIdentity;
        private readonly EntityType _discountEntityType;
        private readonly IApplicationSettingService _applicationSettingService;
        private readonly ApplicationSetting _applicationSetting;
        private readonly IEntityAttributeService _entityAttributeService;
        #endregion

        #region Ctor
        public DiscountAPIController(IDiscountService discountService,
            IMapper mapper,
            IEntityTypeManager entityTypeManager,
            IAccessControl accessControl,
            IUserIdentity userIdentity,
            IApplicationSettingService applicationSettingService,
            IEntityAttributeService entityAttributeService)
        {
            _discountService = discountService;
            _mapper = mapper;
            _entityTypeManager = entityTypeManager;
            _accessControl = accessControl;
            _userIdentity = userIdentity;
            _discountEntityType = _entityTypeManager.GetTypeBySystemNameAsync(typeof(Discount).FullName).Result;
            _applicationSettingService = applicationSettingService;
            _applicationSetting = _applicationSettingService.GetCurrentApplicationSettingAsync()?.Result;
            _entityAttributeService = entityAttributeService;
        }
        #endregion

        #region Discount
        [HttpGet, MapToApiVersion("1.0")]
        public async Task<IActionResult> GetDiscountList([FromQuery] DiscountQueryRequestDto searchModel)
        {
            ApiValidationModel validationModel = await ValidateUserAccessAndCredentials<Discount>(_accessControl, _userIdentity, _applicationSetting);
            if (validationModel != null && validationModel.IsPassed == false)
                return StatusCode((int)validationModel.StatusCode, GenerateErrorResponseModel(validationModel.StatusCode, validationModel.Message));

            if (_discountEntityType == null)
                return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "Discount entity type not yet installed or configured"));

            List<DiscountResponseDto> discountResponseList = new List<DiscountResponseDto>();
            if (searchModel.PageNumber == 0 && searchModel.PageSize == 0)
            {
                var discountList = await _discountService.GetDiscountListAsync(
                    searchQuery: searchModel.SearchQuery,
                    discountTypeIds: searchModel.SearchDiscountTypeIds,
                    discountScopeIds: searchModel.SearchDiscountScopeIds,
                    createdOn: searchModel.SearchCreatedOn,
                    showActiveDiscountsOnly: searchModel.ShowActiveDiscountsOnly);

                if (discountList.Any())
                {
                    discountResponseList = discountList.Select(disc =>
                    {
                        DiscountResponseDto discountResponse = new DiscountResponseDto();
                        discountResponse = _mapper.Map<DiscountResponseDto>(disc);
                        User createdByUser = _userIdentity.GetUserDetailsAsync(disc.CreatedById).Result;
                        discountResponse.CreatedBy = createdByUser?.FirstName + ' ' + createdByUser?.LastName;
                        User modifiedByUser = disc.ModifiedById.IsNotNullOrEmpty() ? _userIdentity.GetUserDetailsAsync(disc.ModifiedById.Value).Result : null;
                        discountResponse.ModifiedBy = modifiedByUser != null ? modifiedByUser?.FirstName + ' ' + modifiedByUser?.LastName : null;
                        // Attributes
                        discountResponse.Attributes = _entityAttributeService.GetEntityAttributeValueListAsync(entityId: disc.Id).Result.ToList();
                        return discountResponse;

                    }).ToList();
                }

                return Ok(GenerateListResponseModel(HttpStatusCode.OK, "Successfully get Discount list", dataList: discountResponseList));
            }
            else
            {
                var discountList = await _discountService.GetDiscountPagedResultListAsync(
                    searchQuery: searchModel.SearchQuery,
                    discountTypeIds: searchModel.SearchDiscountTypeIds,
                    discountScopeIds: searchModel.SearchDiscountScopeIds,
                    createdOn: searchModel.SearchCreatedOn,
                    showActiveDiscountsOnly: searchModel.ShowActiveDiscountsOnly,
                    pageNumber: searchModel.PageNumber,
                    pageSize: searchModel.PageSize);

                if (discountList.Items.Any())
                {
                    discountResponseList = discountList.Items.Select(disc =>
                    {
                        DiscountResponseDto discountResponse = new DiscountResponseDto();
                        discountResponse = _mapper.Map<DiscountResponseDto>(disc);
                        User createdByUser = _userIdentity.GetUserDetailsAsync(disc.CreatedById).Result;
                        discountResponse.CreatedBy = createdByUser?.FirstName + ' ' + createdByUser?.LastName;
                        User modifiedByUser = disc.ModifiedById.IsNotNullOrEmpty() ? _userIdentity.GetUserDetailsAsync(disc.ModifiedById.Value).Result : null;
                        discountResponse.ModifiedBy = modifiedByUser != null ? modifiedByUser?.FirstName + ' ' + modifiedByUser?.LastName : null;
                        // Attributes
                        discountResponse.Attributes = _entityAttributeService.GetEntityAttributeValueListAsync(entityId: disc.Id).Result.ToList();
                        return discountResponse;

                    }).ToList();
                }

                return Ok(GenerateListResponseModel(HttpStatusCode.OK, "Successfully get Discount list", dataList: discountResponseList));
            }
        }

        [HttpGet("{id:guid}"), MapToApiVersion("1.0")]
        public async Task<IActionResult> GetDiscountById(Guid id)
        {
            ApiValidationModel validationModel = await ValidateUserAccessAndCredentials<Discount>(_accessControl, _userIdentity, _applicationSetting);
            if (validationModel != null && validationModel.IsPassed == false)
                return StatusCode((int)validationModel.StatusCode, GenerateErrorResponseModel(validationModel.StatusCode, validationModel.Message));

            if (_discountEntityType == null)
                return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "Discount entity type not yet installed or configured"));

            Discount discount = await _discountService.GetByIdAsync(id);
            if (discount == null)
                return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "Discount doesn't exists"));

            DiscountResponseDto discountResponse = _mapper.Map<DiscountResponseDto>(discount);

            return Ok(GenerateResponseModel(HttpStatusCode.OK, "Successfully Created Discount", discountResponse));
        }

        [HttpPost, MapToApiVersion("1.0")]
        public async Task<IActionResult> CreateDiscount([FromBody] DiscountRequestDto discountRequest)
        {
            ApiValidationModel validationModel = await ValidateUserAccessAndCredentials<Discount>(_accessControl, _userIdentity, _applicationSetting);
            if (validationModel != null && validationModel.IsPassed == false)
                return StatusCode((int)validationModel.StatusCode, GenerateErrorResponseModel(validationModel.StatusCode, validationModel.Message));

            if (_discountEntityType == null)
                return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "Discount entity type not yet installed or configured"));

            if (discountRequest.ValidFrom != DateTime.MinValue)
                discountRequest.ValidFrom = discountRequest.ValidFrom.ConvertToUTC();
            if (discountRequest.ValidTo != DateTime.MinValue)
                discountRequest.ValidTo = discountRequest.ValidTo.ConvertToUTC();
            discountRequest.UsageCount = 0;

            var currentUser = await _userIdentity.GetCurrentApiUserAsync(HttpContext.User);
            
            Discount discount = _mapper.Map<Discount>(discountRequest);
            discount.CreatedById = currentUser.Id;

            await _discountService.InsertAsync(discount);
            // Map Dto here
            DiscountResponseDto discountResponse = _mapper.Map<DiscountResponseDto>(discount);

            return Ok(GenerateResponseModel(HttpStatusCode.OK, "Successfully Created Discount", discountResponse));
        }

        [HttpPut, MapToApiVersion("1.0")]
        public async Task<IActionResult> UpdateDiscount([FromBody] DiscountRequestDto discountRequest)
        {
            ApiValidationModel validationModel = await ValidateUserAccessAndCredentials<Discount>(_accessControl, _userIdentity, _applicationSetting);
            if (validationModel != null && validationModel.IsPassed == false)
                return StatusCode((int)validationModel.StatusCode, GenerateErrorResponseModel(validationModel.StatusCode, validationModel.Message));

            if (discountRequest.Id.IsNullOrEmpty())
                return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "Discount Id doesn't have value"));

            if (_discountEntityType == null)
                return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "Discount entity type not yet installed or configured"));

            if (discountRequest == null)
                return BadRequest(GenerateErrorResponseModel(HttpStatusCode.BadRequest, "Discount body doesn't have value"));

            Discount existingDiscount = await _discountService.GetByIdAsync(discountRequest.Id);
            if (existingDiscount == null)
                return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "Discount doesn't exists"));

            var currentUser = await _userIdentity.GetCurrentApiUserAsync(HttpContext.User);

            if (discountRequest.ValidFrom != DateTime.MinValue)
                discountRequest.ValidFrom = discountRequest.ValidFrom.ConvertToUTC();
            if (discountRequest.ValidTo != DateTime.MinValue)
                discountRequest.ValidTo = discountRequest.ValidTo.ConvertToUTC();

            Discount discount = _mapper.Map(discountRequest, existingDiscount);
            discount.ModifiedById = currentUser.Id;

            await _discountService.UpdateAsync(discount);

            DiscountResponseDto discountResponse = _mapper.Map<DiscountResponseDto>(discount);

            return Ok(GenerateResponseModel(HttpStatusCode.OK, "Successfully updated Discount", discount));
        }

        [HttpDelete("{discountId:guid}"), MapToApiVersion("1.0")]
        public async Task<IActionResult> DeleteDiscount(Guid discountId)
        {
            ApiValidationModel validationModel = await ValidateUserAccessAndCredentials<Discount>(_accessControl, _userIdentity, _applicationSetting);
            if (validationModel != null && validationModel.IsPassed == false)
                return StatusCode((int)validationModel.StatusCode, GenerateErrorResponseModel(validationModel.StatusCode, validationModel.Message));

            if (_discountEntityType == null)
                return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "Discount entity type not yet installed or configured"));

            if (discountId.IsNullOrEmpty())
                return BadRequest(GenerateErrorResponseModel(HttpStatusCode.BadRequest, "Invalid discount Id"));

            var discount = await _discountService.GetByIdAsync(discountId);
            if (discount == null)
                return BadRequest(GenerateErrorResponseModel(HttpStatusCode.BadRequest, "Discount doesn't exists"));
            
            await _discountService.DeleteAsync(discount);

            return Ok("Successfully deleted Discount");
        }
        #endregion

        #region Discount Redemption
        [HttpGet("redemption"), MapToApiVersion("1.0")]
        public async Task<IActionResult> GetDiscountRedemptionList([FromQuery] DiscountRedemptionQueryRequestDto searchModel)
        {
            ApiValidationModel validationModel = await ValidateUserAccessAndCredentials<Discount>(_accessControl, _userIdentity, _applicationSetting);
            if (validationModel != null && validationModel.IsPassed == false)
                return StatusCode((int)validationModel.StatusCode, GenerateErrorResponseModel(validationModel.StatusCode, validationModel.Message));

            if (_discountEntityType == null)
                return NotFound(GenerateResponseModel<DiscountRedemption>(HttpStatusCode.NotFound, "Discount entity type not yet installed or configured"));

            List<DiscountRedemptionResponseDto> discountRedemptionResponseList = new List<DiscountRedemptionResponseDto>();
            if (searchModel.PageNumber == 0 && searchModel.PageSize == 0)
            {
                var discountRedemptionList = await _discountService.GetDiscountRedemptionListAsync(
                    searchOrderId: searchModel.SearchOrderId,
                    searchOrderNbr: searchModel.SearchOrderNbr,
                    searchDiscountCode: searchModel.SearchDiscountCode,
                    searchCustomerId: searchModel.SearchCustomerId);

                if (discountRedemptionList.Any())
                {
                    discountRedemptionResponseList = discountRedemptionList.Select(disc =>
                    {
                        DiscountRedemptionResponseDto discountRedemptionResponse = new DiscountRedemptionResponseDto();
                        discountRedemptionResponse = _mapper.Map<DiscountRedemptionResponseDto>(disc);
                        return discountRedemptionResponse;

                    }).ToList();
                }
                return Ok(GenerateListResponseModel(HttpStatusCode.OK, "Successfully Get discount redemption list", dataList: discountRedemptionResponseList));
            }
            else
            {
                var discountRedemptionList = await _discountService.GetDiscountRedemptionPagedResultListAsync(
                    searchOrderId: searchModel.SearchOrderId,
                    searchOrderNbr: searchModel.SearchOrderNbr,
                    searchDiscountCode: searchModel.SearchDiscountCode,
                    searchCustomerId: searchModel.SearchCustomerId,
                    pageNumber: searchModel.PageNumber, pageSize: searchModel.PageSize
                );
                if (discountRedemptionList.Items.Any())
                {
                    discountRedemptionResponseList = discountRedemptionList.Items.Select(disc =>
                    {
                        DiscountRedemptionResponseDto discountRedemptionResponse = new DiscountRedemptionResponseDto();
                        discountRedemptionResponse = _mapper.Map<DiscountRedemptionResponseDto>(disc);
                        return discountRedemptionResponse;

                    }).ToList();
                }
                return Ok(GenerateListResponseModel(HttpStatusCode.OK, "Successfully Get discount redemption list", dataList: discountRedemptionResponseList));
            }
        }

        [HttpPost("redemption"), MapToApiVersion("1.0")]
        public async Task<IActionResult> CreateDiscountRedemption([FromBody] DiscountRedemptionRequestDto discountRedemptionRequest)
        {
            ApiValidationModel validationModel = await ValidateUserAccessAndCredentials<Discount>(_accessControl, _userIdentity, _applicationSetting);
            if (validationModel != null && validationModel.IsPassed == false)
                return StatusCode((int)validationModel.StatusCode, GenerateErrorResponseModel(validationModel.StatusCode, validationModel.Message));

            if (_discountEntityType == null)
                return NotFound(GenerateResponseModel<DiscountRedemption>(HttpStatusCode.NotFound, "Discount entity type not yet installed or configured"));

            if (discountRedemptionRequest == null)
                return BadRequest(GenerateErrorResponseModel(HttpStatusCode.BadRequest, "Invalid discount redemption"));

            DiscountRedemption discRedemption = _mapper.Map<DiscountRedemption>(discountRedemptionRequest);
            await _discountService.CreateDiscountRedemptionAsync(discRedemption);

            DiscountRedemptionResponseDto discountRedemptionResponse = _mapper.Map<DiscountRedemptionResponseDto>(discRedemption);

            return Ok(GenerateResponseModel(HttpStatusCode.OK, "Successfully redeemed discount", discountRedemptionResponse));
        }

        [HttpPut("redemption"), MapToApiVersion("1.0")]
        public async Task<IActionResult> UpdateDiscountRedemption([FromBody] DiscountRedemptionRequestDto discountRedemptionRequest)
        {
            ApiValidationModel validationModel = await ValidateUserAccessAndCredentials<Discount>(_accessControl, _userIdentity, _applicationSetting);
            if (validationModel != null && validationModel.IsPassed == false)
                return StatusCode((int)validationModel.StatusCode, GenerateErrorResponseModel(validationModel.StatusCode, validationModel.Message));

            if (discountRedemptionRequest.Id.IsNullOrEmpty())
                return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "Invalid Id"));

            if (_discountEntityType == null)
                return NotFound(GenerateResponseModel<DiscountRedemption>(HttpStatusCode.NotFound, "Discount entity type not yet installed or configured"));

            if (discountRedemptionRequest == null)
                return BadRequest(GenerateErrorResponseModel(HttpStatusCode.BadRequest, "Invalid discount redemption"));

            DiscountRedemption existingDiscRedemption = await _discountService.GetDiscountRedemptionByIdAsync(discountRedemptionRequest.Id);
            if (existingDiscRedemption == null)
                return NotFound(GenerateResponseModel<DiscountRedemption>(HttpStatusCode.NotFound, "Discount entity type not yet installed or configured"));

            DiscountRedemption discRedemption = _mapper.Map(discountRedemptionRequest, existingDiscRedemption);

            await _discountService.UpdateDiscountRedemptionAsync(discRedemption);

            DiscountRedemptionResponseDto discountRedemptionResponse = _mapper.Map<DiscountRedemptionResponseDto>(discRedemption);

            return Ok(GenerateResponseModel(HttpStatusCode.OK, "Successfully updated redeemed discount", discountRedemptionResponse));
        }

        [HttpDelete("redemption/{discountRedemptionId:guid}"), MapToApiVersion("1.0")]
        public async Task<IActionResult> DeleteDiscountRedemption(Guid discountRedemptionId)
        {
            ApiValidationModel validationModel = await ValidateUserAccessAndCredentials<Discount>(_accessControl, _userIdentity, _applicationSetting);
            if (validationModel != null && validationModel.IsPassed == false)
                return StatusCode((int)validationModel.StatusCode, GenerateErrorResponseModel(validationModel.StatusCode, validationModel.Message));

            if (_discountEntityType == null)
                return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "Discount entity type not yet installed or configured"));

            if (discountRedemptionId.IsNullOrEmpty())
                return BadRequest(GenerateErrorResponseModel(HttpStatusCode.BadRequest, "Invalid discount Id"));

            var discountRedemption = await _discountService.GetDiscountRedemptionByIdAsync(discountRedemptionId);
            if (discountRedemption == null)
                return BadRequest(GenerateErrorResponseModel(HttpStatusCode.BadRequest, "Discount redemption doesn't exists"));

            await _discountService.DeleteDiscountRedemptionAsync(discountRedemption);

            return Ok("Successfully deleted redeemed discount");

        }
        #endregion
    }
}
