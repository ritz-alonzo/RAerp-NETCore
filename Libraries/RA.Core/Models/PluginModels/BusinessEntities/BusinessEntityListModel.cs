using RA.Core.Models.BaseModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RA.Core.Models.PluginModels.BusinessEntities
{
    public class BusinessEntityListModel : BaseListModel<BusinessEntityModel>
    {
        public bool RedirectByCodeEnabled { get; set; }
    }
}
