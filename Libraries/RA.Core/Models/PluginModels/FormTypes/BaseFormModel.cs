using RA.Core.Models.BaseModels;
using RA.Core.Models.OverviewModels;
using RA.Core.PluginData.FormTypes;
using System.ComponentModel;

namespace RA.Core.Models.PluginModels.FormTypes
{
    public class BaseFormModel : BaseModel
    {
        public BaseFormModel() 
        {
            ApprovedByUser = new UserOverviewModel();
            FormSettings = new FormSettingsModel();
        }

        [DisplayName("Form Name")]
        public string FormTypeName { get; set; }
        public string FormTypeSystemName { get; set; }
        [DisplayName("Form Nbr")]
        public string FormNbr { get; set; }
        [DisplayName("Status")]
        public int StatusId { get; set; }
        public FormStatus Status { get; set; }
        public string Description { get; set; }
        public Guid CreatedById { get; set; }
        public Guid? ModifiedById { get; set; }
        public Guid? ApprovedById { get; set; }
        [DisplayName("Created Date")]
        public DateTime CreatedOn { get; set; }
        [DisplayName("Modified Date")]
        public DateTime? ModifiedOn { get; set; }
        [DisplayName("Approved Date")]
        public DateTime? ApprovedOn { get; set; }
        public DateTime? DeletedOn { get; set; }
        public bool Deleted { get; set; }
        public bool IsNewDoc { get; set; }
        public UserOverviewModel ApprovedByUser { get; set; }
        public FormSettingsModel FormSettings { get; set; }
    }
}
