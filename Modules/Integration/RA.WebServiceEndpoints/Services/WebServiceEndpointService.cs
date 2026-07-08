using Microsoft.EntityFrameworkCore;
using RA.Data.App_Data;
using RA.WebFramework.Extensions;
using RA.WebServiceEndpoints.App_Data;
using RA.WebServiceEndpoints.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RA.WebServiceEndpoints.Services
{
    public class WebServiceEndpointService : IWebServiceEndpointService
    {
        private readonly RAWebServiceEndpointContext _context;
        private readonly DbSet<WebServiceEndpoint> _webServiceEndpoint;

        public WebServiceEndpointService(RAWebServiceEndpointContext context)
        {
            _context = context;
            _webServiceEndpoint = _context.Set<WebServiceEndpoint>();
        }

        #region CRUD

        public virtual async Task<WebServiceEndpoint> GetById(Guid id)
        {
            return await _webServiceEndpoint.FirstOrDefaultAsync(c => c.Id == id);
        }

        public virtual async Task<WebServiceEndpoint> GetEndpointByEndpointName(string endpointName)
        {
            return await _webServiceEndpoint.FirstOrDefaultAsync(c => c.EndpointName.ToLower() == endpointName.ToLower());
        }

        public virtual async Task<IEnumerable<WebServiceEndpoint>> GetList(
            string searchQuery = null,
            DateTime? createdOn = null)
        {
            var query = _webServiceEndpoint.AsQueryable();

            if (!string.IsNullOrEmpty(searchQuery))
                query = query.Where(c =>
                c.EndpointName.ToLower().Contains(searchQuery.ToLower()) ||
                c.EndpointDomain.ToLower().Contains(searchQuery.ToLower()));

            if (createdOn.HasValue)
            {
                createdOn = createdOn.ConvertUTCToLocalDateTime();
                query = query.Where(c => c.CreatedOn.ConvertToUTC() >= createdOn.Value);
            }

            query = query.OrderBy(c => c.CreatedOn);

            return await query.ToListAsync();
        }

        public virtual async Task Insert(WebServiceEndpoint endpoint)
        {
            endpoint.CreatedOn = DateTime.UtcNow;
            await _webServiceEndpoint.AddAsync(endpoint);
            await _context.SaveChangesAsync();
        }

        public virtual async Task Update(WebServiceEndpoint endpoint)
        {
            endpoint.ModifiedOn = DateTime.UtcNow;
            _webServiceEndpoint.Update(endpoint);
            await _context.SaveChangesAsync();
        }

        public virtual async Task Delete(WebServiceEndpoint endpoint)
        {
            endpoint.DeletedOn = DateTime.UtcNow;
            endpoint.Deleted = true;
            _webServiceEndpoint.Update(endpoint);
            await _context.SaveChangesAsync();
        }

        #endregion

    }
}
