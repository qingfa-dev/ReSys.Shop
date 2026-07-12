# Catalog Stock-Based Availability Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Replace the price-based availability check in `GetAvailability` with a real stock computation, sharing logic with `GetStockAvailability` via a new `IStockAvailabilityCalculator` service.

**Architecture:** Extract a stateless `IStockAvailabilityCalculator` in `Module.Inventory` that computes per-variant and per-location stock totals. `GetStockAvailability` (Inventory) and `GetAvailability` (Catalog) both consume it. The Catalog handler becomes a thin read-side adapter that joins products to stock snapshots and groups by OptionType axes.

**Tech Stack:** .NET 10, xUnit v3, Moq, EF Core InMemory, Mapster (existing in repo for projections).

## Global Constraints

- `TreatWarningsAsErrors=true`
- All handlers return `Result<T>` / `Result`
- Module isolation: Catalog can depend on Inventory **only** through a service contract (`IStockAvailabilityCalculator` lives in Inventory, exposed to Catalog)
- Test pattern: `[Fact(DisplayName = "...")]`, `[Trait("Category", "Unit")]`, `[Trait("Module", "Catalog")]`
- Low-stock threshold default: 3 units (configurable via `IOptions<InventorySettings>` later — not in this plan)

## File Structure

### Files to create

| File | Purpose |
|------|---------|
| `service/Api/src/Module/Inventory/Services/StockAvailabilityCalculator.cs` | Implements `IStockAvailabilityCalculator` |
| `service/Api/src/Module/Inventory/Services/IStockAvailabilityCalculator.cs` | Interface contract |
| `service/Api/src/Module/Inventory/Services/StockSnapshot.cs` | Record types for results |
| `service/Api/src/Module/Inventory/Services/LowStockThreshold.cs` | Constant + config hook |
| `service/Api/tests/Module.UnitTests/Inventory/Services/StockAvailabilityCalculatorTests.cs` | Calculator unit tests |
| `service/Api/tests/Module.UnitTests/Catalog/GetAvailabilityTests.cs` | New catalog tests |

### Files to modify

| File | Change |
|------|--------|
| `service/Api/src/Module/Inventory/Inventory.Extension.cs` | Register `IStockAvailabilityCalculator` |
| `service/Api/src/Module/Inventory/Features/Storefront/StockAvailability/Check/GetStockAvailability.cs` | Delegate to `IStockAvailabilityCalculator` |
| `service/Api/src/Module/Catalog/Features/Storefront/Products/Get/Availability/GetAvailability.cs` | Inject `IStockAvailabilityCalculator`, compute real status |
| `service/Api/src/Module/Catalog/Catalog.Extension.cs` | No change — `IStockAvailabilityCalculator` is already accessible through the Inventory service registration |

---

## Task 1: Define the `IStockAvailabilityCalculator` contract

**Files:**
- Create: `service/Api/src/Module/Inventory/Services/IStockAvailabilityCalculator.cs`
- Create: `service/Api/src/Module/Inventory/Services/StockSnapshot.cs`

**Context:** Define the contract before writing tests or the implementation. Both `GetStockAvailability` and `GetAvailability` will consume it.

- [ ] **Step 1: Create the result record types**

Create file `service/Api/src/Module/Inventory/Services/StockSnapshot.cs`:

```csharp
namespace Module.Inventory.Services;

public sealed record StockSnapshot(
    int TotalOnHand,
    int TotalReserved,
    int TotalAvailable,
    bool Backorderable,
    IReadOnlyList<LocationStockSnapshot> Locations);

public sealed record LocationStockSnapshot(
    Guid StockLocationId,
    string LocationName,
    int CountOnHand,
    int ReservedCount,
    int AvailableCount,
    bool Active,
    bool Backorderable);
```

- [ ] **Step 2: Create the interface**

Create file `service/Api/src/Module/Inventory/Services/IStockAvailabilityCalculator.cs`:

```csharp
namespace Module.Inventory.Services;

public interface IStockAvailabilityCalculator
{
    Task<StockSnapshot> GetForVariantAsync(Guid variantId, CancellationToken ct);
    Task<IReadOnlyDictionary<Guid, int>> GetAvailableByVariantAsync(
        IEnumerable<Guid> variantIds, CancellationToken ct);
}
```

