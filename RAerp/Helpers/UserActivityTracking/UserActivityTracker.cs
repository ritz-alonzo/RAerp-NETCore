using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;
using RA.Data.Domain.UserActivityLogs;
using System;
using System.Linq;
using RA.WebFramework.Extensions;
using RA.Data.App_Data;
using RAerp.Services.UserServices;

namespace RAerp.Helpers.UserActivityTracking
{
    public class UserActivityTracker : IActionFilter
    {
        private readonly RAerpContext _erpContext;
        private readonly IUserService _userService;

        public UserActivityTracker(RAerpContext mmsContext, IUserService userService)
        {
            _erpContext = mmsContext;
            _userService = userService;
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            Guid? userId = null;
            var data = "";
            var user = "";

            var routeData = context.RouteData;
            var controller = routeData.Values["controller"];
            var action = routeData.Values["action"];

            var url = $"{controller}/{action}";

            if (!string.IsNullOrEmpty(context.HttpContext.Request.QueryString.Value))
            {
                data = context.HttpContext.Request.QueryString.Value;
            }
            else
            {
                var arguments = context.ActionArguments;

                var value = arguments.FirstOrDefault().Value;

                var convertedValue = JsonConvert.SerializeObject(value);
                data = convertedValue;
            }

            var ipAddress = context.HttpContext.Connection.RemoteIpAddress.ToString();

            var userSession = context.HttpContext.Session.GetString("userIdentity");
            if (userSession != null)
            {
                userId = Guid.Parse(userSession);
                if (userId.IsNotNullOrEmpty())
                {
                    var currentUser = _userService.GetById(userId.Value).Result;
                    if (currentUser != null)
                    {
                        user = currentUser.Username;
                    }
                    else
                    {
                        user = "Guest User";
                    }
                }
            }
            else
            {
                user = "Guest User";
            }

            // will enable this when user activity log is in database
            //SaveUserActivityLog(data, url, user, ipAddress, userId);
        }

        private void SaveUserActivityLog(string data, string url, string user, string ipAddress, Guid? userId)
        {
            var userActivityLog = new UserActivityLog
            {
                Id = Guid.NewGuid(),
                Data = data,
                LastVisitedUrl = url,
                UserName = user,
                UserIpAddress = ipAddress,
                TimeStamp = DateTime.Now,
                UserId = userId.IsNotNullOrEmpty() ? userId.Value : null
            };

            _erpContext.UserActivityLog.Add(userActivityLog);
            _erpContext.SaveChanges();
        }
    }
}
