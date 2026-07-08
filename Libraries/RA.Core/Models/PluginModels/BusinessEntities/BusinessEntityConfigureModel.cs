using Microsoft.AspNetCore.Mvc.Rendering;
using RA.Core.Models.PluginModels.EntityTypes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RA.Core.Models.PluginModels.BusinessEntities
{
    public class BusinessEntityConfigureModel : BaseEntityConfigureModel
    {
        public BusinessEntityConfigureModel()
        {
            AvailableCategoryTypes = new List<SelectListItem>();
        }
        [DisplayName("User mapping to Business Entity Enabled")]
        public bool IsUserMappingEnabled { get; set; }
        public List<Guid> MappedCategoryTypeIds { get; set; }
        public List<SelectListItem> AvailableCategoryTypes { get; set; }
    }
}