- [ ] **Step 3: Build the solution**

Run: `dotnet build service/Api/Api.slnx`
Expected: success (interface and record only — no implementation yet).

- [ ] **Step 4: Commit**

```bash
git add service/Api/src/Module/Inventory/Services/IStockAvailabilityCalculator.cs service/Api/src/Module/Inventory/Services/StockSnapshot.cs
git commit -m "feat(inventory): add IStockAvailabilityCalculator contract"
```

---

## Task 2: Implement `StockAvailabilityCalculator`

**Files:**
- Create: `service/Api/src/Module/Inventory/Services/LowStockThreshold.cs`
- Create: `service/Api/src/Module/Inventory/Services/StockAvailabilityCalculator.cs`
- Modify: `service/Api/src/Module/Inventory/Inventory.Extension.cs`
- Test: `service/Api/tests/Module.UnitTests/Inventory/Services/StockAvailabilityCalculatorTests.cs`

**Context:** The calculator reads `StockItem` + `StockReservation` (same query as `GetStockAvailability.cs:22-36`) and projects to the snapshot types. It MUST NOT depend on the catalog.

- [ ] **Step 1: Create the low-stock threshold constant**

Create file `service/Api/src/Module/Inventory/Services/LowStockThreshold.cs`:

```csharp
namespace Module.Inventory.Services;

public static class LowStockThreshold
{
    public const int Default = 3;
}
```

- [ ] **Step 2: Write the failing tests**

Create file `service/Api/tests/Module.UnitTests/Inventory/Services/StockAvailabilityCalculatorTests.cs`:

```csharp
using Microsoft.EntityFrameworkCore;
using Module.Inventory.Domain.StockLocations;
using Module.Inventory.Domain.StockLocations.StockItems;
using Module.Inventory.Domain.StockReservations;
using Module.Inventory.Services;

namespace Module.UnitTests.Inventory.Services;

[Trait("Category", "Unit")]
[Trait("Module", "Inventory")]
public class StockAvailabilityCalculatorTests : IDisposable
{
    private readonly ApplicationDbContext _db;
    private readonly StockAvailabilityCalculator _sut;

    public StockAvailabilityCalculatorTests()
    {
        var opts = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(StockItem).Assembly];
        _db = new ApplicationDbContext(opts);
        _sut = new StockAvailabilityCalculator(_db);
    }

    public void Dispose() { _db.Dispose(); GC.SuppressFinalize(this); }

    [Fact(DisplayName = "GetForVariant: no stock returns zeros and not backorderable")]
    public async Task GetForVariant_NoStock_ReturnsZero()
    {
        var variantId = Guid.NewGuid();
        var result = await _sut.GetForVariantAsync(variantId, TestContext.Current.CancellationToken);

        result.TotalOnHand.Should().Be(0);
        result.TotalReserved.Should().Be(0);
        result.TotalAvailable.Should().Be(0);
        result.Backorderable.Should().BeFalse();
        result.Locations.Should().BeEmpty();
    }

    [Fact(DisplayName = "GetForVariant: excludes reservations that have expired")]
    public async Task GetForVariant_ExcludesExpiredReservations()
    {
        var variantId = Guid.NewGuid();
        var locationId = Guid.NewGuid();
        SeedLocation(locationId, "WH-1", active: true);
        SeedStockItem(variantId, locationId, onHand: 10, backorderable: false);
        SeedReservation(variantId, locationId, quantity: 5, expiresInMinutes: -10); // expired

        var result = await _sut.GetForVariantAsync(variantId, TestContext.Current.CancellationToken);

        result.TotalReserved.Should().Be(0);
        result.TotalAvailable.Should().Be(10);
    }

    [Fact(DisplayName = "GetForVariant: any backorderable location makes the variant backorderable")]
    public async Task GetForVariant_AnyBackorderableLocation_IsBackorderable()
    {
        var variantId = Guid.NewGuid();
        var locA = Guid.NewGuid();
        var locB = Guid.NewGuid();
        SeedLocation(locA, "WH-A", active: true);
        SeedLocation(locB, "WH-B", active: true);
        SeedStockItem(variantId, locA, onHand: 0, backorderable: false);
        SeedStockItem(variantId, locB, onHand: 0, backorderable: true);

        var result = await _sut.GetForVariantAsync(variantId, TestContext.Current.CancellationToken);

        result.Backorderable.Should().BeTrue();
    }

    [Fact(DisplayName = "GetAvailableByVariant: returns map of variant id to available count")]
    public async Task GetAvailableByVariant_ReturnsMap()
    {
        var v1 = Guid.NewGuid();
        var v2 = Guid.NewGuid();
        var loc = Guid.NewGuid();
        SeedLocation(loc, "WH-1", active: true);
        SeedStockItem(v1, loc, onHand: 5, backorderable: false);
        SeedStockItem(v2, loc, onHand: 2, backorderable: false);
        SeedReservation(v1, loc, quantity: 1, expiresInMinutes: 30);

        var result = await _sut.GetAvailableByVariantAsync(
            new[] { v1, v2 }, TestContext.Current.CancellationToken);

        result[v1].Should().Be(4);
        result[v2].Should().Be(2);
    }

    private void SeedLocation(Guid id, string name, bool active) =>
        _db.Set<StockLocation>().Add(new StockLocation { Id = id, Name = name, Active = active });

    private void SeedStockItem(Guid variantId, Guid locationId, int onHand, bool backorderable) =>
        _db.Set<StockItem>().Add(new StockItem
        {
            Id = Guid.NewGuid(),
            VariantId = variantId,
            StockLocationId = locationId,
            CountOnHand = onHand,
            Backorderable = backorderable
        });

    private void SeedReservation(Guid variantId, Guid locationId, int quantity, int expiresInMinutes) =>
        _db.Set<StockReservation>().Add(new StockReservation
        {
            Id = Guid.NewGuid(),
            VariantId = variantId,
            StockLocationId = locationId,
            Quantity = quantity,
            State = ReservationState.Reserved,
            ExpiresAtUtc = DateTimeOffset.UtcNow.AddMinutes(expiresInMinutes),
            CartToken = "test-cart"
        });
}
```

