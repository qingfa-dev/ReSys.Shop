using Module.Identity.Features.Shared.Admin.Users.Shared.Mappings;
using Module.Identity.Features.Shared.Admin.Users.Shared.Models;

using Shared.Security.Identity.Domain.Users;

namespace Module.UnitTests.Identity.Features.Admin.Users.Shared.Mappings;

[Trait("Category", "Unit")]
[Trait("Module", "Identity")]
[Trait("Feature", "UserShared")]
public class UserMappingTests
{
    [Fact(DisplayName = "Should map request to entity")]
    public void ToEntity_ShouldMapRequestToEntity()
    {
        // Arrange
        var request = new UserRequest
        {
            Email = "test@example.com",
            UserName = "testuser",
            FirstName = "Test",
            LastName = "User",
            PhoneNumber = "1234567890"
        };

        // Act
        var entity = request.MapToDomain();

        // Assert
        entity.Value.Email.Should().Be(request.Email);
        entity.Value.UserName.Should().Be(request.UserName);
        entity.Value.FirstName.Should().Be(request.FirstName);
        entity.Value.LastName.Should().Be(request.LastName);
        entity.Value.PhoneNumber.Should().Be(request.PhoneNumber);
    }

    [Fact(DisplayName = "Should map entity to detail response")]
    public void ToDetail_ShouldMapEntityToResponse()
    {
        // Arrange
        var entity = new User
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            UserName = "testuser",
            FirstName = "Test",
            LastName = "User",
            PhoneNumber = "1234567890",
            CreatedAtUtc = DateTimeOffset.UtcNow,
            IsActive = true
        };

        // Act
        var response = entity.MapToDetail<UserDetailResponse>();

        // Assert
        response.Id.Should().Be(entity.Id);
        response.Email.Should().Be(entity.Email);
        response.UserName.Should().Be(entity.UserName);
        response.FirstName.Should().Be(entity.FirstName);
        response.LastName.Should().Be(entity.LastName);
        response.PhoneNumber.Should().Be(entity.PhoneNumber);
        response.IsActive.Should().Be(entity.IsActive);
    }

    [Fact(DisplayName = "Should map partial entity to detail response")]
    public void ToDetail_ShouldMapPartialEntityToResponse()
    {
        // Arrange
        var entity = new User
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            UserName = "testuser",
            FirstName = "Test",
            LastName = null,
            PhoneNumber = null
        };

        // Act
        var response = entity.MapToDetail<UserDetailResponse>();

        // Assert
        response.LastName.Should().BeEmpty();
        response.PhoneNumber.Should().BeEmpty();
    }
}