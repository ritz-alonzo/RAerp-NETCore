using Microsoft.AspNetCore.Mvc.Rendering;
using RA.Core.Models.BaseModels;
using RA.Core.PluginData.FormTypes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RA.Core.Models.PluginModels.FormTypes
{
    public class BaseFormSearchModel : BaseSearchModel
    {
        public BaseFormSearchModel()
        {
            AvailableFormStatus = new List<SelectListItem>();
        }
        public string FormTypeName { get; set; }
        public string FormTypeSystemName { get; set; }
        [DisplayName("Query")]
        public string SearchQuery { get; set; }
        [DisplayName("Show Deleted Records")]
        public bool ShowDeleted { get; set; }
        [DisplayName("Created Date")]
        public DateTime? SearchCreatedOn { get; set; }
        [DisplayName("Status")]
        public int SearchStatusId { get; set; }
        public List<SelectListItem> AvailableFormStatus { get; set; }
    }
}
