using System.Net;
using CrowdParlay.Users.Application.Exceptions;

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
            var problem = exception switch
            {
                ValidationException e => SanitizeValidationException(e),
                FluentValidation.ValidationException e => SanitizeFluentValidationException(e),
                NotFoundException => SanitizeNotFoundException(),
                ForbiddenException => SanitizeForbiddenException(),
                AlreadyExistsException => SanitizeAlreadyExistsException(),
                _ => SanitizeGenericException()
            };

            context.Response.ContentType = "application/problem+json";
            context.Response.StatusCode = (int)problem.HttpStatusCode;
            await context.Response.WriteAsJsonAsync(problem, problem.GetType(), GlobalSerializerOptions.SnakeCase);
        }
    }

    private static Problem SanitizeGenericException() => new()
    {
        HttpStatusCode = HttpStatusCode.InternalServerError,
        ErrorDescription = "Something went wrong. Try again later."
    };

    private static ValidationProblem SanitizeValidationException(ValidationException exception) => new()
    {
        HttpStatusCode = HttpStatusCode.BadRequest,
        ErrorDescription = "The specified data is invalid.",
        ValidationErrors = exception.Errors.ToDictionary(
            error => error.Key,
            error => error.Value.ToArray())
    };

    private static ValidationProblem SanitizeFluentValidationException(FluentValidation.ValidationException exception) => new()
    {
        HttpStatusCode = HttpStatusCode.BadRequest,
        ErrorDescription = "The specified data is invalid.",
        ValidationErrors = exception.Errors
            .GroupBy(error => error.PropertyName)
            .ToDictionary(
                group => group.Key,
                group => group
                    .Select(failure => failure.ErrorMessage)
                    .ToArray())
    };

    private static Problem SanitizeNotFoundException() => new()
    {
        HttpStatusCode = HttpStatusCode.NotFound,
        ErrorDescription = "The requested resource doesn't exist."
    };

    private static Problem SanitizeForbiddenException() => new()
    {
        HttpStatusCode = HttpStatusCode.Forbidden,
        ErrorDescription = "You have no permission for this action."
    };

    private static Problem SanitizeAlreadyExistsException() => new()
    {
        HttpStatusCode = HttpStatusCode.Conflict,
        ErrorDescription = "Such resource already exists."
    };
}