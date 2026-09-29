using Microsoft.AspNetCore.Http;
using RA.WebFramework.Extensions;
using RAerp.Domain.Users;
using RAerp.Helpers.Security;
using RAerp.Services.UserServices;
using System;
using System.Security.Claims;

namespace RAerp.Helpers.UserHelper
{
    public class UserIdentity : IUserIdentity
    {
        private readonly IUserService _userService;

        public UserIdentity(IUserService userService)
        {
            _userService = userService;
        }

        public async Task<User> GetCurrentUserAsync(HttpContext httpContext)
        {
            User user = null;
            var raerpToken = SessionHelper.RetrieveUserSession(httpContext);
            if (!string.IsNullOrEmpty(raerpToken))
            {
                var userId = Guid.Parse(raerpToken);
                if (userId != Guid.Empty)
                {
                    user = await _userService.GetById(userId);
                }
            }

            return user;
        }

        public async Task<User> GetUserDetailsAsync(Guid userId)
        {
            User user = null;

            if (userId.IsNotNullOrEmpty())
                user = await _userService.GetById(userId);

            return user;
        }

        public async Task<User> GetCurrentApiUserAsync(ClaimsPrincipal user)
        {
            User currentUser = null;
            if (user.Identity is { IsAuthenticated: true })
            {
                var userId = user.Identity.Name;
                if (string.IsNullOrEmpty(userId))
                    return currentUser;

                var convertedUserId = Guid.Parse(userId);
                currentUser = await _userService.GetById(convertedUserId);
            }

            return currentUser;
        }
    }
}
