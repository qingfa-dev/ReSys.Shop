# Module Hygiene & Host Boot Order Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Delete the empty `Module/Webhooks/` directory, replace `NullOrderEventPublisher` with an in-process channel implementation, and move database initialization behind a health check so failed migrations do not produce a half-started host.

**Architecture:** Three independent changes. (1) `Webhooks/` deletion + `AGENTS.md` note. (2) `InProcessOrderEventPublisher` using `System.Threading.Channels.Channel<T>`. (3) `IDatabaseInitializationState` health check wired into Aspire's `WithHealthCheck`.

**Tech Stack:** .NET 10, xUnit v3, Moq, `Microsoft.Extensions.Diagnostics.HealthChecks`.

## Global Constraints

- `TreatWarningsAsErrors=true`
- Test pattern: `[Fact(DisplayName = "...")]`, `[Trait("Category", "Unit")]`
- `NullOrderEventPublisher` deletion MUST NOT break existing tests that depend on it; new tests use `InProcessOrderEventPublisher`
- `Webhooks/` deletion MUST NOT leave dangling references in `AGENTS.md`, `.harness/`, or any code

## File Structure

### Files to delete

| Path | Reason |
|------|--------|
| `service/Api/src/Module/Webhooks/` (entire tree) | Empty module, not registered in `Program.cs` |

### Files to create

| File | Purpose |
|------|---------|
| `service/Api/src/Module/Ordering/Infrastructure/Events/InProcessOrderEventPublisher.cs` | Channel-based implementation |
| `service/Api/src/Shared/Operational/Persistence/Health/DatabaseInitializationHealthCheck.cs` | Health check for DB init state |
| `service/Api/src/Shared/Operational/Persistence/Health/IDatabaseInitializationState.cs` | Shared state contract |
| `service/Api/src/Shared/Operational/Persistence/Health/DatabaseInitializationState.cs` | State implementation |
| `service/Api/tests/Module.UnitTests/Ordering/InProcessOrderEventPublisherTests.cs` | Round-trip test |
| `service/Api/tests/Shared.UnitTests/Shared/DatabaseInitializationHealthCheckTests.cs` | Health check tests |

### Files to modify

| File | Change |
|------|--------|
| `service/Api/src/Module/Ordering/Ordering.Extension.cs:19` | Replace `NullOrderEventPublisher` with `InProcessOrderEventPublisher` for dev |
| `service/Api/src/Shared/Operational/Persistence/Initializers/DatabaseInitializer.cs` | Register state and report completion |
| `service/Api/src/Api/Program.cs:61` | Move `InitializeDatabaseAsync` call to a `HostedService` |
| `AGENTS.md` | Remove the `Webhooks` row from the repo map |

---

## Task 1: Delete the empty `Module/Webhooks/` tree

**Files:**
- Delete: `service/Api/src/Module/Webhooks/` (entire tree)
- Modify: `AGENTS.md`

- [ ] **Step 1: Confirm the directory is empty**

Run: `find service/Api/src/Module/Webhooks -type f`
Expected: empty (no output, or only `.gitkeep` files).

- [ ] **Step 2: Delete the directory**

Run: `rm -rf service/Api/src/Module/Webhooks`
Expected: success.

- [ ] **Step 3: Search for any references to `Module.Webhooks`**

Run: `rg "Module\\.Webhooks" service/ src/ app/ docs/`
Expected: no matches in `*.cs` files. The only match should be in `AGENTS.md` (the repo map) and possibly `.harness/`.

- [ ] **Step 4: Update `AGENTS.md`**

Open `AGENTS.md` and find the line:

```
- `service/Api/src/Module/` — 8 business modules (Catalog, Identity, Inventory, Location, Ordering, Payment, Profile, Shipping), each with `Domain/`, `Features/`, `Persistence/`
```

Remove any parenthetical mention of `Webhooks` if present. The exact wording depends on the current file — preserve the surrounding prose.

- [ ] **Step 5: Search `.harness/` for `Webhooks` references**

Run: `rg -l "Webhooks" .harness/ 2>/dev/null`
Expected: matches (or no matches). For each match, decide whether to remove the `Webhooks` row or leave a note. The `domains.yml` and `principles.yml` files may reference module counts; if so, decrement the count.

- [ ] **Step 6: Build**

Run: `dotnet build service/Api/Api.slnx`
Expected: success.

