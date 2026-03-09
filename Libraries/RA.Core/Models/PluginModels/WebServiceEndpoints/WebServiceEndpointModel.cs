using Microsoft.AspNetCore.Mvc.Rendering;
using RA.Core.Models.BaseModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RA.Core.Models.PluginModels.WebServiceEndpoints
{
    public class WebServiceEndpointModel : BaseModel
    {
        public WebServiceEndpointModel()
        {
            AvailableEntityTypes = new List<SelectListItem>();
        }
        public string WebServiceEndpointSystemName { get; set; }
        [DisplayName("Endpoint Name")]
        public string EndpointName { get; set; }
        [DisplayName("Domain")]
        public string EndpointDomain { get; set; }
        [DisplayName("Description")]
        public string EndpointDescription { get; set; }
        public Guid CreatedById { get; set; }
        [DisplayName("Created Date")]
        public DateTime CreatedOn { get; set; }
        [DisplayName("Modified Date")]
        public DateTime? ModifiedOn { get; set; }
        public DateTime? DeletedOn { get; set; }
        public bool Deleted { get; set; }
        [DisplayName("Entity Type")]
        public Guid? EndpointEntityTypeId { get; set; }
        [DisplayName("Entity Type")]
        public string EndpointEntityTypeName { get; set; }
        public List<SelectListItem> AvailableEntityTypes { get; set; }
        public bool IsMappingVisible { get; set; }
    }
}
