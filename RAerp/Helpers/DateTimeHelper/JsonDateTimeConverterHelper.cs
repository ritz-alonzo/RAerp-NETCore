using RAerp.Services.ApplicationSettingServices;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RAerp.Helpers.DateTimeHelper
{
    /// <summary>
    /// Convert Incoming/Outgoing DateTime to UTC 
    /// </summary>
    public class JsonDateTimeConverterHelper : JsonConverter<DateTime>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public JsonDateTimeConverterHelper(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            => reader.GetDateTime();

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            var utcValue = value.Kind == DateTimeKind.Utc
                ? value
                : DateTime.SpecifyKind(value, DateTimeKind.Utc);

            // Resolve the setting fresh, per request — not cached at converter construction time
            var applicationSettingService = _httpContextAccessor.HttpContext?
                .RequestServices
                .GetService<IApplicationSettingService>();

            var defaultTimeZone = applicationSettingService?
                .GetCurrentApplicationSettingAsync().Result?.DefaultTimeZone
                ?? "Singapore Standard Time"; // fallback if service/setting unavailable

            var tz = TimeZoneInfo.FindSystemTimeZoneById(defaultTimeZone);
            var convertedValue = TimeZoneInfo.ConvertTimeFromUtc(utcValue, tz);

            writer.WriteStringValue(convertedValue);
        }
    }
}
