using RA.Core.Models.OverviewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RA.Core.Models.BaseModels
{
    public class BaseAdminModel
    {
        public BaseAdminModel()
        {
            CreatedByUser = new UserOverviewModel();
        }

        [Key]
        public Guid Id { get; set; }
        public string SystemName { get; set; }
        public string JSNotificationFunction { get; set; }
        public string NotificationMessage { get; set; }
        public UserOverviewModel CreatedByUser { get; set; }
    }
}
