using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

using Shared.Security.Authentication.Contexts.Services;
using Shared.Security.Authentication.Guest.Options;

namespace Shared.UnitTests.Security.Authentication.Context;

[Trait("Category", "Unit")]
[Trait("Module", "Shared")]
[Trait("Feature", "HttpContexts")]
public class CurrentUserTests
{
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock = new();
    private readonly IOptions<GuestSessionSetting> _guestSessionOptions;

    public CurrentUserTests()
    {
        _guestSessionOptions = Options.Create(new GuestSessionSetting());
    }

    private CurrentUser CreateCurrentUser()
    {
        return new CurrentUser(_httpContextAccessorMock.Object, _guestSessionOptions);
    }

    [Fact(DisplayName = "UserId should return sub claim from JWT if present")]
    public void UserId_WithJwtSubClaim_ShouldReturnSub()
    {
        // Arrange
        var userId = "user-123";
        Claim[] claims = new[] { new Claim(JwtRegisteredClaimNames.Sub, userId) };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);
        var httpContext = new DefaultHttpContext { User = principal };
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

        CurrentUser currentUser = CreateCurrentUser();

        // Act
        var result = currentUser.UserId;

        // Assert
        result.Should().Be(userId);
    }

    [Fact(DisplayName = "UserId should return NameIdentifier claim if sub is missing")]
    public void UserId_WithNameIdentifierClaim_ShouldReturnNameIdentifier()
    {
        // Arrange
        var userId = "user-456";
        Claim[] claims = new[] { new Claim(ClaimTypes.NameIdentifier, userId) };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);
        var httpContext = new DefaultHttpContext { User = principal };
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

        CurrentUser currentUser = CreateCurrentUser();

        // Act
        var result = currentUser.UserId;

        // Assert
        result.Should().Be(userId);
    }

    [Fact(DisplayName = "UserName should return name claim from JWT if present")]
    public void UserName_WithJwtNameClaim_ShouldReturnName()
    {
        // Arrange
        var userName = "john.doe";
        Claim[] claims = new[] { new Claim(JwtRegisteredClaimNames.Name, userName) };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);
        var httpContext = new DefaultHttpContext { User = principal };
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

        CurrentUser currentUser = CreateCurrentUser();

        // Act
        var result = currentUser.UserName;

        // Assert
        result.Should().Be(userName);
    }

    [Fact(DisplayName = "Email should return email claim from JWT if present")]
    public void Email_WithJwtEmailClaim_ShouldReturnEmail()
    {
        // Arrange
        var email = "john.doe@example.com";
        Claim[] claims = new[] { new Claim(JwtRegisteredClaimNames.Email, email) };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);
        var httpContext = new DefaultHttpContext { User = principal };
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

        CurrentUser currentUser = CreateCurrentUser();

        // Act
        var result = currentUser.Email;

        // Assert
        result.Should().Be(email);
    }

    [Fact(DisplayName = "IsAuthenticated should return true if user is authenticated")]
    public void IsAuthenticated_WhenAuthenticated_ShouldReturnTrue()
    {
        // Arrange
        var identity = new ClaimsIdentity("TestAuth");
        var principal = new ClaimsPrincipal(identity);
        var httpContext = new DefaultHttpContext { User = principal };
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

        CurrentUser currentUser = CreateCurrentUser();

        // Act
        var result = currentUser.IsAuthenticated;

        // Assert
        result.Should().BeTrue();
    }

    [Fact(DisplayName = "IsAuthenticated should return false if user is not authenticated")]
    public void IsAuthenticated_WhenNotAuthenticated_ShouldReturnFalse()
    {
        // Arrange
        var principal = new ClaimsPrincipal(new ClaimsIdentity());
        var httpContext = new DefaultHttpContext { User = principal };
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

        CurrentUser currentUser = CreateCurrentUser();

        // Act
        var result = currentUser.IsAuthenticated;

        // Assert
        result.Should().BeFalse();
    }

    [Fact(DisplayName = "UserId should return null if no HttpContext")]
    public void UserId_WithNoHttpContext_ShouldReturnNull()
    {
        // Arrange
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns((HttpContext?)null);
        CurrentUser currentUser = CreateCurrentUser();

        // Act
        var result = currentUser.UserId;

        // Assert
        result.Should().BeNull();
    }

    [Fact(DisplayName = "IpAddress should return remote IP address from connection")]
    public void IpAddress_WithRemoteIpAddress_ShouldReturnIpAddress()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        httpContext.Connection.RemoteIpAddress = System.Net.IPAddress.Parse("192.168.1.100");
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);
        
        CurrentUser currentUser = CreateCurrentUser();

        // Act
        var result = currentUser.IpAddress;

        // Assert
        result.Should().Be("192.168.1.100");
    }

    [Fact(DisplayName = "IpAddress should return null if no HttpContext")]
    public void IpAddress_WithNoHttpContext_ShouldReturnNull()
    {
        // Arrange
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns((HttpContext?)null);
        CurrentUser currentUser = CreateCurrentUser();

        // Act
        var result = currentUser.IpAddress;

        // Assert
        result.Should().BeNull();
    }

    [Fact(DisplayName = "Device should return user agent from request headers")]
    public void Device_WithUserAgent_ShouldReturnUserAgent()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) Chrome/120.0.0.0";
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);
        
        CurrentUser currentUser = CreateCurrentUser();

        // Act
        var result = currentUser.Device;

        // Assert
        result.Should().Contain("Chrome");
    }

    [Fact(DisplayName = "Device should return null if no HttpContext")]
    public void Device_WithNoHttpContext_ShouldReturnNull()
    {
        // Arrange
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns((HttpContext?)null);
        CurrentUser currentUser = CreateCurrentUser();

        // Act
        var result = currentUser.Device;

        // Assert
        result.Should().BeNull();
    }
}
