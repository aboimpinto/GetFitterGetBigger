using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using Microsoft.AspNetCore.Authorization;

namespace GetFitterGetBigger.API.Middleware
{
    public class DevelopmentAllowAnonymousFilter : IAuthorizationFilter
    {
        private readonly IWebHostEnvironment _env;

        public DevelopmentAllowAnonymousFilter(IWebHostEnvironment env)
        {
            _env = env;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            // Only apply in Development environment
            if (_env.IsDevelopment())
            {
                // Check if the request has an Authorization header
                bool hasAuthHeader = context.HttpContext.Request.Headers.ContainsKey("Authorization");

                // If we're in development and there's no Authorization header, 
                // bypass authorization by setting a Result
                if (!hasAuthHeader)
                {
                    // Skip authorization by setting a non-null Result
                    context.Result = new EmptyResult();
                }
            }
        }
    }
}
