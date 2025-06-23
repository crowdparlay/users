using CrowdParlay.Users.Domain;
using CrowdParlay.Users.Domain.Abstractions;
using FluentValidation;
using Mediator;

namespace CrowdParlay.Users.Application.Features.Users.Queries;

public static class Search
{
    public sealed record Query(SortingStrategy Order, int Offset, int Count) : IRequest<Page<Response>>;

    public sealed class Validator : AbstractValidator<Query>
    {
        public Validator()
        {
            RuleFor(x => x.Offset).GreaterThanOrEqualTo(0);
            RuleFor(x => x.Count).InclusiveBetween(1, 100);
        }
    }

    public sealed class Handler : IRequestHandler<Query, Page<Response>>
    {
        private readonly IUsersRepository _users;

        public Handler(IUsersRepository users) => _users = users;

        public async ValueTask<Page<Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            var page = await _users.SearchAsync(request.Order, request.Offset, request.Count, cancellationToken);
            return new Page<Response>(
                page.TotalCount,
                page.Items.Select(user => new Response(
                    user.Id,
                    user.Username,
                    user.DisplayName,
                    user.AvatarUrl)));
        }
    }

    public sealed record Response(Guid Id, string Username, string DisplayName, string? AvatarUrl);
}