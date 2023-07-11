using System.Net;
using System.Net.Http.Json;
using CrowdParlay.Users.Application.Features.Users.Commands;
using CrowdParlay.Users.Application.Features.Users.Queries;
using CrowdParlay.Users.IntegrationTests.Attribute;
using CrowdParlay.Users.IntegrationTests.Setups;
using FluentAssertions;

namespace CrowdParlay.Users.IntegrationTests.Tests;

public class UsersControllerTests
{
    [Theory(Timeout = 5000), Setups(typeof(TestContainersSetup), typeof(ServerSetup))]
    public async Task GetByIdRequest_ShouldReturn_SuccessResponse(HttpClient client)
    {
        // Arrange
        var registerRequest = new Register.Command("undrcrxwn123", "Степной ишак", "qwerty123!");
        var registerMessage = await client.PostAsJsonAsync("/api/users/register", registerRequest);
        var registerResponse = await registerMessage.Content.ReadFromJsonAsync<Register.Response>();

        // Act
        var getByIdMessage = await client.GetAsync($"/api/users/{registerResponse!.Id}");

        // Assert
        getByIdMessage.Should().HaveStatusCode(HttpStatusCode.OK);

        var getByIdResponse = await getByIdMessage.Content.ReadFromJsonAsync<GetById.Response>();
        getByIdResponse.Should().Be(new GetById.Response(
            registerResponse.Id,
            registerRequest.Username,
            registerRequest.DisplayName));
    }

    [Theory(Timeout = 5000), Setups(typeof(TestContainersSetup), typeof(ServerSetup))]
    public async Task UpdateUser_ShouldChange_User(HttpClient client)
    {
        // Arrange
        var registerRequest = new Register.Command("undrcrxwn123", "Степной ишак", "qwerty123!");
        var registerMessage = await client.PostAsJsonAsync("/api/users/register", registerRequest);
        var registerResponse = await registerMessage.Content.ReadFromJsonAsync<Register.Response>()!;

        var updateRequest = new Update.Command(
            Id: registerResponse!.Id,
            Username: "akavi",
            DisplayName: "Akavi",
            OldPassword: null,
            NewPassword: null);

        // Act
        var updateMessage = await client.PutAsJsonAsync($"/api/users/{updateRequest.Id}", updateRequest);

        // Assert
        updateMessage.Should().HaveStatusCode(HttpStatusCode.OK);

        var updateResponse = await updateMessage.Content.ReadFromJsonAsync<Update.Response>();
        updateResponse.Should().Be(new Update.Response(
            registerResponse.Id,
            updateRequest.Username!,
            updateRequest.DisplayName!));
    }

    [Theory(Timeout = 5000), Setups(typeof(TestContainersSetup), typeof(ServerSetup))]
    public async Task UpdatePassword_ShouldChange_OnlyPassword(HttpClient client)
    {
        // Arrange
        var registerRequest = new Register.Command("undrcrxwn123", "Степной ишак", "qwerty123!");
        var registerMessage = await client.PostAsJsonAsync("/api/users/register", registerRequest);
        var registerResponse = await registerMessage.Content.ReadFromJsonAsync<Register.Response>()!;

        var updateRequest = new Update.Command(
            Id: registerResponse!.Id,
            Username: null,
            DisplayName: null,
            OldPassword: registerRequest.Password,
            NewPassword: "someNewPassword!");

        // Act
        var updateMessage = await client.PutAsJsonAsync($"/api/users/{updateRequest.Id}", updateRequest);

        // Assert
        updateMessage.Should().HaveStatusCode(HttpStatusCode.OK);

        var updateResponse = await updateMessage.Content.ReadFromJsonAsync<Update.Response>();
        updateResponse.Should().Be(new Update.Response(
            registerResponse.Id,
            registerRequest.Username,
            registerRequest.DisplayName));
    }
}