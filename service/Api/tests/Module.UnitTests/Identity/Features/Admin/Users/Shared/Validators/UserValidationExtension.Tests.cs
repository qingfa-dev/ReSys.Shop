using Module.Identity.Features.Admin.Shared.Models;
using Module.Identity.Features.Admin.Shared.Validators;

namespace Module.UnitTests.Identity.Features.Admin.Users.Shared.Validators;

[Trait("Category", "Unit")]
[Trait("Module", "Identity")]
[Trait("Feature", "UserShared")]
public class UserValidationTests
{
    private readonly TestValidator _validator;

    public UserValidationTests()
    {
        _validator = new TestValidator();
    }

    private sealed record TestRequest : UserParameter;

    private sealed class TestValidator : AbstractValidator<TestRequest>
    {
        public TestValidator()
        {
            this.ApplyUserRules();
        }
    }

    [Theory(DisplayName = "Should have error when email is invalid")]
    [InlineData("")]
    [InlineData("not-an-email")]
    [InlineData("test@")]
    [InlineData("@example.com")]
    public void ShouldHaveError_WhenEmailIsInvalid(string email)
    {
        var model = new TestRequest { Email = email };
        var result = _validator.Validate(model);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UserParameter.Email));
    }

    [Theory(DisplayName = "Should have error when username is invalid")]
    [InlineData("")]
    [InlineData("a")]
    [InlineData("ab")]
    [InlineData("too_long_username_that_exceeds_the_limit_of_characters_allowed_by_the_system_rules")]
    public void ShouldHaveError_WhenUsernameIsInvalid(string userName)
    {
        var model = new TestRequest { UserName = userName };
        var result = _validator.Validate(model);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UserParameter.UserName));
    }

    [Fact(DisplayName = "Should have error when first name is empty")]
    public void ShouldHaveError_WhenFirstNameIsEmpty()
    {
        var model = new TestRequest { FirstName = "" };
        var result = _validator.Validate(model);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UserParameter.FirstName));
    }

    [Fact(DisplayName = "Should be valid when last name is null")]
    public void ShouldBeValid_WhenLastNameIsNull()
    {
        var model = new TestRequest
        {
            Email = "valid@example.com",
            UserName = "valid_user",
            FirstName = "Valid",
            LastName = string.Empty
        };
        var result = _validator.Validate(model);
        result.IsValid.Should().BeTrue();
    }

    [Theory(DisplayName = "Should have error when phone is invalid")]
    [InlineData("invalid-phone")]
    [InlineData("0123")] // Starts with 0, which is invalid according to [1-9] regex
    public void ShouldHaveError_WhenPhoneIsInvalid(string phone)
    {
        var model = new TestRequest
        {
            Email = "valid@example.com",
            UserName = "valid_user",
            FirstName = "Valid",
            PhoneNumber = phone
        };
        var result = _validator.Validate(model);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UserParameter.PhoneNumber));
    }

    [Fact(DisplayName = "Should be valid when all fields are correct")]
    public void ShouldBeValid_WhenAllFieldsAreCorrect()
    {
        var model = new TestRequest
        {
            Email = "valid@example.com",
            UserName = "valid_user",
            FirstName = "Valid",
            LastName = "User",
            PhoneNumber = "+1234567890"
        };
        var result = _validator.Validate(model);
        result.IsValid.Should().BeTrue();
    }
}
