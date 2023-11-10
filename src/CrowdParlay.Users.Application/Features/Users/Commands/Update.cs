using CrowdParlay.Communication;
using CrowdParlay.Users.Application.Abstractions;
using CrowdParlay.Users.Application.Exceptions;
using CrowdParlay.Users.Application.Extensions;
using CrowdParlay.Users.Domain.Abstractions;
using Dodo.Primitives;
using FluentValidation;
using MassTransit;
using Mediator;

namespace CrowdParlay.Users.Application.Features.Users.Commands;

public static class Update
{
    public sealed record Command(
        Uuid Id,
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
            RuleFor(x => x.Id).NotEmpty();
            RuleFor(x => x.OldPassword).NotEqual(x => x.NewPassword).When(x => x.NewPassword is not null);
            RuleFor(x => x.NewPassword).NotEqual(x => x.OldPassword).When(x => x.OldPassword is not null);
            RuleFor(x => x.Username).Username().When(x => x.Username is not null);
            RuleFor(x => x.DisplayName).DisplayName().When(x => x.DisplayName is not null);
            RuleFor(x => x.Email).Email().When(x => x.Email is not null);
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

            user.Username = request.Username ?? user.Username;
            user.DisplayName = request.DisplayName ?? user.DisplayName;
            user.Email = request.Email ?? user.Email;
            user.AvatarUrl = request.AvatarUrl ?? user.AvatarUrl;

            if (request.OldPassword is not null && _passwords.VerifyPassword(user.PasswordHash, request.OldPassword))
                user.PasswordHash = _passwords.HashPassword(request.NewPassword!);

            await _users.UpdateAsync(user, cancellationToken);

            var @event = new UserUpdatedEvent(user.Id.ToString(), user.Username, user.DisplayName, user.AvatarUrl);
            await _broker.Publish(@event, cancellationToken);

            return new Response(user.Id, user.Username, user.Email, user.DisplayName, user.AvatarUrl);
        }
    }

    public sealed record Response(
        Uuid Id,
        string Username,
        string Email,
        string DisplayName,
        string? AvatarUrl);
}