using Microsoft.AspNetCore.Mvc;
using System.Net;

public class ExceptionService
{
    public static async Task<IActionResult> HandleRequestAsync(Func<Task<IActionResult>> action, ILogger log, HttpRequest req, HttpResponse res)
    {
        try
        {
            return await action();
        }
        catch (Exception ex)
        {
            log.LogError($"An error occurred: {ex.Message}");
            return new ObjectResult($"An error occurred: {ex.Message}")
            {
                StatusCode = (int)HttpStatusCode.InternalServerError
            };
        }
    }
}
