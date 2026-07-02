using Shared.Security.Authentication.Tokens.Services.Refresh.Protections;

namespace Shared.UnitTests.Security.Authentication.Tokens.Services.Refresh.Protections;

[Trait("Category", "Unit")]
[Trait("Module", "Shared")]
[Trait("Feature", "TokenTheftDetector")]
public sealed class NoOpTokenTheftDetectorTests
{
    private readonly NoOpTokenTheftDetector _detector = new();

    [Fact(DisplayName = "IsTokenReusedAsync should always return false")]
    public async Task IsTokenReusedAsync_ShouldReturnFalse()
    {
        // Act
        Result<bool> result = await _detector.IsTokenReusedAsync("any-token", Guid.NewGuid());

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeFalse();
    }

    [Fact(DisplayName = "MarkTokenAsUsedAsync should complete without error")]
    public async Task MarkTokenAsUsedAsync_ShouldCompleteSilently()
    {
        // Act
        Func<Task> act = () => _detector.MarkTokenAsUsedAsync("any-token", Guid.NewGuid());

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact(DisplayName = "RevokeAllUserTokensAsync should complete without error")]
    public async Task RevokeAllUserTokensAsync_ShouldCompleteSilently()
    {
        // Act
        Func<Task> act = () => _detector.RevokeAllUserTokensAsync(Guid.NewGuid(), "test_reason");

        // Assert
        await act.Should().NotThrowAsync();
    }
}
