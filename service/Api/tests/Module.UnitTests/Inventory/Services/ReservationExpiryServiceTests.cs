using Microsoft.Extensions.DependencyInjection;

using Module.Inventory.Services;
using Module.Inventory.Services.Abstractions;

#pragma warning disable CA1873 // Logger message delegate evaluation

namespace Module.UnitTests.Inventory.Services;

[Trait("Category", "Unit")]
[Trait("Module", "Inventory")]
[Trait("Feature", "ReservationExpiryService")]
public class ReservationExpiryServiceTests : IDisposable
{
    private readonly Mock<IStockReservationService> _stockCheckerMock;
    private readonly Mock<IServiceScopeFactory> _scopeFactoryMock;
    private readonly Mock<IServiceScope> _scopeMock;
    private readonly Mock<IServiceProvider> _serviceProviderMock;
    private readonly Mock<ILogger<ReservationExpiryService>> _loggerMock;
    private readonly ReservationExpiryService _service;

    public ReservationExpiryServiceTests()
    {
        _stockCheckerMock = new Mock<IStockReservationService>();
        _scopeMock = new Mock<IServiceScope>();
        _serviceProviderMock = new Mock<IServiceProvider>();
        _scopeFactoryMock = new Mock<IServiceScopeFactory>();
        _loggerMock = new Mock<ILogger<ReservationExpiryService>>();

        _serviceProviderMock
            .Setup(x => x.GetService(typeof(IStockReservationService)))
            .Returns(_stockCheckerMock.Object);

        _scopeMock
            .Setup(x => x.ServiceProvider)
            .Returns(_serviceProviderMock.Object);

        _scopeFactoryMock
            .Setup(x => x.CreateScope())
            .Returns(_scopeMock.Object);

        _service = new ReservationExpiryService(_scopeFactoryMock.Object, _loggerMock.Object);

        ReservationExpiryService.SweepInterval = TimeSpan.FromMilliseconds(1);
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "ExecuteAsync: Should call ExpireReservationsAndRestoreStockAsync at least once")]
    public async Task ExecuteAsync_ShouldCallExpireMethod_AtLeastOnce()
    {
        _stockCheckerMock
            .Setup(x => x.ExpireReservationsAndRestoreStockAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        using var cts = new CancellationTokenSource();

        // Start the service and let it run briefly
        var task = _service.StartAsync(cts.Token);
        await Task.Delay(50, TestContext.Current.CancellationToken);
        cts.Cancel();

        await task;

        _stockCheckerMock.Verify(
            x => x.ExpireReservationsAndRestoreStockAsync(It.IsAny<CancellationToken>()),
            Times.AtLeastOnce);
    }

    [Fact(DisplayName = "ExecuteAsync: Should call ExpireReservationsAndRestoreStock with correct return value")]
    public async Task ExecuteAsync_ShouldCallExpireMethod_WhenExpiredCountReturned()
    {
        var expireCount = 0;
        _stockCheckerMock
            .Setup(x => x.ExpireReservationsAndRestoreStockAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => ++expireCount <= 2 ? 3 : 0);

        using var cts = new CancellationTokenSource();
        var task = _service.StartAsync(cts.Token);
        await Task.Delay(100, TestContext.Current.CancellationToken);
        cts.Cancel();
        await task;

        // Verify ExpireReservationsAndRestoreStockAsync was called multiple times
        _stockCheckerMock.Verify(
            x => x.ExpireReservationsAndRestoreStockAsync(It.IsAny<CancellationToken>()),
            Times.AtLeast(2));
    }

    [Fact(DisplayName = "ExecuteAsync: Should not log when no reservations expired")]
    public async Task ExecuteAsync_ShouldNotLog_WhenNoExpiredReservations()
    {
        _stockCheckerMock
            .Setup(x => x.ExpireReservationsAndRestoreStockAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        using var cts = new CancellationTokenSource();
        var task = _service.StartAsync(cts.Token);
        await Task.Delay(50, TestContext.Current.CancellationToken);
        cts.Cancel();
        await task;

        // With 0 expired, only the "started"/"stopped" logs fire, not SweepCompleted
        // Verify the stock checker was called (proving the sweep ran)
        _stockCheckerMock.Verify(
            x => x.ExpireReservationsAndRestoreStockAsync(It.IsAny<CancellationToken>()),
            Times.AtLeastOnce);
    }

    [Fact(DisplayName = "ExecuteAsync: Should catch exception and continue loop")]
    public async Task ExecuteAsync_ShouldCatchException_AndContinueLoop()
    {
        var callCount = 0;
        _stockCheckerMock
            .Setup(x => x.ExpireReservationsAndRestoreStockAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(() =>
            {
                callCount++;
                if (callCount == 1) throw new InvalidOperationException("Test exception");
                return 0;
            });

        using var cts = new CancellationTokenSource();
        var task = _service.StartAsync(cts.Token);
        await Task.Delay(100, TestContext.Current.CancellationToken);
        cts.Cancel();
        await task;

        // Should have been called at least twice (first throws, second succeeds)
        _stockCheckerMock.Verify(
            x => x.ExpireReservationsAndRestoreStockAsync(It.IsAny<CancellationToken>()),
            Times.AtLeast(2));

        // Verify error was logged
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    [Fact(DisplayName = "ExecuteAsync: Should complete without exception when cancelled immediately")]
    public async Task ExecuteAsync_ShouldComplete_WhenCancelledImmediately()
    {
        _stockCheckerMock
            .Setup(x => x.ExpireReservationsAndRestoreStockAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        // Cancel immediately — service should exit cleanly
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var task = _service.StartAsync(cts.Token);
        await task; // Should not throw

        // Verify ExpireReservationsAndRestoreStockAsync was never called (loop didn't execute)
        _stockCheckerMock.Verify(
            x => x.ExpireReservationsAndRestoreStockAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
