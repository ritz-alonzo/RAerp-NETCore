using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using RA.Core.Domain;
using RA.Data.Data;
using RA.WebFramework.Extensions;
using RA.WebServiceEndpoints.Domain;
using RA.WebServiceEndpoints.Models;
using RA.WebServiceEndpoints.Services.RefreshTokenServices;
using RAerp.Controllers.Admin;
using RAerp.Domain.Application;
using RAerp.Domain.Users;
using RAerp.Helpers.Constants;
using RAerp.Helpers.Security;
using RAerp.Services.ApplicationServices;
using RAerp.Services.ApplicationSettingServices;
using RAerp.Services.ExternalLoginServices;
using RAerp.Services.UserServices;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

// 03-04-26 - Need to add Generation and Consuming of client_id and client_secret should be different for every client application.

namespace RA.WebServiceEndpoints.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class WebServiceEndpointsLoginController : AdminApiController
    {
        #region Constants
        private readonly IUserService _userService;
        private string _jwtSecretKey;
        private string _clientId;
        private string _clientSecret;
        private readonly IRefreshTokenService _refreshTokenService;
        private readonly IApplicationSettingService _applicationSettingService;
        private readonly ApplicationSetting _appSettings;
        private readonly IExternalLoginService _externalLoginService;
        #endregion

        #region Ctor
        public WebServiceEndpointsLoginController(IUserService userService, IConfiguration configuration, IRefreshTokenService refreshTokenService, IApplicationSettingService applicationSettingService, IExternalLoginService externalLoginService)
        {
            _userService = userService;
            _jwtSecretKey = configuration.GetValue<string>("ApiSettings:JWTSecretKey");
            _refreshTokenService = refreshTokenService;
            _applicationSettingService = applicationSettingService;
            _appSettings = applicationSettingService.GetCurrentApplicationSettingAsync()?.Result;
            _externalLoginService = externalLoginService;
        }
        #endregion

        [HttpPost]
        [EnableRateLimiting("LoginPolicy")]
        public async Task<IActionResult> Login([FromBody] WebServiceEndpointLoginRequestModel model)
        {
            if (model == null)
                return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "No username and password entered."));
            
            if (model.UserName == null || model.Password == null)
                return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "Username or password should have value."));

            var user = await _userService.GetUserByUsername(model.UserName);
            if (user == null)
                return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "User doesn't exists in the system. "));

            if (user.IsVerified == false)
                return StatusCode((int)HttpStatusCode.Forbidden, GenerateErrorResponseModel(HttpStatusCode.Forbidden, "User account is not verified. Please verify your account. "));

            if (_appSettings.IsEmailVerificationEnabled && user.AccountStatus == RA.Data.Data.UserAccountStatus.Pending)
                return StatusCode((int)HttpStatusCode.Forbidden, GenerateErrorResponseModel(HttpStatusCode.BadRequest, "User account is pending verification. "));

            if (_appSettings.LoginAttemptLimit > 0 && user.FailedLoginAttempt.GetValueOrDefault() >= _appSettings.LoginAttemptLimit)
                return StatusCode((int)HttpStatusCode.Forbidden, GenerateErrorResponseModel(HttpStatusCode.Forbidden, "User account is locked due to multiple failed login attempts. Please contact support. "));

            var userRole = await _userService.GetUserRoleByUserId(user.Id);
            if (userRole == null)
                return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "User doesn't have any role assigned in the system. "));
            // Revoke any existing refresh tokens on fresh login (prevent session stacking)
            await _refreshTokenService.RevokeAllForUserAsync(user.Id);
            
            if (!string.IsNullOrEmpty(user.Password))
            {
                var decrpytedPassword = await EncryptionHelper.DecryptData(user.Password, user.Salt);
                if (decrpytedPassword == model.Password)
                {
                    // JWT Token Generation
                    var accessToken = GenerateAccessToken(user, userRole);
                    var refreshToken = await _refreshTokenService.CreateAsync(user.Id, accessToken); // new family
                                                                                                     // Update user login
                    user.LastLoginDate = DateTime.UtcNow;
                    user.FailedLoginAttempt = 0;
                    user.IsLoggedOn = true;
                    await _userService.Update(user);

                    // ✅ Set the cookies securely
                    SetTokenCookies(accessToken, refreshToken.Token);

                    // ✅ Return only safe UI data, NO TOKENS or SECRETS
                    return Ok(new
                    {
                        Status = HttpStatusCode.OK.ToString(),
                        Role = userRole.Rolename,
                        Username = user.Username,
                        FullName = user?.FirstName + " " + user?.LastName,
                    });
                }
                else
                {
                    user.FailedLoginAttempt = user.FailedLoginAttempt ?? 0 + 1;
                    await _userService.Update(user);
                    return StatusCode((int)HttpStatusCode.BadRequest, GenerateErrorResponseModel(HttpStatusCode.NotFound, "Username doesn't match with provided password. "));
                }
            }
            else
            {
                // Check external login if not null
                ExternalLogin externalLogin = await _externalLoginService.GetByUserIdAsync(user.Id);
                if (externalLogin == null)
                    return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "User cannot be found in the system."));

                // JWT Token Generation
                var accessToken = GenerateAccessToken(user, userRole);
                var refreshToken = await _refreshTokenService.CreateAsync(user.Id, accessToken); // new family
                                                                                                 // Update user login
                user.LastLoginDate = DateTime.UtcNow;
                user.FailedLoginAttempt = 0;
                user.IsLoggedOn = true;
                await _userService.Update(user);

                // ✅ Set the cookies securely
                SetTokenCookies(accessToken, refreshToken.Token);

                // ✅ Return only safe UI data, NO TOKENS or SECRETS
                return Ok(new
                {
                    Status = HttpStatusCode.OK.ToString(),
                    Role = userRole.Rolename,
                    Username = user.Username,
                    FullName = user?.FirstName + " " + user?.LastName,
                });
            }
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh()
        {
            var refreshTokenFromCookie = Request.Cookies["RefreshToken"];
            if (string.IsNullOrEmpty(refreshTokenFromCookie))
                return BadRequest(GenerateErrorResponseModel(HttpStatusCode.BadRequest, "Refresh token is required."));

            var stored = await _refreshTokenService.GetByTokenAsync(refreshTokenFromCookie);

            // Token not found
            if (stored == null)
                return Unauthorized(GenerateErrorResponseModel(HttpStatusCode.Unauthorized, "Invalid refresh token."));

            // ⚠️ Reuse detected — someone used an already-consumed token — breach signal
            if (stored.IsUsed)
            {
                await _refreshTokenService.RevokeByFamilyAsync(stored.Family);
                return Unauthorized(GenerateErrorResponseModel(HttpStatusCode.Unauthorized, "Refresh token reuse detected. Please log in again."));
            }

            if (stored.IsRevoked)
                return Unauthorized(GenerateErrorResponseModel(HttpStatusCode.Unauthorized, "Refresh token has been revoked."));

            if (stored.ExpiresAt < DateTime.UtcNow)
                return Unauthorized(GenerateErrorResponseModel(HttpStatusCode.Unauthorized, "Refresh token has expired."));

            if (string.IsNullOrEmpty(stored.AccessToken))
                return Unauthorized(GenerateErrorResponseModel(HttpStatusCode.Unauthorized, "Access token does not have value"));

            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(stored.AccessToken);

            var accessToken = stored.AccessToken;
            RefreshToken refreshToken = stored;
            if (jwt.ValidTo < DateTime.UtcNow)
            {
                // ✅ Mark old token as used and issue a rotated pair
                await _refreshTokenService.MarkAsUsedAsync(stored.Id);

                var user = await _userService.GetById(stored.UserId);
                var userRole = await _userService.GetUserRoleByUserId(stored.UserId);

                accessToken = GenerateAccessToken(user, userRole);
                refreshToken = await _refreshTokenService.CreateAsync(stored.UserId, accessToken, stored.Family); // same family
            }

            // ✅ Overwrite the old cookies with the newly rotated tokens
            SetTokenCookies(accessToken, refreshToken.Token);
            // ✅ Return only safe UI data, NO TOKENS or SECRETS
            return Ok(new
            {
                Status = HttpStatusCode.OK.ToString()
            });
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            // Extract userId from the access token claim
            if (Guid.TryParse(User.FindFirst(ClaimTypes.Name)?.Value, out var result))
            {
                var user = await _userService.GetById(result);
                if (user == null)
                    return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "User not found."));

                user.IsLoggedOn = false;
                await _userService.Update(user);

                var refreshTokenFromCookie = Request.Cookies["RefreshToken"];
                if (!string.IsNullOrEmpty(refreshTokenFromCookie))
                {
                    var stored = await _refreshTokenService.GetByTokenAsync(refreshTokenFromCookie);
                    if (stored != null)
                    {
                        await _refreshTokenService.RevokeByFamilyAsync(stored.Family);
                    }
                }
                else
                {
                    await _refreshTokenService.RevokeAllForUserAsync(result);
                }

                // ✅ Instruct the browser to delete the cookies by expiring them immediately
                Response.Cookies.Delete("AccessToken");
                Response.Cookies.Delete("RefreshToken");
                Response.Cookies.Delete("client_id");
                Response.Cookies.Delete("client_secret");
            }

            return Ok(new { Status = "OK", Message = "Logged out successfully." });
        }

        [HttpGet("validate")]
        public async Task<IActionResult> ValidateTokenExpiry()
        {
            var authHeader = Request.Headers["Authorization"].ToString();

            if (string.IsNullOrEmpty(authHeader))
                return Unauthorized(GenerateErrorResponseModel(HttpStatusCode.Unauthorized, "No authorization header provided."));

            if (HttpContext.Request.Headers["client_id"].FirstOrDefault() == null)
            {
                return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "No client id in Headers."));
            }

            if (HttpContext.Request.Headers["client_secret"].FirstOrDefault() == null)
            {
                return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "No client secret in Headers."));
            }

            var token = authHeader.Replace("Bearer ", "");
            if (string.IsNullOrEmpty(token))
                return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "Invalid token"));

            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);
            return Ok(new
            {
                ExpirationDate = jwt.ValidTo,
                IsExpired = jwt.ValidTo < DateTime.UtcNow
            });
        }

        [HttpGet("retrieve-client-keys")]
        public async Task<IActionResult> RetrieveClientKeys()
        {
            if (_appSettings == null)
                return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "Application settings not yet configured."));

            if (string.IsNullOrEmpty(_appSettings.ClientId) && string.IsNullOrEmpty(_appSettings.ClientSecret))
                return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "Client keys not yet configured."));

            return Ok(new
            {
                Success = true,
                ClientId = _appSettings?.ClientId,
                ClientSecret = _appSettings?.ClientSecret
            });
        }

        #region OAuth 2.0 Google Login
        [AllowAnonymous]
        [HttpGet("google")]
        public IActionResult GoogleLogin([FromQuery] string successUrl, [FromQuery] string errorUrl)
        {
            if (!successUrl.IsValidUrl() || string.IsNullOrEmpty(successUrl))
            {
                return BadRequest(GenerateErrorResponseModel(HttpStatusCode.BadRequest, "Invalid or missing successUrl."));
            }

            if (!errorUrl.IsValidUrl() || string.IsNullOrEmpty(errorUrl))
            {
                return BadRequest(GenerateErrorResponseModel(HttpStatusCode.BadRequest, "Invalid or missing errorUrl."));
            }

            var redirectUrl = Url.Action(
                nameof(GoogleCallback),
                "WebServiceEndpointsLogin",
                new
                {
                    successUrl,
                    errorUrl
                });

            var properties = new AuthenticationProperties
            {
                RedirectUri = redirectUrl
            };

            return Challenge(
                properties,
                GoogleDefaults.AuthenticationScheme);
        }

        [AllowAnonymous]
        [HttpGet("google-callback")]
        public async Task<IActionResult> GoogleCallback(string successUrl, string errorUrl)
        {
            try
            {
                var result = await HttpContext.AuthenticateAsync(
                CookieAuthenticationDefaults.AuthenticationScheme);

                if (!result.Succeeded)
                {
                    return Unauthorized();
                }

                var principal = result.Principal;

                var email = principal.FindFirstValue(
                    ClaimTypes.Email);

                var googleSubjectId = principal.FindFirstValue(
                    ClaimTypes.NameIdentifier);

                var name = principal.FindFirstValue(
                    ClaimTypes.Name);

                var firstName = principal.FindFirstValue(
                    ClaimTypes.GivenName);

                var lastName = principal.FindFirstValue(
                    ClaimTypes.Surname);

                var picture = principal.FindFirstValue("picture");

                // 1. Find ExternalLogin by:
                //    Provider = Google
                //    ProviderSubjectId = googleSubjectId
                ExternalLogin externalLogin = await _externalLoginService.GetByProviderAndSubjectIdAsync("Google", googleSubjectId);
                if (externalLogin != null)
                {
                    User user = await _userService.GetById(externalLogin.UserId);
                    if (user == null)
                        return BadRequest(GenerateErrorResponseModel(HttpStatusCode.BadRequest, "User not found."));
                    UserRole userRole = await _userService.GetUserRoleByUserId(user.Id);
                    if (userRole == null)
                        return BadRequest(GenerateErrorResponseModel(HttpStatusCode.BadRequest, "User role not found."));
                    // JWT Token Generation
                    var accessToken = GenerateAccessToken(user, userRole);
                    var refreshToken = await _refreshTokenService.CreateAsync(user.Id, accessToken); // new family
                                                                                                     // Update user login
                    user.LastLoginDate = DateTime.UtcNow;
                    user.FailedLoginAttempt = 0;
                    user.IsLoggedOn = true;
                    await _userService.Update(user);

                    // ✅ Set the cookies securely
                    SetTokenCookies(accessToken, refreshToken.Token);

                    // Update ExternalLogin last login time
                    externalLogin.LastLoginOn = DateTime.UtcNow;
                    await _externalLoginService.UpdateAsync(externalLogin);

                }
                else
                {
                    externalLogin = new ExternalLogin();
                    // 2. If not found:
                    User existingUser = await _userService.GetUserByFirstNameAndLastNameAsync(firstName, lastName);
                    // Assign default role to the new user
                    UserRole guestRole = await _userService.GetUserRoleByRoleNameAsync(AdminMessages.GuestRole);
                    if (existingUser != null)
                    {
                        externalLogin.UserId = existingUser.Id;
                    }
                    else
                    {
                        // Create new user
                        User newUser = new User
                        {
                            FirstName = firstName,
                            LastName = lastName,
                            Email = email,
                            Username = email, // or generate a unique username
                            IsVerified = true,
                            AccountStatusId = (int)UserAccountStatus.Active,
                            LastLoginDate = DateTime.UtcNow,
                            IsLoggedOn = true,
                            FailedLoginAttempt = 0,
                        };
                        await _userService.Insert(newUser);
                        if (guestRole != null)
                        {
                            await _userService.InsertMapping(newUser.Id, guestRole.Id);
                        }
                        // Link ExternalLogin to the new user
                        externalLogin.UserId = newUser.Id;
                        existingUser = newUser;
                    }

                    externalLogin.Provider = "Google";
                    externalLogin.ProviderEmail = email;
                    externalLogin.ProviderSubjectId = googleSubjectId;
                    externalLogin.ProviderName = name;
                    externalLogin.ProviderPictureUrl = picture;
                    externalLogin.LastLoginOn = DateTime.UtcNow;
                    await _externalLoginService.InsertAsync(externalLogin);

                    // JWT Token Generation
                    var accessToken = GenerateAccessToken(existingUser, guestRole);
                    var refreshToken = await _refreshTokenService.CreateAsync(existingUser.Id, accessToken);

                    // ✅ Set the cookies securely
                    SetTokenCookies(accessToken, refreshToken.Token);
                }

                return Redirect(successUrl);
            }
            catch
            {
                return Redirect(errorUrl);
            }
        }
        #endregion

        #region Methods
        private string GenerateAccessToken(User user, UserRole userRole)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtSecretKey);

            _clientId = HttpContext.Request.Headers["client_id"].ToString();
            _clientSecret = HttpContext.Request.Headers["client_secret"].ToString();

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Name,   user.Id.ToString()),
                    new Claim(ClaimTypes.Role,   userRole.Rolename),
                    new Claim(ClaimTypes.System, _clientId),
                    new Claim(ClaimTypes.Sid,    _clientSecret)
                }),
                Expires = DateTime.UtcNow.AddHours(1),   // ✅ use UtcNow, not Now
                SigningCredentials = new(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            return tokenHandler.WriteToken(tokenHandler.CreateToken(tokenDescriptor));
        }

        private void SetTokenCookies(string accessToken, string refreshToken)
        {
            // Access Token Cookie (Short lived)
            var accessCookieOptions = new CookieOptions
            {
                HttpOnly = true,      // Prevents JavaScript access (XSS protection)
                Secure = true,        // Ensures cookie is only sent over HTTPS
                SameSite = SameSiteMode.None, // Protects against Cross-Site Request Forgery (CSRF)
                Expires = DateTime.UtcNow.AddHours(1) // Match your JWT expiration
            };
            Response.Cookies.Append("AccessToken", accessToken, accessCookieOptions);

            // Refresh Token Cookie (Long lived)
            var refreshCookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTime.UtcNow.AddDays(7) // Match your refresh token expiration
            };
            Response.Cookies.Append("RefreshToken", refreshToken, refreshCookieOptions);

            // Client Id Cookie (Long Lived)
            var clientIdCookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTime.UtcNow.AddDays(7) // Match your refresh token expiration
            };
            Response.Cookies.Append("client_id", _appSettings?.ClientId, refreshCookieOptions);

            // Client Secret Cookie (Long Lived)
            var clientSecretCookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTime.UtcNow.AddDays(7) // Match your refresh token expiration
            };
            Response.Cookies.Append("client_secret", _appSettings?.ClientSecret, clientSecretCookieOptions);
        }
        #endregion
    }
}
