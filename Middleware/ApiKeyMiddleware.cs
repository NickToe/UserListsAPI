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
      context.Response.StatusCode = 401;
      await context.Response.WriteAsync("Api Key was not provided");
      return;
    }

    var apiKey = context.RequestServices.GetRequiredService<IConfiguration>().GetValue<string>(_apiKey);

    if (!apiKey.Equals(extractedApiKey))
    {
      context.Response.StatusCode = 401;
      await context.Response.WriteAsync("Unauthorized request");
      return;
    }

    await _next(context);
  }
}
