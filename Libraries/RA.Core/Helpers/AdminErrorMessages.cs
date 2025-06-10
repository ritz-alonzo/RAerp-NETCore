using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RA.Core.Helpers
{
    public static class AdminErrorMessages
    {
        #region Users
        public const string UserNotExists = "User doesn't exists";
        public const string UserIdNotExists = "User Id doesn't exists";
        public const string UserRoleNotExists = "User role doesn't exists";
        public const string UserRoleIdNotExists = "User role Id doesn't exists";
        #endregion

        #region AccessRights
        public const string AccessRightsNotExists = "AccessRights doesn't exists";
        #endregion
    }
}