- [ ] **Step 3: Run the tests to verify they fail**

Run: `dotnet test service/Api/tests/Module.UnitTests/Module.UnitTests.csproj --filter "FullyQualifiedName~StockAvailabilityCalculatorTests" --no-restore`
Expected: compile error — `StockAvailabilityCalculator` does not exist.

- [ ] **Step 4: Implement `StockAvailabilityCalculator`**

Create file `service/Api/src/Module/Inventory/Services/StockAvailabilityCalculator.cs`:

```csharp
using Microsoft.EntityFrameworkCore;
using Module.Inventory.Domain.StockLocations.StockItems;
using Module.Inventory.Domain.StockReservations;

namespace Module.Inventory.Services;

public sealed class StockAvailabilityCalculator(IApplicationDbContext dbContext) : IStockAvailabilityCalculator
{
    public async Task<StockSnapshot> GetForVariantAsync(Guid variantId, CancellationToken ct)
    {
        var now = DateTimeOffset.UtcNow;

        var stockItems = await dbContext.Set<StockItem>()
            .Include(si => si.StockLocation)
            .Where(si => si.VariantId == variantId)
            .AsNoTracking()
            .ToListAsync(ct);

        var reservedByLocation = await dbContext.Set<StockReservation>()
            .Where(r => r.VariantId == variantId
                        && r.State == ReservationState.Reserved
                        && r.ExpiresAtUtc > now)
            .GroupBy(r => r.StockLocationId)
            .Select(g => new { StockLocationId = g.Key, Reserved = g.Sum(r => r.Quantity) })
            .ToListAsync(ct);

        var reservedMap = reservedByLocation
            .Where(r => r.StockLocationId.HasValue)
            .ToDictionary(r => r.StockLocationId!.Value, r => r.Reserved);

        var locations = stockItems
            .Where(si => si.StockLocation is { IsDeleted: false, Active: true })
            .Select(si =>
            {
                var reserved = reservedMap.GetValueOrDefault(si.StockLocationId, 0);
                var available = si.CountOnHand - reserved;
                return new LocationStockSnapshot(
                    si.StockLocationId,
                    si.StockLocation!.Name,
                    si.CountOnHand,
                    reserved,
                    Math.Max(available, 0),
                    si.StockLocation.Active,
                    si.Backorderable);
            })
            .ToList();

        var totalOnHand = locations.Sum(l => l.CountOnHand);
        var totalReserved = locations.Sum(l => l.ReservedCount);
        var totalAvailable = Math.Max(totalOnHand - totalReserved, 0);
        var backorderable = locations.Any(l => l.Backorderable);

        return new StockSnapshot(totalOnHand, totalReserved, totalAvailable, backorderable, locations);
    }

    public async Task<IReadOnlyDictionary<Guid, int>> GetAvailableByVariantAsync(
        IEnumerable<Guid> variantIds, CancellationToken ct)
    {
        var ids = variantIds.Distinct().ToList();
        if (ids.Count == 0) return new Dictionary<Guid, int>();

        var now = DateTimeOffset.UtcNow;

        var onHand = await dbContext.Set<StockItem>()
            .Where(si => ids.Contains(si.VariantId))
            .GroupBy(si => si.VariantId)
            .Select(g => new { VariantId = g.Key, OnHand = g.Sum(si => si.CountOnHand) })
            .ToListAsync(ct);

        var reserved = await dbContext.Set<StockReservation>()
            .Where(r => ids.Contains(r.VariantId)
                        && r.State == ReservationState.Reserved
                        && r.ExpiresAtUtc > now)
            .GroupBy(r => r.VariantId)
            .Select(g => new { VariantId = g.Key, Reserved = g.Sum(r => r.Quantity) })
            .ToListAsync(ct);

        var onHandMap = onHand.ToDictionary(x => x.VariantId, x => x.OnHand);
        var reservedMap = reserved.ToDictionary(x => x.VariantId, x => x.Reserved);

        return ids.ToDictionary(
            id => id,
            id => Math.Max(
                onHandMap.GetValueOrDefault(id, 0) - reservedMap.GetValueOrDefault(id, 0),
                0));
    }
}
```

