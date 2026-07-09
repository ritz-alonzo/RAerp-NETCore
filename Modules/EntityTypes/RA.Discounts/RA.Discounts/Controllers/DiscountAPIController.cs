using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RA.Data.Domain.Application;
using RA.Data.Domain.EntityTypes;
using RA.Discounts.Domain;
using RA.Discounts.Models;
using RA.Discounts.Services;
using RA.EntityTypes.Services;
using RA.WebFramework.Extensions;
using RAerp.Controllers.Admin;
using RAerp.Helpers.UserHelper;
using RAerp.Security.AccessRightsControl;
using RAerp.Services.ApplicationSettingServices;
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
        #endregion

        #region Ctor
        public DiscountAPIController(IDiscountService discountService,
            IMapper mapper,
            IEntityTypeManager entityTypeManager,
            IAccessControl accessControl,
            IUserIdentity userIdentity,
            IApplicationSettingService applicationSettingService)
        {
            _discountService = discountService;
            _mapper = mapper;
            _entityTypeManager = entityTypeManager;
            _accessControl = accessControl;
            _userIdentity = userIdentity;
            _discountEntityType = _entityTypeManager.GetTypeBySystemNameAsync(typeof(Discount).FullName).Result;
            _applicationSettingService = applicationSettingService;
            _applicationSetting = _applicationSettingService.GetCurrentApplicationSettingAsync()?.Result;
        }
        #endregion

        #region Discount
        [HttpGet, MapToApiVersion("1.0")]
        public async Task<IActionResult> GetDiscountList([FromQuery] DiscountSearchModel searchModel)
        {
            await ValidateUserAccessAndCredentials<Discount>(_accessControl, _userIdentity, _applicationSetting);

            if (_discountEntityType == null)
                return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "Discount entity type not yet installed or configured"));

            if (searchModel.PageNumber == 0 && searchModel.PageSize == 0)
            {
                var discountList = await _discountService.GetDiscountListAsync();
                return Ok(GenerateListResponseModel<Discount>(HttpStatusCode.OK, "Successfully get Discount list", dataList: discountList));
            }
            else
            {
                var discountList = await _discountService.GetDiscountPagedResultListAsync(
                    searchQuery: searchModel.SearchQuery,
                    discountTypeIds: searchModel.DiscountTypeIds,
                    discountScopeIds: searchModel.DiscountScopeIds,
                    createdOn: searchModel.SearchCreatedOn,
                    createdById: searchModel.CreatedById,
                    showActiveDiscountsOnly: searchModel.ShowActiveDiscountsOnly,
                    pageNumber: searchModel.PageNumber,
                    pageSize: searchModel.PageSize);
                return Ok(GenerateListResponseModel<Discount>(HttpStatusCode.OK, "Successfully get Discount list", dataList: discountList.Items));
            }
        }

        [HttpPost, MapToApiVersion("1.0")]
        public async Task<IActionResult> CreateDiscount([FromBody] Discount discount)
        {
            await ValidateUserAccessAndCredentials<Discount>(_accessControl, _userIdentity, _applicationSetting);

            if (_discountEntityType == null)
                return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "Discount entity type not yet installed or configured"));

            var currentUser = await _userIdentity.GetCurrentApiUserAsync(HttpContext.User);

            discount.CreatedById = currentUser.Id;
            await _discountService.InsertAsync(discount);
            // Map Dto here

            return Ok(GenerateResponseModel<Discount>(HttpStatusCode.OK, "Successfully get Discount list", discount));
        }

        [HttpPut, MapToApiVersion("1.0")]
        public async Task<IActionResult> UpdateDiscount([FromBody] Discount discount)
        {
            await ValidateUserAccessAndCredentials<Discount>(_accessControl, _userIdentity, _applicationSetting);

            if (_discountEntityType == null)
                return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "Discount entity type not yet installed or configured"));

            if (discount == null)
                return BadRequest(GenerateErrorResponseModel(HttpStatusCode.BadRequest, "Discount body doesn't have value"));

            var currentUser = await _userIdentity.GetCurrentApiUserAsync(HttpContext.User);

            discount.ModifiedById = currentUser.Id;
            await _discountService.UpdateAsync(discount);

            return Ok(GenerateResponseModel<Discount>(HttpStatusCode.OK, "Successfully updated Discount", discount));
        }

        [HttpDelete("{discountId:guid}"), MapToApiVersion("1.0")]
        public async Task<IActionResult> DeleteDiscount(Guid discountId)
        {
            await ValidateUserAccessAndCredentials<Discount>(_accessControl, _userIdentity, _applicationSetting);

            if (_discountEntityType == null)
                return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "Discount entity type not yet installed or configured"));

            if (discountId.IsNullOrEmpty())
                return BadRequest(GenerateErrorResponseModel(HttpStatusCode.BadRequest, "Invalid discount Id"));

            var discount = await _discountService.GetByIdAsync(discountId);
            await _discountService.DeleteAsync(discount);

            return Ok(GenerateResponseModel<Discount>(HttpStatusCode.OK, "Successfully deleted Discount", discount));
        }
        #endregion

        #region Discount Redemption
        [HttpGet("redemption"), MapToApiVersion("1.0")]
        public async Task<IActionResult> GetDiscountRedemptionList()
        {
            await ValidateUserAccessAndCredentials<Discount>(_accessControl, _userIdentity, _applicationSetting);

            if (_discountEntityType == null)
                return NotFound(GenerateResponseModel<Discount>(HttpStatusCode.NotFound, "Discount entity type not yet installed or configured"));

            var discountRedemptionList = await _discountService.GetDiscountRedemptionListAsync();

            return Ok(GenerateListResponseModel<DiscountRedemption>(HttpStatusCode.OK, "Successfully get discount redemption list", dataList: discountRedemptionList));
        }

        [HttpPost("redemption"), MapToApiVersion("1.0")]
        public async Task<IActionResult> CreateDiscountRedemption([FromBody] DiscountRedemption discountRedemption)
        {
            await ValidateUserAccessAndCredentials<Discount>(_accessControl, _userIdentity, _applicationSetting);

            if (_discountEntityType == null)
                return NotFound(GenerateResponseModel<Discount>(HttpStatusCode.NotFound, "Discount entity type not yet installed or configured"));

            if (discountRedemption == null)
                return BadRequest(GenerateErrorResponseModel(HttpStatusCode.BadRequest, "Invalid discount redemption"));

            await _discountService.CreateDiscountRedemptionAsync(discountRedemption);

            return Ok(GenerateResponseModel<DiscountRedemption>(HttpStatusCode.OK, "Successfully redeemed discount", discountRedemption));
        }

        [HttpPut("redemption"), MapToApiVersion("1.0")]
        public async Task<IActionResult> UpdateDiscountRedemption([FromBody] DiscountRedemption discountRedemption)
        {
            await ValidateUserAccessAndCredentials<Discount>(_accessControl, _userIdentity, _applicationSetting);

            if (_discountEntityType == null)
                return NotFound(GenerateResponseModel<Discount>(HttpStatusCode.NotFound, "Discount entity type not yet installed or configured"));

            if (discountRedemption == null)
                return BadRequest(GenerateErrorResponseModel(HttpStatusCode.BadRequest, "Invalid discount redemption"));

            await _discountService.UpdateDiscountRedemptionAsync(discountRedemption);

            return Ok(GenerateResponseModel<DiscountRedemption>(HttpStatusCode.OK, "Successfully updated redeemed discount", discountRedemption));
        }

        [HttpDelete("redemption/{discountRedemptionId:guid}"), MapToApiVersion("1.0")]
        public async Task<IActionResult> DeleteDiscountRedemption(Guid discountRedemptionId)
        {
            await ValidateUserAccessAndCredentials<Discount>(_accessControl, _userIdentity, _applicationSetting);

            if (_discountEntityType == null)
                return NotFound(GenerateResponseModel<Discount>(HttpStatusCode.NotFound, "Discount entity type not yet installed or configured"));

            if (discountRedemptionId.IsNullOrEmpty())
                return BadRequest(GenerateErrorResponseModel(HttpStatusCode.BadRequest, "Invalid discount Id"));

            var discountRedemption = await _discountService.GetDiscountRedemptionByIdAsync(discountRedemptionId);
            if (discountRedemption == null)
                return BadRequest(GenerateErrorResponseModel(HttpStatusCode.BadRequest, "Discount redemption doesn't exists"));

            await _discountService.DeleteDiscountRedemptionAsync(discountRedemption);

            return Ok(GenerateResponseModel<DiscountRedemption>(HttpStatusCode.OK, "Successfully deleted redeemed discount", discountRedemption));

        }
        #endregion
    }
}
