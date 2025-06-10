using RA.Core.Models.OverviewModels;
using RA.Core.Models.PortableViewModels;
using RA.Core.Models.UserInfaceModels;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace RA.Core.Models.BaseModels
{
    public class BaseModel
    {
        public BaseModel()
        {
            CreatedByUser = new UserOverviewModel();
            UserInterface = new UserInterfaceAccessModel();
            PluginComponents = new List<PluginViewComponentModel>();
        }
        [Key]
        public Guid Id { get; set; }
        public string JSNotificationFunction { get; set; }
        public string NotificationMessage { get; set; }
        [DisplayName("Created By")]
        public UserOverviewModel CreatedByUser { get; set; }
        public UserInterfaceAccessModel UserInterface { get; set; }
        public List<PluginViewComponentModel> PluginComponents { get; set; }
    }
}
