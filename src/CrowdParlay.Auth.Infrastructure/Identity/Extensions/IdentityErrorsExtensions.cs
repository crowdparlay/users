using FluentValidation.Results;
using Microsoft.AspNetCore.Identity;

namespace CrowdParlay.Auth.Infrastructure.Identity.Extensions;

public static class IdentityErrorsExtensions
{
    public static IEnumerable<ValidationFailure> ToValidationFailures(this IEnumerable<IdentityError> errors, string propertyName) => errors
        .Select(error => new ValidationFailure(
            propertyName: propertyName,
            errorMessage: error.Description))
        .ToArray();
}