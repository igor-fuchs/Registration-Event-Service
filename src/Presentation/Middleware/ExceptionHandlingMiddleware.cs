using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using RegistrationEventService.Domain.Exceptions;

namespace RegistrationEventService.Presentation.Middleware;

/// <summary>
/// Global exception handler middleware.
/// Converts domain and application exceptions into appropriate HTTP responses.
/// </summary>
public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, problemDetails) = exception switch
        {
            UserAlreadyExistsException e => (
                HttpStatusCode.Conflict,
                CreateProblemDetails(context, HttpStatusCode.Conflict, "Conflict", e.Message)),

            UserNotFoundException e => (
                HttpStatusCode.NotFound,
                CreateProblemDetails(context, HttpStatusCode.NotFound, "Not Found", e.Message)),

            DomainException e => (
                HttpStatusCode.BadRequest,
                CreateProblemDetails(context, HttpStatusCode.BadRequest, "Bad Request", e.Message)),

            _ => (
                HttpStatusCode.InternalServerError,
                CreateProblemDetails(context, HttpStatusCode.InternalServerError, "Internal Server Error",
                    "An unexpected error occurred."))
        };

        if (statusCode == HttpStatusCode.InternalServerError)
        {
            _logger.LogError(exception, "Unhandled exception: {Message}", exception.Message);
        }
        else
        {
            _logger.LogWarning("Handled exception: {ExceptionType} - {Message}",
                exception.GetType().Name, exception.Message);
        }

        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/problem+json";

        await context.Response.WriteAsync(JsonSerializer.Serialize(problemDetails, JsonOptions));
    }

    private static ProblemDetails CreateProblemDetails(
        HttpContext context,
        HttpStatusCode status,
        string title,
        string detail)
    {
        return new ProblemDetails
        {
            Status = (int)status,
            Title = title,
            Detail = detail,
            Instance = context.Request.Path
        };
    }
}
