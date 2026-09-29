using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RAerp.Models.EmailModel;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Mail;
using System.Text;

namespace RAerp.Services.EmailServices
{
    public class EmailService : IEmailService
    {
        private readonly SmtpSettings _smtp;
        private readonly BrevoEmailSettings _brevoSettings;

        private static readonly HttpClient _httpClient = new HttpClient();

        public EmailService(IOptions<SmtpSettings> smtp, 
            IOptions<BrevoEmailSettings> brevoSettings)
        {
            _smtp = smtp.Value;
            _brevoSettings = brevoSettings.Value;
        }

        public async Task SendEmailAsync(
            string toEmail,
            string subject,
            string htmlMessage)
        {
            using var client = new SmtpClient(_smtp.Host, _smtp.Port)
            {
                UseDefaultCredentials = false,
                EnableSsl = _smtp.EnableSsl,
                Credentials = new NetworkCredential(
                    _smtp.Username,
                    _smtp.Password)
            };

            var message = new MailMessage
            {
                From = new MailAddress(
                    _smtp.SenderEmail,
                    _smtp.SenderName),
                Subject = subject,
                Body = htmlMessage,
                IsBodyHtml = true
            };

            message.To.Add(toEmail);

            await client.SendMailAsync(message);
        }

        public async Task<BrevoEmailTransactionListModel> GetBrevoEmailTransactionListAsync(string uuid = null, 
            string messageId = null, 
            string receipientEmail = null, 
            DateTime? startDate = null, 
            DateTime? endDate = null)
        {

            BrevoEmailTransactionListModel emailTransactionList = new BrevoEmailTransactionListModel();

            var queryParams = new Dictionary<string, string>();
            if (!string.IsNullOrEmpty(uuid))
            {
                queryParams.Add("uuid", uuid);
            }
            else
            {
                if (!string.IsNullOrEmpty(messageId))
                {
                    queryParams.Add("messageId", messageId);
                }

                if (!string.IsNullOrEmpty(receipientEmail))
                {
                    queryParams.Add("email", receipientEmail);
                }

                if (startDate.HasValue)
                {
                    queryParams.Add("startDate", startDate.Value.ToString("yyyy-MM-dd"));
                }

                if (endDate.HasValue)
                {
                    queryParams.Add("endDate", endDate.Value.ToString("yyyy-MM-dd"));
                }
            }

            var fullUrl = QueryHelpers.AddQueryString(_brevoSettings.GetApiUrl, queryParams);

            var request = new HttpRequestMessage(HttpMethod.Get, fullUrl);
            request.Headers.Add("api-key", _brevoSettings.ApiKey);
            request.Headers.Add("Accept", "application/json");

            try
            {
                HttpResponseMessage response = await _httpClient.SendAsync(request);
                if (response != null && response.StatusCode == HttpStatusCode.OK)
                {
                    emailTransactionList = JsonConvert.DeserializeObject<BrevoEmailTransactionListModel>(await response.Content.ReadAsStringAsync());
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error occurred while retrieving email transaction list from Brevo.", ex);
            }

            return emailTransactionList;
        }

        public async Task<BrevoEmailResponseModel> SendBrevoEmailAsync(string recipientEmail, string receipientName, string subject, string htmlContent)
        {
            if (string.IsNullOrEmpty(_brevoSettings.ApiKey))
            {
                throw new InvalidOperationException("Brevo API key is not configured.");
            }

            if (string.IsNullOrEmpty(receipientName) || string.IsNullOrEmpty(recipientEmail))
            {
                throw new ArgumentException("Recipient name and email are required.");
            }

            if (string.IsNullOrEmpty(subject))
            {
                subject = "RAerp Notification";
            }

            if (string.IsNullOrEmpty(htmlContent)) 
            {
                htmlContent = 
                    "<h2>No email content available.</h2>" +
                    "<br/>" +
                    "<p>This message is auto-generated, please do not reply to it.</p>" +
                    "<br/>" + 
                    "<p>Best regards,<br/>RAerp Team</p>";
            }

            BrevoEmailRequestModel emailRequest = new BrevoEmailRequestModel
            {
                Sender = new BrevoSenderRequestModel
                {
                    Name = _brevoSettings.Name,
                    Email = _brevoSettings.Sender
                },
                To = new List<BrevoReceiverRequestModel>
                {
                    new BrevoReceiverRequestModel
                    {
                        Email = recipientEmail,
                        Name = receipientName
                    }
                },
                Subject = subject,
                HtmlContent = htmlContent
            };

            var request = new HttpRequestMessage(HttpMethod.Post, _brevoSettings.PostApiUrl);

            request.Headers.Add("api-key", _brevoSettings.ApiKey);
            request.Headers.Add("Accept", "application/json");

            HttpContent content = new StringContent(JsonConvert.SerializeObject(emailRequest), Encoding.UTF8, "application/json");
            request.Content = content;
            try
            {
                BrevoEmailResponseModel responseModel = new BrevoEmailResponseModel();
                HttpResponseMessage response = await _httpClient.SendAsync(request);
                if (response != null && response.StatusCode == HttpStatusCode.Created)
                {
                    response.EnsureSuccessStatusCode();
                    responseModel = JsonConvert.DeserializeObject<BrevoEmailResponseModel>(await response.Content.ReadAsStringAsync());
                }
                return responseModel;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error occurred while sending email via Brevo.", ex);
            }
        }
    }
}
