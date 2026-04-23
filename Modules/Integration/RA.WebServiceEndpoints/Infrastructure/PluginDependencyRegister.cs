using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using RA.Core.DataCaching.CacheManagement;
using RA.WebServiceEndpoints.App_Data;
using RA.WebServiceEndpoints.Controllers;
using RA.WebServiceEndpoints.Factories;
using RA.WebServiceEndpoints.Mapping;
using RA.WebServiceEndpoints.Services;
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
            }).AddJwtBearer(c =>
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
