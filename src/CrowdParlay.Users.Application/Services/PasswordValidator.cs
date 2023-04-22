using CrowdParlay.Users.Application.Extensions;
using CrowdParlay.Users.Domain.Entities;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using ValidationException = CrowdParlay.Users.Application.Exceptions.ValidationException;

namespace CrowdParlay.Users.Application.Services;

public class PasswordValidator<T> : Abstractions.IPasswordValidator<T>
{
    private readonly UserManager<User> _userManager;

    public PasswordValidator(UserManager<User> userManager) =>
        _userManager = userManager;

    public async Task ValidateAsync(ValidationContext<T> context, string value)
    {
        var validationTasks = _userManager.PasswordValidators
            .Select(validator => validator.ValidateAsync(_userManager, null!, value))
            .ToArray();

        var validationResults = await Task.WhenAll(validationTasks);
        var validationErrors = validationResults
            .SelectMany(result => result.Errors)
            .ToArray();

        if (validationErrors.Any())
            throw new ValidationException(validationErrors.ToValidationFailures(context.PropertyName));
    }
}