- [ ] **Step 7: Commit**

```bash
git add -A service/Api/src/Module/Webhooks AGENTS.md .harness/
git commit -m "chore(modules): delete empty Webhooks module tree"
```

---

## Task 2: Implement `InProcessOrderEventPublisher`

**Files:**
- Create: `service/Api/src/Module/Ordering/Infrastructure/Events/InProcessOrderEventPublisher.cs`
- Test: `service/Api/tests/Module.UnitTests/Ordering/InProcessOrderEventPublisherTests.cs`

**Context:** Today `NullOrderEventPublisher` drops events silently. Replace with a `Channel<T>`-based implementation for development.

- [ ] **Step 1: Read the existing `IOrderEventPublisher` interface and event types**

Locate `service/Api/src/Module/Ordering/Domain/Orders/Contracts/IOrderEventPublisher.cs` and the `OrderPlacedEvent` record. Note the method signature (e.g. `Task PublishAsync(OrderPlacedEvent evt, CancellationToken ct)`).

- [ ] **Step 2: Write the failing test**

Create file `service/Api/tests/Module.UnitTests/Ordering/InProcessOrderEventPublisherTests.cs`:

```csharp
using System.Threading.Channels;
using Module.Ordering.Domain.Orders.Contracts;
using Module.Ordering.Infrastructure.Events;
using OrderPlacedEvent = Module.Ordering.Domain.Orders.Contracts.OrderPlacedEvent;

namespace Module.UnitTests.Ordering;

[Trait("Category", "Unit")]
[Trait("Module", "Ordering")]
public class InProcessOrderEventPublisherTests
{
    [Fact(DisplayName = "Publish: enqueues event readable from the channel")]
    public async Task Publish_ReaderReceivesEvent()
    {
        var sut = new InProcessOrderEventPublisher();
        var evt = new OrderPlacedEvent(Guid.NewGuid(), "R20260712-ABCDEFGH", 100m, DateTimeOffset.UtcNow);

        await sut.PublishAsync(evt, TestContext.Current.CancellationToken);

        var read = await sut.Reader.ReadAsync(TestContext.Current.CancellationToken);
        read.Should().BeEquivalentTo(evt);
    }

    [Fact(DisplayName = "Publish: ordering is preserved for sequential writes")]
    public async Task Publish_OrderingPreserved()
    {
        var sut = new InProcessOrderEventPublisher();
        var events = Enumerable.Range(0, 100)
            .Select(i => new OrderPlacedEvent(Guid.NewGuid(), $"R-{i}", i, DateTimeOffset.UtcNow))
            .ToList();

        foreach (var e in events) await sut.PublishAsync(e, TestContext.Current.CancellationToken);

        for (var i = 0; i < events.Count; i++)
        {
            var read = await sut.Reader.ReadAsync(TestContext.Current.CancellationToken);
            read.Number.Should().Be(events[i].Number);
        }
    }
}
```

- [ ] **Step 3: Run the test to verify it fails**

Run: `dotnet test service/Api/tests/Module.UnitTests/Module.UnitTests.csproj --filter "FullyQualifiedName~InProcessOrderEventPublisherTests" --no-restore`
Expected: FAIL — `InProcessOrderEventPublisher` does not exist.

- [ ] **Step 4: Implement the publisher**

Create file `service/Api/src/Module/Ordering/Infrastructure/Events/InProcessOrderEventPublisher.cs`:

```csharp
using System.Threading.Channels;
using Module.Ordering.Domain.Orders.Contracts;
using OrderPlacedEvent = Module.Ordering.Domain.Orders.Contracts.OrderPlacedEvent;

namespace Module.Ordering.Infrastructure.Events;

public sealed class InProcessOrderEventPublisher : IOrderEventPublisher
{
    private readonly Channel<OrderPlacedEvent> _channel = Channel.CreateUnbounded<OrderPlacedEvent>(
        new UnboundedChannelOptions { SingleReader = true, SingleWriter = false });

    public ChannelReader<OrderPlacedEvent> Reader => _channel.Reader;

    public async Task PublishAsync(OrderPlacedEvent evt, CancellationToken ct)
    {
        await _channel.Writer.WriteAsync(evt, ct);
    }
}
```

- [ ] **Step 5: Re-run the test**

