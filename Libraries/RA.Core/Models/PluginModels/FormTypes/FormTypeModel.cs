using RA.Core.Models.BaseModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RA.Core.Models.PluginModels.FormTypes
{
    public class FormTypeModel : BaseModel
    {
        public string FormTypeName { get; set; }
        public string FormTypeSystemName { get; set; }
        public string PluginController { get; set; }
        public string PluginConfigurationUrl { get; set; }
        public bool ModalConfigureEnabled { get; set; }
        public bool Installed { get; set; }
    }
}
