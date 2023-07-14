using CrowdParlay.Communication;
using CrowdParlay.Communication.Abstractions;
using CrowdParlay.Users.Application.Abstractions;
using CrowdParlay.Users.Application.Exceptions;
using CrowdParlay.Users.Domain.Abstractions;
using Dodo.Primitives;
using FluentValidation;
using Mediator;

namespace CrowdParlay.Users.Application.Features.Users.Commands;

public static class Update
{
    public sealed record Command(
        Uuid Id,
        string? Username,
        string? DisplayName,
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
        }
    }

    public sealed class Handler : IRequestHandler<Command, Response>
    {
        private readonly IUsersRepository _users;
        private readonly IPasswordService _passwords;
        private readonly IMessageBroker _broker;

        public Handler(IUsersRepository users, IPasswordService passwords, IMessageBroker broker)
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
            user.AvatarUrl = request.AvatarUrl ?? user.AvatarUrl;

            if (request.OldPassword is not null && _passwords.VerifyPassword(user.PasswordHash, request.OldPassword))
                user.PasswordHash = _passwords.HashPassword(request.NewPassword!);

            await _users.UpdateAsync(user, cancellationToken);

            var @event = new UserUpdatedEvent(user.Id.ToString(), user.Username, user.DisplayName, user.AvatarUrl);
            _broker.Users.Publish(@event);

            return new Response(user.Id, user.Username, user.DisplayName, user.AvatarUrl);
        }
    }

    public sealed record Response(Uuid Id, string Username, string DisplayName, string? AvatarUrl);
}