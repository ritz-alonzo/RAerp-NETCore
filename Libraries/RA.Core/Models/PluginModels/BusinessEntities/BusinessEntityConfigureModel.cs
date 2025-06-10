using Microsoft.AspNetCore.Mvc.Rendering;
using RA.Core.Models.PluginModels.EntityTypes;
using System;
using System.Collections.Generic;
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

        public List<Guid> MappedCategoryTypeIds { get; set; }
        public List<SelectListItem> AvailableCategoryTypes { get; set; }
    }
}
