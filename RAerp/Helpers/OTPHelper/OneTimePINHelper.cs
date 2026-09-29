namespace RAerp.Helpers.OTPHelper
{
    public static class OneTimePINHelper
    {
        #region OTP Generation
        public static string GenerateOTP()
        {
            return new Random().Next(000001, 999999).ToString();
        }

        public static string GenerateOTP(string smsApiKey)
        {
            var generatedOTP = "";
            if (string.IsNullOrEmpty(smsApiKey))
                throw new ArgumentNullException(nameof(smsApiKey));

            generatedOTP = new Random().Next(000001, 999999).ToString();

            return generatedOTP;
        }
        #endregion
    }
}
