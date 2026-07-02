using Shared.Performance.Caching.Options;

namespace Shared.UnitTests.Performance.Caching.Options;

[Trait("Category", "Unit")]
[Trait("Module", "Infrastructure")]
[Trait("Feature", "Caching")]
public class CachingSettingResultTests
{
    [Fact(DisplayName = "Failure.MemoryRequired should return Error with expected code and type")]
    public void Failure_MemoryRequired_ShouldReturnExpectedError()
    {
        Error error = CachingSettingResult.Failure.MemoryRequired;

        error.Code.Should().Be("Caching.Memory.Required");
        error.Message.Should().Be("Memory cache options section is required.");
        error.Type.Should().Be(ErrorType.Validation);
    }

    [Fact(DisplayName = "Failure.DistributedRequired should return Error with expected code and type")]
    public void Failure_DistributedRequired_ShouldReturnExpectedError()
    {
        Error error = CachingSettingResult.Failure.DistributedRequired;

        error.Code.Should().Be("Caching.Distributed.Required");
        error.Message.Should().Be("Distributed cache options section is required.");
        error.Type.Should().Be(ErrorType.Validation);
    }

    [Fact(DisplayName = "Failure.HybridRequired should return Error with expected code and type")]
    public void Failure_HybridRequired_ShouldReturnExpectedError()
    {
        Error error = CachingSettingResult.Failure.HybridRequired;

        error.Code.Should().Be("Caching.Hybrid.Required");
        error.Message.Should().Be("Hybrid cache options section is required.");
        error.Type.Should().Be(ErrorType.Validation);
    }

    [Fact(DisplayName = "Failure.ConnectionStringMissing should return Error with connection string name in message")]
    public void Failure_ConnectionStringMissing_ShouldIncludeNameInMessage()
    {
        Error error = CachingSettingResult.Failure.ConnectionStringMissing("ShopCaching");

        error.Code.Should().Be("Caching.ConnectionString.Missing");
        error.Message.Should().Contain("ShopCaching");
        error.Type.Should().Be(ErrorType.Validation);
    }
}
