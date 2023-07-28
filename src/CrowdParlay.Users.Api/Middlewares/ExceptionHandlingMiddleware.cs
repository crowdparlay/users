using System.Net;
using System.Text.Json;
using CrowdParlay.Users.Application.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace CrowdParlay.Users.Api.Middlewares;

public class ExceptionHandlingMiddleware : IMiddleware
{
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(ILogger<ExceptionHandlingMiddleware> logger) => _logger = logger;

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "{ExceptionMessage}", exception.Message);
            Action<Exception, HttpContext> handler = exception switch
            {
                ValidationException => HandleValidationException,
                FluentValidation.ValidationException => HandleFluentValidationException,
                NotFoundException => HandleNotFoundException,
                UnauthorizedException => HandleUnauthorizedAccessException,
                ForbiddenException => HandleAccessDeniedException,
                AlreadyExistsException => HandleAlreadyExistsException,
                _ => HandleGenericException
            };
            
            handler.Invoke(exception, context);
        }
    }

    private static void HandleGenericException(Exception ex, HttpContext context)
    {
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
        context.Response.WriteAsync("An unexpected error occurred.");
    }
    
    private static void HandleValidationException(Exception ex, HttpContext context)
    {
        var exception = (ValidationException)ex;

        var errors = exception.Errors.ToDictionary(
            error => error.Key,
            error => error.Value.ToArray());
        
        var details = new ValidationProblemDetails(errors)
        {
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1"
        };

        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
        context.Response.WriteAsync(JsonSerializer.Serialize(details));
    }

    private static void HandleFluentValidationException(Exception ex, HttpContext context)
    {
        var exception = (FluentValidation.ValidationException)ex;

        var failuresByProperty = exception.Errors
            .GroupBy(error => error.PropertyName)
            .ToDictionary(
                group => group.Key,
                group => group
                    .Select(failure => failure.ErrorMessage)
                    .ToArray());
        
        var details = new ValidationProblemDetails(failuresByProperty)
        {
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1"
        };

        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
        context.Response.WriteAsync(JsonSerializer.Serialize(details));
    }

    private static void HandleNotFoundException(Exception ex, HttpContext context)
    {
        var exception = (NotFoundException)ex;

        var details = new ProblemDetails
        {
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.4",
            Title = "Not found",
            Detail = exception.Message
        };
        
        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = (int)HttpStatusCode.NotFound;
        context.Response.WriteAsync(JsonSerializer.Serialize(details));
    }

    private static void HandleUnauthorizedAccessException(Exception ex, HttpContext context)
    {
        var details = new ProblemDetails
        {
            Status = StatusCodes.Status401Unauthorized,
            Title = "Unauthorized",
            Type = "https://tools.ietf.org/html/rfc7235#section-3.1"
        };
        
        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
        context.Response.WriteAsync(JsonSerializer.Serialize(details));
    }

    private static void HandleAccessDeniedException(Exception ex, HttpContext context)
    {
        var details = new ProblemDetails
        {
            Status = StatusCodes.Status403Forbidden,
            Title = "Forbidden",
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.3"
        };
        
        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
        context.Response.WriteAsync(JsonSerializer.Serialize(details));
    }

    private static void HandleAlreadyExistsException(Exception ex, HttpContext context)
    {
        var details = new ProblemDetails
        {
            Status = StatusCodes.Status409Conflict,
            Title = "Already exists",
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.8"
        };

        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = (int)HttpStatusCode.Conflict;
        context.Response.WriteAsync(JsonSerializer.Serialize(details));
    }
}