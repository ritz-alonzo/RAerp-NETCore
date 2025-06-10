using RA.WebServiceEndpoints.Domain;
using RAerp.Security.AccessRights;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RA.WebServiceEndpoints.Data.Security
{
    public class WebServiceEndpointAccessRightsRecord
    {
        #region CRUD
        public static AccessRecord ViewWebServiceEndpoint = new AccessRecord
        {
            Name = $"View {nameof(WebServiceEndpoint)}",
            Classification = nameof(WebServiceEndpoint),
            SystemName = typeof(WebServiceEndpoint).FullName + $".View{nameof(WebServiceEndpoint)}",
            AccessRecordType = AccessType.View
        };
        public static AccessRecord CreateWebServiceEndpoint = new AccessRecord
        {
            Name = $"Create {nameof(WebServiceEndpoint)}",
            Classification = nameof(WebServiceEndpoint),
            SystemName = typeof(WebServiceEndpoint).FullName + $".Create{nameof(WebServiceEndpoint)}",
            AccessRecordType = AccessType.Create
        };
        public static AccessRecord UpdateWebServiceEndpoint = new AccessRecord
        {
            Name = $"Update {nameof(WebServiceEndpoint)}",
            Classification = nameof(WebServiceEndpoint),
            SystemName = typeof(WebServiceEndpoint).FullName + $".Update{nameof(WebServiceEndpoint)}",
            AccessRecordType = AccessType.Update
        };
        public static AccessRecord DeleteWebServiceEndpoint = new AccessRecord
        {
            Name = $"Delete {nameof(WebServiceEndpoint)}",
            Classification = nameof(WebServiceEndpoint),
            SystemName = typeof(WebServiceEndpoint).FullName + $".Delete{nameof(WebServiceEndpoint)}",
            AccessRecordType = AccessType.Delete
        };
        #endregion
    }
}
