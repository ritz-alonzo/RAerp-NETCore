using RA.Core.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RA.WebServiceEndpoints.Domain
{
    public class WebServiceEndpoint : BaseEntity
    {
        public string EndpointName { get; set; }
        public string EndpointDomain { get; set; }
        public string EndpointDescription { get; set; }
        public Guid? EndpointEntityTypeId { get; set; }
        public Guid CreatedById { get; set; }
        public Guid? ModifiedById { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public DateTime? DeletedOn { get; set; }
        public bool Deleted { get; set; }
    }
}
