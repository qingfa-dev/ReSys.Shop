using Api.Tests.Infrastructure;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Module.Inventory.Domain.StockLocations;
using Module.Inventory.Domain.StockLocations.StockItems;
using Module.Inventory.Domain.StockReservations;
using Module.Inventory.Domain.StockTransfers;
using Module.Inventory.Services.Abstractions;

using Shared.Operational.Persistence.Data;

namespace Api.Tests.Scenarios.Inventory;

[Trait("Category", "Integration")]
[Trait("Module", "Inventory")]
public sealed class StockTransferIntegrationTests(ApiFixture fixture) : InventoryIntegrationTestBase(fixture)
{
    [Fact(DisplayName = "AC-INV-001: Stock transfer Draft → InTransit → Received with full receipt")]
    public async Task StockTransfer_Lifecycle_DraftToReceived()
    {
        // Arrange
        using var scope = Fixture.Factory.Services.CreateScope();
        IApplicationDbContext db = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();

        Guid sourceLocationId = Guid.NewGuid();
        Guid destLocationId = Guid.NewGuid();
        Guid variantId = Guid.NewGuid();

        Result<StockLocation> sourceResult = StockLocationMethod.Create(
            name: "WH-Source",
            active: true,
            id: sourceLocationId);
        sourceResult.IsSuccess.Should().BeTrue();
        db.Set<StockLocation>().Add(sourceResult.Value);

        Result<StockLocation> destResult = StockLocationMethod.Create(
            name: "WH-Dest",
            active: true,
            id: destLocationId);
        destResult.IsSuccess.Should().BeTrue();
        db.Set<StockLocation>().Add(destResult.Value);

        Result<StockItem> stockResult = StockItemMethod.Create(
            stockLocationId: sourceLocationId,
            variantId: variantId,
            backorderable: false,
            countOnHand: 10);
        stockResult.IsSuccess.Should().BeTrue();
        db.Set<StockItem>().Add(stockResult.Value);

        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Create transfer: 5 units from source → dest
        Result<StockTransfer> transferResult = StockTransferExtensions.Create(
            reference: "TR-001",
            sourceLocationId: sourceLocationId,
            destinationLocationId: destLocationId,
            items: [(variantId, 5)]);
        transferResult.IsSuccess.Should().BeTrue();
        StockTransfer transfer = transferResult.Value;
        db.Set<StockTransfer>().Add(transfer);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Assert: Draft state
        transfer.State.Should().Be(TransferState.Draft);

        // Act: Transfer (Draft → InTransit)
        Result transferAction = transfer.Transfer();
        transferAction.IsSuccess.Should().BeTrue();
        transfer.State.Should().Be(TransferState.InTransit);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act: Receive full quantity (InTransit → Received)
        Result receiveResult = transfer.Receive(variantId, 5);
        receiveResult.IsSuccess.Should().BeTrue();
        transfer.State.Should().Be(TransferState.Received, "auto-transition when all items fully received");
        transfer.TransferItems.First().ReceivedQuantity.Should().Be(5);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);
    }

    [Fact(DisplayName = "AC-INV-002: Stock transfer over-receipt is rejected")]
    public async Task StockTransfer_OverReceipt_ReturnsError()
    {
        // Arrange
        using var scope = Fixture.Factory.Services.CreateScope();
        IApplicationDbContext db = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();

        Guid sourceId = Guid.NewGuid();
        Guid destId = Guid.NewGuid();
        Guid variantId = Guid.NewGuid();

        db.Set<StockLocation>().Add(StockLocationMethod.Create("WH-A", true, id: sourceId).Value);
        db.Set<StockLocation>().Add(StockLocationMethod.Create("WH-B", true, id: destId).Value);
        db.Set<StockItem>().Add(StockItemMethod.Create(sourceId, variantId, false, 20).Value);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        StockTransfer transfer = StockTransferExtensions.Create(
            "TR-002", sourceId, destId, [(variantId, 3)]).Value;
        db.Set<StockTransfer>().Add(transfer);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        transfer.Transfer();
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act: Receive more than transferred
        Result overReceive = transfer.Receive(variantId, 5);

        // Assert
        overReceive.IsFailure.Should().BeTrue("receiving more than transferred must be rejected");
        overReceive.Errors.First().Code.Should().Contain("ReceivedExceedsTransferred");
    }

    [Fact(DisplayName = "AC-INV-003: Stock transfer cancel in Draft state")]
    public async Task StockTransfer_CancelDraft_Succeeds()
    {
        // Arrange
        using var scope = Fixture.Factory.Services.CreateScope();
        IApplicationDbContext db = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();

        Guid sourceId = Guid.NewGuid();
        Guid destId = Guid.NewGuid();
        Guid variantId = Guid.NewGuid();

        db.Set<StockLocation>().Add(StockLocationMethod.Create("WH-X", true, id: sourceId).Value);
        db.Set<StockLocation>().Add(StockLocationMethod.Create("WH-Y", true, id: destId).Value);
        db.Set<StockItem>().Add(StockItemMethod.Create(sourceId, variantId, false, 5).Value);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        StockTransfer transfer = StockTransferExtensions.Create(
            "TR-003", sourceId, destId, [(variantId, 2)]).Value;
        db.Set<StockTransfer>().Add(transfer);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act: Cancel from Draft
        Result cancelResult = transfer.Cancel();

        // Assert
        cancelResult.IsSuccess.Should().BeTrue();
        transfer.State.Should().Be(TransferState.Canceled);
    }
}

