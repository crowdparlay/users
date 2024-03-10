using CrowdParlay.Users.Domain.Abstractions;
using CrowdParlay.Users.gRPC;
using Dodo.Primitives;
using Google.Protobuf.WellKnownTypes;
using Google.Rpc;
using Grpc.Core;
using Mapster;
using Status = Google.Rpc.Status;

namespace CrowdParlay.Users.Api.Services.gRPC;

public class UsersGrpcService : UsersService.UsersServiceBase
{
    private readonly IUsersRepository _users;

    public UsersGrpcService(IUsersRepository users) => _users = users;

    public override async Task<User> GetUser(GetUserRequest request, ServerCallContext context)
    {
        var user = await _users.GetByIdAsync(Uuid.Parse(request.Id));
        return user.Adapt<User>();
    }

    public override async Task GetUsers(GetUsersRequest request, IServerStreamWriter<User> responseStream, ServerCallContext context)
    {
        if (request.Ids.Count <= 200)
        {
            var userIds = request.Ids.Select(Guid.Parse);
            var users = _users.GetByIdsAsync(userIds.ToArray());

            await foreach (var user in users)
                await responseStream.WriteAsync(user.Adapt<User>());

            return;
        }

        var status = new Status
        {
            Code = (int)Code.InvalidArgument,
            Message = "Bad Request",
            Details =
            {
                Any.Pack(new BadRequest
                {
                    FieldViolations =
                    {
                        new BadRequest.Types.FieldViolation
                        {
                            Field = nameof(request),
                            Description = "Sequence exceeds the limit of 200 items."
                        }
                    }
                })
            }
        };

        throw status.ToRpcException();
    }
}