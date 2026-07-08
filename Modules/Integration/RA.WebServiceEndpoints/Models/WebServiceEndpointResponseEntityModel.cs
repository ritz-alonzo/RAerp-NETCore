using RA.Core.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RA.WebServiceEndpoints.Models
{
    public class WebServiceEndpointResponseEntityModel<TEntity> 
        where TEntity : BaseEntity
    {
        public string Status { get; set; }
        public TEntity Data { get; set; }
        public string Message { get; set; }
        public bool Success { get; set; }
    }
}
