# PagedResult Envelope Standardization Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Convert every list-returning API endpoint to the `PagedResult<T>` envelope via `IPagedQuery<T>`/`IPagedQueryHandler`, without forcing server-side paging (no `page`/`pageSize` params → all items in one page).

**Architecture:** Align the 15 non-paged list endpoints with the existing `PagedOrAll` convention already used by `GetWishlists`, `GetStatePagedOrAll`, `GetAddresses`. Query records implement `IPagedQuery<Response>`; handlers implement `IPagedQueryHandler` and return `PagedResult<Response>`. EF-backed lists use `ToPagedOrAllAsync`; in-memory computed lists use a `PageModel.IsEmpty` branch plus the in-memory `ToPagedResult`. Envelope responses (`{ value: { items: [...] } }`) are flattened so the item type becomes the feature `Response`. Consumers (`app/Admin` option-type and classification APIs) are updated in the same effort.

**Tech Stack:** .NET 10, MediatR, EF Core, Carter minimal APIs, FluentValidation, Vue 3 + TypeScript 6 (Admin SPA), pnpm, Vitest.

## Global Constraints

- `TreatWarningsAsErrors=true` — every build warning fails the build.
- Vertical-slice feature files: `Features/{Admin|Storefront}/{Feature}/{Action}/` with `static partial class`. Subdirectory is always `Storefront` (never `Store`).
- Result objects, not exceptions — handlers return `PagedResult<T>` (a result type), never throw.
- Modules must not cross-reference — all code below stays inside its own module.
- Preserve existing XML doc comments and `// Contract:` lines in files you modify; only signatures, return statements, and type declarations change.
- AC-002 (feature-conventions script): any top-level `public sealed record Response` without a base type needs a `// EXCEPTION:` comment on the same line or the line above.
- AC-001: Query/Command constructor params must match the allowed patterns — `(Id)`, `(Id, Parameters)`, `(Parameters)`, `(Request)`, `(Request, Parameters)`, `(string x)`, or `(Id..., Request/Parameters)`. Never inline domain fields.
- Global usings cover `Shared.Application.Mediators.Queries`, `Shared.Application.Models.Results`, `Shared.Operational.Persistence.Specifications.Paging`, and `Shared.Operational.Persistence.Specifications.Paging.Extensions` — do not add `using Shared.*` lines.

## Conversion Convention (reference for every task)

**Pattern A — EF-backed list (real paging + all-in-one-page):**

```csharp
public sealed record Query(Parameters Parameters) : IPagedQuery<Response>;

public sealed class PagedQueryHandler(IApplicationDbContext dbContext)
    : IPagedQueryHandler<Query, Response>
{
    public async Task<PagedResult<Response>> Handle(Query request, CancellationToken cancellationToken)
    {
        var pageModel = PageModelExtensions.FromValues(request.Parameters.PageNumber, request.Parameters.PageSize).Value;

        return await dbContext.Set<TSource>()
            .Where(x => ...)
            .OrderBy(x => ...)
            .ToPagedOrAllAsync(x => x.MapToItem<Response>(), pageModel, cancellationToken);
    }
}
```

**Pattern B — in-memory computed list (real paging + all-in-one-page):**

```csharp
public sealed record Query(Parameters Parameters) : IPagedQuery<Response>;

public sealed class PagedQueryHandler(...) : IPagedQueryHandler<Query, Response>
{
    public async Task<PagedResult<Response>> Handle(Query request, CancellationToken cancellationToken)
    {
        var pageModel = PageModelExtensions.FromValues(request.Parameters.PageNumber, request.Parameters.PageSize).Value;
        // ... compute List<Response> items (existing logic) ...
        return pageModel.IsEmpty
            ? PagedResult<Response>.Create(items, 1, Math.Max(1, items.Count), items.Count)
            : items.ToPagedResult(pageModel);
    }
}
```

**Pattern C — bounded / mutation result (single page envelope, no paging params):**

```csharp
public sealed record Query(Request Request) : IPagedQuery<Response>;

public sealed class PagedQueryHandler(...) : IPagedQueryHandler<Query, Response>
{
    public async Task<PagedResult<Response>> Handle(Query request, CancellationToken cancellationToken)
    {
        // ... compute List<Response> items (existing logic) ...
        return PagedResult<Response>.Create(items, 1, Math.Max(1, items.Count), items.Count);
    }
}
```

**Endpoint shape (Patterns A and B):**

```csharp
app.MapGet(Route, async (
    [AsParameters] Parameters parameters,
    [FromRoute] Guid id,
    ISender sender,
    CancellationToken ct) =>
{
    var result = await sender.Send(new Query(id, parameters), ct);
    return result.ToPagedResult();
})
... .Produces<PagedResult<Response>>();
```

**Endpoint shape (Pattern C):** keep existing parameter binding; only change `return result.ToResult();` → `return result.ToPagedResult();` and `.Produces<Result<Response>>()` → `.Produces<PagedResult<Response>>()`.

**Parameters file (Patterns A and B):**

```csharp
namespace Module.X.Features...;

public static partial class FeatureName
{
    public sealed record Parameters : QueryingParameters;
}
```

**Semantics:** no `page`/`pageSize` query params → `PageModel.IsEmpty == true` → all items returned with `page=1, pageSize=totalCount`. With params → real paging, bounds clamped to `[1, PageBounds.MaxPageSize]` (default max 100).

**Test conventions:** xUnit `[Fact(DisplayName: ...)]` + FluentAssertions `Should()`. In-memory `ApplicationDbContext` via `UseInMemoryDatabase(Guid.NewGuid().ToString())` and `ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(<DomainType>).Assembly];`. Handler now constructed as `new <Feature>.PagedQueryHandler(...)`; results assert on `result.Items`, `result.TotalCount`, `result.IsSuccess` (no `result.Value`).

Run single-feature tests with:
`dotnet test service/Api/tests/Module.UnitTests --filter "FullyQualifiedName~<FeatureName>"`

---

### Task 1: Inventory `GetAllStockItems` → `IPagedQuery<Response>` (Pattern A)

**Files:**
- Modify: `service/Api/src/Module/Inventory/Features/Admin/StockItems/GetAll/GetAllStockItems.cs`
- Create: `service/Api/src/Module/Inventory/Features/Admin/StockItems/GetAll/GetAllStockItems.Parameters.cs`
- Modify: `service/Api/src/Module/Inventory/Features/Admin/StockItems/GetAll/GetAllStockItems.Endpoint.cs`
- Test: create `service/Api/tests/Module.UnitTests/Inventory/Features/Admin/StockItems/GetAll/GetAllStockItems.Tests.cs`

**Interfaces:**
- Consumes: `IApplicationDbContext`, `StockItem.MapToListItem<T>()`, `PageModelExtensions.FromValues(int?, int?, PageBounds? = null)`, `ToPagedOrAllAsync<TSource,TDestination>(IQueryable<TSource>, Expression<Func<TSource,TDestination>>, PageModel, CancellationToken)`.
- Produces: `GetAllStockItems.Query(Parameters)`, `GetAllStockItems.PagedQueryHandler`, `GetAllStockItems.Parameters`.

- [ ] **Step 1: Write failing tests**

Create `GetAllStockItems.Tests.cs`:

```csharp
using Module.Inventory.Domain.StockLocations;
using Module.Inventory.Domain.StockLocations.StockItems;
using Module.Inventory.Features.Admin.StockItems.GetAll;

namespace Module.UnitTests.Inventory.Features.Admin.StockItems.GetAll;

[Trait("Category", "Unit")]
[Trait("Module", "Inventory")]
[Trait("Feature", "GetAllStockItems")]
public class GetAllStockItemsTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly GetAllStockItems.PagedQueryHandler _handler;

    public GetAllStockItemsTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(StockItem).Assembly];
        _dbContext = new ApplicationDbContext(options);
        _handler = new GetAllStockItems.PagedQueryHandler(_dbContext);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Handle: Returns all items in one page when no paging params")]
    public async Task Handle_ReturnsAll_WhenNoPagingParams()
    {
        var ct = TestContext.Current.CancellationToken;
        for (var i = 0; i < 3; i++)
        {
            _dbContext.Set<StockItem>().Add(new StockItem { VariantId = Guid.NewGuid(), CountOnHand = i + 1 });
        }
        await _dbContext.SaveChangesAsync(ct);

        var result = await _handler.Handle(new GetAllStockItems.Query(new GetAllStockItems.Parameters()), ct);

        result.IsSuccess.Should().BeTrue();
        result.Items.Should().HaveCount(3);
        result.TotalCount.Should().Be(3);
        result.PageNumber.Should().Be(1);
        result.PageSize.Should().Be(3);
    }

    [Fact(DisplayName = "Handle: Pages when page/pageSize supplied")]
    public async Task Handle_Pages_WhenParamsSupplied()
    {
        var ct = TestContext.Current.CancellationToken;
        for (var i = 0; i < 5; i++)
        {
            _dbContext.Set<StockItem>().Add(new StockItem { VariantId = Guid.NewGuid(), CountOnHand = i + 1 });
        }
        await _dbContext.SaveChangesAsync(ct);

        var result = await _handler.Handle(
            new GetAllStockItems.Query(new GetAllStockItems.Parameters { PageNumber = 2, PageSize = 2 }), ct);

        result.IsSuccess.Should().BeTrue();
        result.Items.Should().HaveCount(2);
        result.PageNumber.Should().Be(2);
        result.TotalCount.Should().Be(5);
    }
}
```

- [ ] **Step 2: Run tests to verify they fail**

Run: `dotnet test service/Api/tests/Module.UnitTests --filter "FullyQualifiedName~GetAllStockItems"`
Expected: FAIL — `PagedQueryHandler` and `Parameters` do not exist; `Query` constructor mismatch.

- [ ] **Step 3: Implement the conversion**

Rewrite `GetAllStockItems.cs` handler:

```csharp
using Module.Inventory.Domain.StockLocations.StockItems;
using Module.Inventory.Features.Admin.StockItems.Shared.Mappings;

namespace Module.Inventory.Features.Admin.StockItems.GetAll;

public static partial class GetAllStockItems
{
    public sealed record Query(Parameters Parameters) : IPagedQuery<Response>;

    /// <summary>Handler for getting all stock items.</summary>
    public sealed class PagedQueryHandler(IApplicationDbContext dbContext)
        : IPagedQueryHandler<Query, Response>
    {
        /// <summary>Gets all stock items, paged or all in a single page.</summary>
        // Contract: pre=request!=null, post=result!=null
        public async Task<PagedResult<Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            var pageModel = PageModelExtensions.FromValues(request.Parameters.PageNumber, request.Parameters.PageSize).Value;

            // Load: Fetch stock items without tracking, ordered for stable paging
            return await dbContext.Set<StockItem>()
                .OrderBy(x => x.Id)
                .ToPagedOrAllAsync(x => x.MapToListItem<Response>(), pageModel, cancellationToken);
        }
    }
}
```

