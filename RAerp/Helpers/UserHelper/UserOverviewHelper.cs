using RA.Core.Models.OverviewModels;
using RAerp.Domain.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RAerp.Helpers.UserHelper
{
    public static class UserOverviewHelper
    {
        public static UserOverviewModel PrepareUserOverviewModel(User user)
        {
            if (user == null)
            {
                return new UserOverviewModel();
            }

            var model = new UserOverviewModel()
            {
                Id = user.Id,
                FirstName= user.FirstName,
                LastName= user.LastName,
                Username = user.Username,
                FullName = user.FirstName + " " + user.LastName
            };

            return model;
        }
    }
}
