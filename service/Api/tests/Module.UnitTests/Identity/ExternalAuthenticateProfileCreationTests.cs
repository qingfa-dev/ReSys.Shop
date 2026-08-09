using Microsoft.AspNetCore.Identity;

using Module.Identity.Features.Shared.Storefront.Auth.Login.External.Authenticate;
using Module.UnitTests.Identity.Fixtures;

using Module.Customer.Features.Storefront.Profiles.Create;
using Shared.Security.Authentication.External.Models;
using Shared.Security.Authentication.External.Providers;
using Shared.Security.Authentication.Tokens.Models;
using Shared.Security.Authentication.Tokens.Services.Access;
using Shared.Security.Authentication.Tokens.Services.Refresh;
using Shared.Security.Identity.Domain.Users;

namespace Module.UnitTests.Identity;

[Trait("Category", "Unit")]
[Trait("Module", "Identity")]
public class ExternalAuthenticateProfileCreationTests
{
    [Fact(DisplayName = "ExternalAuthenticate: profile creation failure returns Result.Failure")]
    public async Task Handle_ProfileCreationThrows_ReturnsFailure()
    {
        var provider = new Mock<IExternalLoginProvider>();
        provider.Setup(x => x.Provider).Returns("google");
        provider.Setup(x => x.ValidateIdTokenAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ExternalUserInfo>.Ok(new ExternalUserInfo
            {
                Provider = "google",
                ProviderSubjectId = "sub-1",
                Email = "new@user.com",
                FirstName = "New",
                LastName = "User"
            }));

        var userManager = IdentityMocks.CreateUserManagerMock<User>();
        userManager.Setup(x => x.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync((User?)null);
        userManager.Setup(x => x.CreateAsync(It.IsAny<User>())).ReturnsAsync(IdentityResult.Success);
        userManager.Setup(x => x.AddToRoleAsync(It.IsAny<User>(), It.IsAny<string>())).ReturnsAsync(IdentityResult.Success);
        userManager.Setup(x => x.AddLoginAsync(It.IsAny<User>(), It.IsAny<UserLoginInfo>())).ReturnsAsync(IdentityResult.Success);
        userManager.Setup(x => x.UpdateAsync(It.IsAny<User>())).ReturnsAsync(IdentityResult.Success);

        var accessTokenService = new Mock<IAccessTokenService>();
        accessTokenService.Setup(x => x.GenerateToken(It.IsAny<TokenRequestModel>()))
            .Returns(Result<TokenResponseModel>.Ok(new TokenResponseModel { Token = "tok", ExpiresIn = 900 }));

        var refreshTokenService = new Mock<IRefreshTokenService>();
        refreshTokenService.Setup(x => x.GenerateAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<RefreshTokenResponseModel>.Ok(new RefreshTokenResponseModel
            {
                Id = Guid.NewGuid(),
                Token = "rt",
                UserId = Guid.NewGuid(),
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                RevokedAt = null,
                RevokedReason = null,
                ReplacedByToken = null,
                IsActive = true
            }));

        var dateTime = new Mock<ISystemDateTime>();
        dateTime.Setup(x => x.UtcNow).Returns(System.DateTimeOffset.UtcNow);

        var currentUser = new Mock<ICurrentUser>();
        currentUser.Setup(x => x.IpAddress).Returns("127.0.0.1");

        var sender = new Mock<ISender>();
        sender.Setup(x => x.Send(It.IsAny<CreateUserProfileCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("profile creation failed"));

        var handler = new ExternalAuthenticate.CommandHandler(
            new[] { provider.Object },
            userManager.Object,
            accessTokenService.Object,
            refreshTokenService.Object,
            dateTime.Object,
            currentUser.Object,
            new Mock<ILogger<ExternalAuthenticate.CommandHandler>>().Object,
            sender.Object);

        var result = await handler.Handle(
            new ExternalAuthenticate.Command(new ExternalAuthenticate.Request { Provider = "google", IdToken = "tok" }),
            TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be("Identity.ExternalLogin.ProfileCreationFailed");
    }
}