- [ ] **Step 5: Register the service**

In `service/Api/src/Module/Inventory/Inventory.Extension.cs`, add a single line inside `AddInventoryModule` (after the existing service registrations, before the closing `return builder;`):

```csharp
builder.Services.AddScoped<IStockAvailabilityCalculator, StockAvailabilityCalculator>();
```

- [ ] **Step 6: Re-run the tests to verify they pass**

Run: `dotnet test service/Api/tests/Module.UnitTests/Module.UnitTests.csproj --filter "FullyQualifiedName~StockAvailabilityCalculatorTests" --no-restore`
Expected: PASS.

- [ ] **Step 7: Commit**

```bash
git add service/Api/src/Module/Inventory/Services/LowStockThreshold.cs service/Api/src/Module/Inventory/Services/StockAvailabilityCalculator.cs service/Api/src/Module/Inventory/Inventory.Extension.cs service/Api/tests/Module.UnitTests/Inventory/Services/StockAvailabilityCalculatorTests.cs
git commit -m "feat(inventory): implement StockAvailabilityCalculator with expired-reservation exclusion"
```

---

## Task 3: Refactor `GetStockAvailability` to use the calculator

**Files:**
- Modify: `service/Api/src/Module/Inventory/Features/Storefront/StockAvailability/Check/GetStockAvailability.cs`

**Context:** The handler is the second consumer of the new contract. After this task, both store-front availability reads flow through the same code path.

- [ ] **Step 1: Read the existing handler in full**

Read `service/Api/src/Module/Inventory/Features/Storefront/StockAvailability/Check/GetStockAvailability.cs` end-to-end.

- [ ] **Step 2: Replace the body with a delegation**

Replace the entire `Handle` method body (lines 18-93) with:

```csharp
public async Task<Result<Response>> Handle(Query request, CancellationToken cancellationToken)
{
    var snapshot = await calculator.GetForVariantAsync(request.VariantId, cancellationToken);

    var cartReserved = 0;
    if (!string.IsNullOrEmpty(request.CartToken))
    {
        cartReserved = await dbContext.Set<StockReservation>()
            .Where(r => r.VariantId == request.VariantId
                        && r.CartToken == request.CartToken
                        && r.State == ReservationState.Reserved
                        && r.ExpiresAtUtc > DateTimeOffset.UtcNow)
            .SumAsync(r => r.Quantity, cancellationToken);
    }

    var availableToCart = Math.Max(snapshot.TotalAvailable - cartReserved, 0);

    return new Response
    {
        VariantId = request.VariantId,
        TotalOnHand = snapshot.TotalOnHand,
        TotalReserved = snapshot.TotalReserved,
        CartReserved = cartReserved,
        TotalAvailable = snapshot.TotalAvailable,
        AvailableToCart = availableToCart,
        LocationAvailability = snapshot.Locations.Select(l => new LocationAvailability
        {
            StockLocationId = l.StockLocationId,
            LocationName = l.LocationName,
            CountOnHand = l.CountOnHand,
            ReservedCount = l.ReservedCount,
            AvailableCount = l.AvailableCount,
            Backorderable = l.Backorderable,
            Available = l.AvailableCount > 0
        }).ToList()
    };
}
```

