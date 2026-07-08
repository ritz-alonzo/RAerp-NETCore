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
            SearchStatusIds = new List<int>();
        }
        // will add more search parameters/fields for the List
        public List<int> SearchStatusIds { get; set; }
        public BusinessEntityListModel BusinessEntities { get; set; }
    }
}
