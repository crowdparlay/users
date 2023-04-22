using FluentValidation;

namespace CrowdParlay.Users.Application.Abstractions;

public interface ICustomValidator<T, in TProperty>
{
    public Task ValidateAsync(ValidationContext<T> context, TProperty value);
}