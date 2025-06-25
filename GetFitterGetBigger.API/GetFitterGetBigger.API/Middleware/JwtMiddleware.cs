using System;
using System.Linq;
using System.Threading.Tasks;
using GetFitterGetBigger.API.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace GetFitterGetBigger.API.Middleware
{
    public class JwtMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IHostEnvironment _env;

        public JwtMiddleware(RequestDelegate next, IHostEnvironment env)
        {
            _next = next;
            _env = env;
        }

        public async Task Invoke(HttpContext context, IJwtService jwtService, IConfiguration configuration)
        {
            var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

            if (token != null)
            {
                var principal = jwtService.GetPrincipalFromToken(token);
                if (principal != null)
                {
                    context.User = principal;
                }
                else
                {
                    context.Response.StatusCode = 401;
                    await context.Response.WriteAsync("{\"error\": \"UserExpired\", \"message\": \"The session has expired. Please log in again.\"}");
                    return;
                }
            }
            else if (_env.IsProduction())
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("{\"error\": \"Unauthorized\", \"message\": \"Authorization header is missing.\"}");
                return;
            }

            await _next(context);
        }
    }
}
