using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RA.Core.Models.PluginModels.OrdersManagement.Payments;
using RA.OrdersManagement.Domain.Payments;
using RA.OrdersManagement.Services.Payments;
using RA.WebFramework.Extensions;
using RAerp.Controllers.Admin;
using RAerp.Helpers.UserHelper;
using RAerp.Security.AccessRightsControl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace RA.OrdersManagement.Controllers.Payments
{
    [ApiController]
    [Route("api/v{version:apiVersion}/payments")]
    [Authorize]
    [ApiVersion("1.0")]
    public class PaymentsAPIController : AdminApiController
    {
        #region Constants
        private readonly IAccessControl _accessControl;
        private readonly IUserIdentity _userIdentity;
        private readonly IPaymentService _paymentService;
        #endregion

        #region Ctor
        public PaymentsAPIController(IAccessControl accessControl,
            IUserIdentity userIdentity,
            IPaymentService paymentService)
        {
            _accessControl = accessControl;
            _userIdentity = userIdentity;
            _paymentService = paymentService;
        }
        #endregion

        #region Version 1.0

        #region Payment
        [HttpGet, MapToApiVersion("1.0")]
        public async Task<IActionResult> GetPaymentList([FromQuery] PaymentSearchModel paymentSearchModel)
        {
            await ValidateUserAccessAndCredentials<Payment>(_accessControl, _userIdentity);

            var paymentList = await _paymentService.GetPaymentListAsync(
                searchQuery: paymentSearchModel?.SearchQuery,
                searchPaymentRefNbr: paymentSearchModel?.SearchPaymentRefNbr,
                searchPaymentOrderNbr: paymentSearchModel?.SearchOrderNbr,
                searchPaymentDate: paymentSearchModel.SearchPaymentDate,
                searchCreatedDate: paymentSearchModel.SearchCreatedOn,
                paymentStatusIds: paymentSearchModel.SearchPaymentStatusId > 0 ? new List<int> { paymentSearchModel.SearchPaymentStatusId } : null,
                formStatusIds: paymentSearchModel.SearchStatusId > 0 ? new List<int> { paymentSearchModel.SearchStatusId } : null,
                showDeleted: paymentSearchModel.ShowDeleted,
                pageNumber: paymentSearchModel.PageNumber,
                pageSize: paymentSearchModel.PageSize
            );

            return Ok(GenerateListResponseModel<Payment>(HttpStatusCode.OK, "Successful", paymentSearchModel.PageNumber, paymentSearchModel.PageSize, paymentList));
        }

        [HttpGet("{idOrFormNbr}"), MapToApiVersion("1.0")]
        public async Task<IActionResult> GetPaymentByIdOrFormNbr(string idOrFormNbr)
        {
            if (!string.IsNullOrEmpty(idOrFormNbr))
                return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "FormId or FormNbr doesn't have value"));

            await ValidateUserAccessAndCredentials<Payment>(_accessControl, _userIdentity);

            Payment payment = null;
            if (Guid.TryParse(idOrFormNbr, out Guid result))
                payment = await _paymentService.GetFormByIdAsync(result);
            else
                payment = await _paymentService.GetFormByFormNbrAsync(idOrFormNbr);

            if (payment == null)
                return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "Payment doesn't exists"));

            return Ok(GenerateResponseModel<Payment>(HttpStatusCode.OK, "Successful", payment));
        }

        [HttpPost, MapToApiVersion("1.0")]
        public async Task<IActionResult> CreatePayment([FromBody] Payment payment)
        {
            if (payment == null)
                return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "Payment is empty"));

            await ValidateUserAccessAndCredentials<Payment>(_accessControl, _userIdentity);

            var currentUser = await _userIdentity.GetCurrentApiUserAsync(HttpContext.User);

            payment.CreatedById = currentUser.Id;
            await _paymentService.InsertFormAsync(payment);

            return Ok(GenerateResponseModel<Payment>(HttpStatusCode.OK, "Successfully Created Payment", payment));
        }

        [HttpPut, MapToApiVersion("1.0")]
        public async Task<IActionResult> UpdatePayment([FromBody] Payment payment)
        {
            if (payment == null)
                return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "Payment is empty"));

            await ValidateUserAccessAndCredentials<Payment>(_accessControl, _userIdentity);

            var currentUser = await _userIdentity.GetCurrentApiUserAsync(HttpContext.User);

            payment.ModifiedById = currentUser?.Id;
            await _paymentService.UpdateFormAsync(payment);

            return Ok(GenerateResponseModel<Payment>(HttpStatusCode.OK, "Successfully Updated Payment", payment));
        }

        [HttpDelete("{idOrFormNbr}"), MapToApiVersion("1.0")]
        public async Task<IActionResult> DeletePayment(string idOrFormNbr)
        {
            await ValidateUserAccessAndCredentials<Payment>(_accessControl, _userIdentity);

            Payment payment = null;
            if (Guid.TryParse(idOrFormNbr, out Guid id))
                payment = await _paymentService.GetFormByIdAsync(id);
            else
                payment = await _paymentService.GetFormByFormNbrAsync(idOrFormNbr);

            if (payment == null)
                return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "Payment doesn't exists"));

            var currentUser = await _userIdentity.GetCurrentApiUserAsync(HttpContext.User);

            payment.ModifiedById = currentUser?.Id;
            await _paymentService.DeleteFormAsync(payment);

            return Ok(GenerateResponseModel<Payment>(HttpStatusCode.OK, "Successfully Deleted Payment"));
        }
        #endregion

        #region Payment Item
        [HttpGet("item/{formId}"), MapToApiVersion("1.0")]
        public async Task<IActionResult> GetPaymentItemList(string formId)
        {
            await ValidateUserAccessAndCredentials<Payment>(_accessControl, _userIdentity);

            List<PaymentItem> paymentItemList = new List<PaymentItem>();
            if (Guid.TryParse(formId, out Guid result))
            {
                if (result.IsNotNullOrEmpty())
                    paymentItemList = _paymentService.GetItemsByFormIdAsync(result).Result.ToList();
            }

            return Ok(GenerateListResponseModel<PaymentItem>(HttpStatusCode.OK, "Succesful", dataList: paymentItemList));
        }

        [HttpPost("item"), MapToApiVersion("1.0")]
        public async Task<IActionResult> InsertPaymentItem([FromBody] List<PaymentItem> paymentItems)
        {
            await ValidateUserAccessAndCredentials<Payment>(_accessControl, _userIdentity);

            if (!paymentItems.Any())
                return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "Payment item doesn't have value"));

            var currentUser = await _userIdentity.GetCurrentApiUserAsync(HttpContext.User);

            foreach (var item in paymentItems)
            {
                if (item.FormId.IsNullOrEmpty())
                    return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "FormId is invalid"));

                item.CreatedById = currentUser.Id;
                await _paymentService.InsertItemAsync(item, saveChangesToDb: true);
            }

            return Ok(GenerateListResponseModel<PaymentItem>(HttpStatusCode.OK, "Successfully added payment items", dataList: paymentItems));
        }

        [HttpPut("item"), MapToApiVersion("1.0")]
        public async Task<IActionResult> UpdatePaymentItem([FromBody] List<PaymentItem> paymentItems)
        {
            await ValidateUserAccessAndCredentials<Payment>(_accessControl, _userIdentity);

            if (!paymentItems.Any())
                return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "Payment item doesn't have value"));

            var currentUser = await _userIdentity.GetCurrentApiUserAsync(HttpContext.User);

            foreach (var item in paymentItems)
            {
                if (item.FormId.IsNullOrEmpty())
                    return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "FormId is invalid"));

                item.ModifiedById = currentUser?.Id;
                await _paymentService.UpdateItemAsync(item, saveChangesToDb: true);
            }

            return Ok(GenerateListResponseModel<PaymentItem>(HttpStatusCode.OK, "Successfully added payment items", dataList: paymentItems));
        }

        [HttpDelete("item"), MapToApiVersion("1.0")]
        public async Task<IActionResult> DeletePaymentItem([FromBody] List<PaymentItem> paymentItems)
        {
            await ValidateUserAccessAndCredentials<Payment>(_accessControl, _userIdentity);

            if (!paymentItems.Any())
                return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "Payment item doesn't have value"));

            var currentUser = await _userIdentity.GetCurrentApiUserAsync(HttpContext.User);

            foreach (var item in paymentItems)
            {
                if (item.FormId.IsNullOrEmpty())
                    return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "FormId is invalid"));

                item.ModifiedById = currentUser?.Id;
                await _paymentService.DeleteItemAsync(item, saveChangesToDb: true);
            }

            return Ok(GenerateListResponseModel<PaymentItem>(HttpStatusCode.OK, "Successfully added order items", dataList: paymentItems));
        }
        #endregion

        #endregion

        #region Version 2.0
        [HttpGet, MapToApiVersion("2.0")]
        public async Task<IActionResult> GetPaymentListv2([FromQuery] PaymentSearchModel paymentSearchModel)
        {
            await ValidateUserAccessAndCredentials<Payment>(_accessControl, _userIdentity);

            var paymentList = await _paymentService.GetPaymentListAsync(
                searchQuery: paymentSearchModel?.SearchQuery,
                searchPaymentRefNbr: paymentSearchModel?.SearchPaymentRefNbr,
                searchPaymentOrderNbr: paymentSearchModel?.SearchOrderNbr,
                searchPaymentDate: paymentSearchModel.SearchPaymentDate,
                searchCreatedDate: paymentSearchModel.SearchCreatedOn,
                paymentStatusIds: paymentSearchModel.SearchPaymentStatusId > 0 ? new List<int> { paymentSearchModel.SearchPaymentStatusId } : null,
                formStatusIds: paymentSearchModel.SearchStatusId > 0 ? new List<int> { paymentSearchModel.SearchStatusId } : null,
                showDeleted: paymentSearchModel.ShowDeleted,
                pageNumber: paymentSearchModel.PageNumber,
                pageSize: paymentSearchModel.PageSize
            );

            return Ok(GenerateListResponseModel<Payment>(HttpStatusCode.OK, "Successful", paymentSearchModel.PageNumber, paymentSearchModel.PageSize, paymentList));
        }
        #endregion
    }
}