Run: `dotnet test service/Api/tests/Module.UnitTests/Module.UnitTests.csproj --filter "FullyQualifiedName~InProcessOrderEventPublisherTests" --no-restore`
Expected: PASS.

- [ ] **Step 6: Build**

Run: `dotnet build service/Api/Api.slnx`
Expected: success.

- [ ] **Step 7: Commit**

```bash
git add service/Api/src/Module/Ordering/Infrastructure/Events/InProcessOrderEventPublisher.cs service/Api/tests/Module.UnitTests/Ordering/InProcessOrderEventPublisherTests.cs
git commit -m "feat(ordering): add InProcessOrderEventPublisher via Channel<T>"
```

---

## Task 3: Wire `InProcessOrderEventPublisher` in `Ordering.Extension.cs`

**Files:**
- Modify: `service/Api/src/Module/Ordering/Ordering.Extension.cs:19`

- [ ] **Step 1: Read the current registration**

Read `service/Api/src/Module/Ordering/Ordering.Extension.cs` line 19.

- [ ] **Step 2: Replace `NullOrderEventPublisher`**

Replace line 19:

```csharp
builder.Services.AddScoped<IOrderEventPublisher, Infrastructure.Events.NullOrderEventPublisher>();
```

with:

```csharp
// Environment-aware: in-process channel for dev, no-op for production
// (the production publisher lands in plan/2026-Q3-event-bus).
if (builder.Environment.IsDevelopment())
    builder.Services.AddSingleton<IOrderEventPublisher, Infrastructure.Events.InProcessOrderEventPublisher>();
else
    builder.Services.AddSingleton<IOrderEventPublisher, Infrastructure.Events.LoggingNullOrderEventPublisher>();
```

- [ ] **Step 3: Create the logging no-op**

Create file `service/Api/src/Module/Ordering/Infrastructure/Events/LoggingNullOrderEventPublisher.cs`:

```csharp
using Microsoft.Extensions.Logging;
using Module.Ordering.Domain.Orders.Contracts;
using OrderPlacedEvent = Module.Ordering.Domain.Orders.Contracts.OrderPlacedEvent;

namespace Module.Ordering.Infrastructure.Events;

public sealed class LoggingNullOrderEventPublisher(ILogger<LoggingNullOrderEventPublisher> logger) : IOrderEventPublisher
{
    private int _count;

    public Task PublishAsync(OrderPlacedEvent evt, CancellationToken ct)
    {
        var n = Interlocked.Increment(ref _count);
        if (n == 1)
        {
            logger.LogWarning(
                "LoggingNullOrderEventPublisher is dropping OrderPlaced events. Configure a real publisher before production cutover. First event: {Number}",
                evt.Number);
        }
        return Task.CompletedTask;
    }
}
```

- [ ] **Step 4: Build**

Run: `dotnet build service/Api/Api.slnx`
Expected: success.

- [ ] **Step 5: Run the full Ordering test suite**

Run: `dotnet test service/Api/tests/Module.UnitTests/Module.UnitTests.csproj --filter "FullyQualifiedName~Ordering" --no-restore`
Expected: all tests pass.

- [ ] **Step 6: Delete the old `NullOrderEventPublisher`**

Run: `git rm service/Api/src/Module/Ordering/Infrastructure/Events/NullOrderEventPublisher.cs`

- [ ] **Step 7: Commit**

```bash
git add service/Api/src/Module/Ordering/Ordering.Extension.cs service/Api/src/Module/Ordering/Infrastructure/Events/LoggingNullOrderEventPublisher.cs
git commit -m "refactor(ordering): use InProcessOrderEventPublisher in development"
```

---

## Task 4: Implement `IDatabaseInitializationState` and health check

**Files:**
- Create: `service/Api/src/Shared/Operational/Persistence/Health/IDatabaseInitializationState.cs`
- Create: `service/Api/src/Shared/Operational/Persistence/Health/DatabaseInitializationState.cs`
- Create: `service/Api/src/Shared/Operational/Persistence/Health/DatabaseInitializationHealthCheck.cs`
- Test: `service/Api/tests/Shared.UnitTests/Shared/DatabaseInitializationHealthCheckTests.cs`

**Context:** Today, when migrations fail, the host builds successfully, liveness returns 200, and the app crashes later. Move the state into a singleton that a health check can read.

- [ ] **Step 1: Write the failing test**

Create file `service/Api/tests/Shared.UnitTests/Shared/DatabaseInitializationHealthCheckTests.cs`:

