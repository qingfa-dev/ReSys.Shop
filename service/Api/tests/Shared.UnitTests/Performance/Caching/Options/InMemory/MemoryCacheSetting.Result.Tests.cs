using Shared.Performance.Caching.Options.InMemory;

namespace Shared.UnitTests.Performance.Caching.Options.InMemory;

[Trait("Category", "Unit")]
[Trait("Module", "Infrastructure")]
[Trait("Feature", "Caching")]
public class MemoryCacheResultTests
{
    [Fact(DisplayName = "Failure.DefaultExpirationOutOfRange should return Error with expected code and type")]
    public void Failure_DefaultExpirationOutOfRange_ShouldReturnExpectedError()
    {
        Error error = MemoryCacheResult.Failure.DefaultExpirationOutOfRange;

        error.Code.Should().Be("Caching.Memory.DefaultExpiration.OutOfRange");
        error.Message.Should().Be(
            $"Memory cache default expiration must be between {MemoryCacheConstants.Constraints.DefaultExpirationMinutesMin} and {MemoryCacheConstants.Constraints.DefaultExpirationMinutesMax} minutes.");
        error.Type.Should().Be(ErrorType.Validation);
    }

    [Fact(DisplayName = "Failure.CompactionPercentageOutOfRange should return Error with expected code and type")]
    public void Failure_CompactionPercentageOutOfRange_ShouldReturnExpectedError()
    {
        Error error = MemoryCacheResult.Failure.CompactionPercentageOutOfRange;

        error.Code.Should().Be("Caching.Memory.CompactionPercentage.OutOfRange");
        error.Message.Should().Be(
            $"Memory cache compaction percentage must be between {MemoryCacheConstants.Constraints.CompactionPercentageMin} and {MemoryCacheConstants.Constraints.CompactionPercentageMax}.");
        error.Type.Should().Be(ErrorType.Validation);
    }

    [Fact(DisplayName = "Failure.SizeLimitOutOfRange should return Error with expected code and type")]
    public void Failure_SizeLimitOutOfRange_ShouldReturnExpectedError()
    {
        Error error = MemoryCacheResult.Failure.SizeLimitOutOfRange;

        error.Code.Should().Be("Caching.Memory.SizeLimit.OutOfRange");
        error.Message.Should().Be(
            $"Memory cache size limit must be at least {MemoryCacheConstants.Constraints.SizeLimitBytesMin} byte(s).");
        error.Type.Should().Be(ErrorType.Validation);
    }
}
