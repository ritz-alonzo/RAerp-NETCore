using RA.Core.Models.PluginModels.EntityTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RA.Core.Models.PluginModels.BusinessEntities
{
    public class BusinessEntitySearchModel : BaseEntitySearchModel
    {
        public BusinessEntitySearchModel()
        {
            BusinessEntities = new BusinessEntityListModel();
        }
        // will add more search parameters/fields for the List

        public BusinessEntityListModel BusinessEntities { get; set; }
    }
}
