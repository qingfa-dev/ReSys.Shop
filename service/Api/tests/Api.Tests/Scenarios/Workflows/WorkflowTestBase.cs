using System.Net.Http.Headers;

using Api.Tests.Infrastructure;
using Api.Tests.Infrastructure.Http;
using Api.Tests.Scenarios.Identity.Helpers;

namespace Api.Tests.Scenarios.Workflows;

[Collection("ApiIntegration")]
public abstract class WorkflowTestBase : ApiIntegrationTestBase
{
    protected WorkflowTestBase(ApiFixture fixture) : base(fixture)
    {
    }

    protected void SetAuthToken(string token)
    {
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    protected void ClearAuth()
    {
        Client.DefaultRequestHeaders.Authorization = null;
    }

    protected static (string Email, string Password, string UserName) TestCredentials()
    {
        string email = IdentityTestHelper.ValidEmail();
        string userName = IdentityTestHelper.ValidUserName();
        return (email, IdentityTestHelper.ValidPassword, userName);
    }
}
