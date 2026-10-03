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
        ExternalUserInfo info = new() { Provider = provider, ProviderSubjectId = subjectId, Email = email, FirstName = firstName, LastName = lastName };

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
        ExternalUserInfo info = new() { Provider = "google", ProviderSubjectId = "sub-789", Email = "user@gmail.com", FirstName = "Jane", LastName = null };

        // Assert
        info.LastName.Should().BeNull();
    }

    [Fact(DisplayName = "ExternalUserInfo should have structural equality")]
    public void ExternalUserInfo_ShouldHaveStructuralEquality()
    {
        // Arrange
        ExternalUserInfo info1 = new() { Provider = "google", ProviderSubjectId = "sub-1", Email = "a@b.com", FirstName = "Alice", LastName = "Smith" };
        ExternalUserInfo info2 = new() { Provider = "google", ProviderSubjectId = "sub-1", Email = "a@b.com", FirstName = "Alice", LastName = "Smith" };
        ExternalUserInfo info3 = new() { Provider = "google", ProviderSubjectId = "sub-2", Email = "c@d.com", FirstName = "Bob", LastName = "Jones" };

        // Assert
        info1.Should().Be(info2);
        info1.Should().NotBe(info3);
    }
}
