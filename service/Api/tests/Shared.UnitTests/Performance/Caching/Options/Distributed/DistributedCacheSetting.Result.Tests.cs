using Shared.Performance.Caching.Options.Distributed;

namespace Shared.UnitTests.Performance.Caching.Options.Distributed;

[Trait("Category", "Unit")]
[Trait("Module", "Infrastructure")]
[Trait("Feature", "Caching")]
public class DistributedCacheResultTests
{
    [Fact(DisplayName = "Failure.TypeRequired should return Error with expected code and type")]
    public void Failure_TypeRequired_ShouldReturnExpectedError()
    {
        Error error = DistributedCacheResult.Failure.TypeRequired;

        error.Code.Should().Be("Caching.Distributed.Type.Required");
        error.Message.Should().Be("Distributed cache type is required.");
        error.Type.Should().Be(ErrorType.Validation);
    }

    [Fact(DisplayName = "Failure.TypeInvalid should return Error with expected code and type")]
    public void Failure_TypeInvalid_ShouldReturnExpectedError()
    {
        Error error = DistributedCacheResult.Failure.TypeInvalid;

        error.Code.Should().Be("Caching.Distributed.Type.Invalid");
        error.Message.Should().Be(
            $"Distributed cache type must be '{string.Join("', '", DistributedCacheConstant.Patterns.ValidTypes)}'.");
        error.Type.Should().Be(ErrorType.Validation);
    }

    [Fact(DisplayName = "Failure.DefaultExpirationMinutesGreaterThanZero should return Error with expected code and type")]
    public void Failure_DefaultExpirationMinutesGreaterThanZero_ShouldReturnExpectedError()
    {
        Error error = DistributedCacheResult.Failure.DefaultExpirationMinutesGreaterThanZero;

        error.Code.Should().Be("Caching.Distributed.DefaultExpirationMinutes.GreaterThanZero");
        error.Message.Should().Be(
            $"Distributed cache default expiration must be greater than {DistributedCacheConstant.Constraints.DefaultExpirationMinutesMin} minutes.");
        error.Type.Should().Be(ErrorType.Validation);
    }
}
