using RA.Core.Models.BaseModels;

namespace RAerp.Models.UsersModel
{
    public class UserRoleSearchModel : BaseSearchModel
    {
        public UserRoleSearchModel()
        {
            UserRoles = new UserRoleListModel();
        }
        public string SystemName { get; set; }
        public string SearchQuery { get; set; }
        public DateTime? SearchCreatedOn { get; set; }
        public UserRoleListModel UserRoles { get; set; }
    }
}
