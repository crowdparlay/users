using CrowdParlay.Users.Application.Features.Users.Commands;
using CrowdParlay.Users.gRPC;
using CrowdParlay.Users.IntegrationTests.Fixtures;
using FluentAssertions;
using Grpc.Core;
using Grpc.Net.Client;
using Mediator;
using Microsoft.Extensions.DependencyInjection;

namespace CrowdParlay.Users.IntegrationTests.Tests;

public class UsersGrpcServiceTests : IAssemblyFixture<WebApplicationFixture>
{
    private readonly IServiceProvider _services;
    private readonly GrpcChannel _channel;

    public UsersGrpcServiceTests(WebApplicationFixture fixture)
    {
        var client = fixture.WebApplicationFactory.CreateClient();
        _channel = GrpcChannel.ForAddress(client.BaseAddress!, new GrpcChannelOptions { HttpClient = client });
        _services = fixture.Services;
    }

    [Fact(DisplayName = "Get user by ID")]
    public async Task GetUserById()
    {
        // Arrange
        await using var scope = _services.CreateAsyncScope();
        var mediator = scope.ServiceProvider.GetRequiredService<ISender>();

        var user = await mediator.Send(new Register.Command(
            username: "john_doe",
            displayName: "John Doe",
            email: "johndoe@example.com",
            avatarUrl: null,
            password: "Test_Pa55w0RD!"));

        // Act
        var client = new UsersService.UsersServiceClient(_channel);
        var request = new GetUserRequest { Id = user.Id.ToString() };
        var response = await client.GetUserAsync(request);

        // Assert
        response.Should().BeEquivalentTo(new User
        {
            Id = user.Id.ToString(),
            Username = user.Username,
            DisplayName = user.DisplayName,
            AvatarUrl = user.AvatarUrl
        });
    }

    [Fact(DisplayName = "Get users by IDs")]
    public async Task GetUsersByIds()
    {
        // Arrange
        await using var scope = _services.CreateAsyncScope();
        var mediator = scope.ServiceProvider.GetRequiredService<ISender>();

        var users = new List<Register.Response>();
        for (var i = 0; i < 300; i++)
        {
            var user = await mediator.Send(new Register.Command(
                username: Guid.NewGuid().ToString("N")[..25],
                displayName: "John Doe",
                email: $"john{i}@example.com",
                avatarUrl: null,
                password: "Test_Pa55w0RD!"));

            users.Add(user);
        }

        var firstUsers = users.Take(120).ToArray();
        var firstUsersIds = firstUsers.Select(user => user.Id.ToString());
        var expectedResponse = firstUsers.Select(user => new User
        {
            Id = user.Id.ToString(),
            Username = user.Username,
            DisplayName = user.DisplayName,
            AvatarUrl = user.AvatarUrl
        });

        // Act
        var client = new UsersService.UsersServiceClient(_channel);
        var request = new GetUsersRequest { Ids = { firstUsersIds } };
        var actualResponse = client.GetUsers(request).ResponseStream.ReadAllAsync().ToBlockingEnumerable();

        // Assert
        actualResponse.Should().BeEquivalentTo(expectedResponse);
    }
}