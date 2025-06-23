using CrowdParlay.Communication;
using CrowdParlay.Users.Application.Abstractions;
using CrowdParlay.Users.Application.Exceptions;
using CrowdParlay.Users.Application.Extensions;
using CrowdParlay.Users.Domain.Abstractions;
using FluentValidation;
using MassTransit;
using Mediator;
using ValidationException = CrowdParlay.Users.Application.Exceptions.ValidationException;

namespace CrowdParlay.Users.Application.Features.Users.Commands;

public static class Update
{
    public sealed record Command(
        Guid Id,
        string? Username,
        string? DisplayName,
        string? Email,
        string? AvatarUrl,
        string? OldPassword,
        string? NewPassword) : IRequest<Response>;

    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(command => command.Id).NotEmpty();

            RuleFor(command => command.OldPassword)
                .NotEqual(command => command.NewPassword)
                .When(command => command.NewPassword is not null);

            When(command => command.NewPassword is not null, () =>
                RuleFor(command => command.NewPassword)
                    .NotEqual(command => command.OldPassword)
                    .Password());

            RuleFor(command => command.Username).Username()
                .When(command => command.Username is not null);

            RuleFor(command => command.DisplayName).DisplayName()
                .When(command => command.DisplayName is not null);

            RuleFor(command => command.Email).Email()
                .When(command => command.Email is not null);
        }
    }

    public sealed class Handler : IRequestHandler<Command, Response>
    {
        private readonly IUsersRepository _users;
        private readonly IPasswordService _passwords;
        private readonly IPublishEndpoint _broker;

        public Handler(IUsersRepository users, IPasswordService passwords, IPublishEndpoint broker)
        {
            _users = users;
            _passwords = passwords;
            _broker = broker;
        }

        public async ValueTask<Response> Handle(Command request, CancellationToken cancellationToken)
        {
            var user =
                await _users.GetByIdAsync(request.Id, cancellationToken)
                ?? throw new NotFoundException("User with the specified ID doesn't exist.");

            // If you want to change the password
            if (request.NewPassword is not null)
            {
                // Which you don't have set yet
                if (user.PasswordHash is null)
                {
                    // Then you must leave the old password unspecified
                    if (request.OldPassword is not null)
                        throw new ValidationException(nameof(request.OldPassword), "This account is not protected with a password yet.");
                }
                // Which you already have set
                else
                {
                    // Then you must specify the old password correctly
                    if (!_passwords.VerifyPassword(user.PasswordHash, request.OldPassword!))
                        throw new ValidationException(nameof(request.OldPassword), "The specified password is invalid.");
                }

                // For the changes to take effect
                user.PasswordHash = _passwords.HashPassword(request.NewPassword);
            }

            user.Username = request.Username ?? user.Username;
            user.DisplayName = request.DisplayName ?? user.DisplayName;
            user.Email = request.Email ?? user.Email;
            user.AvatarUrl = request.AvatarUrl ?? user.AvatarUrl;
            await _users.UpdateAsync(user, cancellationToken);

            var @event = new UserUpdatedEvent(user.Id.ToString(), user.Username, user.DisplayName, user.AvatarUrl);
            await _broker.Publish(@event, cancellationToken);

            return new Response(user.Id, user.Username, user.DisplayName, user.Email, user.AvatarUrl);
        }
    }

    public sealed record Response(
        Guid Id,
        string Username,
        string DisplayName,
        string Email,
        string? AvatarUrl);
}