using Module.Profile.Domain;

namespace Module.UnitTests.Profile.Domain;

[Trait("Category", "Unit")]
[Trait("Module", "Identity")]
[Trait("Feature", "ProfileValidation")]
public class ProfileValidationTests
{
    private sealed class FirstNameTestModel
    {
        public string? FirstName { get; set; }
    }

    private sealed class FirstNameValidatorRequired : AbstractValidator<FirstNameTestModel>
    {
        public FirstNameValidatorRequired()
        {
            RuleFor(x => x.FirstName).ApplyFirstNameRules();
        }
    }

    private sealed class FirstNameValidatorOptional : AbstractValidator<FirstNameTestModel>
    {
        public FirstNameValidatorOptional()
        {
            RuleFor(x => x.FirstName).ApplyFirstNameRules(isRequired: false);
        }
    }

    [Fact]
    public void ApplyFirstNameRules_WhenNullAndRequired_ShouldHaveError()
    {
        var validator = new FirstNameValidatorRequired();
        var result = validator.TestValidate(new FirstNameTestModel { FirstName = null });
        result.ShouldHaveValidationErrorFor(x => x.FirstName)
            .WithErrorCode(UserProfileResult.Failure.FirstNameRequired.Code);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void ApplyFirstNameRules_WhenEmptyAndRequired_ShouldHaveError(string? firstName)
    {
        var validator = new FirstNameValidatorRequired();
        var result = validator.TestValidate(new FirstNameTestModel { FirstName = firstName });
        result.ShouldHaveValidationErrorFor(x => x.FirstName)
            .WithErrorCode(UserProfileResult.Failure.FirstNameRequired.Code);
    }

    [Fact]
    public void ApplyFirstNameRules_WhenExceedsMaxLength_ShouldHaveError()
    {
        var validator = new FirstNameValidatorRequired();
        var longName = new string('a', UserProfileConstant.Constraints.MaxFirstNameLength + 1);
        var result = validator.TestValidate(new FirstNameTestModel { FirstName = longName });
        result.ShouldHaveValidationErrorFor(x => x.FirstName)
            .WithErrorCode(UserProfileResult.Failure.FirstNameTooLong.Code);
    }

    [Theory]
    [InlineData("John")]
    [InlineData("Jane")]
    [InlineData("Mary-Jean-Pierre")]
    public void ApplyFirstNameRules_WhenValid_ShouldPass(string firstName)
    {
        var validator = new FirstNameValidatorRequired();
        var result = validator.TestValidate(new FirstNameTestModel { FirstName = firstName });
        result.ShouldNotHaveValidationErrorFor(x => x.FirstName);
    }

    [Fact]
    public void ApplyFirstNameRules_WhenAtMaxLength_ShouldPass()
    {
        var validator = new FirstNameValidatorRequired();
        var name = new string('a', UserProfileConstant.Constraints.MaxFirstNameLength);
        var result = validator.TestValidate(new FirstNameTestModel { FirstName = name });
        result.ShouldNotHaveValidationErrorFor(x => x.FirstName);
    }

    [Fact]
    public void ApplyFirstNameRules_WhenSingleChar_ShouldPass()
    {
        var validator = new FirstNameValidatorRequired();
        var result = validator.TestValidate(new FirstNameTestModel { FirstName = "J" });
        result.ShouldNotHaveValidationErrorFor(x => x.FirstName);
    }

    [Fact]
    public void ApplyFirstNameRules_WhenNullAndOptional_ShouldPass()
    {
        var validator = new FirstNameValidatorOptional();
        var result = validator.TestValidate(new FirstNameTestModel { FirstName = null });
        result.ShouldNotHaveValidationErrorFor(x => x.FirstName);
    }

    [Fact]
    public void ApplyFirstNameRules_WhenEmptyAndOptional_ShouldPass()
    {
        var validator = new FirstNameValidatorOptional();
        var result = validator.TestValidate(new FirstNameTestModel { FirstName = "" });
        result.ShouldNotHaveValidationErrorFor(x => x.FirstName);
    }

    [Fact]
    public void ApplyFirstNameRules_WhenOptionalExceedsMaxLength_ShouldHaveError()
    {
        var validator = new FirstNameValidatorOptional();
        var longName = new string('a', UserProfileConstant.Constraints.MaxFirstNameLength + 1);
        var result = validator.TestValidate(new FirstNameTestModel { FirstName = longName });
        result.ShouldHaveValidationErrorFor(x => x.FirstName)
            .WithErrorCode(UserProfileResult.Failure.FirstNameTooLong.Code);
    }

    private sealed class LastNameTestModel
    {
        public string? LastName { get; set; }
    }

    private sealed class LastNameValidatorRequired : AbstractValidator<LastNameTestModel>
    {
        public LastNameValidatorRequired()
        {
            RuleFor(x => x.LastName).ApplyLastNameRules();
        }
    }

    private sealed class LastNameValidatorOptional : AbstractValidator<LastNameTestModel>
    {
        public LastNameValidatorOptional()
        {
            RuleFor(x => x.LastName).ApplyLastNameRules(isRequired: false);
        }
    }

    [Fact]
    public void ApplyLastNameRules_WhenNullAndRequired_ShouldHaveError()
    {
        var validator = new LastNameValidatorRequired();
        var result = validator.TestValidate(new LastNameTestModel { LastName = null });
        result.ShouldHaveValidationErrorFor(x => x.LastName)
            .WithErrorCode(UserProfileResult.Failure.LastNameRequired.Code);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void ApplyLastNameRules_WhenEmptyAndRequired_ShouldHaveError(string? lastName)
    {
        var validator = new LastNameValidatorRequired();
        var result = validator.TestValidate(new LastNameTestModel { LastName = lastName });
        result.ShouldHaveValidationErrorFor(x => x.LastName)
            .WithErrorCode(UserProfileResult.Failure.LastNameRequired.Code);
    }

    [Fact]
    public void ApplyLastNameRules_WhenExceedsMaxLength_ShouldHaveError()
    {
        var validator = new LastNameValidatorRequired();
        var longName = new string('a', UserProfileConstant.Constraints.MaxLastNameLength + 1);
        var result = validator.TestValidate(new LastNameTestModel { LastName = longName });
        result.ShouldHaveValidationErrorFor(x => x.LastName)
            .WithErrorCode(UserProfileResult.Failure.LastNameTooLong.Code);
    }

    [Theory]
    [InlineData("Smith")]
    [InlineData("O'Brien")]
    [InlineData("Van Der Waal")]
    public void ApplyLastNameRules_WhenValid_ShouldPass(string lastName)
    {
        var validator = new LastNameValidatorRequired();
        var result = validator.TestValidate(new LastNameTestModel { LastName = lastName });
        result.ShouldNotHaveValidationErrorFor(x => x.LastName);
    }

    [Fact]
    public void ApplyLastNameRules_WhenAtMaxLength_ShouldPass()
    {
        var validator = new LastNameValidatorRequired();
        var name = new string('a', UserProfileConstant.Constraints.MaxLastNameLength);
        var result = validator.TestValidate(new LastNameTestModel { LastName = name });
        result.ShouldNotHaveValidationErrorFor(x => x.LastName);
    }

    [Fact]
    public void ApplyLastNameRules_WhenNullAndOptional_ShouldPass()
    {
        var validator = new LastNameValidatorOptional();
        var result = validator.TestValidate(new LastNameTestModel { LastName = null });
        result.ShouldNotHaveValidationErrorFor(x => x.LastName);
    }

    [Fact]
    public void ApplyLastNameRules_WhenEmptyAndOptional_ShouldPass()
    {
        var validator = new LastNameValidatorOptional();
        var result = validator.TestValidate(new LastNameTestModel { LastName = "" });
        result.ShouldNotHaveValidationErrorFor(x => x.LastName);
    }

    [Fact]
    public void ApplyLastNameRules_WhenOptionalExceedsMaxLength_ShouldHaveError()
    {
        var validator = new LastNameValidatorOptional();
        var longName = new string('a', UserProfileConstant.Constraints.MaxLastNameLength + 1);
        var result = validator.TestValidate(new LastNameTestModel { LastName = longName });
        result.ShouldHaveValidationErrorFor(x => x.LastName)
            .WithErrorCode(UserProfileResult.Failure.LastNameTooLong.Code);
    }

    private sealed class DateOfBirthTestModel
    {
        public DateTimeOffset? DateOfBirth { get; set; }
    }

    private static readonly DateTime FixedToday = new(2026, 6, 15, 0, 0, 0, DateTimeKind.Utc);

    private static Mock<ISystemDateTime> CreateMockDateTime()
    {
        var mock = new Mock<ISystemDateTime>();
        mock.Setup(x => x.UtcNow).Returns(FixedToday);
        return mock;
    }

    private sealed class DateOfBirthValidator : AbstractValidator<DateOfBirthTestModel>
    {
        public DateOfBirthValidator(ISystemDateTime systemDateTime)
        {
            RuleFor(x => x.DateOfBirth).ApplyDateOfBirthRules(systemDateTime);
        }
    }

    [Fact]
    public void ApplyDateOfBirthRules_WhenFutureDate_ShouldHaveError()
    {
        var mockDateTime = CreateMockDateTime();
        var validator = new DateOfBirthValidator(mockDateTime.Object);
        var futureDate = new DateTimeOffset(2026, 6, 16, 0, 0, 0, TimeSpan.Zero);
        var result = validator.TestValidate(new DateOfBirthTestModel { DateOfBirth = futureDate });

        result.ShouldHaveValidationErrorFor(x => x.DateOfBirth)
            .WithErrorCode(UserProfileResult.Failure.DateOfBirthFuture.Code);
    }

    [Fact]
    public void ApplyDateOfBirthRules_WhenTooOld_ShouldHaveError()
    {
        var mockDateTime = CreateMockDateTime();
        var validator = new DateOfBirthValidator(mockDateTime.Object);
        var tooOld = new DateTimeOffset(1906, 6, 14, 0, 0, 0, TimeSpan.Zero);
        var result = validator.TestValidate(new DateOfBirthTestModel { DateOfBirth = tooOld });

        result.ShouldHaveValidationErrorFor(x => x.DateOfBirth)
            .WithErrorCode(UserProfileResult.Failure.DateOfBirthTooOld.Code);
    }

    [Fact]
    public void ApplyDateOfBirthRules_WhenToday_ShouldPass()
    {
        var mockDateTime = CreateMockDateTime();
        var validator = new DateOfBirthValidator(mockDateTime.Object);
        var today = new DateTimeOffset(2026, 6, 15, 0, 0, 0, TimeSpan.Zero);
        var result = validator.TestValidate(new DateOfBirthTestModel { DateOfBirth = today });

        result.ShouldNotHaveValidationErrorFor(x => x.DateOfBirth);
    }

    [Fact]
    public void ApplyDateOfBirthRules_WhenExactlyMaxAge_ShouldPass()
    {
        var mockDateTime = CreateMockDateTime();
        var validator = new DateOfBirthValidator(mockDateTime.Object);
        var maxAge = new DateTimeOffset(1906, 6, 15, 0, 0, 0, TimeSpan.Zero);
        var result = validator.TestValidate(new DateOfBirthTestModel { DateOfBirth = maxAge });

        result.ShouldNotHaveValidationErrorFor(x => x.DateOfBirth);
    }

    [Fact]
    public void ApplyDateOfBirthRules_WhenValidBirthDate_ShouldPass()
    {
        var mockDateTime = CreateMockDateTime();
        var validator = new DateOfBirthValidator(mockDateTime.Object);
        var validDate = new DateTimeOffset(1990, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var result = validator.TestValidate(new DateOfBirthTestModel { DateOfBirth = validDate });

        result.ShouldNotHaveValidationErrorFor(x => x.DateOfBirth);
    }
}