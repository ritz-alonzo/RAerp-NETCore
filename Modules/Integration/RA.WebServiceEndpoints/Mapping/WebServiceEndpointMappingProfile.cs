using AutoMapper;
using RA.WebServiceEndpoints.Data;
using RA.WebServiceEndpoints.Domain;
using RA.Core.Models.PluginModels.WebServiceEndpoints;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RA.WebServiceEndpoints.Mapping
{
    public class WebServiceEndpointMappingProfile : Profile
    {
        public WebServiceEndpointMappingProfile()
        {
            #region Web Service Endpoint

            CreateMap<WebServiceEndpoint, WebServiceEndpointModel>();
            CreateMap<WebServiceEndpointModel, WebServiceEndpoint>();

            CreateMap<WebServiceEndpointSetting, WebServiceEndpointConfigureModel>();
            CreateMap<WebServiceEndpointConfigureModel, WebServiceEndpointSetting>();

            #endregion
        }
    }
}