```csharp
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Shared.Operational.Persistence.Health;

namespace Shared.UnitTests.Shared;

[Trait("Category", "Unit")]
[Trait("Module", "Shared")]
public class DatabaseInitializationHealthCheckTests
{
    [Fact(DisplayName = "HealthCheck: incomplete state returns Unhealthy")]
    public async Task Incomplete_ReturnsUnhealthy()
    {
        var state = new DatabaseInitializationState();
        var check = new DatabaseInitializationHealthCheck(state);

        var result = await check.CheckHealthAsync(new HealthCheckContext(), TestContext.Current.CancellationToken);

        result.Status.Should().Be(HealthStatus.Unhealthy);
    }

    [Fact(DisplayName = "HealthCheck: complete state returns Healthy")]
    public async Task Complete_ReturnsHealthy()
    {
        var state = new DatabaseInitializationState();
        state.MarkComplete();
        var check = new DatabaseInitializationHealthCheck(state);

        var result = await check.CheckHealthAsync(new HealthCheckContext(), TestContext.Current.CancellationToken);

        result.Status.Should().Be(HealthStatus.Healthy);
    }

    [Fact(DisplayName = "HealthCheck: failure state returns Unhealthy with description")]
    public async Task Failure_ReturnsUnhealthyWithDescription()
    {
        var state = new DatabaseInitializationState();
        state.MarkFailed(new InvalidOperationException("migration X failed"));
        var check = new DatabaseInitializationHealthCheck(state);

        var result = await check.CheckHealthAsync(new HealthCheckContext(), TestContext.Current.CancellationToken);

        result.Status.Should().Be(HealthStatus.Unhealthy);
        result.Description.Should().Contain("migration X failed");
    }
}
```

- [ ] **Step 2: Run the test to verify it fails**

Run: `dotnet test service/Api/tests/Shared.UnitTests/Shared.UnitTests.csproj --filter "FullyQualifiedName~DatabaseInitializationHealthCheckTests" --no-restore`
Expected: FAIL — types do not exist.

- [ ] **Step 3: Create the state interface**

Create file `service/Api/src/Shared/Operational/Persistence/Health/IDatabaseInitializationState.cs`:

```csharp
namespace Shared.Operational.Persistence.Health;

public interface IDatabaseInitializationState
{
    bool IsComplete { get; }
    Exception? Failure { get; }
    void MarkComplete();
    void MarkFailed(Exception ex);
}
```

- [ ] **Step 4: Create the state implementation**

Create file `service/Api/src/Shared/Operational/Persistence/Health/DatabaseInitializationState.cs`:

```csharp
namespace Shared.Operational.Persistence.Health;

public sealed class DatabaseInitializationState : IDatabaseInitializationState
{
    private int _complete;
    public bool IsComplete => Volatile.Read(ref _complete) == 1;
    public Exception? Failure { get; private set; }

    public void MarkComplete() => Volatile.Write(ref _complete, 1);

    public void MarkFailed(Exception ex) => Failure = ex;
}
```

- [ ] **Step 5: Create the health check**

Create file `service/Api/src/Shared/Operational/Persistence/Health/DatabaseInitializationHealthCheck.cs`:

```csharp
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Shared.Operational.Persistence.Health;

public sealed class DatabaseInitializationHealthCheck(IDatabaseInitializationState state) : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken ct = default)
    {
        if (state.Failure is { } ex)
            return Task.FromResult(HealthCheckResult.Unhealthy(description: $"Database initialization failed: {ex.Message}", exception: ex));

        return Task.FromResult(state.IsComplete
            ? HealthCheckResult.Healthy("Database initialized.")
            : HealthCheckResult.Unhealthy("Database initialization in progress."));
    }
}
```

- [ ] **Step 6: Re-run the test**

Run: `dotnet test service/Api/tests/Shared.UnitTests/Shared.UnitTests.csproj --filter "FullyQualifiedName~DatabaseInitializationHealthCheckTests" --no-restore`
Expected: PASS.

- [ ] **Step 7: Commit**

```bash
git add service/Api/src/Shared/Operational/Persistence/Health/IDatabaseInitializationState.cs service/Api/src/Shared/Operational/Persistence/Health/DatabaseInitializationState.cs service/Api/src/Shared/Operational/Persistence/Health/DatabaseInitializationHealthCheck.cs service/Api/tests/Shared.UnitTests/Shared/DatabaseInitializationHealthCheckTests.cs
git commit -m "feat(shared): add IDatabaseInitializationState and health check"
```

