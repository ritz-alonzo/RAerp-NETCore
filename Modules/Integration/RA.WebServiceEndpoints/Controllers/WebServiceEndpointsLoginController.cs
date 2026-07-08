using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using RA.Core.Domain;
using RA.Data.Domain.Users;
using RA.WebFramework.Extensions;
using RA.WebServiceEndpoints.Domain;
using RA.WebServiceEndpoints.Models;
using RA.WebServiceEndpoints.Services.RefreshTokenServices;
using RAerp.Helpers.Security;
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
    public class WebServiceEndpointsLoginController : ControllerBase
    {
        #region Constants
        private readonly IUserService _userService;
        private string _jwtSecretKey;
        private string _clientId;
        private string _clientSecret;
        private readonly IRefreshTokenService _refreshTokenService;
        #endregion

        #region Ctor
        public WebServiceEndpointsLoginController(IUserService userService, IConfiguration configuration, IRefreshTokenService refreshTokenService)
        {
            _userService = userService;
            _jwtSecretKey = configuration.GetValue<string>("ApiSettings:JWTSecretKey");
            _refreshTokenService = refreshTokenService;
        }
        #endregion

        [HttpPost]
        [EnableRateLimiting("LoginPolicy")]
        public async Task<IActionResult> Login([FromBody] WebServiceEndpointLoginRequestModel model)
        {
            if (model == null)
                return NotFound(new WebServiceEndpointResponseErrorModel() { Status = HttpStatusCode.NotFound.ToString(), Message = "No username and password entered." });
            
            // Add check of client ID and client secret in database. For now, we are hardcoding the values in appsettings.json file.
            if (HttpContext.Request.Headers["client_id"].FirstOrDefault() == null)
            {
                return NotFound(new WebServiceEndpointResponseErrorModel() { Status = HttpStatusCode.NotFound.ToString(), Message = "No client id in Headers." });
            }

            if (HttpContext.Request.Headers["client_secret"].FirstOrDefault() == null)
            {
                return NotFound(new WebServiceEndpointResponseErrorModel() { Status = HttpStatusCode.NotFound.ToString(), Message = "No client secret in Headers." });
            }

            _clientId = HttpContext.Request.Headers["client_id"].ToString();
            _clientSecret = HttpContext.Request.Headers["client_secret"].ToString();

            if (model.UserName == null || model.Password == null)
                return NotFound(new WebServiceEndpointResponseErrorModel() { Status = HttpStatusCode.NotFound.ToString(), Message = "Username or password should have value." });

            var user = await _userService.GetUserByUsername(model.UserName);
            if (user == null)
                return NotFound(new WebServiceEndpointResponseErrorModel() { Status = HttpStatusCode.NotFound.ToString(), Message = "User doesn't exists in the system. " });

            var userRole = await _userService.GetUserRoleByUserId(user.Id);
            if (userRole == null)
                return NotFound(new WebServiceEndpointResponseErrorModel() { Status = HttpStatusCode.NotFound.ToString(), Message = "User doesn't have any role assigned in the system. " });
            // Revoke any existing refresh tokens on fresh login (prevent session stacking)
            await _refreshTokenService.RevokeAllForUserAsync(user.Id);
            
            var decrpytedPassword = await EncryptionHelper.DecryptData(user.Password, user.Salt);
            if (decrpytedPassword == model.Password)
            {
                // JWT Token Generation
                var accessToken = GenerateAccessToken(user, userRole);
                var refreshToken = await _refreshTokenService.CreateAsync(user.Id, accessToken); // new family

                return Ok(new WebServiceEndpointLoginResponseModel
                {
                    Status = HttpStatusCode.OK.ToString(),
                    Token = accessToken,
                    RefreshToken = refreshToken.Token
                });
            }
            else
            {
                return NotFound(new WebServiceEndpointResponseErrorModel() { Status = HttpStatusCode.NotFound.ToString(), Message = "Username doesn't match with provided password. " });
            }
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] WebServiceEndpointRefreshTokenRequestModel model)
        {
            if (string.IsNullOrEmpty(model?.RefreshToken))
                return BadRequest(new WebServiceEndpointResponseErrorModel { Status = "BadRequest", Message = "Refresh token is required." });

            if (HttpContext.Request.Headers["client_id"].FirstOrDefault() == null)
                return NotFound(new WebServiceEndpointResponseErrorModel { Status = HttpStatusCode.NotFound.ToString(), Message = "No client id in Headers." });

            if (HttpContext.Request.Headers["client_secret"].FirstOrDefault() == null)
                return NotFound(new WebServiceEndpointResponseErrorModel { Status = HttpStatusCode.NotFound.ToString(), Message = "No client secret in Headers." });

            var stored = await _refreshTokenService.GetByTokenAsync(model.RefreshToken);

            // Token not found
            if (stored == null)
                return Unauthorized(new WebServiceEndpointResponseErrorModel { Status = "Unauthorized", Message = "Invalid refresh token." });

            // ⚠️ Reuse detected — someone used an already-consumed token — breach signal
            if (stored.IsUsed)
            {
                await _refreshTokenService.RevokeByFamilyAsync(stored.Family);
                return Unauthorized(new WebServiceEndpointResponseErrorModel { Status = "Unauthorized", Message = "Refresh token reuse detected. Please log in again." });
            }

            if (stored.IsRevoked)
                return Unauthorized(new WebServiceEndpointResponseErrorModel { Status = "Unauthorized", Message = "Refresh token has been revoked." });

            if (stored.ExpiresAt < DateTime.UtcNow)
                return Unauthorized(new WebServiceEndpointResponseErrorModel { Status = "Unauthorized", Message = "Refresh token has expired." });

            if (string.IsNullOrEmpty(stored.AccessToken))
                return Unauthorized(new WebServiceEndpointResponseErrorModel { Status = "Unauthorized", Message = "Access token does not have value" });

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

            return Ok(new WebServiceEndpointLoginResponseModel
            {
                Status = HttpStatusCode.OK.ToString(),
                Token = accessToken,
                RefreshToken = refreshToken.Token
            });
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout([FromBody] WebServiceEndpointRefreshTokenRequestModel model)
        {
            // Revoke specific token if provided, otherwise revoke all for user
            if (!string.IsNullOrEmpty(model?.RefreshToken))
            {
                var stored = await _refreshTokenService.GetByTokenAsync(model.RefreshToken);
                if (stored != null)
                    await _refreshTokenService.RevokeByFamilyAsync(stored.Family);
            }
            else
            {
                // Extract userId from the access token claim
                if (Guid.TryParse(User.FindFirst(ClaimTypes.Name)?.Value, out var result))
                {
                    await _refreshTokenService.RevokeAllForUserAsync(result);
                }
            }

            return Ok(new { Status = "OK", Message = "Logged out successfully." });
        }

        [HttpGet("validate")]
        public async Task<IActionResult> ValidateTokenExpiry()
        {
            var authHeader = Request.Headers["Authorization"].ToString();

            if (string.IsNullOrEmpty(authHeader))
                return Unauthorized();

            if (HttpContext.Request.Headers["client_id"].FirstOrDefault() == null)
            {
                return NotFound(new WebServiceEndpointResponseErrorModel() { Status = HttpStatusCode.NotFound.ToString(), Message = "No client id in Headers." });
            }

            if (HttpContext.Request.Headers["client_secret"].FirstOrDefault() == null)
            {
                return NotFound(new WebServiceEndpointResponseErrorModel() { Status = HttpStatusCode.NotFound.ToString(), Message = "No client secret in Headers." });
            }

            var token = authHeader.Replace("Bearer ", "");
            if (string.IsNullOrEmpty(token))
                return NotFound(new WebServiceEndpointResponseErrorModel() { Status = HttpStatusCode.NotFound.ToString(), Message = "Invalid token" });

            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);
            return Ok(new
            {
                ExpirationDate = jwt.ValidTo,
                IsExpired = jwt.ValidTo < DateTime.UtcNow
            });
        }

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
        #endregion
    }
}
