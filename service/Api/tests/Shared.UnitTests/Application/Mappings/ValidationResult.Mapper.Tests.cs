using FluentValidation.Results;

using Shared.Application.Mappings;

namespace Shared.UnitTests.Application.Mappings;

[Trait("Category", "Unit")]
[Trait("Module", "Validation")]
[Trait("Feature", "Mappings")]
public class ValidationResultMapperTests
{
    public class ToError
    {
        [Fact(DisplayName = "ToError: Should map ValidationFailure to Error with code and message")]
        public void WithValidFailure_ShouldMapToError()
        {
            var failure = new ValidationFailure("Email", "Email is required")
            {
                ErrorCode = "NotEmptyValidator"
            };

            Error result = failure.ToError();

            result.Code.Should().Be("NotEmptyValidator");
            result.Message.Should().Be("Email is required");
            result.Type.Should().Be(ErrorType.Validation);
        }

        [Fact(DisplayName = "ToError: Should include propertyName and attemptedValue in metadata")]
        public void WithValidFailure_ShouldIncludePropertyAndAttemptedValueInMetadata()
        {
            var failure = new ValidationFailure("Email", "Email is invalid")
            {
                AttemptedValue = "invalid-email"
            };

            Error result = failure.ToError();

            result.Metadata.Should().ContainKey("propertyName").WhoseValue.Should().Be("Email");
            result.Metadata.Should().ContainKey("attemptedValue").WhoseValue.Should().Be("invalid-email");
        }

        [Fact(DisplayName = "ToError: Should include FormattedMessagePlaceholderValues in metadata")]
        public void WithPlaceholderValues_ShouldIncludeThemInMetadata()
        {
            var failure = new ValidationFailure("Password", "Password must be at least 8 characters")
            {
                ErrorCode = "MinimumLengthValidator",
                FormattedMessagePlaceholderValues = new Dictionary<string, object>
                {
                    { "MinLength", 8 }
                }
            };

            Error result = failure.ToError();

            result.Metadata.Should().ContainKey("MinLength").WhoseValue.Should().Be(8);
        }

        [Fact(DisplayName = "ToError: Should include additional metadata")]
        public void WithAdditionalMetadata_ShouldIncludeThem()
        {
            var failure = new ValidationFailure("Email", "Email is invalid");

            Error result = failure.ToError(("Resource", "User"), ("Field", "email"));

            result.Metadata.Should().ContainKey("Resource").WhoseValue.Should().Be("User");
            result.Metadata.Should().ContainKey("Field").WhoseValue.Should().Be("email");
        }

        [Fact(DisplayName = "ToError: Should throw ArgumentNullException when failure is null")]
        public void WithNullFailure_ShouldThrowArgumentNullException()
        {
            ValidationFailure? failure = null;

            Action action = () => failure!.ToError();

            action.Should().Throw<ArgumentNullException>();
        }

        [Fact(DisplayName = "ToError: Should use default error code when ErrorCode is null")]
        public void WithNullErrorCode_ShouldUseDefault()
        {
            var failure = new ValidationFailure("Field", "Error occurred")
            {
                ErrorCode = null
            };

            Error result = failure.ToError();

            result.Code.Should().Be("ValidationError");
        }
    }

    public class ToErrorsEnumerable
    {
        [Fact(DisplayName = "ToErrors(IEnumerable): Should map multiple ValidationFailures to Errors")]
        public void WithMultipleFailures_ShouldMapToErrors()
        {
            var failures = new List<ValidationFailure>
            {
                new("Email", "Email is required") { ErrorCode = "NotEmptyValidator" },
                new("Password", "Password is too short") { ErrorCode = "MinimumLengthValidator" }
            };

            List<Error> results = failures.ToErrors();

            results.Should().HaveCount(2);
            results[0].Code.Should().Be("NotEmptyValidator");
            results[1].Code.Should().Be("MinimumLengthValidator");
        }

        [Fact(DisplayName = "ToErrors(IEnumerable): Should return empty list when failures is null")]
        public void WithNullFailures_ShouldReturnEmptyList()
        {
            IEnumerable<ValidationFailure>? failures = null;

            List<Error> results = failures.ToErrors();

            results.Should().BeEmpty();
        }

        [Fact(DisplayName = "ToErrors(IEnumerable): Should return empty list when failures is empty")]
        public void WithEmptyFailures_ShouldReturnEmptyList()
        {
            var failures = new List<ValidationFailure>();

            List<Error> results = failures.ToErrors();

            results.Should().BeEmpty();
        }

        [Fact(DisplayName = "ToErrors(IEnumerable): Should include additional metadata in each error")]
        public void WithAdditionalMetadata_ShouldIncludeInEachError()
        {
            var failures = new List<ValidationFailure>
            {
                new("Email", "Email is required") { ErrorCode = "NotEmptyValidator" }
            };

            List<Error> results = failures.ToErrors(("Resource", "User"));

            results.Should().ContainSingle();
            results[0].Metadata.Should().ContainKey("Resource").WhoseValue.Should().Be("User");
        }
    }