Create `GetAllStockItems.Parameters.cs`:

```csharp
namespace Module.Inventory.Features.Admin.StockItems.GetAll;

public static partial class GetAllStockItems
{
    public sealed record Parameters : QueryingParameters;
}
```

Rewrite `GetAllStockItems.Endpoint.cs`:

```csharp
using Module.Inventory.Features.Shared;

namespace Module.Inventory.Features.Admin.StockItems.GetAll;

public static partial class GetAllStockItems
{
    /// <summary>Gets all stock items.</summary>
    public class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            // Map: GET /admin/inventory/stock-items — gets all stock items (optionally paged)
            app.MapGet(InventoryFeature.Admin.StockItems.GetAll.Route, async (
                [AsParameters] Parameters parameters,
                ISender sender,
                CancellationToken ct) =>
            {
                var result = await sender.Send(new Query(parameters), ct);
                return result.ToPagedResult();
            })
            .WithName(nameof(GetAllStockItems))
            .WithTags(InventoryFeature.Tags.StockItem)
            .HasPermission(InventoryFeature.Admin.StockItems.GetAll.Permission)
            .WithSummary(InventoryFeature.Admin.StockItems.GetAll.Summary)
            .WithDescription(InventoryFeature.Admin.StockItems.GetAll.Description)
            .Produces<PagedResult<Response>>();
        }
    }
}
```

- [ ] **Step 4: Run tests to verify they pass**

Run: `dotnet test service/Api/tests/Module.UnitTests --filter "FullyQualifiedName~GetAllStockItems"`
Expected: PASS (2 tests).

- [ ] **Step 5: Build**

Run: `dotnet build service/Api/src/Api/Api.csproj`
Expected: Build succeeded, 0 warnings.

- [ ] **Step 6: Commit**

```bash
git add service/Api/src/Module/Inventory/Features/Admin/StockItems/GetAll
git add service/Api/tests/Module.UnitTests/Inventory/Features/Admin/StockItems/GetAll
git commit -m "refactor(inventory): return PagedResult from GetAllStockItems"
```

---

### Task 2: Inventory `GetStockSummary` → `IPagedQuery<Response>` (Pattern B)

**Files:**
- Modify: `service/Api/src/Module/Inventory/Features/Admin/StockItems/Summary/GetStockSummary.cs`
- Create: `service/Api/src/Module/Inventory/Features/Admin/StockItems/Summary/GetStockSummary.Parameters.cs`
- Modify: `service/Api/src/Module/Inventory/Features/Admin/StockItems/Summary/GetStockSummary.Endpoint.cs`
- Test: modify `service/Api/tests/Module.UnitTests/Inventory/Features/Admin/StockItems/Summary/GetStockSummary.Tests.cs`

**Interfaces:**
- Consumes: existing handler body (stock items + reservations aggregation).
- Produces: `GetStockSummary.Query(Parameters)`, `GetStockSummary.PagedQueryHandler`, `GetStockSummary.Parameters`.

- [ ] **Step 1: Update tests to the new handler/result shape**

In `GetStockSummary.Tests.cs`:
- Line 14: `private readonly GetStockSummary.QueryHandler _handler;` → `private readonly GetStockSummary.PagedQueryHandler _handler;`
- Line 26: `new GetStockSummary.QueryHandler(_dbContext)` → `new GetStockSummary.PagedQueryHandler(_dbContext)`
- Line 55: `new GetStockSummary.Query()` → `new GetStockSummary.Query(new GetStockSummary.Parameters())`
- Assertions: `result.Value.Should().HaveCount(1)` → `result.Items.Should().HaveCount(1)`; `var summary = result.Value[0];` → `var summary = result.Items[0];`; `result.Value[0].LocationBreakdown[0].IsLowStock` → `result.Items[0].LocationBreakdown[0].IsLowStock`; `result.Value.Should().BeEmpty()` → `result.Items.Should().BeEmpty()`.
- Add a paging test after the empty test:

```csharp
[Fact(DisplayName = "Handle: Pages per-variant summaries when params supplied")]
public async Task Handle_Pages_WhenParamsSupplied()
{
    var ct = TestContext.Current.CancellationToken;
    var loc = Guid.NewGuid();
    _dbContext.Set<StockLocation>().Add(new StockLocation { Id = loc, Name = "Loc", Active = true });
    await _dbContext.SaveChangesAsync(ct);
    for (var i = 0; i < 3; i++)
    {
        _dbContext.Set<StockItem>().Add(new StockItem { VariantId = Guid.NewGuid(), StockLocationId = loc, CountOnHand = 1 });
    }
    await _dbContext.SaveChangesAsync(ct);

    var result = await _handler.Handle(
        new GetStockSummary.Query(new GetStockSummary.Parameters { PageNumber = 1, PageSize = 2 }), ct);

    result.IsSuccess.Should().BeTrue();
    result.Items.Should().HaveCount(2);
    result.TotalCount.Should().Be(3);
}
```

- [ ] **Step 2: Run tests to verify they fail**

Run: `dotnet test service/Api/tests/Module.UnitTests --filter "FullyQualifiedName~GetStockSummary"`
Expected: FAIL — compile errors (missing `PagedQueryHandler`, `Parameters`, `Query(Parameters)`).

- [ ] **Step 3: Implement the conversion**

Modify `GetStockSummary.cs`:
- Change `public sealed record Query : IQuery<List<Response>>;` → `public sealed record Query(Parameters Parameters) : IPagedQuery<Response>;`
- Rename class `QueryHandler` → `PagedQueryHandler`, `IQueryHandler<Query, List<Response>>` → `IPagedQueryHandler<Query, Response>`, return type `Task<Result<List<Response>>>` → `Task<PagedResult<Response>>`.
- Keep the entire body through `var grouped = stockItems.GroupBy(...)...ToList();` unchanged. Replace the final `return grouped.ToList();` with:

```csharp
            var pageModel = PageModelExtensions.FromValues(request.Parameters.PageNumber, request.Parameters.PageSize).Value;
            var results = grouped.OrderBy(s => s.VariantId).ToList();

            // Transform: Return all in one page or honor caller-supplied paging
            return pageModel.IsEmpty
                ? PagedResult<Response>.Create(results, 1, Math.Max(1, results.Count), results.Count)
                : results.ToPagedResult(pageModel);
```

Create `GetStockSummary.Parameters.cs`:

```csharp
namespace Module.Inventory.Features.Admin.StockItems.Summary;

public static partial class GetStockSummary
{
    public sealed record Parameters : QueryingParameters;
}
```

Modify `GetStockSummary.Endpoint.cs`:
- Add `[AsParameters] Parameters parameters,` as first handler param; `new Query()` → `new Query(parameters)`; `return result.ToResult();` → `return result.ToPagedResult();`; `.Produces<Result<List<Response>>>()` → `.Produces<PagedResult<Response>>()`.

- [ ] **Step 4: Run tests to verify they pass**

Run: `dotnet test service/Api/tests/Module.UnitTests --filter "FullyQualifiedName~GetStockSummary"`
Expected: PASS (4 tests).

- [ ] **Step 5: Build**

Run: `dotnet build service/Api/src/Api/Api.csproj`
Expected: Build succeeded, 0 warnings.

- [ ] **Step 6: Commit**

```bash
git add service/Api/src/Module/Inventory/Features/Admin/StockItems/Summary
git add service/Api/tests/Module.UnitTests/Inventory/Features/Admin/StockItems/Summary
git commit -m "refactor(inventory): return PagedResult from GetStockSummary"
```

---

### Task 3: Inventory `GetLowStockItems` → `IPagedQuery<Response>` (Pattern B)

**Files:**
- Modify: `service/Api/src/Module/Inventory/Features/Admin/StockItems/LowStock/GetLowStockItems.cs`
- Create: `service/Api/src/Module/Inventory/Features/Admin/StockItems/LowStock/GetLowStockItems.Parameters.cs`
- Modify: `service/Api/src/Module/Inventory/Features/Admin/StockItems/LowStock/GetLowStockItems.Endpoint.cs`
- Test: create `service/Api/tests/Module.UnitTests/Inventory/Features/Admin/StockItems/LowStock/GetLowStockItems.Tests.cs`

**Interfaces:**
- Consumes: existing handler body (stock items load + in-memory threshold filter).
- Produces: `GetLowStockItems.Query(Request, Parameters)`, `GetLowStockItems.PagedQueryHandler`, `GetLowStockItems.Parameters`.

- [ ] **Step 1: Write failing tests**

Create `GetLowStockItems.Tests.cs`:

```csharp
using Module.Inventory.Domain.StockLocations;
using Module.Inventory.Domain.StockLocations.StockItems;
using Module.Inventory.Features.Admin.StockItems.LowStock;

namespace Module.UnitTests.Inventory.Features.Admin.StockItems.LowStock;

[Trait("Category", "Unit")]
[Trait("Module", "Inventory")]
[Trait("Feature", "GetLowStockItems")]
public class GetLowStockItemsTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly GetLowStockItems.PagedQueryHandler _handler;

    public GetLowStockItemsTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(StockItem).Assembly];
        _dbContext = new ApplicationDbContext(options);
        _handler = new GetLowStockItems.PagedQueryHandler(_dbContext);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Handle: Returns only items at or below threshold")]
    public async Task Handle_ReturnsLowStockItems()
    {
        var ct = TestContext.Current.CancellationToken;
        var loc = Guid.NewGuid();
        _dbContext.Set<StockLocation>().Add(new StockLocation { Id = loc, Name = "Loc", Active = true, LowStockThreshold = 5 });
        await _dbContext.SaveChangesAsync(ct);
        _dbContext.Set<StockItem>().Add(new StockItem { VariantId = Guid.NewGuid(), StockLocationId = loc, CountOnHand = 3 });
        _dbContext.Set<StockItem>().Add(new StockItem { VariantId = Guid.NewGuid(), StockLocationId = loc, CountOnHand = 10 });
        await _dbContext.SaveChangesAsync(ct);

        var result = await _handler.Handle(
            new GetLowStockItems.Query(new GetLowStockItems.Request(), new GetLowStockItems.Parameters()), ct);

        result.IsSuccess.Should().BeTrue();
        result.Items.Should().ContainSingle();
        result.Items[0].Status.Should().Be("low");
    }

    [Fact(DisplayName = "Handle: Pages results when params supplied")]
    public async Task Handle_Pages_WhenParamsSupplied()
    {
        var ct = TestContext.Current.CancellationToken;
        var loc = Guid.NewGuid();
        _dbContext.Set<StockLocation>().Add(new StockLocation { Id = loc, Name = "Loc", Active = true, LowStockThreshold = 5 });
        await _dbContext.SaveChangesAsync(ct);
        for (var i = 0; i < 4; i++)
        {
            _dbContext.Set<StockItem>().Add(new StockItem { VariantId = Guid.NewGuid(), StockLocationId = loc, CountOnHand = 1 });
        }
        await _dbContext.SaveChangesAsync(ct);

        var result = await _handler.Handle(
            new GetLowStockItems.Query(new GetLowStockItems.Request(), new GetLowStockItems.Parameters { PageSize = 2 }), ct);

        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(4);
    }
}
```

