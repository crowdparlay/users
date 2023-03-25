using CrowdParlay.Auth.Application.Abstractions;
using CrowdParlay.Auth.Application.Abstractions.Identity;
using CrowdParlay.Auth.Application.Exceptions;
using CrowdParlay.Auth.Application.Extensions;
using FluentValidation;
using Mediator;

namespace CrowdParlay.Auth.Application.Features.Users.Commands;

public static class Register
{
    public sealed record Command(string Email, string Password) : IRequest<Response>;

    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator(IPasswordValidator<Command> passwordValidator)
        {
            RuleFor(x => x.Email).EmailAddress();
            RuleFor(x => x.Password).Apply(passwordValidator);
        }
    }

    internal sealed class Handler : IRequestHandler<Command, Response>
    {
        private readonly IUserService _users;
        private readonly ICurrentUserProvider _issuer;

        public Handler(IUserService users, ICurrentUserProvider issuer)
        {
            _users = users;
            _issuer = issuer;
        }

        public async ValueTask<Response> Handle(Command request, CancellationToken cancellationToken)
        {
            if (_issuer.IsAuthenticated)
                throw new ForbiddenException();

            var sameExists = await _users.FindByEmailAsync(request.Email) is not null;
            if (sameExists)
                throw new AlreadyExistsException("User with the specified email already exists.");

            var errorDescriptions = await _users.CreateAsync(request.Email, request.Password);
            if (errorDescriptions is not null)
                throw new Exceptions.ValidationException(nameof(request.Password), errorDescriptions);

            var user =
                await _users.FindByEmailAsync(request.Email)
                ?? throw new InvalidOperationException();

            return new Response(user.Id, user.Email!);
        }
    }

    public sealed record Response(Guid Id, string Email);
}