    public class ToErrorList
    {
        [Fact(DisplayName = "ToErrorList: Should map invalid ValidationResult to Errors")]
        public void WithInvalidResult_ShouldMapToErrors()
        {
            var validationResult = new ValidationResult(
                new List<ValidationFailure>
                {
                    new("Email", "Email is required")
                });

            List<Error> results = validationResult.ToErrorList();

            results.Should().HaveCount(1);
            results[0].Message.Should().Be("Email is required");
        }

        [Fact(DisplayName = "ToErrorList: Should return empty list when ValidationResult is null")]
        public void WithNullResult_ShouldReturnEmptyList()
        {
            ValidationResult? validationResult = null;

            List<Error> results = validationResult!.ToErrorList();

            results.Should().BeEmpty();
        }
    }

    public class ToErrorsGeneric
    {
        [Fact(DisplayName = "ToErrors<T>: Should return failure Result<T> with errors when invalid")]
        public void WithInvalidResult_ShouldReturnFailureWithErrors()
        {
            var validationResult = new ValidationResult(
                new List<ValidationFailure>
                {
                    new("Email", "Email is required") { ErrorCode = "NotEmptyValidator" }
                });

            Result<string> result = validationResult.ToErrors<string>();

            result.IsFailure.Should().BeTrue();
            result.Errors.Should().HaveCount(1);
            result.Errors[0].Code.Should().Be("NotEmptyValidator");
        }

        [Fact(DisplayName = "ToErrors<T>: Should throw when validation result is valid")]
        public void WithValidResult_ShouldThrowInvalidOperationException()
        {
            var validationResult = new ValidationResult();

            Action action = () => validationResult.ToErrors<string>();

            action.Should().Throw<InvalidOperationException>()
                .WithMessage("Cannot convert valid ValidationResult to failure.");
        }

        [Fact(DisplayName = "ToErrors<T>: Should throw ArgumentNullException when result is null")]
        public void WithNullResult_ShouldThrowArgumentNullException()
        {
            ValidationResult? validationResult = null;

            Action action = () => validationResult!.ToErrors<string>();

            action.Should().Throw<ArgumentNullException>();
        }
    }

    public class ToErrorsResult
    {
        [Fact(DisplayName = "ToErrors: Should return failure Result with errors when invalid")]
        public void WithInvalidResult_ShouldReturnFailureWithErrors()
        {
            var validationResult = new ValidationResult(
                new List<ValidationFailure>
                {
                    new("Email", "Email is required")
                });

            Result result = validationResult.ToErrors();

            result.IsFailure.Should().BeTrue();
            result.Errors.Should().HaveCount(1);
        }

        [Fact(DisplayName = "ToErrors: Should return Ok when validation result is valid")]
        public void WithValidResult_ShouldReturnOk()
        {
            var validationResult = new ValidationResult();

            Result result = validationResult.ToErrors();

            result.IsSuccess.Should().BeTrue();
        }

        [Fact(DisplayName = "ToErrors: Should throw ArgumentNullException when result is null")]
        public void WithNullResult_ShouldThrowArgumentNullException()
        {
            ValidationResult? validationResult = null;

            Action action = () => validationResult!.ToErrors();

            action.Should().Throw<ArgumentNullException>();
        }
    }

    public class HasErrors
    {
        [Fact(DisplayName = "HasErrors: Should return true when ValidationResult has errors")]
        public void WithInvalidResult_ShouldReturnTrue()
        {
            var validationResult = new ValidationResult(
                new List<ValidationFailure>
                {
                    new("Email", "Email is required")
                });

            bool result = validationResult.HasErrors();

            result.Should().BeTrue();
        }

        [Fact(DisplayName = "HasErrors: Should return false when ValidationResult is valid")]
        public void WithValidResult_ShouldReturnFalse()
        {
            var validationResult = new ValidationResult();

            bool result = validationResult.HasErrors();

            result.Should().BeFalse();
        }

        [Fact(DisplayName = "HasErrors: Should return false when ValidationResult is null")]
        public void WithNullResult_ShouldReturnFalse()
        {
            ValidationResult? validationResult = null;

            bool result = validationResult!.HasErrors();

            result.Should().BeFalse();
        }

        [Fact(DisplayName = "HasErrors: Should return false when invalid but no errors in collection")]
        public void WithInvalidResultButNoErrors_ShouldReturnFalse()
        {
            var validationResult = new ValidationResult(
                new List<ValidationFailure>());

            bool result = validationResult.HasErrors();

            result.Should().BeFalse();
        }
    }
}