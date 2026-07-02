using Shared.Performance.Caching.Options.Hybrid;

namespace Shared.UnitTests.Performance.Caching.Options.Hybrid;

[Trait("Category", "Unit")]
[Trait("Module", "Infrastructure")]
[Trait("Feature", "Caching")]
public class DistributedCacheSettingResultTests
{
    [Fact(DisplayName = "Failure.DefaultExpirationOutOfRange should return Error with expected code and type")]
    public void Failure_DefaultExpirationOutOfRange_ShouldReturnExpectedError()
    {
        Error error = DistributedCacheSettingResult.Failure.DefaultExpirationOutOfRange;

        error.Code.Should().Be("Caching.Hybrid.DefaultExpiration.OutOfRange");
        error.Message.Should().Be(
            $"Default expiration must be between {HybridCacheSettingConstant.Constraints.DefaultExpirationMinutesMin} and {HybridCacheSettingConstant.Constraints.DefaultExpirationMinutesMax} minutes.");
        error.Type.Should().Be(ErrorType.Validation);
    }

    [Fact(DisplayName = "Failure.PayloadBytesOutOfRange should return Error with expected code and type")]
    public void Failure_PayloadBytesOutOfRange_ShouldReturnExpectedError()
    {
        Error error = DistributedCacheSettingResult.Failure.PayloadBytesOutOfRange;

        error.Code.Should().Be("Caching.Hybrid.MaximumPayloadBytes.OutOfRange");
        error.Message.Should().Be(
            $"Maximum payload size must be between {HybridCacheSettingConstant.Constraints.MaximumPayloadBytesMin} and {HybridCacheSettingConstant.Constraints.MaximumPayloadBytesMax} bytes.");
        error.Type.Should().Be(ErrorType.Validation);
    }

    [Fact(DisplayName = "Failure.KeyLengthOutOfRange should return Error with expected code and type")]
    public void Failure_KeyLengthOutOfRange_ShouldReturnExpectedError()
    {
        Error error = DistributedCacheSettingResult.Failure.KeyLengthOutOfRange;

        error.Code.Should().Be("Caching.Hybrid.MaximumKeyLength.OutOfRange");
        error.Message.Should().Be(
            $"Maximum key length must be between {HybridCacheSettingConstant.Constraints.MaximumKeyLengthMin} and {HybridCacheSettingConstant.Constraints.MaximumKeyLengthMax} characters.");
        error.Type.Should().Be(ErrorType.Validation);
    }
}
