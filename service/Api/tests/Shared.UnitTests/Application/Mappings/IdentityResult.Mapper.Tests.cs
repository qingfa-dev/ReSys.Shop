using Microsoft.AspNetCore.Identity;

using Shared.Application.Mappings;

namespace Shared.UnitTests.Application.Mappings;

[Trait("Category", "Unit")]
[Trait("Module", "Identity")]
[Trait("Feature", "Mappings")]
public class IdentityResultMapperTests
{
    public class ToError
    {
        [Fact(DisplayName = "ToError: Should map IdentityError to Error with code and description")]
        public void WithValidError_ShouldMapToError()
        {
            var identityError = new IdentityError
            {
                Code = "DuplicateUserName",
                Description = "Username 'test' is already taken."
            };

            Error result = identityError.ToError();

            result.Code.Should().Be("DuplicateUserName");
            result.Message.Should().Be("Username 'test' is already taken.");
            result.Type.Should().Be(ErrorType.Validation);
        }

        [Fact(DisplayName = "ToError: Should include additional metadata")]
        public void WithAdditionalMetadata_ShouldIncludeThem()
        {
            var identityError = new IdentityError
            {
                Code = "InvalidToken",
                Description = "Token is invalid."
            };

            Error result = identityError.ToError(("Resource", "User"));

            result.Metadata.Should().ContainKey("Resource").WhoseValue.Should().Be("User");
        }

        [Fact(DisplayName = "ToError: Should throw ArgumentNullException when error is null")]
        public void WithNullError_ShouldThrowArgumentNullException()
        {
            IdentityError? identityError = null;

            Action action = () => identityError!.ToError();

            action.Should().Throw<ArgumentNullException>();
        }

        [Fact(DisplayName = "ToError: Should use defaults when Code and Description are null")]
        public void WithNullCodeAndDescription_ShouldUseDefaults()
        {
            var identityError = new IdentityError();

            Error result = identityError.ToError();

            result.Code.Should().Be(default!);
            result.Message.Should().Be("An identity error occurred");
        }
    }

    public class ToErrorsEnumerable
    {
        [Fact(DisplayName = "ToErrors(IEnumerable): Should map multiple IdentityErrors to Errors")]
        public void WithMultipleErrors_ShouldMapToErrors()
        {
            List<IdentityError> errors =
            [
                new() { Code = "Error1", Description = "Description 1" },
                new() { Code = "Error2", Description = "Description 2" }
            ];

            List<Error> results = errors.ToErrors();

            results.Should().HaveCount(2);
            results[0].Code.Should().Be("Error1");
            results[1].Code.Should().Be("Error2");
        }

        [Fact(DisplayName = "ToErrors(IEnumerable): Should return empty list when errors is null")]
        public void WithNullErrors_ShouldReturnEmptyList()
        {
            IEnumerable<IdentityError>? errors = null;

            List<Error> results = errors.ToErrors();

            results.Should().BeEmpty();
        }

        [Fact(DisplayName = "ToErrors(IEnumerable): Should return empty list when errors is empty")]
        public void WithEmptyErrors_ShouldReturnEmptyList()
        {
            List<IdentityError> errors = [];

            List<Error> results = errors.ToErrors();

            results.Should().BeEmpty();
        }

        [Fact(DisplayName = "ToErrors(IEnumerable): Should include additional metadata in each error")]
        public void WithAdditionalMetadata_ShouldIncludeInEachError()
        {
            List<IdentityError> errors =
            [
                new() { Code = "Error1", Description = "Description 1" }
            ];

            List<Error> results = errors.ToErrors(("Resource", "User"));

            results.Should().ContainSingle();
            results[0].Metadata.Should().ContainKey("Resource").WhoseValue.Should().Be("User");
        }
    }

    public class ToErrorList
    {
        [Fact(DisplayName = "ToErrorList: Should map failed IdentityResult to errors")]
        public void WithFailedResult_ShouldMapToErrors()
        {
            IdentityResult identityResult = IdentityResult.Failed(
                new IdentityError { Code = "Error1", Description = "Description 1" });

            List<Error> results = identityResult.ToErrorList();

            results.Should().HaveCount(1);
            results[0].Code.Should().Be("Error1");
        }

        [Fact(DisplayName = "ToErrorList: Should return empty list when IdentityResult succeeded")]
        public void WithSucceededResult_ShouldReturnEmptyList()
        {
            IdentityResult identityResult = IdentityResult.Success;

            List<Error> results = identityResult.ToErrorList();

            results.Should().BeEmpty();
        }

        [Fact(DisplayName = "ToErrorList: Should throw ArgumentNullException when result is null")]
        public void WithNullResult_ShouldThrowArgumentNullException()
        {
            IdentityResult? identityResult = null;

            Action action = () => identityResult!.ToErrorList();

            action.Should().Throw<ArgumentNullException>();
        }
    }

    public class ToErrorsGeneric
    {
        [Fact(DisplayName = "ToErrors<T>: Should throw when identity result succeeds")]
        public void WhenSucceeded_ShouldThrow()
        {
            IdentityResult identityResult = IdentityResult.Success;

            Action action = () => identityResult.ToResult<string>();

            action.Should().Throw<InvalidOperationException>()
                .WithMessage("Cannot convert successful IdentityResult to failure.");
        }

