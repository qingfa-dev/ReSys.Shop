using Shared.Security.Authentication.External.Models;

namespace Shared.UnitTests.Security.Authentication.External.Models;

[Trait("Category", "Unit")]
[Trait("Module", "Shared")]
[Trait("Feature", "ExternalAuth")]
public sealed class ExternalUserModelTests
{
    [Fact(DisplayName = "ExternalUserInfo should set all properties from constructor")]
    public void ExternalUserInfo_Constructor_SetsProperties()
    {
        // Arrange
        string provider = "google";
        string subjectId = "sub-123456";
        string email = "user@gmail.com";
        string firstName = "John";
        string lastName = "Doe";

        // Act
        ExternalUserInfo info = new(provider, subjectId, email, firstName, lastName);

        // Assert
        info.Provider.Should().Be(provider);
        info.ProviderSubjectId.Should().Be(subjectId);
        info.Email.Should().Be(email);
        info.FirstName.Should().Be(firstName);
        info.LastName.Should().Be(lastName);
    }

    [Fact(DisplayName = "ExternalUserInfo should accept null LastName")]
    public void ExternalUserInfo_ShouldAcceptNullLastName()
    {
        // Act
        ExternalUserInfo info = new("google", "sub-789", "user@gmail.com", "Jane", null);

        // Assert
        info.LastName.Should().BeNull();
    }

    [Fact(DisplayName = "ExternalUserInfo should have structural equality")]
    public void ExternalUserInfo_ShouldHaveStructuralEquality()
    {
        // Arrange
        ExternalUserInfo info1 = new("google", "sub-1", "a@b.com", "Alice", "Smith");
        ExternalUserInfo info2 = new("google", "sub-1", "a@b.com", "Alice", "Smith");
        ExternalUserInfo info3 = new("google", "sub-2", "c@d.com", "Bob", "Jones");

        // Assert
        info1.Should().Be(info2);
        info1.Should().NotBe(info3);
    }
}
