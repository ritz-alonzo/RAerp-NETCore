using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using RA.Core.Domain;
using RA.WebServiceEndpoints.Models;
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
    [Route("api/[controller]")]
    public class WebServiceEndpointsLoginController : ControllerBase
    {
        private readonly IUserService _userService;
        private string _jwtSecretKey;
        private string _clientId;
        private string _clientSecret;

        public WebServiceEndpointsLoginController(IUserService userService, IConfiguration configuration)
        {
            _userService = userService;
            _jwtSecretKey = configuration.GetValue<string>("ApiSettings:JWTSecretKey");
        }

        [HttpPost]
        [EnableRateLimiting("LoginPolicy")]
        public async Task<IActionResult> Login([FromBody] WebServiceEndpointLoginRequestModel model)
        {
            if (model == null)
                return NotFound(new WebServiceEndpointResponseErrorModel() { Status = HttpStatusCode.NotFound.ToString(), Message = "No username and password entered." });

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

            // Add check of client ID and client secret in database. For now, we are hardcoding the values in appsettings.json file.

            if (model.UserName == null || model.Password == null)
                return NotFound(new WebServiceEndpointResponseErrorModel() { Status = HttpStatusCode.NotFound.ToString(), Message = "Username or password should have value." });

            var user = await _userService.GetUserByUsername(model.UserName);

            if (user == null)
                return NotFound(new WebServiceEndpointResponseErrorModel() { Status = HttpStatusCode.NotFound.ToString(), Message = "User doesn't exists in the system. " });

            var decrpytedPassword = await EncryptionHelper.DecryptData(user.Password, user.Salt);

            if (decrpytedPassword == model.Password)
            {
                // JWT Token Generation
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_jwtSecretKey);

                var tokenDescriptor = new SecurityTokenDescriptor()
                {
                    Subject = new ClaimsIdentity(new Claim[]
                    {
                        new Claim(ClaimTypes.Name, user.Id.ToString()),
                        new Claim(ClaimTypes.System, _clientId),
                        new Claim(ClaimTypes.Sid, _clientSecret)
                    }),
                    Expires = DateTime.Now.AddDays(1),
                    SigningCredentials = new(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                };

                var token = tokenHandler.CreateToken(tokenDescriptor);
                var loginResponse = new WebServiceEndpointLoginResponseModel();
                loginResponse.UserName = user.Username;
                loginResponse.FirstName = user.FirstName;
                loginResponse.LastName = user.LastName;
                loginResponse.Status = HttpStatusCode.OK.ToString();
                loginResponse.Token = tokenHandler.WriteToken(token);

                
                return Ok(loginResponse);
            }
            else
            {
                return NotFound(new WebServiceEndpointResponseErrorModel() { Status = HttpStatusCode.NotFound.ToString(), Message = "Username doesn't match with provided password. " });
            }
        }
    }
}
