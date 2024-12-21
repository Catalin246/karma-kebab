using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Functions.Worker.Http;
using System.Net;

namespace Middlewares
{
    public class GatewayHeaderMiddleware : IFunctionsWorkerMiddleware
    {
        private readonly ILogger<GatewayHeaderMiddleware> _logger;

        public GatewayHeaderMiddleware(ILogger<GatewayHeaderMiddleware> logger)
        {
            _logger = logger;
        }

        public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
        {
            // Retrieve the HTTP request data asynchronously
            var httpRequest = await context.GetHttpRequestDataAsync();

            if (httpRequest != null)
            {
                // Log incoming request details
                _logger.LogInformation("Incoming request: {Method} {Path}", httpRequest.Method, httpRequest.Url);

                // Check for the presence of the custom header "X-From-Gateway"
                if (!httpRequest.Headers.Contains("X-From-Gateway"))
                {
                    // If the header is missing, create a 403 Forbidden response
                    var response = httpRequest.CreateResponse(HttpStatusCode.Forbidden);
                    await response.WriteStringAsync("Forbidden: Missing Gateway Header");

                    // Store the response in the context for later use
                    context.Items["Response"] = response;

                    // Return early, no need to call next() in the pipeline
                    return;
                }

                // Check if the header value is "true"
                var gatewayHeader = httpRequest.Headers.GetValues("X-From-Gateway").FirstOrDefault();
                if (gatewayHeader != "true")
                {
                    // If the header value is incorrect, return a 403 Forbidden response
                    var response = httpRequest.CreateResponse(HttpStatusCode.Forbidden);
                    await response.WriteStringAsync("Forbidden: Invalid Gateway Header");

                    // Store the response in the context for later use
                    context.Items["Response"] = response;

                    // Return early, no need to call next()
                    return;
                }
            }

            // Proceed to the next middleware or function if the header is valid
            await next(context);
        }
    }
}
