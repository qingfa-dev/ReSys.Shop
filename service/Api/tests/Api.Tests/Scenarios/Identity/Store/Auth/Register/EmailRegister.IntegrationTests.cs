using System.Net;

using Api.Tests.Infrastructure;
using Api.Tests.Scenarios.Identity.Helpers;

namespace Api.Tests.Scenarios.Identity.Store.Auth.Register;

public sealed class EmailRegisterIntegrationTests(ApiFixture fixture) : ApiIntegrationTestBase(fixture)
{
    [Fact]
    public async Task Register_WithValidData_Returns200()
    {
        var request = new
        {
            email = IdentityTestHelper.ValidEmail(),
            userName = IdentityTestHelper.ValidUserName(),
            password = IdentityTestHelper.ValidPassword,
            firstName = "John",
            lastName = "Doe"
        };

        HttpResponseMessage response = await Client.PostAsJsonAsync(
            "/api/store/identity/auth/register", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Register_WithMissingEmail_Returns422()
    {
        var request = new
        {
            email = "",
            userName = IdentityTestHelper.ValidUserName(),
            password = IdentityTestHelper.ValidPassword,
            firstName = "John"
        };

        HttpResponseMessage response = await Client.PostAsJsonAsync(
            "/api/store/identity/auth/register", request);

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task Register_WithMissingPassword_Returns422()
    {
        var request = new
        {
            email = IdentityTestHelper.ValidEmail(),
            userName = IdentityTestHelper.ValidUserName(),
            password = "",
            firstName = "John"
        };

        HttpResponseMessage response = await Client.PostAsJsonAsync(
            "/api/store/identity/auth/register", request);

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task Register_WithWeakPassword_Returns422()
    {
        var request = new
        {
            email = IdentityTestHelper.ValidEmail(),
            userName = IdentityTestHelper.ValidUserName(),
            password = "weak",
            firstName = "John"
        };

        HttpResponseMessage response = await Client.PostAsJsonAsync(
            "/api/store/identity/auth/register", request);

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task Register_WithDuplicateEmail_Returns409()
    {
        string email = IdentityTestHelper.ValidEmail();

        var request = new
        {
            email,
            userName = IdentityTestHelper.ValidUserName("first"),
            password = IdentityTestHelper.ValidPassword,
            firstName = "John"
        };

        HttpResponseMessage firstResponse = await Client.PostAsJsonAsync(
            "/api/store/identity/auth/register", request);
        firstResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var duplicateRequest = new
        {
            email,
            userName = IdentityTestHelper.ValidUserName("second"),
            password = IdentityTestHelper.ValidPassword,
            firstName = "Jane"
        };

        HttpResponseMessage duplicateResponse = await Client.PostAsJsonAsync(
            "/api/store/identity/auth/register", duplicateRequest);

        duplicateResponse.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Register_WithDuplicateUsername_Returns409()
    {
        string userName = IdentityTestHelper.ValidUserName();

        var request = new
        {
            email = IdentityTestHelper.ValidEmail("first"),
            userName,
            password = IdentityTestHelper.ValidPassword,
            firstName = "John"
        };

        HttpResponseMessage firstResponse = await Client.PostAsJsonAsync(
            "/api/store/identity/auth/register", request);
        firstResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var duplicateRequest = new
        {
            email = IdentityTestHelper.ValidEmail("second"),
            userName,
            password = IdentityTestHelper.ValidPassword,
            firstName = "Jane"
        };

        HttpResponseMessage duplicateResponse = await Client.PostAsJsonAsync(
            "/api/store/identity/auth/register", duplicateRequest);

        duplicateResponse.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }
}