---

## Task 5: Wire health check + state into `Program.cs` and `DatabaseInitializer`

**Files:**
- Modify: `service/Api/src/Api/Program.cs:58-61`
- Modify: `service/Api/src/Shared/Operational/Persistence/Initializers/DatabaseInitializer.cs`

**Context:** Move the eager `InitializeDatabaseAsync` call into a `BackgroundService` and register the health check.

- [ ] **Step 1: Read the current `Program.cs` boot sequence**

Read `service/Api/src/Api/Program.cs` lines 55-62.

- [ ] **Step 2: Register the state + health check in `Program.cs`**

Replace the lines 58-61 block:

```csharp
bool runMigrations = builder.Configuration.GetValue<bool>("DatabaseInitialization:RunMigrations");
bool runSeeders = !app.Environment.IsProduction();
await app.InitializeDatabaseAsync(runMigrations: runMigrations, runSeeders: runSeeders);
```

with:

```csharp
bool runMigrations = builder.Configuration.GetValue<bool>("DatabaseInitialization:RunMigrations");
bool runSeeders = !app.Environment.IsProduction();

builder.Services.AddSingleton<IDatabaseInitializationState, DatabaseInitializationState>();
builder.Services.AddHostedService<DatabaseInitializerHostedService>();
builder.Services.AddHealthChecks()
    .AddCheck<DatabaseInitializationHealthCheck>("database_initialization", tags: new[] { "ready" });
```

- [ ] **Step 3: Create the hosted service**

Create file `service/Api/src/Shared/Operational/Persistence/Initializers/DatabaseInitializerHostedService.cs`:

```csharp
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Shared.Operational.Persistence.Health;

namespace Shared.Operational.Persistence.Initializers;

public sealed class DatabaseInitializerHostedService(
    IHostApplicationLifetime lifetime,
    IDatabaseInitializationState state,
    IDatabaseInitializer initializer,
    ILogger<DatabaseInitializerHostedService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await initializer.InitializeAsync(stoppingToken);
            state.MarkComplete();
            logger.LogInformation("Database initialization complete.");
        }
        catch (Exception ex)
        {
            state.MarkFailed(ex);
            logger.LogCritical(ex, "Database initialization failed.");
            lifetime.StopApplication();
        }
    }
}
```

- [ ] **Step 4: Refactor `DatabaseInitializer` to implement `IDatabaseInitializer`**

Read `service/Api/src/Shared/Operational/Persistence/Initializers/DatabaseInitializer.cs` end-to-end. The current `InitializeDatabaseAsync` is an extension method on `WebApplication`. Refactor:

1. Extract the body into a new `DatabaseInitializer` class that implements `IDatabaseInitializer.InitializeAsync(CancellationToken)`.
2. The new class receives `IApplicationDbContext` (or the resolver), configuration, and the seeder registry via DI.
3. Register the new class in `AddOperational` or `AddApplication`.

The exact refactor depends on the current `DatabaseInitializer` shape — read it before coding. Keep all existing behavior (migrations + seeders) intact.

- [ ] **Step 5: Build**

Run: `dotnet build service/Api/Api.slnx`
Expected: success.

- [ ] **Step 6: Run the full test suite**

Run: `dotnet test service/Api/tests/Module.UnitTests/Module.UnitTests.csproj --no-restore && dotnet test service/Api/tests/Shared.UnitTests/Shared.UnitTests.csproj --no-restore`
Expected: all tests pass.

- [ ] **Step 7: Commit**

```bash
git add service/Api/src/Api/Program.cs service/Api/src/Shared/Operational/Persistence/Initializers/DatabaseInitializerHostedService.cs service/Api/src/Shared/Operational/Persistence/Initializers/DatabaseInitializer.cs
git commit -m "feat(shared): move database initialization to hosted service with health check"
```

---

## Task 6: Integration test for `/health/ready` reporting init status

**Files:**
- Create: `service/Api/tests/Api.Tests/Scenarios/Host/HealthCheckReadinessTests.cs`

**Context:** Verify that `/health/ready` returns 503 when init has not completed, and 200 after completion.

- [ ] **Step 1: Write the failing test**

