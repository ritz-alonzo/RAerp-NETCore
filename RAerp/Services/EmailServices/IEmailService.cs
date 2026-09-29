using RAerp.Models.EmailModel;

namespace RAerp.Services.EmailServices
{
    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail, string subject, string htmlMessage);
        Task<BrevoEmailTransactionListModel> GetBrevoEmailTransactionListAsync(string uuid = null,
            string messageId = null,
            string receipientEmail = null,
            DateTime? startDate = null,
            DateTime? endDate = null);
        Task<BrevoEmailResponseModel> SendBrevoEmailAsync(string recipientEmail, string receipientName, string subject, string htmlContent);
    }
}