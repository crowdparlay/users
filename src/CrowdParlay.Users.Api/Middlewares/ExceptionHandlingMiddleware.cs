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
            var response = exception switch
            {
                ValidationException validationException => HandleValidationException(validationException),
                FluentValidation.ValidationException validationException => HandleFluentValidationException(validationException),
                NotFoundException => HandleNotFoundException(),
                ForbiddenException => HandleForbiddenException(),
                AlreadyExistsException => HandleAlreadyExistsException(),
                _ => HandleException(exception)
            };

            context.Response.ContentType = "application/problem+json";
            context.Response.StatusCode = (int)response.StatusCode;
            await context.Response.WriteAsJsonAsync(response.Problem, response.Problem.GetType(), GlobalSerializerOptions.SnakeCase);
        }
    }

    private ProblemResponse HandleException(Exception exception)
    {
        _logger.LogError(exception, "{ExceptionMessage}", exception.Message);
        return new ProblemResponse(
            HttpStatusCode.InternalServerError,
            new Problem("Something went wrong. Try again later."));
    }

    private static ProblemResponse HandleValidationException(ValidationException exception) => new(
        HttpStatusCode.BadRequest,
        new ValidationProblem("The specified data is invalid.", exception.Errors.ToDictionary(
            error => error.Key,
            error => error.Value.ToArray())));

    private static ProblemResponse HandleFluentValidationException(FluentValidation.ValidationException exception) => new(
        HttpStatusCode.BadRequest,
        new ValidationProblem("The specified data is invalid.", exception.Errors
            .GroupBy(error => error.PropertyName)
            .ToDictionary(
                group => group.Key,
                group => group
                    .Select(failure => failure.ErrorMessage)
                    .ToArray())));

    private static ProblemResponse HandleNotFoundException() => new(
        HttpStatusCode.NotFound,
        new Problem("The requested resource doesn't exist."));

    private static ProblemResponse HandleForbiddenException() => new(
        HttpStatusCode.Forbidden,
        new Problem("You have no permission for this action."));

    private static ProblemResponse HandleAlreadyExistsException() => new(
        HttpStatusCode.Conflict,
        new Problem("Such resource already exists."));

    private record ProblemResponse(HttpStatusCode StatusCode, Problem Problem);
}