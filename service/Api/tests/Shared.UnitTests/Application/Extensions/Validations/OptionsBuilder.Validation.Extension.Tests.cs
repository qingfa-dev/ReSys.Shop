using FluentValidation;
using FluentValidation.Results;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using Shared.Application.Extensions.Validations;

namespace Shared.UnitTests.Application.Extensions.Validations;

[Trait("Category", "Unit")]
[Trait("Module", "Shared")]
[Trait("Feature", "Extensions")]
public class OptionsBuilderExtensionsTests
{
    public record TestOptions
    {
        public string? Name { get; init; }
        public int Value { get; init; }
    }

    public class TestValidator : AbstractValidator<TestOptions>
    {
        public TestValidator()
        {
            RuleFor(x => x.Name).NotEmpty();
        }
    }

    public class FluentValidateOptionsTests
    {
        public class Validate
        {
            [Fact]
            public void WhenNameMismatch_ReturnsSkip()
            {
                var sut = new FluentValidateOptions<TestOptions>(
                    Mock.Of<IServiceProvider>(), "named");

                var result = sut.Validate("other_name", new TestOptions { Name = "x", Value = 1 });

                result.Should().BeSameAs(ValidateOptionsResult.Skip);
            }

            [Fact]
            public void WhenNameIsNull_DoesNotSkip()
            {
                var (mockServiceProvider, _) = CreateMockServiceProviderWithValidValidator();

                var sut = new FluentValidateOptions<TestOptions>(
                    mockServiceProvider.Object, name: null);

                var result = sut.Validate("any", new TestOptions { Name = "x", Value = 1 });

                result.Skipped.Should().BeFalse();
                result.Succeeded.Should().BeTrue();
            }

            [Fact]
            public void WhenNameMatches_DoesNotSkip()
            {
                var (mockServiceProvider, _) = CreateMockServiceProviderWithValidValidator();

                var sut = new FluentValidateOptions<TestOptions>(
                    mockServiceProvider.Object, "cfg");

                var result = sut.Validate("cfg", new TestOptions { Name = "x", Value = 1 });

                result.Skipped.Should().BeFalse();
                result.Succeeded.Should().BeTrue();
            }

            [Fact]
            public void WhenOptionsIsNull_ThrowsArgumentNullException()
            {
                var sut = new FluentValidateOptions<TestOptions>(
                    Mock.Of<IServiceProvider>(), "cfg");

                Action act = () => sut.Validate("cfg", null!);

                act.Should().Throw<ArgumentNullException>();
            }

            [Fact]
            public void WhenValidatorReturnsSuccess_ReturnsValidateOptionsResultSuccess()
            {
                var (mockServiceProvider, mockValidator) = CreateMockServiceProviderWithValidValidator();
                mockValidator.Setup(v => v.Validate(It.IsAny<TestOptions>()))
                    .Returns(new ValidationResult());

                var sut = new FluentValidateOptions<TestOptions>(
                    mockServiceProvider.Object, null);

                var result = sut.Validate(null, new TestOptions { Name = "x", Value = 1 });

                result.Should().BeSameAs(ValidateOptionsResult.Success);
            }

            [Fact]
            public void WhenValidatorReturnsFailure_ReturnsValidateOptionsResultFailWithFormattedErrors()
            {
                var (mockServiceProvider, mockValidator) = CreateMockServiceProviderWithValidValidator();
                var failures = new List<ValidationFailure>
                {
                    new("Name", "must not be empty")
                };
                mockValidator.Setup(v => v.Validate(It.IsAny<TestOptions>()))
                    .Returns(new ValidationResult(failures));

                var sut = new FluentValidateOptions<TestOptions>(
                    mockServiceProvider.Object, null);

                var result = sut.Validate(null, new TestOptions { Name = null, Value = 1 });

                result.Failed.Should().BeTrue();
                result.Failures.Should().Contain(
                    "Validation failed for TestOptions.Name with the error: must not be empty");
            }

            [Fact]
            public void WhenValidatorReturnsMultipleFailures_ReturnsAllFormattedErrors()
            {
                var (mockServiceProvider, mockValidator) = CreateMockServiceProviderWithValidValidator();
                var failures = new List<ValidationFailure>
                {
                    new("Name", "must not be empty"),
                    new("Value", "must be greater than 0")
                };
                mockValidator.Setup(v => v.Validate(It.IsAny<TestOptions>()))
                    .Returns(new ValidationResult(failures));

                var sut = new FluentValidateOptions<TestOptions>(
                    mockServiceProvider.Object, null);

                var result = sut.Validate(null, new TestOptions { Name = null, Value = 0 });

                result.Failed.Should().BeTrue();
                result.Failures.Should().Contain(
                    "Validation failed for TestOptions.Name with the error: must not be empty");
                result.Failures.Should().Contain(
                    "Validation failed for TestOptions.Value with the error: must be greater than 0");
            }
        }
    }

    public class ValidateFluentValidation
    {
        [Fact]
        public void RegistersFluentValidateOptionsAsSingleton()
        {
            var services = new ServiceCollection();
            var builder = new OptionsBuilder<TestOptions>(services, "test");

            var result = builder.ValidateFluentValidation();

            result.Should().BeSameAs(builder);
            services.Should().ContainSingle(sd =>
                sd.ServiceType == typeof(IValidateOptions<TestOptions>)
                && sd.Lifetime == ServiceLifetime.Singleton);
        }

        [Fact]
        public void ReturnsSameBuilder()
        {
            var services = new ServiceCollection();
            var builder = new OptionsBuilder<TestOptions>(services, "test");

            var result = builder.ValidateFluentValidation();

            result.Should().BeSameAs(builder);
        }
    }

    private static (Mock<IServiceProvider>, Mock<IValidator<TestOptions>>) CreateMockServiceProviderWithValidValidator()
    {
        var mockValidator = new Mock<IValidator<TestOptions>>();
        mockValidator.Setup(v => v.Validate(It.IsAny<TestOptions>()))
            .Returns(new ValidationResult());

        var mockInnerServiceProvider = new Mock<IServiceProvider>();
        mockInnerServiceProvider.Setup(sp => sp.GetService(typeof(IValidator<TestOptions>)))
            .Returns(mockValidator.Object);

        var mockScope = new Mock<IServiceScope>();
        mockScope.Setup(s => s.ServiceProvider)
            .Returns(mockInnerServiceProvider.Object);

        var mockScopeFactory = new Mock<IServiceScopeFactory>();
        mockScopeFactory.Setup(sf => sf.CreateScope())
            .Returns(mockScope.Object);

        var mockServiceProvider = new Mock<IServiceProvider>();
        mockServiceProvider.Setup(sp => sp.GetService(typeof(IServiceScopeFactory)))
            .Returns(mockScopeFactory.Object);

        return (mockServiceProvider, mockValidator);
    }
}