- [ ] **Step 2: Run tests to verify they fail**

Run: `dotnet test service/Api/tests/Module.UnitTests --filter "FullyQualifiedName~GetLowStockItems"`
Expected: FAIL — compile errors.

- [ ] **Step 3: Implement the conversion**

Modify `GetLowStockItems.cs`:
- `public sealed record Query(Request Request) : IQuery<List<Response>>;` → `public sealed record Query(Request Request, Parameters Parameters) : IPagedQuery<Response>;`
- Rename `QueryHandler` → `PagedQueryHandler`, `IQueryHandler<Query, List<Response>>` → `IPagedQueryHandler<Query, Response>`, return type → `Task<PagedResult<Response>>`.
- Keep the body through the `results` computation unchanged; replace `return results;` with:

```csharp
            var pageModel = PageModelExtensions.FromValues(request.Parameters.PageNumber, request.Parameters.PageSize).Value;
            var ordered = results.OrderBy(r => r.Id).ToList();

            // Transform: Return all in one page or honor caller-supplied paging
            return pageModel.IsEmpty
                ? PagedResult<Response>.Create(ordered, 1, Math.Max(1, ordered.Count), ordered.Count)
                : ordered.ToPagedResult(pageModel);
```

Create `GetLowStockItems.Parameters.cs`:

```csharp
namespace Module.Inventory.Features.Admin.StockItems.LowStock;

public static partial class GetLowStockItems
{
    public sealed record Parameters : QueryingParameters;
}
```

Modify `GetLowStockItems.Endpoint.cs`:
- Add `[AsParameters] Parameters parameters,` as first handler param; `new Query(new Request { LocationId = locationId, Threshold = threshold })` → `new Query(new Request { LocationId = locationId, Threshold = threshold }, parameters)`; `return result.ToResult();` → `return result.ToPagedResult();`; `.Produces<Result<List<Response>>>()` → `.Produces<PagedResult<Response>>()`.

- [ ] **Step 4: Run tests to verify they pass**

Run: `dotnet test service/Api/tests/Module.UnitTests --filter "FullyQualifiedName~GetLowStockItems"`
Expected: PASS (2 tests).

- [ ] **Step 5: Build**

Run: `dotnet build service/Api/src/Api/Api.csproj`
Expected: Build succeeded, 0 warnings.

- [ ] **Step 6: Commit**

```bash
git add service/Api/src/Module/Inventory/Features/Admin/StockItems/LowStock
git add service/Api/tests/Module.UnitTests/Inventory/Features/Admin/StockItems/LowStock
git commit -m "refactor(inventory): return PagedResult from GetLowStockItems"
```

---

### Task 4: Inventory Storefront `GetCartReservations` → `IPagedQuery<Response>` (Pattern B)

**Files:**
- Modify: `service/Api/src/Module/Inventory/Features/Storefront/CartReservations/Status/GetCartReservations.cs`
- Create: `service/Api/src/Module/Inventory/Features/Storefront/CartReservations/Status/GetCartReservations.Parameters.cs`
- Modify: `service/Api/src/Module/Inventory/Features/Storefront/CartReservations/Status/GetCartReservations.Endpoint.cs`
- Test: modify `service/Api/tests/Module.UnitTests/Inventory/Features/Storefront/CartReservations/Status/GetCartReservations.Tests.cs`

**Interfaces:**
- Consumes: existing handler body.
- Produces: `GetCartReservations.Query(string CartToken, Parameters)`, `PagedQueryHandler`, `Parameters`.

- [ ] **Step 1: Update tests to the new handler/result shape**

In `GetCartReservations.Tests.cs`:
- `QueryHandler` → `PagedQueryHandler` in field + constructor.
- Every `new GetCartReservations.Query("<token>")` → `new GetCartReservations.Query("<token>", new GetCartReservations.Parameters())`.
- `result.Value` → `result.Items` in assertions; `result.IsSuccess.Should().BeTrue()` stays.
- Add a paging test asserting `Items` slice + `TotalCount` when `Parameters { PageSize = 1 }` is passed with 2 seeded reservations.

- [ ] **Step 2: Run tests to verify they fail**

Run: `dotnet test service/Api/tests/Module.UnitTests --filter "FullyQualifiedName~GetCartReservations"`
Expected: FAIL — compile errors.

- [ ] **Step 3: Implement the conversion**

Modify `GetCartReservations.cs`:
- `public sealed record Query(string CartToken) : IQuery<List<Response>>;` → `public sealed record Query(string CartToken, Parameters Parameters) : IPagedQuery<Response>;`
- Rename handler/interface/return type per Pattern B.
- Keep the reservations-load and `Select(...)` mapping; assign to `var items = reservations.Select(r => new Response {...}).ToList();` then replace the `return items...` with:

```csharp
            var pageModel = PageModelExtensions.FromValues(request.Parameters.PageNumber, request.Parameters.PageSize).Value;
            var ordered = items.OrderBy(i => i.ExpiresAtUtc).ToList();

            // Transform: Return all in one page or honor caller-supplied paging
            return pageModel.IsEmpty
                ? PagedResult<Response>.Create(ordered, 1, Math.Max(1, ordered.Count), ordered.Count)
                : ordered.ToPagedResult(pageModel);
```

Create `GetCartReservations.Parameters.cs`:

```csharp
namespace Module.Inventory.Features.Storefront.CartReservations.Status;

public static partial class GetCartReservations
{
    public sealed record Parameters : QueryingParameters;
}
```

Modify `GetCartReservations.Endpoint.cs`:
- Add `[AsParameters] Parameters parameters,` as first handler param; `new Query(cartToken)` → `new Query(cartToken, parameters)`; `ToResult()` → `ToPagedResult()`; `.Produces<Result<List<Response>>>()` → `.Produces<PagedResult<Response>>()`.

- [ ] **Step 4: Run tests to verify they pass**

Run: `dotnet test service/Api/tests/Module.UnitTests --filter "FullyQualifiedName~GetCartReservations"`
Expected: PASS.

- [ ] **Step 5: Build**

Run: `dotnet build service/Api/src/Api/Api.csproj`
Expected: Build succeeded, 0 warnings.

- [ ] **Step 6: Commit**

```bash
git add service/Api/src/Module/Inventory/Features/Storefront/CartReservations/Status
git add service/Api/tests/Module.UnitTests/Inventory/Features/Storefront/CartReservations/Status
git commit -m "refactor(inventory): return PagedResult from GetCartReservations"
```

---

### Task 5: Inventory Storefront `GetStockAvailability` → `IPagedQuery<LocationAvailability>` (Pattern C, envelope flattened)

> **Behavior change (approved):** the response drops the aggregate fields `VariantId`, `TotalOnHand`, `TotalReserved`, `CartReserved`, `TotalAvailable`, `AvailableToCart`. The per-location `LocationAvailability` list becomes the paged `Items`.

**Files:**
- Modify: `service/Api/src/Module/Inventory/Features/Storefront/StockAvailability/Check/GetStockAvailability.cs`
- Modify: `service/Api/src/Module/Inventory/Features/Storefront/StockAvailability/Check/GetStockAvailability.Response.cs`
- Modify: `service/Api/src/Module/Inventory/Features/Storefront/StockAvailability/Check/GetStockAvailability.Endpoint.cs`
- Test: modify `service/Api/tests/Module.UnitTests/Inventory/Features/Storefront/StockAvailability/Check/GetStockAvailability.Tests.cs`

**Interfaces:**
- Consumes: `IStockAvailabilityCalculator.GetForVariantAsync(Guid, CancellationToken)` returning snapshot with `.Locations` (each with `StockLocationId`, `LocationName`, `CountOnHand`, `ReservedCount`, `AvailableCount`, `Backorderable`).
- Produces: `GetStockAvailability.Query(Request)` unchanged shape, `PagedQueryHandler`, top-level `GetStockAvailability.Response` (the former `LocationAvailability`).

- [ ] **Step 1: Update tests to the new result shape**

In `GetStockAvailability.Tests.cs`:
- `QueryHandler` → `PagedQueryHandler` in field + constructor.
- Assertions: `result.Value.LocationAvailability` → `result.Items`; aggregate-field assertions (`result.Value.TotalAvailable` etc.) are deleted. Verify the `LocationAvailability` row fields instead. Example replacement:

```csharp
var result = await _handler.Handle(new GetStockAvailability.Query(new GetStockAvailability.Request { VariantId = _variantId }), ct);

result.IsSuccess.Should().BeTrue();
result.Items.Should().NotBeEmpty();
result.Items[0].StockLocationId.Should().Be(locA);
result.Items[0].Available.Should().BeTrue();
```

- [ ] **Step 2: Run tests to verify they fail**

Run: `dotnet test service/Api/tests/Module.UnitTests --filter "FullyQualifiedName~GetStockAvailability"`
Expected: FAIL — compile errors.

- [ ] **Step 3: Implement the conversion**

Rewrite `GetStockAvailability.Response.cs`:

```csharp
namespace Module.Inventory.Features.Storefront.StockAvailability.Check;

public static partial class GetStockAvailability
{
    // EXCEPTION: computed availability DTO — composite of stock item + reservation data
    public sealed record Response
    {
        public Guid StockLocationId { get; init; }
        public string LocationName { get; init; } = string.Empty;
        public int CountOnHand { get; init; }
        public int ReservedCount { get; init; }
        public int AvailableCount { get; init; }
        public bool Backorderable { get; init; }
        public bool Available { get; init; }
    }
}
```

Modify `GetStockAvailability.cs`:
- `public sealed record Query(Request Request) : IQuery<Response>;` → `public sealed record Query(Request Request) : IPagedQuery<Response>;`
- Rename `QueryHandler` → `PagedQueryHandler`, interface → `IPagedQueryHandler<Query, Response>`, return type → `Task<PagedResult<Response>>`.
- Replace the body from `var availableToCart = ...` onward: keep the snapshot load and `cartReserved` computation, drop the aggregate `Response { VariantId = ..., ... }` construction, and return the location rows:

```csharp
            var items = snapshot.Locations.Select(l => new Response
            {
                StockLocationId = l.StockLocationId,
                LocationName = l.LocationName,
                CountOnHand = l.CountOnHand,
                ReservedCount = l.ReservedCount,
                AvailableCount = l.AvailableCount,
                Backorderable = l.Backorderable,
                Available = l.AvailableCount > 0
            }).ToList();

            return PagedResult<Response>.Create(items, 1, Math.Max(1, items.Count), items.Count);
```

