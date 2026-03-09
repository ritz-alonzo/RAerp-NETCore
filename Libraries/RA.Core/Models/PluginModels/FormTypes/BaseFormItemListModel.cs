using RA.Core.Models.UserInfaceModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RA.Core.Models.PluginModels.FormTypes
{
    public class BaseFormItemListModel<TItemModel>
        where TItemModel : BaseFormItemModel
    {
        public BaseFormItemListModel()
        {
            Items = new List<TItemModel>();
            UserInterface = new UserInterfaceAccessModel();
            FormSettings = new FormSettingsModel();
        }
        public List<TItemModel> Items { get; set; }
        public long TotalItems { get; set; }
        public long PageNumber { get; set; }
        public int PageSize { get; set; }
        public UserInterfaceAccessModel UserInterface { get; set; }
        // form settings
        public FormSettingsModel FormSettings { get; set; }
    }
}