[Trait("Category", "Integration")]
[Trait("Module", "Inventory")]
public sealed class ReservationExpiryIntegrationTests(ApiFixture fixture) : InventoryIntegrationTestBase(fixture)
{
    [Fact(DisplayName = "AC-INV-004: Expired reservation is detected by IsExpired")]
    public async Task Reservation_Expired_IsExpiredReturnsTrue()
    {
        // Arrange: create reservation that expired 1 minute ago
        using var scope = Fixture.Factory.Services.CreateScope();
        IApplicationDbContext db = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();

        var variantId = Guid.NewGuid();
        var reservation = StockReservationMethod.SeedForTest(
            variantId: variantId,
            quantity: 3,
            state: ReservationState.Reserved,
            expiresAtUtc: DateTimeOffset.UtcNow.AddMinutes(-1),
            stockLocationId: Guid.NewGuid());
        db.Set<StockReservation>().Add(reservation);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Assert
        reservation.IsExpired().Should().BeTrue();
    }

    [Fact(DisplayName = "AC-INV-005: Active (non-expired) reservation is not expired")]
    public async Task Reservation_Active_IsExpiredReturnsFalse()
    {
        // Arrange: create reservation expiring in 30 minutes
        using var scope = Fixture.Factory.Services.CreateScope();
        IApplicationDbContext db = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();

        var variantId = Guid.NewGuid();
        var reservation = StockReservationMethod.SeedForTest(
            variantId: variantId,
            quantity: 2,
            state: ReservationState.Reserved,
            expiresAtUtc: DateTimeOffset.UtcNow.AddMinutes(30),
            stockLocationId: Guid.NewGuid());
        db.Set<StockReservation>().Add(reservation);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Assert
        reservation.IsExpired().Should().BeFalse();
    }

    [Fact(DisplayName = "AC-INV-006: Reservation expiry sweep expires overdue reservations")]
    public async Task Reservation_ExpirySweep_ExpiresOverdueReservations()
    {
        // Arrange: create 2 reservations — one expired, one active
        using var scope = Fixture.Factory.Services.CreateScope();
        IApplicationDbContext db = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();

        var variantId = Guid.NewGuid();

        var expired = StockReservationMethod.SeedForTest(
            variantId: variantId,
            quantity: 5,
            state: ReservationState.Reserved,
            expiresAtUtc: DateTimeOffset.UtcNow.AddMinutes(-10),
            stockLocationId: Guid.NewGuid());

        var active = StockReservationMethod.SeedForTest(
            variantId: variantId,
            quantity: 3,
            state: ReservationState.Reserved,
            expiresAtUtc: DateTimeOffset.UtcNow.AddMinutes(30),
            stockLocationId: Guid.NewGuid());

        db.Set<StockReservation>().AddRange(expired, active);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act: Run expiry sweep
        IStockReservationService reservationService = scope.ServiceProvider
            .GetRequiredService<IStockReservationService>();
        int expiredCount = await reservationService.ExpireReservationsAndRestoreStockAsync(
            TestContext.Current.CancellationToken);

        // Assert
        expiredCount.Should().BeGreaterThanOrEqualTo(1, "at least the overdue reservation must be expired");

        // Reload from DB
        var reloadedExpired = await db.Set<StockReservation>()
            .FirstOrDefaultAsync(r => r.Id == expired.Id, TestContext.Current.CancellationToken);
        reloadedExpired.Should().NotBeNull();
        reloadedExpired!.State.Should().Be(ReservationState.Expired);

        var reloadedActive = await db.Set<StockReservation>()
            .FirstOrDefaultAsync(r => r.Id == active.Id, TestContext.Current.CancellationToken);
        reloadedActive.Should().NotBeNull();
        reloadedActive!.State.Should().Be(ReservationState.Reserved, "active reservation must not be expired");
    }
}
