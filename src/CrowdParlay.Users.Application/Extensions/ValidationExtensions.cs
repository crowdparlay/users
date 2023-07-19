using FluentValidation;

namespace CrowdParlay.Users.Application.Extensions;

public static class ValidationExtensions
{
    public static IRuleBuilder<T, string> Username<T>(this IRuleBuilder<T, string> ruleBuilder) => ruleBuilder
        .NotEmpty()
        .Length(5, 25)
        .Matches(@"\w*");

    public static IRuleBuilder<T, string> DisplayName<T>(this IRuleBuilder<T, string> ruleBuilder) => ruleBuilder
        .NotEmpty()
        .MaximumLength(25);

    public static IRuleBuilder<T, string> Password<T>(this IRuleBuilder<T, string> ruleBuilder) => ruleBuilder
        .NotEmpty()
        .Length(5, 25)
        .Must(x => x.Any(char.IsDigit)).WithMessage("Username must contain a digit.")
        .Must(x => x.Any(char.IsLetter)).WithMessage("Username must contain a letter.")
        .Matches(@"[\w!?@#]*");
}