using System.Reflection;

using Shared.Operational.Persistence.Specifications.Helpers;

namespace Shared.UnitTests.Operational.Persistence.Specifications.Helpers;

[Trait("Category", "Unit")]
[Trait("Module", "Shared")]
[Trait("Feature", "Persistence")]
public sealed class QueryHelperTests
{
    private sealed class TestModel
    {
        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }
        public string? EmailAddress { get; set; }
        public bool IsActive { get; set; }
        public string X { get; set; } = string.Empty;
        public string SHORTNAME { get; set; } = string.Empty;
        public string FULLNAME { get; set; } = string.Empty;
    }

    #region GetPropertyCaseInsensitive

    [Theory]
    [InlineData("Name", "Name")]
    [InlineData("name", "Name")]
    [InlineData("NAME", "Name")]
    [InlineData("Age", "Age")]
    [InlineData("email_address", "EmailAddress")]
    [InlineData("email-address", "EmailAddress")]
    [InlineData("EmailAddress", "EmailAddress")]
    [InlineData("is_active", "IsActive")]
    [InlineData("Email_Address", "EmailAddress")]
    [InlineData("EMAIL_ADDRESS", "EmailAddress")]
    [InlineData("emailaddress", "EmailAddress")]
    [InlineData("emailAddress", "EmailAddress")]
    [InlineData("IS_ACTIVE", "IsActive")]
    [InlineData("is-active", "IsActive")]
    [InlineData("short_name", "SHORTNAME")]
    [InlineData("x", "X")]
    [InlineData("_x", "X")]
    [InlineData("fullname", "FULLNAME")]
    [InlineData("FULLNAME", "FULLNAME")]
    [InlineData("email_-_address", "EmailAddress")]
    public void GetPropertyCaseInsensitive_ShouldResolveProperty(string inputName, string expectedPropertyName)
    {
        // Act
        PropertyInfo? property = QueryHelper.GetPropertyCaseInsensitive(typeof(TestModel), inputName);

        // Assert
        property.Should().NotBeNull();
        property.Name.Should().Be(expectedPropertyName);
    }

    [Fact]
    public void GetPropertyCaseInsensitive_ShouldReturnNull_WhenPropertyNotFound()
    {
        // Act
        PropertyInfo? property = QueryHelper.GetPropertyCaseInsensitive(typeof(TestModel), "NonExistent");

        // Assert
        property.Should().BeNull();
    }

    [Fact]
    public void GetPropertyCaseInsensitive_ShouldThrow_WhenTypeIsNull()
    {
        // Act
        Action act = () => QueryHelper.GetPropertyCaseInsensitive(null!, "Name");

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void GetPropertyCaseInsensitive_ShouldReturnNull_WhenNameIsEmpty(string? inputName)
    {
        // Act
        PropertyInfo? property = QueryHelper.GetPropertyCaseInsensitive(typeof(TestModel), inputName!);

        // Assert
        property.Should().BeNull();
    }

    [Fact]
    public void GetPropertyCaseInsensitive_ShouldUseExactMatchCache_OnRepeatedCall()
    {
        // Validate: Repeated calls with identical name should hit ExactMatchCache
        // and return the same PropertyInfo instance.

        // Act
        PropertyInfo? first = QueryHelper.GetPropertyCaseInsensitive(typeof(TestModel), "Name");
        PropertyInfo? second = QueryHelper.GetPropertyCaseInsensitive(typeof(TestModel), "Name");

        // Assert
        second.Should().NotBeNull();
        second.Should().BeSameAs(first);
        second.Name.Should().Be("Name");
    }

    [Fact]
    public void GetPropertyCaseInsensitive_ShouldUseResolvedCache_OnVariantAfterResolve()
    {
        // Validate: A snake_case variant that was resolved should hit ResolvedCache
        // on subsequent calls with the same input.

        // Act
        PropertyInfo? first = QueryHelper.GetPropertyCaseInsensitive(typeof(TestModel), "email_address");
        PropertyInfo? second = QueryHelper.GetPropertyCaseInsensitive(typeof(TestModel), "email_address");

        // Assert
        second.Should().NotBeNull();
        second.Should().BeSameAs(first);
        second.Name.Should().Be("EmailAddress");
    }

    [Fact]
    public void GetPropertyCaseInsensitive_ShouldNotCache_NonExistentProperty()
    {
        // Validate: A non-existent property should not prevent subsequent lookup
        // of an existent property (ResolvedCache stores null but ExactMatch doesn't).

        // Act
        PropertyInfo? notFound = QueryHelper.GetPropertyCaseInsensitive(typeof(TestModel), "NONEXISTENT");
        PropertyInfo? found = QueryHelper.GetPropertyCaseInsensitive(typeof(TestModel), "Name");

        // Assert
        notFound.Should().BeNull();
        found.Should().NotBeNull();
        found.Name.Should().Be("Name");
    }

    [Theory]
    [InlineData("_")]
    [InlineData("-")]
    public void GetPropertyCaseInsensitive_ShouldReturnNull_WhenNameIsOnlySeparators(string inputName)
    {
        // Act
        PropertyInfo? property = QueryHelper.GetPropertyCaseInsensitive(typeof(TestModel), inputName);

        // Assert
        property.Should().BeNull();
    }

    [Fact]
    public void GetPropertyCaseInsensitive_ShouldResolve_WithLeadingTrailingWhitespace()
    {
        // Validate: Trim() normalizes input before cache lookup.
        // Act
        PropertyInfo? property = QueryHelper.GetPropertyCaseInsensitive(typeof(TestModel), "  Name  ");

        // Assert
        property.Should().NotBeNull();
        property.Name.Should().Be("Name");
    }

    [Fact]
    public void GetPropertyCaseInsensitive_ShouldUseResolvedCache_ForNonExistentProperty()
    {
        // Validate: ResolvedCache stores null for non-existent names, preventing
        // repeat resolution calls. Both calls to the same unknown name return null.

        // Act
        PropertyInfo? first = QueryHelper.GetPropertyCaseInsensitive(typeof(TestModel), "MISSING_PROP");
        PropertyInfo? second = QueryHelper.GetPropertyCaseInsensitive(typeof(TestModel), "MISSING_PROP");

        // Assert
        first.Should().BeNull();
        second.Should().BeNull();
    }

    [Fact]
    public void GetPropertyCaseInsensitive_ThreadSafety_ShouldNotThrow()
    {
        // Contract: ConcurrentDictionary must handle parallel access without corruption.
        string[] names = { "Name", "name", "EMAIL_ADDRESS", "is_active", "Age", "email-address", "IsActive" };
        int iterations = 100;
        ParallelOptions parallelOptions = new() { MaxDegreeOfParallelism = 4 };

        // Act
        Action act = () =>
        {
            Parallel.ForEach(names, parallelOptions, name =>
            {
                for (int i = 0; i < iterations; i++)
                {
                    PropertyInfo? property = QueryHelper.GetPropertyCaseInsensitive(typeof(TestModel), name);
                    if (property == null)
                    {
                        throw new InvalidOperationException(
                            $"Property '{name}' returned null on iteration {i}");
                    }
                }
            });
        };

        // Assert
        act.Should().NotThrow();
    }

    #endregion
}
