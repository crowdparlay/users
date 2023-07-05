using CrowdParlay.Communication.Abstractions;
using CrowdParlay.Users.Application.Exceptions;
using CrowdParlay.Users.Domain.Abstractions;
using Dodo.Primitives;
using FluentValidation;
using Mediator;

namespace CrowdParlay.Users.Application.Features.Users.Commands;

public static class Read
{
    public sealed record Command(Uuid Id) : IRequest<Response>;
    
    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Id).NotEmpty();
        }
    }

    public sealed class Handler : IRequestHandler<Command, Response>
    {
        private readonly IUsersRepository _users;
        
        public Handler(IUsersRepository users)
        {
            _users = users;
        }
        
        public async ValueTask<Response> Handle(Command request, CancellationToken cancellationToken)
        {
            var user = await _users.GetByIdAsync(request.Id, cancellationToken);
            if (user is null)
                throw new NotFoundException("User with the specified ID doesn't exist.");

            return new Response(user.Id, user.Username, user.DisplayName);
        }
    }


    public sealed record Response(Uuid UserId, string Username, string DisplayName);
}