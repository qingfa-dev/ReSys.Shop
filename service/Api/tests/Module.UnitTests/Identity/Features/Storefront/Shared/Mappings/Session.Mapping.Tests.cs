using Module.Identity.Features.Shared.Storefront.Auth.Sessions.Get;
using Module.Identity.Features.Shared.Storefront.Shared.Mappings;
using Module.UnitTests.Identity.Fixtures;

using Shared.Security.Identity.Domain.Users;

namespace Module.UnitTests.Identity.Features.Storefront.Shared.Mappings;

[Trait("Category", "Unit")]
[Trait("Module", "Identity")]
[Trait("Feature", "Session/Mapping")]
public class SessionMappingTests
{
    [Fact(DisplayName = "Should map user composite onto a SessionResponseModel response")]
    public void MapToSessionResponse_ShouldMapAllProperties()
    {
        var user = UserMethod.Create("testuser", "test@example.com", "Test", "User").Value;
        var roles = new[] { "Admin", "User" };
        var permissions = new HashSet<string> { "read:products", "write:products" };

        var result = (user, roles, permissions).MapToSessionResponse<GetSession.Response>();

        result.Id.Should().Be(user.Id);
        result.Email.Should().Be(user.Email);
        result.UserName.Should().Be(user.UserName);
        result.Roles.Should().BeEquivalentTo(roles);
        result.Permissions.Should().BeEquivalentTo(permissions);
    }

    [Fact(DisplayName = "Should map empty roles and permissions onto a SessionResponseModel response")]
    public void MapToSessionResponse_ShouldMapEmptyCollections()
    {
        var user = UserMethod.Create("testuser", "test@example.com", "Test", "User").Value;

        var result = (user, new string[0], new HashSet<string>()).MapToSessionResponse<GetSession.Response>();

        result.Roles.Should().BeEmpty();
        result.Permissions.Should().BeEmpty();
    }

    [Fact(DisplayName = "Should coerce null email and username to empty strings")]
    public void MapToSessionResponse_ShouldCoerceNullEmailAndUserName()
    {
        var user = new User { Id = Guid.NewGuid(), UserName = null, Email = null };

        var result = (user, new string[0], new HashSet<string>()).MapToSessionResponse<GetSession.Response>();

        result.Email.Should().Be(string.Empty);
        result.UserName.Should().Be(string.Empty);
    }
}
