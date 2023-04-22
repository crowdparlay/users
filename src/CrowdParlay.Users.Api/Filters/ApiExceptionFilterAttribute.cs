using CrowdParlay.Users.Application.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CrowdParlay.Users.Api.Filters;

/// <summary>
/// An exception filter that modifies the given HTTP context state in accordance with the provided custom exception data.
/// Is responsible for generating custom error responses, such as validation, authorization errors and others.
/// </summary>
public class ApiExceptionFilterAttribute : ExceptionFilterAttribute
{
    private static readonly Dictionary<Type, Action<ExceptionContext>> ExceptionHandlers = new()
    {
        [typeof(ValidationException)] = HandleValidationException,
        [typeof(FluentValidation.ValidationException)] = HandleFluentValidationException,
        [typeof(NotFoundException)] = HandleNotFoundException,
        [typeof(UnauthorizedAccessException)] = HandleUnauthorizedAccessException,
        [typeof(ForbiddenException)] = HandleAccessDeniedException,
        [typeof(AlreadyExistsException)] = HandleAlreadyExistsException
    };

    public override void OnException(ExceptionContext context)
    {
        HandleException(context);
        base.OnException(context);
    }

    private static void HandleException(ExceptionContext context)
    {
        var type = context.Exception.GetType();

        if (ExceptionHandlers.ContainsKey(type))
        {
            ExceptionHandlers[type].Invoke(context);
            return;
        }

        if (!context.ModelState.IsValid)
            HandleInvalidModelStateException(context);
    }

    private static void HandleValidationException(ExceptionContext context)
    {
        var exception = (ValidationException)context.Exception;

        var errors = exception.Errors.ToDictionary(
            error => error.Key,
            error => error.Value.ToArray());
        
        var details = new ValidationProblemDetails(errors)
        {
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1"
        };

        context.Result = new BadRequestObjectResult(details);
        context.ExceptionHandled = true;
    }

    private static void HandleFluentValidationException(ExceptionContext context)
    {
        var exception = (FluentValidation.ValidationException)context.Exception;

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

        context.Result = new BadRequestObjectResult(details);
        context.ExceptionHandled = true;
    }

    private static void HandleInvalidModelStateException(ExceptionContext context)
    {
        var details = new ValidationProblemDetails(context.ModelState)
        {
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1"
        };

        context.Result = new BadRequestObjectResult(details);
        context.ExceptionHandled = true;
    }

    private static void HandleNotFoundException(ExceptionContext context)
    {
        var exception = (NotFoundException)context.Exception;

        var details = new ProblemDetails
        {
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.4",
            Title = "Not found",
            Detail = exception.Message
        };

        context.Result = new NotFoundObjectResult(details);
        context.ExceptionHandled = true;
    }

    private static void HandleUnauthorizedAccessException(ExceptionContext context)
    {
        var details = new ProblemDetails
        {
            Status = StatusCodes.Status401Unauthorized,
            Title = "Unauthorized",
            Type = "https://tools.ietf.org/html/rfc7235#section-3.1"
        };

        context.Result = new ObjectResult(details)
        {
            StatusCode = StatusCodes.Status401Unauthorized
        };

        context.ExceptionHandled = true;
    }

    private static void HandleAccessDeniedException(ExceptionContext context)
    {
        var details = new ProblemDetails
        {
            Status = StatusCodes.Status403Forbidden,
            Title = "Forbidden",
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.3"
        };

        context.Result = new ObjectResult(details)
        {
            StatusCode = StatusCodes.Status403Forbidden
        };

        context.ExceptionHandled = true;
    }

    private static void HandleAlreadyExistsException(ExceptionContext context)
    {
        var details = new ProblemDetails
        {
            Status = StatusCodes.Status409Conflict,
            Title = "Already exists",
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.8"
        };

        context.Result = new ObjectResult(details)
        {
            StatusCode = StatusCodes.Status409Conflict
        };

        context.ExceptionHandled = true;
    }
}