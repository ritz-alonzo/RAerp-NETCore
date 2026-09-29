namespace RAerp.Models.EmailModel
{
    public class BrevoEmailSettings
    {
        public string ApiKey { get; set; }
        public string Sender { get; set; }
        public string Name { get; set; }
        public string PostApiUrl { get; set; }
        public string GetApiUrl { get; set; }
    }
    public class BrevoEmailRequestModel
    {
        public BrevoSenderRequestModel Sender { get; set; } = new BrevoSenderRequestModel();
        public List<BrevoReceiverRequestModel> To { get; set; } = new List<BrevoReceiverRequestModel>();
        public string Subject { get; set; }
        public string HtmlContent { get; set; }
    }

    public class BrevoSenderRequestModel
    {
        public string Name { get; set; }
        public string Email { get; set; }
    }

    public class BrevoReceiverRequestModel
    {
        public string Name { get; set; }
        public string Email { get; set; }
    }

    public class BrevoEmailResponseModel
    {
        public string MessageId { get; set; }
    }

    // Transaction List
    public class BrevoEmailTransactionListModel
    {
        public int Count { get; set; }
        public List<BrevoEmailTransactionModel> TransactionalEmails { get; set; } = new List<BrevoEmailTransactionModel>();
    }

    public class BrevoEmailTransactionModel
    {
        public string UuId { get; set; }
        public string MessageId { get; set; }
        public string Subject { get; set; }
        public string Email { get; set; }
        public string TemplateId { get; set; }
        public DateTime Date { get; set; }
    }
}
