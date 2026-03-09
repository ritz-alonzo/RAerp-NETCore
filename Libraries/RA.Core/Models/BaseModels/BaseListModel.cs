using RA.Core.Models.PluginModels.FormTypes;
using RA.Core.Models.UserInfaceModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RA.Core.Models.BaseModels
{
    public class BaseListModel<TModel> 
        where TModel : BaseModel
    {
        public BaseListModel()
        {
            Items = new List<TModel>();
            UserInterface = new UserInterfaceAccessModel();
            FormSettings = new FormSettingsModel();
        }
        public List<TModel> Items { get; set; }
        public long TotalItems { get; set; }
        public long PageNumber { get; set; }
        public int PageSize { get; set; }
        public UserInterfaceAccessModel UserInterface { get; set; }
        // form settings
        public FormSettingsModel FormSettings { get; set; } 
    }
}
