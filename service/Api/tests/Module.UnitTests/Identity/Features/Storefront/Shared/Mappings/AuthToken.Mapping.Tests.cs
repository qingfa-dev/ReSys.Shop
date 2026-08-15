using Module.Identity.Features.Shared.Storefront.Auth.Login.Password;
using Module.Identity.Features.Shared.Storefront.Shared.Mappings;

using Shared.Security.Authentication.Tokens.Models;

namespace Module.UnitTests.Identity.Features.Storefront.Shared.Mappings;

[Trait("Category", "Unit")]
[Trait("Module", "Identity")]
[Trait("Feature", "AuthToken/Mapping")]
public class AuthTokenMappingTests
{
    [Fact(DisplayName = "Should map access and refresh tokens onto a BaseTokenResponseModel response")]
    public void MapToTokenResponse_ShouldMapAllProperties()
    {
        var accessToken = new TokenResponseModel
        {
            Token = "access.jwt.token",
            ExpiresIn = 900
        };
        var refreshToken = new RefreshTokenResponseModel
        {
            Token = "refresh.token",
            ExpiresAt = DateTimeOffset.UnixEpoch.AddSeconds(2_112_000)
        };

        var result = (accessToken, refreshToken).MapToTokenResponse<PasswordLogin.Response>();

        result.AccessToken.Should().Be("access.jwt.token");
        result.AccessTokenExpiresIn.Should().Be(900);
        result.RefreshToken.Should().Be("refresh.token");
        result.RefreshTokenExpiresIn.Should().Be(2_112_000);
    }
}