Modify `GetStockAvailability.Endpoint.cs`: `ToResult()` → `ToPagedResult()`; `.Produces<Result<Response>>()` → `.Produces<PagedResult<Response>>()`.

- [ ] **Step 4: Run tests to verify they pass**

Run: `dotnet test service/Api/tests/Module.UnitTests --filter "FullyQualifiedName~GetStockAvailability"`
Expected: PASS.

- [ ] **Step 5: Build**

Run: `dotnet build service/Api/src/Api/Api.csproj`
Expected: Build succeeded, 0 warnings.

- [ ] **Step 6: Commit**

```bash
git add service/Api/src/Module/Inventory/Features/Storefront/StockAvailability/Check
git add service/Api/tests/Module.UnitTests/Inventory/Features/Storefront/StockAvailability/Check
git commit -m "refactor(inventory): return PagedResult<LocationAvailability> from GetStockAvailability"
```

---

### Task 6: Catalog Admin `ListVariantImages` → `IPagedQuery<VariantImageDetailResponse>` (Pattern A, envelope flattened)

**Files:**
- Modify: `service/Api/src/Module/Catalog/Features/Admin/Products/Variants/Images/ListByVariant/ListVariantImages.cs`
- Create: `service/Api/src/Module/Catalog/Features/Admin/Products/Variants/Images/ListByVariant/ListVariantImages.Parameters.cs`
- Delete: `service/Api/src/Module/Catalog/Features/Admin/Products/Variants/Images/ListByVariant/ListVariantImages.Response.cs`
- Modify: `service/Api/src/Module/Catalog/Features/Admin/Products/Variants/Images/ListByVariant/ListVariantImages.Endpoint.cs`
- Test: modify `service/Api/tests/Module.UnitTests/Catalog/Features/Admin/Products/Variants/Images/ListByVariant/ListVariantImages.Tests.cs`

**Interfaces:**
- Consumes: `VariantImage.MapToDetail<T>()`.
- Produces: `ListVariantImages.Query(Guid VariantId, Parameters)` with response type `VariantImageDetailResponse` (from `Module.Catalog.Features.Admin.Products.Variants.Images.Shared.Models`), `PagedQueryHandler`.

- [ ] **Step 1: Update tests to the new handler/result shape**

In `ListVariantImages.Tests.cs`:
- `QueryHandler` → `PagedQueryHandler`; every `new ListVariantImages.Query(<id>)` → `new ListVariantImages.Query(<id>, new ListVariantImages.Parameters())`.
- `result.Value.Images` → `result.Items` in assertions.
- Add paging test: seed 3 images, query with `Parameters { PageSize = 2 }`, assert `Items` count 2 and `TotalCount` 3.

- [ ] **Step 2: Run tests to verify they fail**

Run: `dotnet test service/Api/tests/Module.UnitTests --filter "FullyQualifiedName~ListVariantImages"`
Expected: FAIL — compile errors.

- [ ] **Step 3: Implement the conversion**

Rewrite `ListVariantImages.cs` handler:

```csharp
using Module.Catalog.Domain.Products.Variants.Images;
using Module.Catalog.Features.Admin.Products.Variants.Images.Shared.Mappings;
using Module.Catalog.Features.Admin.Products.Variants.Images.Shared.Models;

namespace Module.Catalog.Features.Admin.Products.Variants.Images.ListByVariant;

/// <summary>
/// Defines the use case for listing images by variant.
/// </summary>
public static partial class ListVariantImages
{
    public sealed record Query(Guid VariantId, Parameters Parameters) : IPagedQuery<VariantImageDetailResponse>;

    /// <summary>
    /// Handles listing all images for a given variant, ordered by display position.
    /// </summary>
    public sealed class PagedQueryHandler(IApplicationDbContext dbContext)
        : IPagedQueryHandler<Query, VariantImageDetailResponse>
    {
        /// <summary>
        /// Executes the query: loads images for the variant ordered by position, paged or all in one page.
        /// </summary>
        // Contract: pre=query!=null, post=result!=null
        public async Task<PagedResult<VariantImageDetailResponse>> Handle(Query query, CancellationToken cancellationToken)
        {
            var pageModel = PageModelExtensions.FromValues(query.Parameters.PageNumber, query.Parameters.PageSize).Value;

            // Filter: Load images scoped to the variant, ordered by display position
            return await dbContext.Set<VariantImage>()
                .Where(x => x.VariantId == query.VariantId)
                .OrderBy(x => x.Position)
                .ToPagedOrAllAsync(x => x.MapToDetail<VariantImageDetailResponse>(), pageModel, cancellationToken);
        }
    }
}
```

Delete `ListVariantImages.Response.cs` (envelope no longer needed; the item type is the shared `VariantImageDetailResponse`).

Create `ListVariantImages.Parameters.cs`:

```csharp
namespace Module.Catalog.Features.Admin.Products.Variants.Images.ListByVariant;

public static partial class ListVariantImages
{
    public sealed record Parameters : QueryingParameters;
}
```

Modify `ListVariantImages.Endpoint.cs`:
- Add `[AsParameters] Parameters parameters,` as first handler param; `new Query(variantId)` → `new Query(variantId, parameters)`; `ToResult()` → `ToPagedResult()`; `.Produces<Result<Response>>()` → `.Produces<PagedResult<VariantImageDetailResponse>>()`. Add `using Module.Catalog.Features.Admin.Products.Variants.Images.Shared.Models;`.

- [ ] **Step 4: Run tests to verify they pass**

Run: `dotnet test service/Api/tests/Module.UnitTests --filter "FullyQualifiedName~ListVariantImages"`
Expected: PASS.

- [ ] **Step 5: Build**

Run: `dotnet build service/Api/src/Api/Api.csproj`
Expected: Build succeeded, 0 warnings.

- [ ] **Step 6: Commit**

```bash
git add service/Api/src/Module/Catalog/Features/Admin/Products/Variants/Images/ListByVariant
git add service/Api/tests/Module.UnitTests/Catalog/Features/Admin/Products/Variants/Images/ListByVariant
git commit -m "refactor(catalog): return PagedResult from ListVariantImages"
```

---

### Task 7: Catalog Admin `ListVariantsByProduct` → `IPagedQuery<Response>` (Pattern A, envelope flattened)

**Files:**
- Modify: `service/Api/src/Module/Catalog/Features/Admin/Products/Variants/List/ListVariantsByProduct.cs`
- Create: `service/Api/src/Module/Catalog/Features/Admin/Products/Variants/List/ListVariantsByProduct.Parameters.cs`
- Modify: `service/Api/src/Module/Catalog/Features/Admin/Products/Variants/List/ListVariantsByProduct.Response.cs`
- Modify: `service/Api/src/Module/Catalog/Features/Admin/Products/Variants/List/ListVariantsByProduct.Endpoint.cs`
- Test: modify `service/Api/tests/Module.UnitTests/Catalog/Features/Admin/Products/Variants/List/ListVariantsByProduct.Tests.cs`

**Interfaces:**
- Consumes: `Variant.MapToDetail<T>()` with eager includes.
- Produces: `ListVariantsByProduct.Query(Guid ProductId, Parameters)`, `PagedQueryHandler`, top-level `Response : VariantDetailResponse`.

- [ ] **Step 1: Update tests to the new handler/result shape**

In `ListVariantsByProduct.Tests.cs`:
- `QueryHandler` → `PagedQueryHandler`; `new ListVariantsByProduct.Query(<productId>)` → `new ListVariantsByProduct.Query(<productId>, new ListVariantsByProduct.Parameters())`.
- `result.Value.Items` → `result.Items`.
- Add paging test: seed 3 variants, query with `Parameters { PageSize = 2 }`, assert `Items` count 2, `TotalCount` 3.

- [ ] **Step 2: Run tests to verify they fail**

Run: `dotnet test service/Api/tests/Module.UnitTests --filter "FullyQualifiedName~ListVariantsByProduct"`
Expected: FAIL — compile errors.

- [ ] **Step 3: Implement the conversion**

Rewrite `ListVariantsByProduct.Response.cs` (promote the item):

```csharp
using Module.Catalog.Features.Admin.Products.Variants.Shared.Models;

namespace Module.Catalog.Features.Admin.Products.Variants.List;

public static partial class ListVariantsByProduct
{
    public sealed record Response : VariantDetailResponse;
}
```

Rewrite `ListVariantsByProduct.cs` handler:

```csharp
using Module.Catalog.Domain.Products.Variants;
using Module.Catalog.Features.Admin.Products.Variants.Shared.Mappings;

namespace Module.Catalog.Features.Admin.Products.Variants.List;

/// <summary>
/// Defines the use case for listing variants by product.
/// </summary>
public static partial class ListVariantsByProduct
{
    public sealed record Query(Guid ProductId, Parameters Parameters) : IPagedQuery<Response>;

    /// <summary>
    /// Lists non-deleted variants for a product, including prices, option-value
    /// associations, and images, paged or all in one page.
    /// </summary>
    public sealed class PagedQueryHandler(IApplicationDbContext dbContext)
        : IPagedQueryHandler<Query, Response>
    {
        // Contract: pre=query.ProductId!=Guid.Empty, post=result!=null
        public async Task<PagedResult<Response>> Handle(Query query, CancellationToken cancellationToken)
        {
            var pageModel = PageModelExtensions.FromValues(query.Parameters.PageNumber, query.Parameters.PageSize).Value;

            // Load: Fetch non-deleted variants for product with relations, ordered by position
            return await dbContext.Set<Variant>()
                .Include(x => x.Prices)
                .Include(x => x.OptionValueVariants)
                    .ThenInclude(ovv => ovv.OptionValue)
                .Include(x => x.VariantImages)
                .Where(x => x.ProductId == query.ProductId && !x.IsDeleted)
                .OrderBy(x => x.Position)
                .ToPagedOrAllAsync(x => x.MapToDetail<Response>(), pageModel, cancellationToken);
        }
    }
}
```

> Note: `ToPagedOrAllAsync` applies `.AsNoTracking()` internally, replacing the old `.AsNoTracking()`.

Create `ListVariantsByProduct.Parameters.cs`:

```csharp
namespace Module.Catalog.Features.Admin.Products.Variants.List;

public static partial class ListVariantsByProduct
{
    public sealed record Parameters : QueryingParameters;
}
```

Modify `ListVariantsByProduct.Endpoint.cs`:
- Add `[AsParameters] Parameters parameters,`; `new Query(productId)` → `new Query(productId, parameters)`; `ToResult()` → `ToPagedResult()`; `.Produces<Result<Response>>()` → `.Produces<PagedResult<Response>>()`.

- [ ] **Step 4: Run tests to verify they pass**

Run: `dotnet test service/Api/tests/Module.UnitTests --filter "FullyQualifiedName~ListVariantsByProduct"`
Expected: PASS.

- [ ] **Step 5: Build**

Run: `dotnet build service/Api/src/Api/Api.csproj`
Expected: Build succeeded, 0 warnings.

