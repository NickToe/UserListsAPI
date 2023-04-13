using Application.Exceptions;

namespace WebUI.Middleware;

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
            throw new ApiKeyException("API key was not provided");
        }

        var apiKey = context.RequestServices.GetRequiredService<IConfiguration>().GetValue<string>(_apiKey) ?? throw new ConfigException("API key 'XApiKey' was not found in configuration");

        if (!apiKey.Equals(extractedApiKey))
        {
            throw new ApiKeyException("API key is not valid");
        }

        await _next(context);
    }
}
