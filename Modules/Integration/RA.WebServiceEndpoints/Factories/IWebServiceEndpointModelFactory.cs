using RA.WebServiceEndpoints.Domain;
using RA.Core.Models.PluginModels.WebServiceEndpoints;

namespace RA.WebServiceEndpoints.Factories
{
    public interface IWebServiceEndpointModelFactory
    {
        Task<WebServiceEndpointListModel> PrepareWebServiceEndpointListModelAsync(WebServiceEndpointSearchModel searchModel);
        Task<WebServiceEndpointModel> PrepareWebServiceEndpointModelAsync(WebServiceEndpointModel model, WebServiceEndpoint webServiceEndpoint);
        Task<WebServiceEndpointSearchModel> PrepareWebServiceEndpointSearchModelAsync(WebServiceEndpointSearchModel searchModel, int pageNumber, int pageSize);
    }
}