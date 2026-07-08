using Microsoft.AspNetCore.Http;
using RA.Data.Domain.Users;
using System.Security.Claims;

namespace RAerp.Helpers.UserHelper
{
    public interface IUserIdentity
    {
        Task<User> GetCurrentUserAsync(HttpContext httpContext);
        Task<User> GetUserDetailsAsync(Guid userId);
        Task<User> GetCurrentApiUserAsync(ClaimsPrincipal user);
    }
}