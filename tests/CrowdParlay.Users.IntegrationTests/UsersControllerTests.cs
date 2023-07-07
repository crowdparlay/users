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
        var registrationResponse = await client.PostAsync("/api/users/register", new StringContent(
            """
            {
                "username": "undrcrxwn123",
                "displayName": "Степной ишак",
                "password": "qwerty123!"
            }
            """, Encoding.UTF8, MediaTypeNames.Application.Json));
        var registerDto = JsonConvert.DeserializeObject<Register.Response>(
            await registrationResponse.Content.ReadAsStringAsync()
            );
        if(registerDto is null)
            Assert.Fail("Response from /api/users/register is null.");
        
        var readResponse = await client.GetAsync($"/api/users/read/{registerDto.Id}");
        var dto = JsonConvert.DeserializeObject<Read.Response>(
            await readResponse.Content.ReadAsStringAsync()
        );
        
        if(dto is null)
            Assert.Fail("Response from /api/users/read is null.");

        readResponse.EnsureSuccessStatusCode();
        readResponse.Should().HaveStatusCode(HttpStatusCode.OK);
        
        dto.Should().NotBeNull();
        dto.UserId.Should().Be(registerDto.Id);
        dto.Username.Should().Be(registerDto.Username);
        dto.DisplayName.Should()
            .Be("Степной ишак"); // Не имеем DisplayName в ответе от Register, поэтому использую строку
    }

    
    [Theory(Timeout = 5000), Setups(typeof(TestContainersSetup), typeof(ServerSetup))]
    public async Task UpdateUser_ShouldResponseUpdatedUser(HttpClient client)
    {
        var registrationResponse = await client.PostAsync("/api/users/register", new StringContent(
            """
            {
                "username": "undrcrxwn123",
                "displayName": "Степной ишак",
                "password": "qwerty123!"
            }
            """, Encoding.UTF8, MediaTypeNames.Application.Json));
        var registerDto = JsonConvert.DeserializeObject<Register.Response>(
            await registrationResponse.Content.ReadAsStringAsync()
            );
        if(registerDto is null)
            Assert.Fail("Response from /api/users/register is null.");
        
        var updateResponse = await client.PostAsync($"/api/users/update/{registerDto.Id}", new StringContent(
            """     
            {
                "username": "akavi",
                "displayName": "Akavi",
                "oldPassword": "qwerty123!",
                "newPassword": "someNewItem!"
            }
            """, Encoding.UTF8, MediaTypeNames.Application.Json));
        var dto = JsonConvert.DeserializeObject<Update.Response>(
            await updateResponse.Content.ReadAsStringAsync()
        );
        
        if(dto is null)
            Assert.Fail("Response from /api/users/update is null.");

        updateResponse.EnsureSuccessStatusCode();
        updateResponse.Should().HaveStatusCode(HttpStatusCode.OK);
        
        dto.Should().NotBeNull();
        dto.Username.Should().Be("akavi");
        dto.DisplayName.Should()
            .Be("Akavi");
    }
}