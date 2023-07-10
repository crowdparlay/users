using System.Net;
using System.Net.Mime;
using System.Text;
using CrowdParlay.Users.Application.Features.Users.Commands;
using CrowdParlay.Users.IntegrationTests.Attribute;
using CrowdParlay.Users.IntegrationTests.Setups;
using FluentAssertions;
using Newtonsoft.Json;

namespace CrowdParlay.Users.IntegrationTests;

public class UsersControllerTests
{

    [Theory(Timeout = 5000), Setups(typeof(TestContainersSetup), typeof(ServerSetup))]
    public async Task ReadUser_ShouldResponse(HttpClient client)
    {
        var registration = await client.PostAsync("/api/users/register", new StringContent(
            """
            {
                "username": "undrcrxwn123",
                "displayName": "Степной ишак",
                "password": "qwerty123!"
            }
            """, Encoding.UTF8, MediaTypeNames.Application.Json));
        var register = JsonConvert.DeserializeObject<Register.Response>(
            await registration.Content.ReadAsStringAsync()
            );
        register.Should().NotBeNull();
        
        var response = await client.GetAsync($"/api/users/read/{register!.Id}");
        var user = await response.Content.ReadAsStringAsync();
        var dto = JsonConvert.DeserializeObject<Read.Response>(
            user
        );

        response.EnsureSuccessStatusCode();
        response.Should().HaveStatusCode(HttpStatusCode.OK);
        
        dto.Should().NotBeNull();
        dto.Id.Should().Be(register.Id);
        dto.Username.Should().Be(register.Username);
        dto.DisplayName.Should().Be("Степной ишак"); // Не имеем DisplayName в ответе от Register, поэтому использую строку
    }

    
    [Theory(Timeout = 5000), Setups(typeof(TestContainersSetup), typeof(ServerSetup))]
    public async Task UpdateUser_ShouldResponseUpdatedUser(HttpClient client)
    {
        var registration = await client.PostAsync("/api/users/register", new StringContent(
            """
            {
                "username": "undrcrxwn123",
                "displayName": "Степной ишак",
                "password": "qwerty123!"
            }
            """, Encoding.UTF8, MediaTypeNames.Application.Json));
        var createdUser = await registration.Content.ReadAsStringAsync();
        var register = JsonConvert.DeserializeObject<Register.Response>(
            createdUser
        );
        register.Should().NotBeNull();
        
        var update = await client.PostAsync($"/api/users/update/{register.Id}", new StringContent(
            """     
            {
                "username": "akavi",
                "displayName": "Akavi",
                "oldPassword": "qwerty123!",
                "newPassword": "someNewItem!"
            }
            """, Encoding.UTF8, MediaTypeNames.Application.Json));
        var updatedUser = await update.Content.ReadAsStringAsync();
        var dto = JsonConvert.DeserializeObject<Update.Response>(
            updatedUser
        );

        update.EnsureSuccessStatusCode();
        update.Should().HaveStatusCode(HttpStatusCode.OK);
        
        dto.Should().NotBeNull();
        dto.Username.Should().Be("akavi");
        dto.DisplayName.Should().Be("Akavi");
    }
}