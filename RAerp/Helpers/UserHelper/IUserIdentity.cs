using Microsoft.AspNetCore.Http;
using RA.Data.Domain.Users;

namespace RAerp.Helpers.UserHelper
{
    public interface IUserIdentity
    {
        User GetCurrentUser(HttpContext httpContext);
        User GetUserDetails(Guid userId);
    }
}