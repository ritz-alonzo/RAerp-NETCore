using System.Text.Json.Serialization;

namespace RAerp.DTO.Paymongo
{
    public class PaymongoCheckoutRequestDto
    {
        [JsonPropertyName("payload")]
        public PaymongoPayloadRequestDto Payload { get; set; } = new PaymongoPayloadRequestDto();
    }

    public class PaymongoPayloadRequestDto
    {
        [JsonPropertyName("data")]
        public PaymongoCheckoutDataRequestDto Data { get; set; } = new PaymongoCheckoutDataRequestDto();
    }

    public class PaymongoCheckoutDataRequestDto
    {
        [JsonPropertyName("attributes")]
        public PaymongoCheckoutAttributesRequestDto Attributes { get; set; } = new PaymongoCheckoutAttributesRequestDto();
    }

    public class PaymongoCheckoutAttributesRequestDto
    {
        //[JsonPropertyName("amount")]
        //public int Amount { get; set; }
        //[JsonPropertyName("currency")]
        //public string Currency { get; set; } = "PHP";
        //[JsonPropertyName("payment_method_allowed")]
        //public List<string> PaymentMethodAllowed { get; set; } = new List<string> { "card" };
        //[JsonPropertyName("payment_method_options")]
        //public PaymongoPaymentMethodOptionsRequestDto PaymentMethodOptions { get; set; } = new PaymongoPaymentMethodOptionsRequestDto();
        //[JsonPropertyName("redirect_url")]
        //public PaymongoRedirectUrlRequestDto RedirectUrl { get; set; } = new PaymongoRedirectUrlRequestDto();
    }
}
