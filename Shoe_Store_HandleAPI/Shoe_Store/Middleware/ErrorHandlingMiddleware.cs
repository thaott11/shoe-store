using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Threading.Tasks;

namespace Shoe_Store_HandleAPI.Middleware
{
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;

        public ErrorHandlingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            context.Response.OnStarting(() =>
            {
                if (context.Response.StatusCode == (int)HttpStatusCode.Forbidden)
                {
                    context.Response.Redirect("/Error/Forbidden");
                }
                else if (context.Response.StatusCode == (int)HttpStatusCode.NotFound)
                {
                    context.Response.Redirect("/Error/NotFound");
                }
                return Task.CompletedTask;
            });

            await _next(context); 
        }
    }
}