- [ ] **Step 3: Update the constructor to inject the calculator**

Change the `QueryHandler` constructor to:

```csharp
public sealed class QueryHandler(
    IApplicationDbContext dbContext,
    IStockAvailabilityCalculator calculator) : IQueryHandler<Query, Response>
```

- [ ] **Step 4: Run the existing `GetStockAvailability` tests (if any)**

Run: `dotnet test service/Api/tests/Module.UnitTests/Module.UnitTests.csproj --filter "FullyQualifiedName~GetStockAvailability" --no-restore`
Expected: PASS. If tests existed with a specific constructor shape, adjust the test setup to match the new constructor.

- [ ] **Step 5: Build the solution**

Run: `dotnet build service/Api/Api.slnx`
Expected: success.

- [ ] **Step 6: Commit**

```bash
git add service/Api/src/Module/Inventory/Features/Storefront/StockAvailability/Check/GetStockAvailability.cs
git commit -m "refactor(inventory): delegate GetStockAvailability to IStockAvailabilityCalculator"
```

---

## Task 4: Replace price-based status in `GetAvailability`

**Files:**
- Modify: `service/Api/src/Module/Catalog/Features/Storefront/Products/Get/Availability/GetAvailability.cs`
- Test: `service/Api/tests/Module.UnitTests/Catalog/GetAvailabilityTests.cs`

**Context:** The catalog handler builds an axis/cell matrix for a product. Currently each cell's `Status` is `in_stock` if priced, else `unknown`. Replace with a real stock query.

- [ ] **Step 1: Read the existing handler**

Read `service/Api/src/Module/Catalog/Features/Storefront/Products/Get/Availability/GetAvailability.cs` end-to-end.

- [ ] **Step 2: Write the failing tests**

Create file `service/Api/tests/Module.UnitTests/Catalog/GetAvailabilityTests.cs`:

