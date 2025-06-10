using RA.Core.Models.BaseModels;
using System;

namespace RAerp.Models.UsersModel
{
    public class UserSearchModel : BaseSearchModel
    {
        public UserSearchModel()
        {
            Users = new UserListModel();
        }
        public string SystemName { get; set; }
        public string SearchQuery { get; set; }
        public DateTime? SearchCreatedOn { get; set; }
        public UserListModel Users { get; set; }
    }
}
