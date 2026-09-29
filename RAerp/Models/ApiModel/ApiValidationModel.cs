using System.Net;

namespace RAerp.Models.ApiModel
{
    public class ApiValidationModel
    {
        public bool IsPassed { get; set; } = true;
        public string Message { get; set; }
        public HttpStatusCode StatusCode { get; set; }
    }
}