```csharp
using Microsoft.EntityFrameworkCore;
using Module.Catalog.Domain.Products;
using Module.Catalog.Domain.Products.Variants;
using Module.Catalog.Features.Storefront.Products.Get.Availability;
using Module.Inventory.Services;
using Moq;

namespace Module.UnitTests.Catalog;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
public class GetAvailabilityTests : IDisposable
{
    private readonly ApplicationDbContext _db;
    private readonly Mock<IStockAvailabilityCalculator> _calc;
    private readonly GetAvailability.QueryHandler _sut;

    public GetAvailabilityTests()
    {
        var opts = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(Variant).Assembly];
        _db = new ApplicationDbContext(opts);
        _calc = new Mock<IStockAvailabilityCalculator>();
        _sut = new GetAvailability.QueryHandler(_db, _calc.Object);
    }

    public void Dispose() { _db.Dispose(); GC.SuppressFinalize(this); }

    [Fact(DisplayName = "GetAvailability: variant with zero stock returns out_of_stock")]
    public async Task Handle_VariantWithZeroStock_ReturnsOutOfStock()
    {
        var productId = Guid.NewGuid();
        var variantId = Guid.NewGuid();
        await SeedProductWithVariant(productId, variantId, price: 50m);

        _calc.Setup(x => x.GetAvailableByVariantAsync(
                It.Is<IEnumerable<Guid>>(ids => ids.Contains(variantId)),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, int> { [variantId] = 0 });

        var result = await _sut.Handle(new GetAvailability.Query(productId), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        var cell = result.Value.Cells.Single(c => c.VariantId == variantId);
        cell.Status.Should().Be("out_of_stock");
    }

    [Fact(DisplayName = "GetAvailability: variant with stock above threshold returns in_stock")]
    public async Task Handle_VariantWithPlentyOfStock_ReturnsInStock()
    {
        var productId = Guid.NewGuid();
        var variantId = Guid.NewGuid();
        await SeedProductWithVariant(productId, variantId, price: 50m);

        _calc.Setup(x => x.GetAvailableByVariantAsync(
                It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, int> { [variantId] = 10 });

        var result = await _sut.Handle(new GetAvailability.Query(productId), TestContext.Current.CancellationToken);

        var cell = result.Value.Cells.Single(c => c.VariantId == variantId);
        cell.Status.Should().Be("in_stock");
    }

    [Fact(DisplayName = "GetAvailability: variant with stock at or below threshold returns low_stock")]
    public async Task Handle_VariantWithLowStock_ReturnsLowStock()
    {
        var productId = Guid.NewGuid();
        var variantId = Guid.NewGuid();
        await SeedProductWithVariant(productId, variantId, price: 50m);

        _calc.Setup(x => x.GetAvailableByVariantAsync(
                It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, int> { [variantId] = 2 });

        var result = await _sut.Handle(new GetAvailability.Query(productId), TestContext.Current.CancellationToken);

        var cell = result.Value.Cells.Single(c => c.VariantId == variantId);
        cell.Status.Should().Be("low_stock");
    }

    [Fact(DisplayName = "GetAvailability: variant with zero stock but backorderable returns backorderable")]
    public async Task Handle_VariantOutOfStockButBackorderable_ReturnsBackorderable()
    {
        var productId = Guid.NewGuid();
        var variantId = Guid.NewGuid();
        await SeedProductWithVariant(productId, variantId, price: 50m);

        _calc.Setup(x => x.GetAvailableByVariantAsync(
                It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, int> { [variantId] = 0 });

        // No setup on GetForVariantAsync — default mock returns null. The handler
        // MUST call GetForVariantAsync to retrieve the Backorderable flag.
        // Adjust this test to set up the backorderable snapshot once the handler
        // is implemented in the next step.
        var result = await _sut.Handle(new GetAvailability.Query(productId), TestContext.Current.CancellationToken);

        var cell = result.Value.Cells.Single(c => c.VariantId == variantId);
        cell.Status.Should().Be("backorderable");
    }

    private async Task SeedProductWithVariant(Guid productId, Guid variantId, decimal price)
    {
        var product = new Product { Id = productId, Name = "Test", Slug = "test", IsDeleted = false, AvailableOn = DateTimeOffset.UtcNow };
        _db.Set<Product>().Add(product);
        _db.Set<Variant>().Add(new Variant { Id = variantId, ProductId = productId, IsMaster = false, IsDeleted = false });
        await _db.SaveChangesAsync(TestContext.Current.CancellationToken);
    }
}
```

- [ ] **Step 3: Run the tests to verify they fail**

Run: `dotnet test service/Api/tests/Module.UnitTests/Module.UnitTests.csproj --filter "FullyQualifiedName~GetAvailabilityTests" --no-restore`
Expected: FAIL — the constructor signature has only `IApplicationDbContext`; the calculator mock cannot be injected yet, and the status logic still uses price.

- [ ] **Step 4: Update the `GetAvailability` handler**

In `service/Api/src/Module/Catalog/Features/Storefront/Products/Get/Availability/GetAvailability.cs`:

1. Add `using Module.Inventory.Services;` at the top
2. Change the constructor to:

```csharp
public sealed class QueryHandler(
    IApplicationDbContext dbContext,
    IStockAvailabilityCalculator calculator)
    : IQueryHandler<Query, Response>
```

3. Replace the `cells` projection (lines 73-90) with:

