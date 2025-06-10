using RA.Core.Models.BaseModels;
using RAerp.Models.UsersModel;

namespace RAerp.Models.AccessRightsControlModel
{
    public class AccessRightsListModel : BaseAdminListModel<AccessRightsModel>
    {
        public AccessRightsListModel() 
        {
            UserRoles = new List<UserRoleModel>();
            AccessRightsRecords = new List<AccessRecordModel>();
        }
        public List<UserRoleModel> UserRoles { get; set; }
        public List<AccessRecordModel> AccessRightsRecords { get; set; }
    }
}
