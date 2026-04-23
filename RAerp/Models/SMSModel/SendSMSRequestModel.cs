namespace RAerp.Models.SMSModel
{
    public class SendSMSRequestModel
    {
        public string SMSAPIKey { get; set; }
        public string Number { get; set; }
        public string Message { get; set; }
        public string SenderName { get; set; }
    }
}
