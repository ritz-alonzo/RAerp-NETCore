using Microsoft.AspNetCore.Http;
using RA.Data.Domain.Users;

namespace RAerp.Helpers.UserHelper
{
    public interface IUserIdentity
    {
        Task<User> GetCurrentUserAsync(HttpContext httpContext);
        Task<User> GetUserDetailsAsync(Guid userId);
    }
}