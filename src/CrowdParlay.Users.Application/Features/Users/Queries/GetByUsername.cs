using CrowdParlay.Users.Application.Exceptions;
using CrowdParlay.Users.Domain.Abstractions;
using Dodo.Primitives;
using Mediator;

namespace CrowdParlay.Users.Application.Features.Users.Queries;

public static class GetByUsername
{
    public sealed record Query(string Username) : IRequest<Response>;

    public sealed class Handler : IRequestHandler<Query, Response>
    {
        private readonly IUsersRepository _users;

        public Handler(IUsersRepository users) => _users = users;

        public async ValueTask<Response> Handle(Query request, CancellationToken cancellationToken)
        {
            var user =
                await _users.GetByUsernameExactAsync(request.Username, cancellationToken)
                ?? throw new NotFoundException("User with the specified username doesn't exist.");

            return new Response(user.Id, user.Username, user.DisplayName, user.AvatarUrl);
        }
    }


    public sealed record Response(Uuid Id, string Username, string DisplayName, string? AvatarUrl);
}