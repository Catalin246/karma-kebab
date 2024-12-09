using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Middlewares
{
    public class GatewayHeaderMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GatewayHeaderMiddleware> _logger;

        public GatewayHeaderMiddleware(RequestDelegate next, ILogger<GatewayHeaderMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Log the incoming request method and path
            _logger.LogInformation("Incoming request: {Method} {Path}", context.Request.Method, context.Request.Path);

            // Check if the X-From-Gateway header is present and valid
            if (context.Request.Headers["X-From-Gateway"] != "true")
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsync("Forbidden: Invalid Gateway Header");
                return;
            }

            // Pass the request to the next middleware in the pipeline
            await _next(context);
        }
    }
}
