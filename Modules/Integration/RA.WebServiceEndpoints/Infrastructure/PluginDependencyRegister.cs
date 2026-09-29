using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using RA.Core.DataCaching.CacheManagement;
using RA.WebServiceEndpoints.App_Data;
using RA.WebServiceEndpoints.Controllers;
using RA.WebServiceEndpoints.Factories;
using RA.WebServiceEndpoints.Mapping;
using RA.WebServiceEndpoints.Services;
using RA.WebServiceEndpoints.Services.RefreshTokenServices;
using RAerp.PluginServiceProvider;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.RateLimiting;
using System.Threading.Tasks;

namespace RA.WebServiceEndpoints.Infrastructure
{
    public class PluginDependencyRegister : IPluginStartup
    {
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            // data context
            services.AddDbContextPool<RAWebServiceEndpointContext>(c => c.UseSqlServer(configuration.GetConnectionString("CurrentConnection")));
            // controllers
            // mvc controller
            services.AddMvc()
                .AddApplicationPart(typeof(WebServiceEndpointsController).Assembly);
            // api controller
            //services.AddControllers()
            //    .AddApplicationPart(typeof(WebServiceEndpointsAPIController).Assembly);
            // service
            services.AddTransient<IWebServiceEndpointService, WebServiceEndpointService>();
            services.AddTransient<IRefreshTokenService, RefreshTokenService>();
            // factory
            services.AddTransient<IWebServiceEndpointModelFactory, WebServiceEndpointModelFactory>();
            // automapper profile
            services.AddAutoMapper(cfg => { cfg.AddProfile<WebServiceEndpointMappingProfile>(); });

            // api token 
            var jwtSecretKey = configuration.GetValue<string>("ApiSettings:JWTSecretKey");
            services.AddAuthentication(c =>
            {
                c.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                c.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;

                // IMPORTANT:
                // Google needs a sign-in scheme
                c.DefaultSignInScheme =
                    CookieAuthenticationDefaults.AuthenticationScheme;
            })
            .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddJwtBearer(c =>
            {
                c.RequireHttpsMetadata = false;
                c.SaveToken = true;
                c.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters()
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtSecretKey)),
                    ValidateIssuer = false,
                    ValidateAudience = false
                };

                // ✅ Add this block to extract the token from the cookie
                c.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        // The cookie name "AccessToken" must exactly match what you set in your Login endpoint
                        if (context.Request.Cookies.TryGetValue("AccessToken", out var token))
                        {
                            context.Token = token;
                        }
                        return Task.CompletedTask;
                    }
                };
            })
            .AddGoogle(GoogleDefaults.AuthenticationScheme, options =>
            {
                options.ClientId =
                    configuration[
                        "OAuth2.0:Google:ClientId"]!;

                options.ClientSecret =
                    configuration[
                        "OAuth2.0:Google:ClientSecret"]!;

                options.CallbackPath = "/signin-google";
            });

            // rate limiting for requests
            services.AddRateLimiter(options =>
            {
                options.AddFixedWindowLimiter("LoginPolicy", config =>
                {
                    config.PermitLimit = 5;                        // Max 5 requests
                    config.Window = TimeSpan.FromMinutes(1);       // per 1 minute
                    config.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                    config.QueueLimit = 0;                         // No queuing — reject immediately
                });
                options.AddPolicy("LoginPerIp", httpContext =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                        factory: _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = 5,
                            Window = TimeSpan.FromMinutes(1),
                            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                            QueueLimit = 0
                        }));
                options.AddFixedWindowLimiter("TransactionPolicy", config =>
                {
                    config.PermitLimit = 100;                        // Max 100 requests
                    config.Window = TimeSpan.FromMinutes(1);       // per 1 minute
                    config.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                    config.QueueLimit = 0;                         // No queuing — reject immediately
                });
                // Return 429 Too Many Requests
                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            });
        }
    }
}