- [ ] **Step 6: Commit**

```bash
git add service/Api/src/Module/Catalog/Features/Admin/Products/Variants/List
git add service/Api/tests/Module.UnitTests/Catalog/Features/Admin/Products/Variants/List
git commit -m "refactor(catalog): return PagedResult from ListVariantsByProduct"
```

---

### Task 8: Catalog Admin `GetProductOptionTypes` → `IPagedQuery<Response>` (Pattern B, envelope flattened)

**Files:**
- Modify: `service/Api/src/Module/Catalog/Features/Admin/Products/OptionTypes/Get/GetProductOptionTypes.cs`
- Create: `service/Api/src/Module/Catalog/Features/Admin/Products/OptionTypes/Get/GetProductOptionTypes.Parameters.cs`
- Modify: `service/Api/src/Module/Catalog/Features/Admin/Products/OptionTypes/Get/GetProductOptionTypes.Response.cs`
- Modify: `service/Api/src/Module/Catalog/Features/Admin/Products/OptionTypes/Get/GetProductOptionTypes.Endpoint.cs`
- Test: modify `service/Api/tests/Module.UnitTests/Catalog/Features/Admin/Products/OptionTypes/Get/GetProductOptionTypesUseCase.Tests.cs`

**Interfaces:**
- Consumes: `OptionType.MapToListItem<T>(bool, int)`.
- Produces: `GetProductOptionTypes.Query(Guid Id, Parameters)`, `PagedQueryHandler`, top-level `Response : ProductOptionTypeItemResponse`.

- [ ] **Step 1: Update tests to the new handler/result shape**

In `GetProductOptionTypesUseCase.Tests.cs`:
- `QueryHandler` → `PagedQueryHandler`; `new GetProductOptionTypes.Query(<id>)` → `new GetProductOptionTypes.Query(<id>, new GetProductOptionTypes.Parameters())`.
- `result.Value.Items` → `result.Items`; NotFound test asserts `result.IsSuccess.Should().BeFalse()` and `result.StatusCode == 404` (unchanged semantics).
- Add paging test: seed 3 option types + product, query with `Parameters { PageSize = 2 }`, assert `Items` count 2, `TotalCount` 3.

- [ ] **Step 2: Run tests to verify they fail**

Run: `dotnet test service/Api/tests/Module.UnitTests --filter "FullyQualifiedName~GetProductOptionTypes"`
Expected: FAIL — compile errors.

- [ ] **Step 3: Implement the conversion**

Rewrite `GetProductOptionTypes.Response.cs`:

```csharp
using Module.Catalog.Features.Admin.Products.OptionTypes.Shared.Mappings;

namespace Module.Catalog.Features.Admin.Products.OptionTypes.Get;

public static partial class GetProductOptionTypes
{
    public sealed record Response : ProductOptionTypeItemResponse;
}
```

Modify `GetProductOptionTypes.cs`:
- `public sealed record Query(Guid Id) : IQuery<Response>;` → `public sealed record Query(Guid Id, Parameters Parameters) : IPagedQuery<Response>;`
- Rename handler/interface/return type per Pattern B.
- Keep the product-exists check and the `allOptionTypes`/`assignedPositions` loads. Replace the final block:

```csharp
            // Compute: Map each option type with IsAssigned flag and Position
            var items = allOptionTypes.Select(ot =>
            {
                var isAssigned = assignedPositions.ContainsKey(ot.Id);
                return ot.MapToListItem<Response>(
                    isAssigned,
                    isAssigned ? assignedPositions[ot.Id] : 0);
            }).OrderBy(i => i.Position).ToList();

            var pageModel = PageModelExtensions.FromValues(request.Parameters.PageNumber, request.Parameters.PageSize).Value;
            return pageModel.IsEmpty
                ? PagedResult<Response>.Create(items, 1, Math.Max(1, items.Count), items.Count)
                : items.ToPagedResult(pageModel);
```

> Note: the two early returns `ProductResult.Errors.NotFound(request.Id)` convert implicitly to `PagedResult<Response>` via the built-in `Error → PagedResult<T>` operator — no change needed.

Create `GetProductOptionTypes.Parameters.cs`:

```csharp
namespace Module.Catalog.Features.Admin.Products.OptionTypes.Get;

public static partial class GetProductOptionTypes
{
    public sealed record Parameters : QueryingParameters;
}
```

Modify `GetProductOptionTypes.Endpoint.cs`:
- Add `[AsParameters] Parameters parameters,`; `new Query(id)` → `new Query(id, parameters)`; `ToResult()` → `ToPagedResult()`; `.Produces<Result<Response>>()` → `.Produces<PagedResult<Response>>()`.

- [ ] **Step 4: Run tests to verify they pass**

Run: `dotnet test service/Api/tests/Module.UnitTests --filter "FullyQualifiedName~GetProductOptionTypes"`
Expected: PASS.

- [ ] **Step 5: Build**

Run: `dotnet build service/Api/src/Api/Api.csproj`
Expected: Build succeeded, 0 warnings.

- [ ] **Step 6: Commit**

```bash
git add service/Api/src/Module/Catalog/Features/Admin/Products/OptionTypes/Get
git add service/Api/tests/Module.UnitTests/Catalog/Features/Admin/Products/OptionTypes/Get
git commit -m "refactor(catalog): return PagedResult from GetProductOptionTypes"
```

---

### Task 9: Catalog Admin `GetProductClassifications` → `IPagedQuery<Response>` (Pattern B, envelope flattened)

**Files:**
- Modify: `service/Api/src/Module/Catalog/Features/Admin/Products/Classifications/Get/GetProductClassifications.cs`
- Create: `service/Api/src/Module/Catalog/Features/Admin/Products/Classifications/Get/GetProductClassifications.Parameters.cs`
- Modify: `service/Api/src/Module/Catalog/Features/Admin/Products/Classifications/Get/GetProductClassifications.Response.cs`
- Modify: `service/Api/src/Module/Catalog/Features/Admin/Products/Classifications/Get/GetProductClassifications.Endpoint.cs`
- Test: modify `service/Api/tests/Module.UnitTests/Catalog/Features/Admin/Products/Classifications/Get/GetProductClassificationsUseCase.Tests.cs`

**Interfaces:**
- Consumes: `Taxon.MapToListItem<T>(bool, int)`.
- Produces: `GetProductClassifications.Query(Guid Id, Parameters)`, `PagedQueryHandler`, top-level `Response : ClassificationItemResponse`.

- [ ] **Step 1: Update tests to the new handler/result shape**

In `GetProductClassificationsUseCase.Tests.cs`:
- `QueryHandler` → `PagedQueryHandler`; `new GetProductClassifications.Query(<id>)` → `new GetProductClassifications.Query(<id>, new GetProductClassifications.Parameters())`.
- `result.Value.Items` → `result.Items`.
- Add paging test: seed 3 taxons + product, query with `Parameters { PageSize = 2 }`, assert `Items` count 2, `TotalCount` 3.

- [ ] **Step 2: Run tests to verify they fail**

Run: `dotnet test service/Api/tests/Module.UnitTests --filter "FullyQualifiedName~GetProductClassifications"`
Expected: FAIL — compile errors.

- [ ] **Step 3: Implement the conversion**

Rewrite `GetProductClassifications.Response.cs`:

```csharp
using Module.Catalog.Features.Admin.Products.Classifications.Shared.Mappings;

namespace Module.Catalog.Features.Admin.Products.Classifications.Get;

public static partial class GetProductClassifications
{
    public sealed record Response : ClassificationItemResponse;
}
```

Modify `GetProductClassifications.cs`:
- `public sealed record Query(Guid Id) : IQuery<Response>;` → `public sealed record Query(Guid Id, Parameters Parameters) : IPagedQuery<Response>;`
- Rename handler/interface/return type per Pattern B.
- Keep the product-exists check and loads. Replace the final block:

```csharp
            // Compute: Map each taxon with IsAssigned flag and Position
            var items = allTaxons.Select(t =>
            {
                var isAssigned = assignedPositions.ContainsKey(t.Id);
                return t.MapToListItem<Response>(
                    isAssigned,
                    isAssigned ? assignedPositions[t.Id] : 0);
            }).OrderBy(i => i.Position).ToList();

            var pageModel = PageModelExtensions.FromValues(request.Parameters.PageNumber, request.Parameters.PageSize).Value;
            return pageModel.IsEmpty
                ? PagedResult<Response>.Create(items, 1, Math.Max(1, items.Count), items.Count)
                : items.ToPagedResult(pageModel);
```

Create `GetProductClassifications.Parameters.cs`:

```csharp
namespace Module.Catalog.Features.Admin.Products.Classifications.Get;

public static partial class GetProductClassifications
{
    public sealed record Parameters : QueryingParameters;
}
```

Modify `GetProductClassifications.Endpoint.cs`: add `[AsParameters] Parameters parameters,`; `new Query(id, parameters)`; `ToPagedResult()`; `.Produces<PagedResult<Response>>()`.

- [ ] **Step 4: Run tests to verify they pass**

Run: `dotnet test service/Api/tests/Module.UnitTests --filter "FullyQualifiedName~GetProductClassifications"`
Expected: PASS.

- [ ] **Step 5: Build**

Run: `dotnet build service/Api/src/Api/Api.csproj`
Expected: Build succeeded, 0 warnings.

- [ ] **Step 6: Commit**

```bash
git add service/Api/src/Module/Catalog/Features/Admin/Products/Classifications/Get
git add service/Api/tests/Module.UnitTests/Catalog/Features/Admin/Products/Classifications/Get
git commit -m "refactor(catalog): return PagedResult from GetProductClassifications"
```

---

### Task 10: Catalog Admin `GetVariantOptionValues` → `IPagedQuery<Response>` (Pattern B, envelope flattened)

**Files:**
- Modify: `service/Api/src/Module/Catalog/Features/Admin/Products/Variants/OptionValues/Get/GetVariantOptionValues.cs`
- Create: `service/Api/src/Module/Catalog/Features/Admin/Products/Variants/OptionValues/Get/GetVariantOptionValues.Parameters.cs`
- Modify: `service/Api/src/Module/Catalog/Features/Admin/Products/Variants/OptionValues/Get/GetVariantOptionValues.Response.cs`
- Modify: `service/Api/src/Module/Catalog/Features/Admin/Products/Variants/OptionValues/Get/GetVariantOptionValues.Endpoint.cs`
- Modify: `service/Api/src/Module/Catalog/Features/Admin/Products/Variants/OptionValues/Get/GetVariantOptionValues.Validator.cs`
- Test: modify `service/Api/tests/Module.UnitTests/Catalog/Features/Admin/Products/Variants/OptionValues/Get/GetVariantOptionValues.Tests.cs`

