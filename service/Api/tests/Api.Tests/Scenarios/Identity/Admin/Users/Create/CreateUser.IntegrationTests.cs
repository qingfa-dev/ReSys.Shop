using System.Net;

using Api.Tests.Infrastructure;
using Api.Tests.Infrastructure.Auth;
using Api.Tests.Scenarios.Identity.Helpers;

using Module.Identity.Features.Admin.Users.Shared.Models;

namespace Api.Tests.Scenarios.Identity.Admin.Users.Create;

public sealed class CreateUserIntegrationTests(ApiFixture fixture) : IdentityIntegrationTestBase(fixture)
{
    [Fact]
    public async Task CreateUser_WithValidRequest_Returns201()
    {
        string userName = IdentityTestHelper.ValidUserName();
        string email = IdentityTestHelper.ValidEmail();

        var request = new
        {
            email,
            userName,
            firstName = "John",
            lastName = "Doe",
            emailConfirmed = true,
            phoneNumberConfirmed = false
        };

        HttpResponseMessage response = await Client.PostAsAdminRawAsync(
            "/api/identity/users", request);
        ApiResponse result = await response.ReadApiResponseAsync();

        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.OK);
        UserDetailResponse? value = result.DeserializeValue<UserDetailResponse>();
        value.Should().NotBeNull();
        value!.Id.Should().NotBeEmpty();
        value.Email.Should().Be(email);
        value.UserName.Should().Be(userName);
        value.FirstName.Should().Be("John");
        value.LastName.Should().Be("Doe");
        value.IsActive.Should().BeTrue();
        value.CreatedAtUtc.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(30));
    }

    [Fact]
    public async Task CreateUser_WithDuplicateEmail_Returns409()
    {
        string email = IdentityTestHelper.ValidEmail();
        string userName1 = IdentityTestHelper.ValidUserName();
        string userName2 = IdentityTestHelper.ValidUserName();

        var firstRequest = new
        {
            email,
            userName = userName1,
            firstName = "First",
            lastName = "User",
            emailConfirmed = true
        };

        HttpResponseMessage firstResponse = await Client.PostAsAdminRawAsync(
            "/api/identity/users", firstRequest);
        firstResponse.IsSuccessStatusCode.Should().BeTrue();

        var duplicateRequest = new
        {
            email,
            userName = userName2,
            firstName = "Second",
            lastName = "User",
            emailConfirmed = true
        };

        HttpResponseMessage duplicateResponse = await Client.PostAsAdminRawAsync(
            "/api/identity/users", duplicateRequest);
        ApiResponse duplicateResult = await duplicateResponse.ReadApiResponseAsync();

        duplicateResult.IsSuccess.Should().BeFalse();
        duplicateResult.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task CreateUser_WithDuplicateUserName_Returns409()
    {
        string userName = IdentityTestHelper.ValidUserName();
        string email1 = IdentityTestHelper.ValidEmail();
        string email2 = IdentityTestHelper.ValidEmail();

        var firstRequest = new
        {
            email = email1,
            userName,
            firstName = "First",
            lastName = "User",
            emailConfirmed = true
        };

        HttpResponseMessage firstResponse = await Client.PostAsAdminRawAsync(
            "/api/identity/users", firstRequest);
        firstResponse.IsSuccessStatusCode.Should().BeTrue();

        var duplicateRequest = new
        {
            email = email2,
            userName,
            firstName = "Second",
            lastName = "User",
            emailConfirmed = true
        };

        HttpResponseMessage duplicateResponse = await Client.PostAsAdminRawAsync(
            "/api/identity/users", duplicateRequest);
        ApiResponse duplicateResult = await duplicateResponse.ReadApiResponseAsync();

        duplicateResult.IsSuccess.Should().BeFalse();
        duplicateResult.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task CreateUser_WithMissingRequiredFields_Returns422()
    {
        var request = new
        {
            firstName = "NoEmail"
        };

        HttpResponseMessage response = await Client.PostAsAdminRawAsync(
            "/api/identity/users", request);
        ApiResponse result = await response.ReadApiResponseAsync();

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task CreateUser_WithoutAuth_Returns401()
    {
        var request = new
        {
            email = "test@example.com",
            userName = "testuser",
            firstName = "Test",
            lastName = "User"
        };

        HttpResponseMessage response = await Client.PostAsJsonAsync(
            "/api/identity/users", request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
