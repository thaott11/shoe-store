using System.Net;

namespace Shoe_Store_HandleAPI.Middleware
{
    public class ErrorHandlingMiddlewareApi
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErrorHandlingMiddlewareApi> _logger;

        public ErrorHandlingMiddlewareApi(RequestDelegate next, ILogger<ErrorHandlingMiddlewareApi> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            await _next(context); // Chuyển tiếp đến middleware tiếp theo

            // Kiểm tra các trạng thái lỗi HTTP
            if (context.Response.StatusCode == (int)HttpStatusCode.Forbidden)
            {
                _logger.LogWarning("403 Forbidden: {Path}", context.Request.Path);
                context.Response.Redirect("/Error/Forbidden");
            }
            else if (context.Response.StatusCode == (int)HttpStatusCode.NotFound)
            {
                _logger.LogWarning("404 Not Found: {Path}", context.Request.Path);
                context.Response.Redirect("/Error/NotFound");
            }
        }
    }
}