**Interfaces:**
- Consumes: existing loads (`OptionValue` + `OptionType`, `OptionValueVariant`).
- Produces: `GetVariantOptionValues.Query(Guid VariantId, Parameters)`, `PagedQueryHandler`, top-level `Response` (former `OptionValueItem`).

- [ ] **Step 1: Update tests to the new handler/result shape**

In `GetVariantOptionValues.Tests.cs`:
- `QueryHandler` → `PagedQueryHandler`; `new GetVariantOptionValues.Query(<id>)` → `new GetVariantOptionValues.Query(<id>, new GetVariantOptionValues.Parameters())`.
- `result.Value.Items` → `result.Items`.
- Add paging test: seed 3 option values + variant, query with `Parameters { PageSize = 2 }`, assert `Items` count 2, `TotalCount` 3.

- [ ] **Step 2: Run tests to verify they fail**

Run: `dotnet test service/Api/tests/Module.UnitTests --filter "FullyQualifiedName~GetVariantOptionValues"`
Expected: FAIL — compile errors.

- [ ] **Step 3: Implement the conversion**

Rewrite `GetVariantOptionValues.Response.cs`:

```csharp
namespace Module.Catalog.Features.Admin.Products.Variants.OptionValues.Get;

public static partial class GetVariantOptionValues
{
    // EXCEPTION: computed option-value DTO — fields incompatible with OptionValueDetailResponse (different property names + IsAssigned)
    public sealed record Response
    {
        public Guid OptionValueId { get; init; }
        public Guid OptionTypeId { get; init; }
        public string OptionTypeName { get; init; } = string.Empty;
        public string Name { get; init; } = string.Empty;
        public string? Presentation { get; init; }
        public bool IsAssigned { get; init; }
    }
}
```

Modify `GetVariantOptionValues.cs`:
- `public sealed record Query(Guid VariantId) : IQuery<Response>;` → `public sealed record Query(Guid VariantId, Parameters Parameters) : IPagedQuery<Response>;`
- Rename handler/interface/return type per Pattern B.
- Keep the variant-exists check and loads. Replace the final block:

```csharp
            // Transform: Enrich each option value with its assignment status
            var items = allOptionValues.Select(ov => new Response
            {
                OptionValueId = ov.Id,
                OptionTypeId = ov.OptionTypeId,
                OptionTypeName = ov.OptionType.Name,
                Name = ov.Name,
                Presentation = ov.Presentation,
                IsAssigned = assignedOptionValueIds.Contains(ov.Id)
            }).OrderBy(i => i.OptionTypeName).ThenBy(i => i.Name).ToList();

            var pageModel = PageModelExtensions.FromValues(request.Parameters.PageNumber, request.Parameters.PageSize).Value;
            return pageModel.IsEmpty
                ? PagedResult<Response>.Create(items, 1, Math.Max(1, items.Count), items.Count)
                : items.ToPagedResult(pageModel);
```

Create `GetVariantOptionValues.Parameters.cs`:

```csharp
namespace Module.Catalog.Features.Admin.Products.Variants.OptionValues.Get;

public static partial class GetVariantOptionValues
{
    public sealed record Parameters : QueryingParameters;
}
```

Modify `GetVariantOptionValues.Endpoint.cs`: add `[AsParameters] Parameters parameters,`; `new Query(variantId, parameters)`; `ToPagedResult()`; `.Produces<PagedResult<Response>>()`.

Modify `GetVariantOptionValues.Validator.cs` — add page-size rule after the existing rule:

```csharp
            RuleFor(x => x.Parameters.PageSize)
                .Must(value => value.HasValue && value.Value >= 1 && value.Value <= 100)
                .WithErrorCode("InvalidPageSize")
                .When(x => x.Parameters.PageSize.HasValue);
```

- [ ] **Step 4: Run tests to verify they pass**

Run: `dotnet test service/Api/tests/Module.UnitTests --filter "FullyQualifiedName~GetVariantOptionValues"`
Expected: PASS.

- [ ] **Step 5: Build**

Run: `dotnet build service/Api/src/Api/Api.csproj`
Expected: Build succeeded, 0 warnings.

- [ ] **Step 6: Commit**

```bash
git add service/Api/src/Module/Catalog/Features/Admin/Products/Variants/OptionValues/Get
git add service/Api/tests/Module.UnitTests/Catalog/Features/Admin/Products/Variants/OptionValues/Get
git commit -m "refactor(catalog): return PagedResult from GetVariantOptionValues"
```

---

### Task 11: Catalog Admin `SyncTaxonRules` → `IPagedQuery<Response>` (Pattern C, envelope flattened)

