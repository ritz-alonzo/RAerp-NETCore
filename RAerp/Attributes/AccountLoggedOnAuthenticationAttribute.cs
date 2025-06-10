using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using RAerp.Helpers.Security;

namespace RAerp.Attributes
{
    [AttributeUsage(AttributeTargets.All, Inherited = false)]
    public class AccountLoggedOnAuthenticationAttribute : Attribute, IActionFilter
    {
        public void OnActionExecuted(ActionExecutedContext context)
        {
            
        }

        /// <summary>
        /// When user is not logged on, redirect to Login page
        /// </summary>
        /// <param name="context"></param>
        public void OnActionExecuting(ActionExecutingContext context)
        {
            // will add possible scenarios
            if (!SessionHelper.SessionGenerated(context.HttpContext))
                context.Result = new RedirectResult("/Users/Login");
        }
    }
}
