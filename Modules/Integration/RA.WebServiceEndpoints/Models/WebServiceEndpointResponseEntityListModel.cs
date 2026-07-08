using RA.Core.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RA.WebServiceEndpoints.Models
{
    public class WebServiceEndpointResponseEntityListModel<TEntity> where TEntity : BaseEntity
    {
        public WebServiceEndpointResponseEntityListModel()
        {
            Data = new List<TEntity>();
        }
        public string Status { get; set; }
        public List<TEntity> Data { get; set; }
        public string Message { get; set; }
        public bool Success { get; set; }
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    }
}
