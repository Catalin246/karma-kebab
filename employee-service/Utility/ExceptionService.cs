using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;

public class ExceptionService
{
    public static async Task<HttpResponseData> HandleRequestAsync(
    Func<Task<HttpResponseData>> action,
    ILogger log,
    HttpRequestData req)
{
    try
    {
        return await action();
    }
    catch (Exception ex)
    {
        log.LogError($"An error occurred: {ex.Message}");
        var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
        await errorResponse.WriteStringAsync("An error occurred while processing the request.");
        return errorResponse;
    }
}

}
