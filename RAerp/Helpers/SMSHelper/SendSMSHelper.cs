using Newtonsoft.Json;
using RAerp.Domain.Users;
using RAerp.Helpers.OTPHelper;
using RAerp.Helpers.PublicAPIEndpoints;
using RAerp.Models.SMSModel;
using System;
using System.Threading.Tasks;

namespace RAerp.Helpers.SMSHelper
{
    public static class SendSMSHelper
    {
        private static readonly HttpClient _httpClient = new HttpClient();

        #region SendSMS

        public static async Task<User> SendSMSOTPRequest(User user, string smsApiKey)
        {
            if (string.IsNullOrEmpty(smsApiKey))
                throw new ArgumentNullException(nameof(smsApiKey));

            var generatedOTP = OneTimePINHelper.GenerateOTP(smsApiKey);

            var requestModel = new SendSMSRequestModel()
            {
                SMSAPIKey = smsApiKey,
                Number = user?.ContactNo,
                Message = $"Your OTP is: {generatedOTP}",
                SenderName = "RAerp"
            };
            var payLoad = JsonConvert.SerializeObject(requestModel);
            var content = new StringContent(payLoad, System.Text.Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(APIEndpointConstants.SMSAPIEndpoint, content);
            var responseContent = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
                // Handle error response
                throw new Exception($"Failed to send SMS: {responseContent}");

            user.OneTimePIN = generatedOTP;
            user.IsVerified = false;

            return user;
        }

        #endregion
    }
}