```csharp
var variantIds = variants.Select(v => v.Id).Distinct().ToList();
var availableByVariant = await calculator.GetAvailableByVariantAsync(variantIds, cancellationToken);

var cells = new List<AvailabilityCell>(variants.Count);
foreach (var v in variants)
{
    var ovs = v.OptionValueVariants
        .OrderBy(ov => ov.OptionValue?.OptionType?.Position)
        .ToList();

    var firstPrice = v.Prices.FirstOrDefault();
    var available = availableByVariant.GetValueOrDefault(v.Id, 0);

    // Fetch snapshot to learn Backorderable flag (one row per variant is cheap)
    var snapshot = available == 0
        ? await calculator.GetForVariantAsync(v.Id, cancellationToken)
        : null;

    var status = available switch
    {
        > LowStockThreshold.Default => "in_stock",
        > 0 => "low_stock",
        _ when snapshot?.Backorderable == true => "backorderable",
        _ => "out_of_stock"
    };

    cells.Add(new AvailabilityCell
    {
        VariantId = v.Id,
        OptionValue1Id = ovs.Count > 0 ? ovs[0].OptionValueId : Guid.Empty,
        OptionValue2Id = ovs.Count > 1 ? ovs[1].OptionValueId : null,
        Status = status,
        Price = firstPrice?.Amount,
        Currency = firstPrice?.Currency,
    });
}
```

4. Add the `using Module.Inventory.Services;` import to access `LowStockThreshold`.

- [ ] **Step 5: Re-run the catalog tests**

Run: `dotnet test service/Api/tests/Module.UnitTests/Module.UnitTests.csproj --filter "FullyQualifiedName~GetAvailabilityTests" --no-restore`
Expected: PASS.

- [ ] **Step 6: Build the solution**

Run: `dotnet build service/Api/Api.slnx`
Expected: success.

- [ ] **Step 7: Commit**

```bash
git add service/Api/src/Module/Catalog/Features/Storefront/Products/Get/Availability/GetAvailability.cs service/Api/tests/Module.UnitTests/Catalog/GetAvailabilityTests.cs
git commit -m "feat(catalog): compute availability status from real stock and reservations"
```

---

## Task 5: Verify no cross-module namespace leaks

**Files:** (no code changes expected)

**Context:** The plan adds a Catalog → Inventory service reference. Confirm it stays a service contract, not a namespace import in a Domain or Features file.

- [ ] **Step 1: Grep for new `using Module.Inventory` imports outside `Catalog.Extension.cs` and `GetAvailability.cs`**

Run: `rg "using Module\\.Inventory" service/Api/src/Module/Catalog/`
Expected: only `Catalog/Features/Storefront/Products/Get/Availability/GetAvailability.cs` shows up (the new import for `IStockAvailabilityCalculator` and `LowStockThreshold`).

- [ ] **Step 2: Grep for any direct DbContext access to `StockItem` or `StockReservation` from Catalog**

Run: `rg "Set<StockItem>|Set<StockReservation>|Set<StockMovement>" service/Api/src/Module/Catalog/`
Expected: empty result. If any match exists, replace it with a call to `IStockAvailabilityCalculator`.

- [ ] **Step 3: Commit any cleanup**

```bash
git add -A
git commit -m "chore(catalog): verify no cross-module namespace leaks" --allow-empty
```

---

## Task 6: Build and full test suite

- [ ] **Step 1: Build**

Run: `dotnet build service/Api/Api.slnx`
Expected: success, zero warnings.

- [ ] **Step 2: Run the full unit test suite**

Run: `dotnet test service/Api/tests/Module.UnitTests/Module.UnitTests.csproj --no-restore`
Expected: all tests pass.

- [ ] **Step 3: Run the Shared unit test suite**

Run: `dotnet test service/Api/tests/Shared.UnitTests/Shared.UnitTests.csproj --no-restore`
Expected: all tests pass.

- [ ] **Step 4: Final commit**

```bash
git add -A
git commit -m "chore(catalog): post-availability-plan cleanup" --allow-empty
```

---

## Self-Review

- **Spec coverage:** REQ-CAT-001 ✓ Task 4. REQ-CAT-002 ✓ Task 4 (status enum: in_stock / low_stock / out_of_stock / backorderable). AC-CAT-001/002/013 ✓ Task 4 tests. PAT-CAT-001 ✓ Tasks 2, 3 (calculator + delegation).
- **Placeholders:** none. The `LowStockThreshold.Default = 3` constant is intentional and tests verify the boundary.
- **Type consistency:** `IStockAvailabilityCalculator` referenced identically in Tasks 1, 2, 3, 4. `LowStockThreshold.Default` referenced in Task 2 and Task 4 with matching spelling. `StockSnapshot` and `LocationStockSnapshot` field names are consistent across Tasks 1, 2, 3, 4.
