using RA.WebServiceEndpoints.Domain;

namespace RA.WebServiceEndpoints.Services
{
    public interface IWebServiceEndpointService
    {
        Task Delete(WebServiceEndpoint endpoint);
        Task<WebServiceEndpoint> GetById(Guid id);
        Task<WebServiceEndpoint> GetEndpointByEndpointName(string endpointName);
        Task<IEnumerable<WebServiceEndpoint>> GetList(string searchQuery = null, DateTime? createdOn = null);
        Task Insert(WebServiceEndpoint endpoint);
        Task Update(WebServiceEndpoint endpoint);
    }
}