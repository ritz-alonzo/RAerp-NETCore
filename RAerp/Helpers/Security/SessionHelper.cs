using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RA.WebFramework.Extensions;
using System;
using System.Security.Cryptography;
using System.Text;

namespace RAerp.Helpers.Security
{
    /// <summary>
    /// Generate session
    /// Remove session
    /// Inherit to Controller to use HttpContext
    /// User session methods
    /// </summary>
    public static class SessionHelper
    {
        public static async Task GenerateSession(Guid entityId, HttpContext httpContext)
        {
            //var initialSessionId = entityId.ToString().Split("-")[0];
            // for now store in session user Id 
            if (entityId.IsNullOrEmpty()) 
                throw new ArgumentNullException("Cannot generate Session, id is not found");

            var token = entityId.ToString();
            var tokenKey = EncryptionHelper.GenerateSalt();
            var encryptedSession = await EncryptionHelper.EncryptData(token, tokenKey);
            httpContext.Session.SetString("raerpToken", encryptedSession);
            httpContext.Session.SetString("raerpTokenKey", tokenKey);
        }

        public static string RetrieveUserSession(HttpContext httpContext)
        {
            var userSessionToken = "";
            var token = httpContext.Session.GetString("raerpToken");
            var tokenKey = httpContext.Session.GetString("raerpTokenKey");

            if (!string.IsNullOrEmpty(token) && !string.IsNullOrEmpty(tokenKey))
            {
                userSessionToken = EncryptionHelper.DecryptData(token, tokenKey).Result.ToString();
            }

            return userSessionToken;
        }

        public static bool SessionGenerated(HttpContext httpContext)
        {
            return httpContext.Session.GetString("raerpToken") != null && httpContext.Session.GetString("raerpTokenKey") != null;
        }

        public static void ClearSession(HttpContext httpContext)
        {
            httpContext.Session.Clear();
        }

        public static void ValidateAndClearSession(HttpContext httpContext)
        {
            if (SessionGenerated(httpContext))
                httpContext.Session.Clear();
        }
    }
}
