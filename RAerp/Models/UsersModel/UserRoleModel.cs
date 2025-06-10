using RA.Core.Models.BaseModels;
using System.ComponentModel;

namespace RAerp.Models.UsersModel
{
    public class UserRoleModel : BaseAdminModel
    {
        [DisplayName("Role")]
        public string Rolename { get; set; }
        [DisplayName("Created On")]
        public DateTime CreatedOn { get; set; }
        [DisplayName("Modified On")]
        public DateTime? ModifiedOn { get; set; }
        [DisplayName("Deleted")]
        public bool Deleted { get; set; }
    }
}
