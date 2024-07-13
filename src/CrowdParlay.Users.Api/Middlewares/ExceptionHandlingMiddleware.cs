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
            var response = exception switch
            {
                ValidationException e => SanitizeValidationException(e),
                FluentValidation.ValidationException e => SanitizeFluentValidationException(e),
                NotFoundException => SanitizeNotFoundException(),
                ForbiddenException => SanitizeForbiddenException(),
                AlreadyExistsException => SanitizeAlreadyExistsException(),
                _ => SanitizeGenericException()
            };

            context.Response.ContentType = "application/problem+json";
            context.Response.StatusCode = (int)response.StatusCode;
            await context.Response.WriteAsJsonAsync(response.Problem, response.Problem.GetType(), GlobalSerializerOptions.SnakeCase);
        }
    }

    private static ProblemResponse SanitizeGenericException() => new(
        HttpStatusCode.InternalServerError,
        new Problem("Something went wrong. Try again later."));

    private static ProblemResponse SanitizeValidationException(ValidationException exception) => new(
        HttpStatusCode.BadRequest,
        new ValidationProblem("The specified data is invalid.", exception.Errors.ToDictionary(
            error => error.Key,
            error => error.Value.ToArray())));

    private static ProblemResponse SanitizeFluentValidationException(FluentValidation.ValidationException exception) => new(
        HttpStatusCode.BadRequest,
        new ValidationProblem("The specified data is invalid.", exception.Errors
            .GroupBy(error => error.PropertyName)
            .ToDictionary(
                group => group.Key,
                group => group
                    .Select(failure => failure.ErrorMessage)
                    .ToArray())));

    private static ProblemResponse SanitizeNotFoundException() => new(
        HttpStatusCode.NotFound,
        new Problem("The requested resource doesn't exist."));

    private static ProblemResponse SanitizeForbiddenException() => new(
        HttpStatusCode.Forbidden,
        new Problem("You have no permission for this action."));

    private static ProblemResponse SanitizeAlreadyExistsException() => new(
        HttpStatusCode.Conflict,
        new Problem("Such resource already exists."));

    private record ProblemResponse(HttpStatusCode StatusCode, Problem Problem);
}