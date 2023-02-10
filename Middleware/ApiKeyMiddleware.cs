using System.Net;
using System.Text.Json;

namespace UserListsAPI.Middleware;

public class ApiKeyMiddleware
{
    private readonly RequestDelegate _next;
    private readonly string _apiKey = "XApiKey";

    public ApiKeyMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (!context.Request.Headers.TryGetValue(_apiKey, out var extractedApiKey))
        {
            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            context.Response.ContentType = "application/json";
            var result = JsonSerializer.Serialize(new { statusCode = context.Response.StatusCode, message = "Api Key was not provided" });
            await context.Response.WriteAsync(result);
            return;
        }

        var apiKey = context.RequestServices.GetRequiredService<IConfiguration>().GetValue<string>(_apiKey);

        if (!apiKey.Equals(extractedApiKey))
        {
            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            context.Response.ContentType = "application/json";
            var result = JsonSerializer.Serialize(new { statusCode = context.Response.StatusCode, message = "Unauthorized request" });
            await context.Response.WriteAsync(result);
            return;
        }

        await _next(context);
    }
}
