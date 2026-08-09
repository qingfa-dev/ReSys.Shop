using System.Net;

using Api.Tests.Infrastructure;
using Api.Tests.Infrastructure.Http;
using Api.Tests.Scenarios.Identity.Helpers;

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

using Shared.Security.Identity.Domain.Users;

namespace Api.Tests.Scenarios.Workflows;

public sealed class ForgotPasswordResetWorkflowTests(ApiFixture fixture) : WorkflowTestBase(fixture)
{
    [Fact]
    public async Task ForgotPassword_Reset_LoginNewPassword_OldPasswordFails()
    {
        var client = Client;
        ClearAuth();

        var (email, password, userName) = TestCredentials();
        var registerBody = new { email, userName, password, firstName = "Forgot", lastName = "Pwd" };

        HttpResponseMessage registerResp = await client.PostAsJsonAsync(
            "/api/storefront/identity/auth/register", registerBody);
        registerResp.IsSuccessStatusCode.Should().BeTrue();

        var forgotBody = new { email };
        HttpResponseMessage forgotResp = await client.PostAsJsonAsync(
            "/api/storefront/identity/passwords/forgot", forgotBody);
        forgotResp.StatusCode.Should().Be(HttpStatusCode.NoContent);

        Guid userId;
        string resetToken;
        using (var scope = Fixture.Factory.Services.CreateScope())
        {
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
            var user = await userManager.FindByEmailAsync(email);
            user.Should().NotBeNull();
            userId = user!.Id;
            resetToken = await userManager.GeneratePasswordResetTokenAsync(user);
        }

        string newPassword = "NewSecurePass1!";
        var resetBody = new { userId = userId.ToString(), token = resetToken, newPassword };

        HttpResponseMessage resetResp = await client.PostAsJsonAsync(
            "/api/storefront/identity/passwords/reset", resetBody);
        resetResp.IsSuccessStatusCode.Should().BeTrue();

        var loginNewBody = new { credential = email, password = newPassword };
        HttpResponseMessage loginNewResp = await client.PostAsJsonAsync(
            "/api/storefront/identity/auth/login/password", loginNewBody);
        loginNewResp.StatusCode.Should().Be(HttpStatusCode.OK);

        var loginOldBody = new { credential = email, password };
        HttpResponseMessage loginOldResp = await client.PostAsJsonAsync(
            "/api/storefront/identity/auth/login/password", loginOldBody);
        loginOldResp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
