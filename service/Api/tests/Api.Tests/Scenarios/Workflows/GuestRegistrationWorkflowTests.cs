using System.Net;
using Api.Tests.Infrastructure;
using Api.Tests.Infrastructure.Http;

using Api.Tests.Scenarios.Identity.Helpers;

using Microsoft.Extensions.DependencyInjection;

using Shared.Governance.Conventions;
using Shared.Security.Identity.Domain.Users;

namespace Api.Tests.Scenarios.Workflows;

public sealed class GuestRegistrationWorkflowTests(ApiFixture fixture) : WorkflowTestBase(fixture)
{
    [Fact]
    public async Task Guest_Register_VerifyEmail_Login_AccessProfile()
    {
        HttpClient client = Client;
        var (email, password, userName) = TestCredentials();

        // Step 1: Register
        var registerBody = new
        {
            email,
            userName,
            password,
            firstName = "Workflow",
            lastName = "Tester"
        };

        HttpResponseMessage registerResponse = await client.PostAsJsonAsync(
            "/api/storefront/identity/auth/register", registerBody);
        registerResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        ApiResponse registerResult = await registerResponse.ReadApiResponseAsync();
        registerResult.IsSuccess.Should().BeTrue();

        // Step 2: Request email verification resend
        var resendBody = new { email };
        HttpResponseMessage resendResponse = await client.PostAsJsonAsync(
            "/api/storefront/identity/emails/resend", resendBody);
        resendResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Step 3: Confirm email — resolve token from DB and encode it
        string token;
        Guid userId;
        using (var scope = Fixture.Factory.Services.CreateScope())
        {
            var userManager = scope.ServiceProvider.GetRequiredService<Microsoft.AspNetCore.Identity.UserManager<User>>();
            var user = await userManager.FindByEmailAsync(email);
            user.Should().NotBeNull();
            userId = user!.Id;
            token = Base64Converter.ToBase64Url(await userManager.GenerateEmailConfirmationTokenAsync(user));
        }

        var confirmBody = new { userId = userId.ToString(), token };
        HttpResponseMessage confirmResponse = await client.PostAsJsonAsync(
            "/api/storefront/identity/emails/confirm", confirmBody);
        confirmResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Step 4: Login
        var loginBody = new { credential = email, password };
        HttpResponseMessage loginResponse = await client.PostAsJsonAsync(
            "/api/storefront/identity/auth/login/password", loginBody);
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        ApiResponse loginResult = await loginResponse.ReadApiResponseAsync();
        loginResult.IsSuccess.Should().BeTrue();

        // Step 5: Access profile with the login token
        string accessToken = IdentityTestHelper.GetAccessToken(loginResult);
        accessToken.Should().NotBeNullOrEmpty();
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

        HttpResponseMessage profileResponse = await client.GetAsync("/api/storefront/profiles/profiles");
        profileResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        ApiResponse profileResult = await profileResponse.ReadApiResponseAsync();
        profileResult.IsSuccess.Should().BeTrue();
    }
}
