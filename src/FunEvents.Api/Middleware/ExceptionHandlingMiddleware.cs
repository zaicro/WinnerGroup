using FunEvents.Application.Wrappers;
using System.Net;
using System.Text.Json;

namespace FunEvents.Api.Middleware;

public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;

    public ExceptionHandlingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ArgumentException ex)
        {
            await HandleExceptionAsync(
                context,
                HttpStatusCode.BadRequest,
                ex.Message,
                [ex.ParamName ?? ""]);
        }
        catch (KeyNotFoundException ex)
        {
            await HandleExceptionAsync(
                context,
                HttpStatusCode.NotFound,
                ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            await HandleExceptionAsync(
                context,
                HttpStatusCode.Conflict,
                ex.Message);
        }
        catch (Exception)
        {
            await HandleExceptionAsync(
                context,
                HttpStatusCode.InternalServerError,
                "An unexpected error occurred.");
        }
    }

    private static async Task HandleExceptionAsync(
        HttpContext context,
        HttpStatusCode statusCode,
        string message,
        List<string>? errors = null)
    {
        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/json";

        var response = Response<object>.Failure(
            message,
            errors ?? []);

        await context.Response.WriteAsync(
            JsonSerializer.Serialize(response));
    }
}