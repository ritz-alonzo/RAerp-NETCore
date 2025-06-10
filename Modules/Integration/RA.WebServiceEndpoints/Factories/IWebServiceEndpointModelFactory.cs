using RA.WebServiceEndpoints.Domain;
using RA.Core.Models.PluginModels.WebServiceEndpoints;

namespace RA.WebServiceEndpoints.Factories
{
    public interface IWebServiceEndpointModelFactory
    {
        Task<WebServiceEndpointListModel> PrepareWebServiceEndpointListModel(WebServiceEndpointSearchModel searchModel);
        Task<WebServiceEndpointModel> PrepareWebServiceEndpointModel(WebServiceEndpointModel model, WebServiceEndpoint webServiceEndpoint);
        Task<WebServiceEndpointSearchModel> PrepareWebServiceEndpointSearchModel(WebServiceEndpointSearchModel searchModel, int pageNumber, int pageSize);
    }
}