Create file `service/Api/tests/Api.Tests/Scenarios/Host/HealthCheckReadinessTests.cs`:

```csharp
using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.Operational.Persistence.Health;

namespace Api.Tests.Scenarios.Host;

[Trait("Category", "Integration")]
public class HealthCheckReadinessTests : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _factory;

    public HealthCheckReadinessTests(ApiFactory factory) { _factory = factory; }

    [Fact(DisplayName = "/health/ready: returns 503 when DB init is incomplete")]
    public async Task Ready_Unhealthy_WhenInitIncomplete()
    {
        // Force the init state to incomplete by replacing the registered instance
        using var scope = _factory.Services.CreateScope();
        var state = scope.ServiceProvider.GetRequiredService<IDatabaseInitializationState>();
        // The shared fixture may already be complete; reset it via reflection only in test:
        var completeField = typeof(DatabaseInitializationState).GetField("_complete",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        completeField?.SetValue(state, 0);

        var client = _factory.CreateClient();
        var response = await client.GetAsync("/health/ready", TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable);
    }

    [Fact(DisplayName = "/health/ready: returns 200 when DB init is complete")]
    public async Task Ready_Healthy_WhenInitComplete()
    {
        using var scope = _factory.Services.CreateScope();
        var state = scope.ServiceProvider.GetRequiredService<IDatabaseInitializationState>();
        state.MarkComplete();

        var client = _factory.CreateClient();
        var response = await client.GetAsync("/health/ready", TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
```

- [ ] **Step 2: Run the test to verify it fails**

Run: `dotnet test service/Api/tests/Api.Tests/Api.Tests.csproj --filter "FullyQualifiedName~HealthCheckReadinessTests" --no-restore`
Expected: FAIL — `/health/ready` does not exist or the health check is not registered.

- [ ] **Step 3: Verify `/health/ready` is mapped**

In `Program.cs`, find the call to `app.MapDefaultEndpoints()`. Confirm Aspire's default endpoint mapping includes `/health/ready` with the `ready` tag. If not, add:

```csharp
app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready")
});
```

- [ ] **Step 4: Re-run the test**

Run: `dotnet test service/Api/tests/Api.Tests/Api.Tests.csproj --filter "FullyQualifiedName~HealthCheckReadinessTests" --no-restore`
Expected: PASS.

- [ ] **Step 5: Commit**

```bash
git add service/Api/tests/Api.Tests/Scenarios/Host/HealthCheckReadinessTests.cs service/Api/src/Api/Program.cs
git commit -m "test(host): verify /health/ready reports DB init state"
```

---

## Task 7: Build and full test suite

- [ ] **Step 1: Build**

Run: `dotnet build service/Api/Api.slnx`
Expected: success.

- [ ] **Step 2: Run unit tests**

Run: `dotnet test service/Api/tests/Module.UnitTests/Module.UnitTests.csproj --no-restore`
Expected: PASS.

- [ ] **Step 3: Run Shared unit tests**

Run: `dotnet test service/Api/tests/Shared.UnitTests/Shared.UnitTests.csproj --no-restore`
Expected: PASS.

- [ ] **Step 4: Run integration tests**

Run: `dotnet test service/Api/tests/Api.Tests/Api.Tests.csproj --no-restore`
Expected: PASS.

- [ ] **Step 5: Final commit**

```bash
git add -A
git commit -m "chore(host): post-hygiene-plan cleanup" --allow-empty
```

---

## Self-Review

- **Spec coverage:** REQ-MOD-001 ✓ Task 1. REQ-MOD-002 ✓ Tasks 2, 3. REQ-HOST-001 ✓ Tasks 4, 5. AC-HOST-001 ✓ Task 6.
- **Placeholders:** Task 5 Step 4 has a refactor with open-ended instructions ("depends on the current DatabaseInitializer shape — read it before coding"). This is intentional; the engineer MUST read the existing file and follow the existing patterns. If the refactor is larger than expected, split it into a follow-up task.
- **Type consistency:** `IDatabaseInitializationState`, `DatabaseInitializationState`, `DatabaseInitializationHealthCheck` referenced consistently across Tasks 4, 5, 6. `IOrderEventPublisher` referenced consistently in Tasks 2, 3. `InProcessOrderEventPublisher.Reader` and `PublishAsync` used in Tasks 2, 3 with matching signatures.
