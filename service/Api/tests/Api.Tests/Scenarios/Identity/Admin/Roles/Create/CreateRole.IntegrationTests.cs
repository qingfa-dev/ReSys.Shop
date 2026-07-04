using System.Net;

using Api.Tests.Infrastructure;
using Api.Tests.Infrastructure.Auth;

using Module.Identity.Features.Admin.Roles.Shared.Models;

namespace Api.Tests.Scenarios.Identity.Admin.Roles.Create;

public sealed class CreateRoleIntegrationTests(ApiFixture fixture) : IdentityIntegrationTestBase(fixture)
{
    [Fact]
    public async Task CreateRole_WithValidRequest_Returns201()
    {
        string roleName = $"testrole{Guid.NewGuid():N}"[..15];

        var request = new
        {
            name = roleName,
            description = "A test role for integration testing"
        };

        HttpResponseMessage response = await Client.PostAsAdminRawAsync(
            "/api/identity/roles", request);
        ApiResponse result = await response.ReadApiResponseAsync();

        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.OK);
        RoleDetailResponse? value = result.DeserializeValue<RoleDetailResponse>();
        value.Should().NotBeNull();
        value!.Id.Should().NotBeEmpty();
        value.Name.Should().Be(roleName);
        value.Description.Should().Be("A test role for integration testing");
        value.IsSystem.Should().BeFalse();
        value.CreatedAtUtc.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(30));
    }

    [Fact]
    public async Task CreateRole_WithDuplicateName_Returns409()
    {
        string roleName = $"testrole{Guid.NewGuid():N}"[..15];

        var firstRequest = new
        {
            name = roleName,
            description = "First role"
        };

        HttpResponseMessage firstResponse = await Client.PostAsAdminRawAsync(
            "/api/identity/roles", firstRequest);
        firstResponse.IsSuccessStatusCode.Should().BeTrue();

        var duplicateRequest = new
        {
            name = roleName,
            description = "Duplicate role"
        };

        HttpResponseMessage duplicateResponse = await Client.PostAsAdminRawAsync(
            "/api/identity/roles", duplicateRequest);
        ApiResponse duplicateResult = await duplicateResponse.ReadApiResponseAsync();

        duplicateResult.IsSuccess.Should().BeFalse();
        duplicateResult.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task CreateRole_WithMissingName_Returns422()
    {
        var request = new
        {
            description = "No name provided"
        };

        HttpResponseMessage response = await Client.PostAsAdminRawAsync(
            "/api/identity/roles", request);
        ApiResponse result = await response.ReadApiResponseAsync();

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task CreateRole_WithoutAuth_Returns401()
    {
        var request = new
        {
            name = "UnauthorizedRole",
            description = "Should not be created"
        };

        HttpResponseMessage response = await Client.PostAsJsonAsync(
            "/api/identity/roles", request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
