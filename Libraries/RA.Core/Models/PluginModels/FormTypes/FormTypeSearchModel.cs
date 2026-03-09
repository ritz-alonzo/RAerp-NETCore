using RA.Core.Models.BaseModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RA.Core.Models.PluginModels.FormTypes
{
    public class FormTypeSearchModel : BaseSearchModel
    {
        public FormTypeSearchModel()
        {
            FormTypes = new FormTypeListModel();
        }
        public string FormTypeSystemName { get; set; }
        public string FormTypeName { get; set; }
        public string SearchQuery { get; set; }
        public DateTime? SearchInstalledOn { get; set; }
        public FormTypeListModel FormTypes { get; set; }
    }
}
