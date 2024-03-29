﻿using System.Net;
using System.Text.Json;
using Application.Exceptions;

namespace WebUI.Middleware;

public class ErrorHandlerMiddleware
{
    private readonly RequestDelegate _next;

    public ErrorHandlerMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception error)
        {
            switch (error)
            {
                case AppException e:
                    // custom application error
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    break;
                case ConfigException e:
                    // something is wrong with the configuration
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    break;
                case ApiKeyException e:
                    context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    break;
                default:
                    // unhandled error
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    break;
            }

            var logger = context.RequestServices.GetRequiredService<ILogger<ErrorHandlerMiddleware>>();
            logger.LogError("Exception generated for: endpoint({endpoint}), path({path}), routeValues({routeValues})", context.GetEndpoint(), context.Request.Path, context.Request.RouteValues);
            logger.LogError("Exception: {type}: {exception}", error.GetType(), error?.Message);
            logger.LogError("Stacktrace: \n{stacktrace}", error?.StackTrace?.Trim());

            context.Response.ContentType = "application/json";
            var result = JsonSerializer.Serialize(new { statusCode = context.Response.StatusCode, message = error?.Message });
            await context.Response.WriteAsync(result);
        }
    }
}