        [Fact(DisplayName = "ToErrors<T>: Should return failure with mapped errors")]
        public void WhenFailed_ShouldReturnFailureWithMappedErrors()
        {
            IdentityError identityError1 = new() { Code = "ErrorCode1", Description = "Error Description 1" };
            IdentityError identityError2 = new() { Code = "ErrorCode2", Description = "Error Description 2" };
            IdentityResult identityResult = IdentityResult.Failed(identityError1, identityError2);

            Result<string> result = identityResult.ToResult<string>();

            result.IsFailure.Should().BeTrue();
            result.Errors.Should().HaveCount(2);
            result.Errors[0].Code.Should().Be("ErrorCode1");
            result.Errors[0].Message.Should().Be("Error Description 1");
            result.Errors[1].Code.Should().Be("ErrorCode2");
            result.Errors[1].Message.Should().Be("Error Description 2");
        }

        [Fact(DisplayName = "ToErrors<T>: Should handle single error")]
        public void WithSingleError_ShouldMapCorrectly()
        {
            IdentityError identityError = new() { Code = "DuplicateUserName", Description = "Username 'test' is already taken." };
            IdentityResult identityResult = IdentityResult.Failed(identityError);

            Result<string> result = identityResult.ToResult<string>();

            result.IsFailure.Should().BeTrue();
            result.Errors.Should().HaveCount(1);
            result.Errors[0].Code.Should().Be("DuplicateUserName");
        }

        [Fact(DisplayName = "ToErrors<T>: Should throw ArgumentNullException when result is null")]
        public void WithNullResult_ShouldThrowArgumentNullException()
        {
            IdentityResult? identityResult = null;

            Action action = () => identityResult!.ToResult<string>();

            action.Should().Throw<ArgumentNullException>();
        }
    }

    public class ToErrorsResult
    {
        [Fact(DisplayName = "ToErrors: Should return Ok when identity result succeeds")]
        public void WhenSucceeded_ShouldReturnOk()
        {
            IdentityResult identityResult = IdentityResult.Success;

            Result result = identityResult.ToResult();

            result.IsSuccess.Should().BeTrue();
        }

        [Fact(DisplayName = "ToErrors: Should return failure with mapped errors")]
        public void WhenFailed_ShouldReturnFailureWithMappedErrors()
        {
            IdentityError identityError1 = new() { Code = "InvalidToken", Description = "Token is invalid." };
            IdentityError identityError2 = new() { Code = "ExpiredToken", Description = "Token has expired." };
            IdentityResult identityResult = IdentityResult.Failed(identityError1, identityError2);

            Result result = identityResult.ToResult();

            result.IsFailure.Should().BeTrue();
            result.Errors.Should().HaveCount(2);
            result.Errors[0].Code.Should().Be("InvalidToken");
            result.Errors[0].Message.Should().Be("Token is invalid.");
            result.Errors[1].Code.Should().Be("ExpiredToken");
            result.Errors[1].Message.Should().Be("Token has expired.");
        }

        [Fact(DisplayName = "ToErrors: Should handle empty errors collection")]
        public void WithEmptyErrors_ShouldReturnFailure()
        {
            IdentityResult identityResult = IdentityResult.Failed();

            Result result = identityResult.ToResult();

            result.IsFailure.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }

        [Fact(DisplayName = "ToErrors: Should throw ArgumentNullException when result is null")]
        public void WithNullResult_ShouldThrowArgumentNullException()
        {
            IdentityResult? identityResult = null;

            Action action = () => identityResult!.ToResult();

            action.Should().Throw<ArgumentNullException>();
        }
    }

    public class HasErrors
    {
        [Fact(DisplayName = "HasErrors: Should return true when IdentityResult has errors")]
        public void WithFailedResult_ShouldReturnTrue()
        {
            IdentityResult identityResult = IdentityResult.Failed(
                new IdentityError { Code = "Error", Description = "Error occurred" });

            bool result = identityResult.HasErrors();

            result.Should().BeTrue();
        }

        [Fact(DisplayName = "HasErrors: Should return false when IdentityResult succeeded")]
        public void WithSucceededResult_ShouldReturnFalse()
        {
            IdentityResult identityResult = IdentityResult.Success;

            bool result = identityResult.HasErrors();

            result.Should().BeFalse();
        }

        [Fact(DisplayName = "HasErrors: Should return false when IdentityResult is null")]
        public void WithNullResult_ShouldReturnFalse()
        {
            IdentityResult? identityResult = null;

            bool result = identityResult.HasErrors();

            result.Should().BeFalse();
        }

        [Fact(DisplayName = "HasErrors: Should return false when failed but no errors in collection")]
        public void WithFailedResultButNoErrors_ShouldReturnFalse()
        {
            IdentityResult identityResult = IdentityResult.Failed();

            bool result = identityResult.HasErrors();

            result.Should().BeFalse();
        }
    }
}