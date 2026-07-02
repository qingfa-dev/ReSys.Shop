using System.Net;

using Api.Tests.Infrastructure;

using Shared.Security.AntiForgery.Endpoints;

namespace Api.Tests.Scenarios.AntiForgery;

public sealed class AntiForgeryTokenTests(ApiFixture fixture) : ApiIntegrationTestBase(fixture)
{
    [Fact]
    public async Task GetToken_ReturnsOk()
    {
        HttpResponseMessage response = await Client.GetAsync("/api/v1/antiforgery/token");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetToken_ReturnsSuccessResult()
    {
        HttpResponseMessage response = await Client.GetAsync("/api/v1/antiforgery/token");
        ApiResponse result = await response.ReadApiResponseAsync();

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task GetToken_ReturnsTokenResponse()
    {
        HttpResponseMessage response = await Client.GetAsync("/api/v1/antiforgery/token");
        ApiResponse result = await response.ReadApiResponseAsync();
        TokenResponse? value = result.DeserializeValue<TokenResponse>();

        value.Should().NotBeNull();
        value!.Token.Should().NotBeNullOrEmpty();
        value.HeaderName.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GetToken_ReturnsExpectedHeaderName()
    {
        HttpResponseMessage response = await Client.GetAsync("/api/v1/antiforgery/token");
        ApiResponse apiResult = await response.ReadApiResponseAsync();
        TokenResponse? value = apiResult.DeserializeValue<TokenResponse>();

        value.Should().NotBeNull();
        value!.HeaderName.Should().Be("X-XSRF-TOKEN");
    }

    [Fact]
    public async Task GetToken_SetsAntiforgeryCookie()
    {
        HttpResponseMessage response = await Client.GetAsync("/api/v1/antiforgery/token");

        bool hasXsrfCookie = response.Headers
            .Any(h => h.Value.Any(v => v.Contains("XSRF-TOKEN")));

        hasXsrfCookie.Should().BeTrue();
    }
}
