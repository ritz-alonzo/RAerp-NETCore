using Microsoft.AspNetCore.Http;
using RA.Data.Domain.Users;
using RA.WebFramework.Extensions;
using RAerp.Helpers.Security;
using RAerp.Services.UserServices;
using System;

namespace RAerp.Helpers.UserHelper
{
    public class UserIdentity : IUserIdentity
    {
        private readonly IUserService _userService;

        public UserIdentity(IUserService userService)
        {
            _userService = userService;
        }

        public User GetCurrentUser(HttpContext httpContext)
        {
            User user = null;
            var raerpToken = SessionHelper.RetrieveUserSession(httpContext);
            if (!string.IsNullOrEmpty(raerpToken))
            {
                var userId = Guid.Parse(raerpToken);
                if (userId != Guid.Empty)
                {
                    user = _userService.GetById(userId).Result;
                }
            }

            return user;
        }

        public User GetUserDetails(Guid userId)
        {
            User user = null;

            if (userId.IsNotNullOrEmpty())
            {
                user = _userService.GetById(userId).Result;
            }

            return user;
        }
    }
}
