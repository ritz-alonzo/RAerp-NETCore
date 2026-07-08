using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RA.WebServiceEndpoints.Models
{
    public class WebServiceEndpointLoginResponseModel
    {
        public string Status { get; set; }
        public string Token { get; set; }
        public string RefreshToken { get; set; }
    }
}