> This is a POST command. The result is the synced rule set; it returns a single-page `PagedResult` (no paging params accepted). The `Command` record implements `IPagedQuery<Response>` (the repo's marker for "returns `PagedResult`").

**Files:**
- Modify: `service/Api/src/Module/Catalog/Features/Admin/Taxonomies/Taxons/Rules/Sync/SyncTaxonRules.cs`
- Modify: `service/Api/src/Module/Catalog/Features/Admin/Taxonomies/Taxons/Rules/Sync/SyncTaxonRules.Response.cs`
- Modify: `service/Api/src/Module/Catalog/Features/Admin/Taxonomies/Taxons/Rules/Sync/SyncTaxonRules.Endpoint.cs`
- Test: modify `service/Api/tests/Module.UnitTests/Catalog/Features/Admin/Taxonomies/Taxons/Rules/Sync/SyncTaxonRulesUseCase.Tests.cs`

**Interfaces:**
- Consumes: existing command body (`TAutoClassificationService`, logger).
- Produces: `SyncTaxonRules.Command(Guid TaxonId, Request)` implementing `IPagedQuery<Response>`, `PagedQueryHandler`, top-level `Response : TaxonRuleListResponse`.

- [ ] **Step 1: Update tests to the new result shape**

In `SyncTaxonRulesUseCase.Tests.cs`:
- `CommandHandler` → `PagedQueryHandler` (update field + constructor args — the handler keeps `IApplicationDbContext`, `IAutoClassificationService`, `ILogger` constructor args in that order).
- Assertions: `result.Value.Rules` → `result.Items`; `result.Value.Should().BeNull()`/empty assertions → `result.Items.Should().BeEmpty()`.
- NotFound assertion (`result.IsSuccess.Should().BeFalse()` + status 404) unchanged — `TaxonResult.Errors.NotFound` converts implicitly.

- [ ] **Step 2: Run tests to verify they fail**

Run: `dotnet test service/Api/tests/Module.UnitTests --filter "FullyQualifiedName~SyncTaxonRules"`
Expected: FAIL — compile errors.

- [ ] **Step 3: Implement the conversion**

Rewrite `SyncTaxonRules.Response.cs`:

```csharp
using Module.Catalog.Features.Admin.Taxonomies.Taxons.Rules.Shared.Models;

namespace Module.Catalog.Features.Admin.Taxonomies.Taxons.Rules.Sync;

public static partial class SyncTaxonRules
{
    public sealed record Response : TaxonRuleListResponse;
}
```

Modify `SyncTaxonRules.cs`:
- `public sealed record Command(Guid TaxonId, Request Request) : ICommand<Response>;` → `public sealed record Command(Guid TaxonId, Request Request) : IPagedQuery<Response>;`
- Rename `CommandHandler` → `PagedQueryHandler`, `ICommandHandler<Command, Response>` → `IPagedQueryHandler<Command, Response>`, return type `Task<Result<Response>>` → `Task<PagedResult<Response>>`.
- Keep the entire body (sync + auto-classification) through `var mapped = updatedRules.Select(...).ToList();` unchanged. Replace `return new Response { Rules = mapped };` with:

```csharp
            return PagedResult<Response>.Create(mapped, 1, Math.Max(1, mapped.Count), mapped.Count);
```

Modify `SyncTaxonRules.Endpoint.cs`: `ToResult()` → `ToPagedResult()`; `.Produces<Result<Response>>(StatusCodes.Status200OK)` → `.Produces<PagedResult<Response>>(StatusCodes.Status200OK)`.

- [ ] **Step 4: Run tests to verify they pass**

Run: `dotnet test service/Api/tests/Module.UnitTests --filter "FullyQualifiedName~SyncTaxonRules"`
Expected: PASS.

- [ ] **Step 5: Build**

Run: `dotnet build service/Api/src/Api/Api.csproj`
Expected: Build succeeded, 0 warnings.

- [ ] **Step 6: Commit**

```bash
git add service/Api/src/Module/Catalog/Features/Admin/Taxonomies/Taxons/Rules/Sync
git add service/Api/tests/Module.UnitTests/Catalog/Features/Admin/Taxonomies/Taxons/Rules/Sync
git commit -m "refactor(catalog): return PagedResult from SyncTaxonRules"
```

---

### Task 12: Catalog Storefront `GetSimilarProducts` → `IPagedQuery<Response>` (Pattern C, envelope flattened)

**Files:**
- Modify: `service/Api/src/Module/Catalog/Features/Storefront/Products/Get/Similar/GetSimilarProducts.cs`
- Modify: `service/Api/src/Module/Catalog/Features/Storefront/Products/Get/Similar/GetSimilarProducts.Response.cs`
- Modify: `service/Api/src/Module/Catalog/Features/Storefront/Products/Get/Similar/GetSimilarProducts.Endpoint.cs`
- Test: modify `service/Api/tests/Module.UnitTests/Catalog/Features/Storefront/Products/Get/Similar/GetSimilarProductsTests.cs`

**Interfaces:**
- Consumes: `IVectorSearchService.FindSimilarVariantIdsAsync(...)`.
- Produces: `GetSimilarProducts.Query(Guid Id, int TopK = 20)` implementing `IPagedQuery<Response>`, `PagedQueryHandler`, top-level `Response` (former `SimilarProductItem`).

- [ ] **Step 1: Update tests to the new result shape**

In `GetSimilarProductsTests.cs`:
- `QueryHandler` → `PagedQueryHandler`.
- `result.Value.Items` → `result.Items` (both the empty and populated paths).
- NotFound test asserts `result.IsSuccess.Should().BeFalse()` — unchanged (returns `PagedResult<Response>.NotFound()`).

- [ ] **Step 2: Run tests to verify they fail**

Run: `dotnet test service/Api/tests/Module.UnitTests --filter "FullyQualifiedName~GetSimilarProducts"`
Expected: FAIL — compile errors.

- [ ] **Step 3: Implement the conversion**

Rewrite `GetSimilarProducts.Response.cs`:

```csharp
namespace Module.Catalog.Features.Storefront.Products.Get.Similar;

public static partial class GetSimilarProducts
{
    // EXCEPTION: search-result DTO — composite of variant + product data, no domain entity
    public sealed record Response
    {
        public Guid VariantId { get; init; }
        public Guid ProductId { get; init; }
        public string ProductName { get; init; } = string.Empty;
        public string Sku { get; init; } = string.Empty;
        public decimal Price { get; init; }
    }
}
```

Modify `GetSimilarProducts.cs`:
- `public sealed record Query(Guid Id, int TopK = 20) : ICommand<Response>;` → `public sealed record Query(Guid Id, int TopK = 20) : IPagedQuery<Response>;`
- Rename `QueryHandler` → `PagedQueryHandler`, `ICommandHandler<Query, Response>` → `IPagedQueryHandler<Query, Response>`, return type `Task<Result<Response>>` → `Task<PagedResult<Response>>`.
- `return Result<Response>.NotFound();` → `return PagedResult<Response>.NotFound();`
- Both `return Result<Response>.Ok(new Response { Items = [] });` → `return PagedResult<Response>.Create(items: [], page: 1, pageSize: 0, totalCount: 0);`
- `new SimilarProductItem` → `new Response` in the item construction; final return becomes:

```csharp
            return PagedResult<Response>.Create(items, 1, Math.Max(1, items.Count), items.Count);
```

Modify `GetSimilarProducts.Endpoint.cs`: `ToResult()` → `ToPagedResult()`; `.Produces<Result<Response>>()` → `.Produces<PagedResult<Response>>()`.

- [ ] **Step 4: Run tests to verify they pass**

Run: `dotnet test service/Api/tests/Module.UnitTests --filter "FullyQualifiedName~GetSimilarProducts"`
Expected: PASS.

- [ ] **Step 5: Build**

Run: `dotnet build service/Api/src/Api/Api.csproj`
Expected: Build succeeded, 0 warnings.

- [ ] **Step 6: Commit**

```bash
git add service/Api/src/Module/Catalog/Features/Storefront/Products/Get/Similar
git add service/Api/tests/Module.UnitTests/Catalog/Features/Storefront/Products/Get/Similar
git commit -m "refactor(catalog): return PagedResult from GetSimilarProducts"
```

---

### Task 13: Catalog Storefront `SearchByImage` → `IPagedQuery<Response>` (Pattern C, envelope flattened)

**Files:**
- Modify: `service/Api/src/Module/Catalog/Features/Storefront/Products/SearchByImage/SearchByImage.cs`
- Modify: `service/Api/src/Module/Catalog/Features/Storefront/Products/SearchByImage/SearchByImage.Response.cs`
- Modify: `service/Api/src/Module/Catalog/Features/Storefront/Products/SearchByImage/SearchByImage.Endpoint.cs`
- Test: modify `service/Api/tests/Module.UnitTests/Catalog/Features/Storefront/Products/SearchByImage/SearchByImageTests.cs`

**Interfaces:**
- Consumes: `IInferenceClient`, `IVectorSearchService`.
- Produces: `SearchByImage.Command(Request)` implementing `IPagedQuery<Response>`, `PagedQueryHandler`, top-level `Response` (former `SearchResultItem`).

- [ ] **Step 1: Update tests to the new result shape**

In `SearchByImageTests.cs`:
- `SearchByImageFeature.QueryHandler` → `SearchByImageFeature.PagedQueryHandler` (field + constructor).
- `result.Value.Items` → `result.Items` in all four success-path assertions (empty-image, zero-bytes, results, and `result.Value.Items.Should().NotBeEmpty()` / `ContainSingle(...)`).
- Validation-error tests assert `result.IsSuccess.Should().BeFalse()` — unchanged (`SearchByImageResult.Errors.FileTooLarge` converts implicitly to `PagedResult`).

- [ ] **Step 2: Run tests to verify they fail**

Run: `dotnet test service/Api/tests/Module.UnitTests --filter "FullyQualifiedName~SearchByImage"`
Expected: FAIL — compile errors.

- [ ] **Step 3: Implement the conversion**

Rewrite `SearchByImage.Response.cs`:

```csharp
namespace Module.Catalog.Features.Storefront.Products.SearchByImage;

public static partial class SearchByImage
{
    // EXCEPTION: search-result DTO — composite of Variant + embedding data
    public sealed record Response
    {
        public Guid VariantId { get; init; }
        public Guid ProductId { get; init; }
        public string ProductName { get; init; } = string.Empty;
        public string Sku { get; init; } = string.Empty;
        public decimal Price { get; init; }
        public string? ImageUrl { get; init; }
    }
}
```

Modify `SearchByImage.cs`:
- `public sealed record Command(Request Request) : ICommand<Response>;` → `public sealed record Command(Request Request) : IPagedQuery<Response>;`
- Rename `QueryHandler` → `PagedQueryHandler`, `ICommandHandler<Command, Response>` → `IPagedQueryHandler<Command, Response>`, return type `Task<Result<Response>>` → `Task<PagedResult<Response>>`.
- Both `return new Response();` (empty image / no matches) → `return PagedResult<Response>.Create(items: [], page: 1, pageSize: 0, totalCount: 0);`
- `new SearchResultItem` → `new Response` in `MapToItem`; final return becomes:

```csharp
            return PagedResult<Response>.Create(items, 1, Math.Max(1, items.Count), items.Count);
```

Modify `SearchByImage.Endpoint.cs`: `ToResult()` → `ToPagedResult()`; `.Produces<Result<Response>>()` → `.Produces<PagedResult<Response>>()`.

- [ ] **Step 4: Run tests to verify they pass**

Run: `dotnet test service/Api/tests/Module.UnitTests --filter "FullyQualifiedName~SearchByImage"`
Expected: PASS.

- [ ] **Step 5: Build**

Run: `dotnet build service/Api/src/Api/Api.csproj`
Expected: Build succeeded, 0 warnings.

- [ ] **Step 6: Commit**

```bash
git add service/Api/src/Module/Catalog/Features/Storefront/Products/SearchByImage
git add service/Api/tests/Module.UnitTests/Catalog/Features/Storefront/Products/SearchByImage
git commit -m "refactor(catalog): return PagedResult from SearchByImage"
```

---

### Task 14: Identity Admin `GetUserRoles` → `IPagedQuery<Response>` (Pattern B, envelope flattened)

**Files:**
- Modify: `service/Api/src/Module/Identity/Features/Admin/Users/Roles/Get/GetUserRoles.cs`
- Create: `service/Api/src/Module/Identity/Features/Admin/Users/Roles/Get/GetUserRoles.Parameters.cs`
- Modify: `service/Api/src/Module/Identity/Features/Admin/Users/Roles/Get/GetUserRoles.Response.cs`
- Modify: `service/Api/src/Module/Identity/Features/Admin/Users/Roles/Get/GetUserRoles.Endpoint.cs`
- Test: modify `service/Api/tests/Module.UnitTests/Identity/Features/Admin/Users/Roles/Get/GetUserRoles.Tests.cs`

**Interfaces:**
- Consumes: `UserManager<User>`, `RoleManager<Role>`.
- Produces: `GetUserRoles.Query(Guid Id, Parameters)`, `PagedQueryHandler`, top-level `Response : RoleListResponse`.

- [ ] **Step 1: Update tests to the new handler/result shape**

In `GetUserRoles.Tests.cs`:
- `QueryHandler` → `PagedQueryHandler`; `new GetUserRoles.Query(<id>)` → `new GetUserRoles.Query(<id>, new GetUserRoles.Parameters())`.
- `result.Value.Roles` → `result.Items`.
- NotFound test asserts `result.IsSuccess.Should().BeFalse()` — unchanged (`UserResult.Failure.NotFound` converts implicitly).

- [ ] **Step 2: Run tests to verify they fail**

Run: `dotnet test service/Api/tests/Module.UnitTests --filter "FullyQualifiedName~GetUserRoles"`
Expected: FAIL — compile errors.

- [ ] **Step 3: Implement the conversion**

Rewrite `GetUserRoles.Response.cs`:

```csharp
using Module.Identity.Features.Admin.Roles.Shared.Models;

namespace Module.Identity.Features.Admin.Users.Roles.Get;

public static partial class GetUserRoles
{
    public sealed record Response : RoleListResponse
    {
        public bool IsAssigned { get; init; }
    }
}
```

Modify `GetUserRoles.cs`:
- `public sealed record Query(Guid Id) : IQuery<Response>;` → `public sealed record Query(Guid Id, Parameters Parameters) : IPagedQuery<Response>;`
- Rename handler/interface/return type per Pattern B.
- Keep the user-exists check and role loads. Replace the final block:

```csharp
            // Transform: Build response with each role and its assignment status for the user
            var roles = allRoles.Select(role => new Response
            {
                Name = role.Name!,
                Description = role.Description,
                IsAssigned = userRolesSet.Contains(role.Name!)
            }).OrderBy(r => r.Name).ToList();

            var pageModel = PageModelExtensions.FromValues(request.Parameters.PageNumber, request.Parameters.PageSize).Value;
            return pageModel.IsEmpty
                ? PagedResult<Response>.Create(roles, 1, Math.Max(1, roles.Count), roles.Count)
                : roles.ToPagedResult(pageModel);
```

Create `GetUserRoles.Parameters.cs`:

```csharp
namespace Module.Identity.Features.Admin.Users.Roles.Get;

public static partial class GetUserRoles
{
    public sealed record Parameters : QueryingParameters;
}
```

Modify `GetUserRoles.Endpoint.cs`: add `[AsParameters] Parameters parameters,`; `new Query(id, parameters)`; `ToPagedResult()`; `.Produces<PagedResult<Response>>()`.

- [ ] **Step 4: Run tests to verify they pass**

Run: `dotnet test service/Api/tests/Module.UnitTests --filter "FullyQualifiedName~GetUserRoles"`
Expected: PASS.

- [ ] **Step 5: Build**

Run: `dotnet build service/Api/src/Api/Api.csproj`
Expected: Build succeeded, 0 warnings.

- [ ] **Step 6: Commit**

```bash
git add service/Api/src/Module/Identity/Features/Admin/Users/Roles/Get
git add service/Api/tests/Module.UnitTests/Identity/Features/Admin/Users/Roles/Get
git commit -m "refactor(identity): return PagedResult from GetUserRoles"
```

---

### Task 15: Shipping Storefront `GetShippingMethods` → `IPagedQuery<Response>` (Pattern B, envelope flattened)

**Files:**
- Modify: `service/Api/src/Module/Shipping/Features/Storefront/Shipping/Methods/GetShippingMethods.cs`
- Create: `service/Api/src/Module/Shipping/Features/Storefront/Shipping/Methods/GetShippingMethods.Parameters.cs`
- Modify: `service/Api/src/Module/Shipping/Features/Storefront/Shipping/Methods/GetShippingMethods.Response.cs`
- Modify: `service/Api/src/Module/Shipping/Features/Storefront/Shipping/Methods/GetShippingMethods.Endpoint.cs`
- Modify: `service/Api/src/Module/Shipping/Features/Storefront/Shipping/Methods/GetShippingMethods.Validator.cs`
- Test: modify `service/Api/tests/Module.UnitTests/Shipping/Features/Storefront/Shipping/Methods/GetShippingMethodsHandlerTests.cs`

**Interfaces:**
- Consumes: `ShippingMethod` entity.
- Produces: `GetShippingMethods.Query(Parameters)`, `PagedQueryHandler`, top-level `Response` (former `ShippingMethodDto`).

- [ ] **Step 1: Update tests to the new handler/result shape**

In `GetShippingMethodsHandlerTests.cs`:
- `QueryHandler` → `PagedQueryHandler`; `new GetShippingMethods.Query()` → `new GetShippingMethods.Query(new GetShippingMethods.Parameters())`.
- `result.Value.Methods` → `result.Items`.
- Add paging test: seed 3 active methods, query with `Parameters { PageSize = 2 }`, assert `Items` count 2, `TotalCount` 3.

- [ ] **Step 2: Run tests to verify they fail**

Run: `dotnet test service/Api/tests/Module.UnitTests --filter "FullyQualifiedName~GetShippingMethods"`
Expected: FAIL — compile errors.

- [ ] **Step 3: Implement the conversion**

Rewrite `GetShippingMethods.Response.cs`:

```csharp
namespace Module.Shipping.Features.Storefront.Shipping.Methods;

public static partial class GetShippingMethods
{
    // EXCEPTION: DTO mapped from domain ShippingMethod entities — no single shipping method entity
    public sealed record Response
    {
        public Guid Id { get; init; }
        public string Name { get; init; } = default!;
        public string? AdminName { get; init; }
        public string? Code { get; init; }
        public string CalculatorType { get; init; } = default!;
        public int Position { get; init; }
    }
}
```

Modify `GetShippingMethods.cs`:
- `public sealed record Query : IQuery<Response>;` → `public sealed record Query(Parameters Parameters) : IPagedQuery<Response>;`
- Rename `QueryHandler` → `PagedQueryHandler`, `IQueryHandler<Query, Response>` → `IPagedQueryHandler<Query, Response>`, return type `Task<Result<Response>>` → `Task<PagedResult<Response>>`.
- Keep the load; replace the final return:

```csharp
            // Map: Return list of available shipping methods.
            // EXCEPTION: no domain entity — maps from domain ShippingMethod entities to DTOs
            var items = methods.Select(m => new Response
            {
                Id = m.Id,
                Name = m.Name,
                AdminName = m.AdminName,
                Code = m.Code,
                CalculatorType = m.CalculatorType,
                Position = m.Position
            }).OrderBy(m => m.Position).ToList();

            var pageModel = PageModelExtensions.FromValues(request.Parameters.PageNumber, request.Parameters.PageSize).Value;
            return pageModel.IsEmpty
                ? PagedResult<Response>.Create(items, 1, Math.Max(1, items.Count), items.Count)
                : items.ToPagedResult(pageModel);
```

Create `GetShippingMethods.Parameters.cs`:

```csharp
namespace Module.Shipping.Features.Storefront.Shipping.Methods;

public static partial class GetShippingMethods
{
    public sealed record Parameters : QueryingParameters;
}
```

Modify `GetShippingMethods.Endpoint.cs`: add `[AsParameters] Parameters parameters,`; `new Query()` → `new Query(parameters)`; `ToResult()` → `ToPagedResult()`; `.Produces<Result<Response>>()` → `.Produces<PagedResult<Response>>()`.

Modify `GetShippingMethods.Validator.cs` — add:

```csharp
            RuleFor(x => x.Parameters.PageSize)
                .Must(value => value.HasValue && value.Value >= 1 && value.Value <= 100)
                .WithErrorCode("InvalidPageSize")
                .When(x => x.Parameters.PageSize.HasValue);
```

- [ ] **Step 4: Run tests to verify they pass**

Run: `dotnet test service/Api/tests/Module.UnitTests --filter "FullyQualifiedName~GetShippingMethods"`
Expected: PASS.

- [ ] **Step 5: Build**

Run: `dotnet build service/Api/src/Api/Api.csproj`
Expected: Build succeeded, 0 warnings.

- [ ] **Step 6: Commit**

```bash
git add service/Api/src/Module/Shipping/Features/Storefront/Shipping/Methods
git add service/Api/tests/Module.UnitTests/Shipping/Features/Storefront/Shipping/Methods
git commit -m "refactor(shipping): return PagedResult from GetShippingMethods"
```

---

### Task 16: Admin SPA consumers — option types & classifications APIs

> The two assignment-list APIs consumed by `ProductDetail.vue` change shape from `Result<{ items: T[] }>` to `PagedResult<T>`. The storefront SPA and `ApiTests/*.http` have no references to converted endpoints (verified), so these are the only consumers to update.

**Files:**
- Modify: `app/Admin/src/features/catalog/services/productOptionTypeApi.ts`
- Modify: `app/Admin/src/features/catalog/services/productClassificationApi.ts`
- Modify: `app/Admin/src/features/catalog/views/ProductDetail.vue:144-148,154-158`

**Interfaces:**
- Consumes: API `PagedResult<T>` JSON (`items`, `isSuccess`, `page`, `pageSize`, `totalCount`).
- Produces: `ProductOptionTypeApi.getOptionTypes(productId): Promise<PagedResult<OptionTypeAssignment>>`, `ProductClassificationApi.getClassifications(productId): Promise<PagedResult<ClassificationAssignment>>`.

- [ ] **Step 1: Update the two service methods**

`app/Admin/src/features/catalog/services/productOptionTypeApi.ts`:

```typescript
import { post, get } from '@/shared/api/client'
import { CATALOG } from '@/shared/constants/api'
import type { PagedResult } from '@/shared/types'

export interface OptionTypeAssignment {
  optionTypeId: string
  name: string
  presentation: string | null
  position: number
  isAssigned: boolean
}

interface OptionTypeSyncItem {
  optionTypeId: string
  position: number
}

export class ProductOptionTypeApi {
  private static getBase(productId: string): string {
    return `${CATALOG}/products/${productId}/option-types`
  }

  static getOptionTypes(productId: string): Promise<PagedResult<OptionTypeAssignment>> {
    return get<PagedResult<OptionTypeAssignment>>(ProductOptionTypeApi.getBase(productId))
  }

  static syncOptionTypes(productId: string, items: OptionTypeSyncItem[]): Promise<Result<void>> {
    return post<Result<void>>(`${ProductOptionTypeApi.getBase(productId)}/sync`, { items })
  }
}
```

`app/Admin/src/features/catalog/services/productClassificationApi.ts` — same change pattern: `getClassifications` returns `Promise<PagedResult<ClassificationAssignment>>`, `Result` import narrowed to `PagedResult` (keep `Result` import for `syncClassifications`).

- [ ] **Step 2: Update ProductDetail.vue call sites**

`app/Admin/src/features/catalog/views/ProductDetail.vue`:
- Line 145: `if (result.isSuccess && result.value?.items) {` → `if (result.isSuccess && result.items) {`
- Line 146: `unassignedOptionTypes.value = result.value.items.filter(...)` → `result.items.filter(...)`
- Line 147: `assignedOptionTypes.value = result.value.items.filter(...)` → `result.items.filter(...)`
- Line 155: `if (result.isSuccess && result.value?.items) {` → `if (result.isSuccess && result.items) {`
- Line 156: `result.value.items.filter(...)` → `result.items.filter(...)`
- Line 157: `result.value.items.filter(...)` → `result.items.filter(...)`

- [ ] **Step 3: Lint and run Admin unit tests**

Run: `cd app/Admin && pnpm run lint`
Expected: no errors.

Run: `cd app/Admin && pnpm run test:unit`
Expected: PASS.

- [ ] **Step 4: Commit**

```bash
git add app/Admin/src/features/catalog/services/productOptionTypeApi.ts
git add app/Admin/src/features/catalog/services/productClassificationApi.ts
git add app/Admin/src/features/catalog/views/ProductDetail.vue
git commit -m "refactor(admin): consume PagedResult for product option types and classifications"
```

---

### Task 17: Full verification

**Files:** none.

- [ ] **Step 1: Build the whole solution**

Run: `dotnet build`
Expected: Build succeeded, 0 warnings (warnings-as-errors).

- [ ] **Step 2: Run all unit tests**

Run: `dotnet test service/Api/tests/Module.UnitTests`
Expected: all PASS.

- [ ] **Step 3: Run Shared unit tests**

Run: `dotnet test service/Api/tests/Shared.UnitTests`
Expected: all PASS.

- [ ] **Step 4: Run Admin SPA checks**

Run: `cd app/Admin && pnpm run lint && pnpm run test:unit`
Expected: no lint errors, all tests PASS.

- [ ] **Step 5: Run Store SPA checks**

Run: `cd app/Store && pnpm run lint && pnpm run test:unit`
Expected: no lint errors, all tests PASS.

- [ ] **Step 6: Run convention scripts**

Run: `bash scripts/check-feature-conventions.sh`
Expected: all checks PASS (AC-001/002/003/005).

Run: `bash scripts/check-cross-module-refs.sh`
Expected: no new cross-module reference violations.

- [ ] **Step 7: Grep for any remaining non-paged list endpoints**

Run: `rg -n "Produces<(List<|IEnumerable<|IReadOnlyList<|Result<List<)" --glob "*Endpoint.cs" service/Api/src/Module`
Expected: no matches (all list endpoints now `PagedResult`).

- [ ] **Step 8: Commit**

```bash
git status  # confirm working tree clean of stray changes
```
No commit needed unless step 7 found stragglers.

---

## Self-Review

**Spec coverage:** All 15 endpoints from the spec's Tier 1 (Tasks 1–4) and Tier 2 (Tasks 5–15) are covered; Tier 3 (tree, matrix, dashboards, `CalculateShipping`) intentionally untouched. Consumer updates (Task 16) and verification (Task 17) close out the spec's Consumers/Testing/Success-criteria sections. The approved `GetStockAvailability` flatten (dropping aggregate fields) is Task 5 with an explicit behavior-change note.

**Placeholders:** No TBD/TODO; every task carries concrete code, exact paths, and runnable verification commands. `GetStockSummary`/`GetCartReservations`/`GetUserRoles`/`ListVariantImages`/`ListVariantsByProduct`/`GetProductOptionTypes`/`GetProductClassifications`/`GetVariantOptionValues`/`GetShippingMethods` tests are described as precise edits to the existing files with code shown for the changed assertions and new paging tests.

**Type consistency:** Response types used in endpoints (`PagedResult<Response>`, `PagedResult<VariantImageDetailResponse>`) match the handler return types; promoted `Response` records keep their base types (`StockItemListItemResponse`, `VariantDetailResponse`, `ProductOptionTypeItemResponse`, `ClassificationItemResponse`, `TaxonRuleListResponse`, `RoleListResponse`) satisfying AC-002, or carry `// EXCEPTION:` comments (option values, availability, similar, search, shipping methods). `PageModelExtensions.FromValues`, `ToPagedOrAllAsync`, and in-memory `ToPagedResult` signatures match the Shared sources verified during research.
