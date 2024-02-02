using FluentValidation;

namespace CrowdParlay.Users.Application.Extensions;

public static class ValidationExtensions
{
    public static IRuleBuilderOptions<T, string?> Username<T>(this IRuleBuilder<T, string?> ruleBuilder) => ruleBuilder
        .NotEmpty()
        .Length(5, 25)
        .Must(x => x.Any(char.IsLetter)).WithMessage("Username must contain a letter.")
        .Matches("^ *[A-Za-z0-9_]*$ *");

    public static IRuleBuilderOptions<T, string?> DisplayName<T>(this IRuleBuilder<T, string?> ruleBuilder) => ruleBuilder
        .NotEmpty()
        .MaximumLength(25);

    public static IRuleBuilder<T, string> Password<T>(this IRuleBuilder<T, string> ruleBuilder) => ruleBuilder
        .NotEmpty()
        .Length(5, 25)
        .Must(x => x.Any(char.IsDigit)).WithMessage("Password must contain a digit.")
        .Must(x => x.Any(char.IsLetter)).WithMessage("Password must contain a letter.")
        .Matches("^[A-Za-z0-9!?_@#]*$").WithMessage("Password can only contain letters, digits and characters ! ? _ @ #");

    public static IRuleBuilderOptions<T, string?> Email<T>(this IRuleBuilder<T, string?> ruleBuilder) => ruleBuilder
        .NotEmpty()
        .Length(5, 50)
        .Matches(@"^ *[\w\.-]+@[\w\.-]+\.\w+ *$").WithMessage("Invalid email address format.");
}