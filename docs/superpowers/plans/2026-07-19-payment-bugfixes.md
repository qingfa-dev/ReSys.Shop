# Payment System Bug Fixes Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Fix 6 defects in the Payment module: domain Result ignored in webhook jobs, non-nullable FK with SetNull, missing transaction scope in batch void, Bogus gateway always returns "succeeded", CancelAsync hardcodes empty metadata, misleading idempotency comment.

**Architecture:** Each fix is a single-file change to existing classes in `service/Api/src/Module/Payment/`. Tests use the existing `Module.UnitTests` project with in-memory database and Moq. No new projects, no new dependencies, no architectural changes.

**Tech Stack:** .NET 10, EF Core 10 (InMemory for tests), FluentAssertions, Moq, xUnit, Hangfire (unchanged).

## Global Constraints

- Result objects, not exceptions — domain failures return `Result.IsFailure`, never throw
- `TreatWarningsAsErrors=true` — zero warnings on `dotnet build`
- Modules never reference each other — Payment module only
- `Shared` depends on nothing within `service/`; `Module` depends only on `Shared`
- All command handlers use `ISender` (MediatR) — no direct cross-module references
- `IApplicationDbContext.BeginTransactionAsync(IsolationLevel, CancellationToken)` returns `IDatabaseTransaction` with `CommitAsync`/`RollbackAsync`

---

### Task 1: Fix Ignored Domain Result in Webhook Background Job (FW-001)

**Files:**
- Modify: `service/Api/src/Module/Payment/Backgrounds/ProcessStripeWebhookEventJob.cs:59-107`

**Interfaces:**
- Consumes: `PaymentCapture.Complete()` → `Result`, `PaymentCapture.Fail()` → `Result`, `PaymentCapture.Refund(decimal)` → `Result`
- Produces: No interface change — method signatures unchanged; behavior only

- [ ] **Step 1: Modify `HandlePaymentIntentSucceeded` to check Result**

```csharp
private async Task HandlePaymentIntentSucceeded(Event stripeEvent, CancellationToken ct)
{
    var intent = stripeEvent.Data.Object as PaymentIntent;
    if (intent is null) return;

    var payment = await _dbContext.Set<PaymentCapture>()
        .FirstOrDefaultAsync(p => p.ResponseCode == intent.Id, ct);
    if (payment is null) return;
    if (payment.State == PaymentRecordState.Completed) return;

    var result = payment.Complete();
    if (result.IsFailure)
    {
        _logger.LogWarning("Cannot complete payment {PaymentId} (state={State}): {Message}",
            payment.Id, payment.State, result.Message);
        return;
    }

    await _dbContext.SaveChangesAsync(ct);
}
```

- [ ] **Step 2: Modify `HandlePaymentIntentFailed` to check Result**

```csharp
private async Task HandlePaymentIntentFailed(Event stripeEvent, CancellationToken ct)
{
    var intent = stripeEvent.Data.Object as PaymentIntent;
    if (intent is null) return;

    var payment = await _dbContext.Set<PaymentCapture>()
        .FirstOrDefaultAsync(p => p.ResponseCode == intent.Id, ct);
    if (payment is null) return;

    var result = payment.Fail();
    if (result.IsFailure)
    {
        _logger.LogWarning("Cannot fail payment {PaymentId} (state={State}): {Message}",
            payment.Id, payment.State, result.Message);
        return;
    }

    await _dbContext.SaveChangesAsync(ct);
}
```

- [ ] **Step 3: Modify `HandleChargeRefunded` to check Result**

```csharp
private async Task HandleChargeRefunded(Event stripeEvent, CancellationToken ct)
{
    var charge = stripeEvent.Data.Object as Charge;
    if (charge is null || string.IsNullOrEmpty(charge.PaymentIntentId)) return;

    var payment = await _dbContext.Set<PaymentCapture>()
        .FirstOrDefaultAsync(p => p.ResponseCode == charge.PaymentIntentId, ct);
    if (payment is null) return;

    if (charge.AmountRefunded > 0)
    {
        var newRefunded = charge.AmountRefunded / 100m;
        var delta = newRefunded - payment.RefundedAmount;
        if (delta > 0)
        {
            var result = payment.Refund(delta);
            if (result.IsFailure)
            {
                _logger.LogWarning("Cannot refund payment {PaymentId} (state={State}): {Message}",
                    payment.Id, payment.State, result.Message);
                return;
            }
        }
    }
    payment.ModifiedAtUtc = DateTimeOffset.UtcNow;
    await _dbContext.SaveChangesAsync(ct);
}
```

- [ ] **Step 4: Build to verify no warnings**

```bash
dotnet build service/Api/src/Module
```
Expected: Build succeeded with 0 warnings.

- [ ] **Step 5: Run existing webhook job tests**

```bash
dotnet test service/Api/tests/Module.UnitTests --filter "FullyQualifiedName~ProcessStripeWebhookEventJob"
```
Expected: All 4 tests pass.

---

### Task 2: Make PaymentMethodId Nullable (FK-002)

**Files:**
- Modify: `service/Api/src/Module/Payment/Domain/PaymentCaptures/PaymentCapture.cs:27`
- Modify: `service/Api/src/Module/Payment/Persistence/Configurations/Payments/PaymentRecordConfiguration.cs:33` (verify only, no functional change needed)

**Interfaces:**
- Consumes: `PaymentRecordConfiguration` FK mapping already uses `SetNull`
- Produces: `PaymentCapture.PaymentMethodId` changes from `Guid` to `Guid?`

- [ ] **Step 1: Change PaymentMethodId to nullable Guid in PaymentCapture.cs**

Open `service/Api/src/Module/Payment/Domain/PaymentCaptures/PaymentCapture.cs`. Change line 27 from:

```csharp
public Guid PaymentMethodId { get; set; }
```

to:

```csharp
public Guid? PaymentMethodId { get; set; }
```

- [ ] **Step 2: Verify PaymentRecordConfiguration.cs needs no change**

Open `service/Api/src/Module/Payment/Persistence/Configurations/Payments/PaymentRecordConfiguration.cs`. Line 33 is:

```csharp
builder.HasOne(x => x.PaymentMethod).WithMany(pm => pm.Payments).HasForeignKey(x => x.PaymentMethodId).OnDelete(DeleteBehavior.SetNull);
```

This is now valid because the FK property is nullable. No change needed.

- [ ] **Step 3: Verify validation rules handle nullable Guid**

Open `service/Api/src/Module/Payment/Domain/PaymentCaptures/PaymentCapture.Validation.cs`. Line 46-52:

```csharp
public static IRuleBuilderOptions<T, Guid> ApplyPaymentMethodIdRules<T>(this IRuleBuilder<T, Guid> ruleBuilder)
```

This takes `Guid`, not `Guid?`. The caller supplies `Guid?` which FluentValidation's `.NotEmpty()` handles correctly for nullable types (empty = `Guid.Empty`). If callers pass a nullable, cast with `.GetValueOrDefault()` or change the validator to accept `Guid?`. Check callers:

```bash
rg "ApplyPaymentMethodIdRules" service/Api/src/Module/Payment/
```

If callers pass `Guid?`, change the validator signature to `IRuleBuilder<T, Guid?>` and drop the `.GetValueOrDefault()` cast.

- [ ] **Step 4: Build to verify no warnings**

```bash
dotnet build service/Api/src/Module
```
Expected: Build succeeded with 0 warnings.

- [ ] **Step 5: Run existing payment validation tests**

```bash
dotnet test service/Api/tests/Module.UnitTests --filter "FullyQualifiedName~PaymentMethod"
```
Expected: All existing tests pass.

---

### Task 3: Add Transaction Scope to VoidOrderPayments (TX-003)

**Files:**
- Modify: `service/Api/src/Module/Payment/Features/Shared/Commands/VoidOrderPayments.cs:23-56`

**Interfaces:**
- Consumes: `IApplicationDbContext.BeginTransactionAsync(IsolationLevel.ReadCommitted, ct)` → `IDatabaseTransaction` with `CommitAsync(ct)` and `RollbackAsync(ct)`
- Consumes: `IGatewayRegistry.GetGateway(string)` → `Result<IPaymentGatewayActionProvider>`
- Consumes: `IPaymentProcessingService.VoidTransactionAsync(PaymentCapture, IPaymentGatewayActionProvider, GatewayOptions, object?, CancellationToken)` → `Result<PaymentProcessingResult>`
- Produces: No interface change — `Handle` method signature unchanged

- [ ] **Step 1: Rewrite the Handle method with transaction scope**

Replace the entire `Handle` method body (lines 23-56) in `VoidOrderPayments.cs` with:

```csharp
public async Task<Result> Handle(VoidOrderPaymentsCommand command, CancellationToken ct)
{
    var payments = await dbContext.Set<PaymentCapture>()
        .Where(p => p.OrderId == command.OrderId)
        .ToListAsync(ct);

    await using var transaction = await dbContext.BeginTransactionAsync(
        System.Data.IsolationLevel.ReadCommitted, ct);

    try
    {
        foreach (var payment in payments)
        {
            var gatewayResult = gatewayRegistry.GetGateway(payment.ProviderKey);
            if (gatewayResult.IsFailure)
            {
                await transaction.RollbackAsync(ct);
                return PaymentCaptureResult.Failure.ProviderNotRegistered(payment.ProviderKey);
            }

            var options = new GatewayOptions
            {
                Email = string.Empty,
                Customer = string.Empty,
                OrderId = payment.OrderId.ToString(),
                PaymentId = payment.Number,
                IdempotencyKey = GatewayConstants.Idempotency.ForPayment(payment.Number),
                StatementDescriptorSuffix = string.Empty,
            };

            var voidResult = await processingService.VoidTransactionAsync(
                payment, gatewayResult.Value, options, null, ct);
            if (voidResult.IsFailure)
            {
                await transaction.RollbackAsync(ct);
                return voidResult.Errors;
            }
        }

        await dbContext.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);
        return Result.Ok();
    }
    catch
    {
        await transaction.RollbackAsync(ct);
        throw;
    }
}
```

- [ ] **Step 2: Add required using for IsolationLevel**

Add at the top of the file (if not already present):

```csharp
using System.Data;
```

- [ ] **Step 3: Build to verify no warnings**

```bash
dotnet build service/Api/src/Module
```
Expected: Build succeeded with 0 warnings.

- [ ] **Step 4: Run existing VoidOrderPayments tests**

```bash
dotnet test service/Api/tests/Module.UnitTests --filter "FullyQualifiedName~VoidOrderPayments"
```
Expected: Tests pass. Note: existing tests use in-memory database which doesn't support real transactions. The `ApplicationDbContext` in the in-memory provider may throw `NotSupportedException` for `BeginTransactionAsync`. If it does, the tests need updating (see Task 8).

---

### Task 4: Override GetPaymentStatusAsync in BogusGateway (GW-004)

**Files:**
- Modify: `service/Api/src/Module/Payment/Services/Provider/Bogus/BogusGateway.cs` — add new override after line 68

**Interfaces:**
- Produces: `BogusGateway.GetPaymentStatusAsync(string, CancellationToken)` → `Task<string>`

- [ ] **Step 1: Add a status store to BogusGateway and override GetPaymentStatusAsync**

Add a `ConcurrentDictionary` field and override method at the end of the `BogusGateway` class (after `CreateSetupIntentAsync`):

```csharp
private readonly ConcurrentDictionary<string, string> _intentStatuses = new();

public override Task<string> GetPaymentStatusAsync(string responseCode, CancellationToken ct)
{
    if (_intentStatuses.TryGetValue(responseCode, out var status))
        return Task.FromResult(status);
    return Task.FromResult("succeeded");
}
```

- [ ] **Step 2: Update SimulateGatewayResponse to store intent status**

In the `SimulateGatewayResponse` method, after the response is created, store the status keyed by the authorization code. Modify the method:

```csharp
private Task<Result<PaymentGatewayResponse>> SimulateGatewayResponse(
    decimal amount, object? source, GatewayOptions options)
{
    var cardNumber = source as string;
    if (cardNumber == TestCards.Declined)
    {
        return Task.FromResult<Result<PaymentGatewayResponse>>(BogusGatewayResult.Errors.CardDeclined);
    }
    if (cardNumber == TestCards.InsufficientFunds)
    {
        return Task.FromResult<Result<PaymentGatewayResponse>>(BogusGatewayResult.Errors.InsufficientFunds);
    }
    if (cardNumber != TestCards.Success && cardNumber is not null)
    {
        return Task.FromResult<Result<PaymentGatewayResponse>>(BogusGatewayResult.Errors.UnknownCard);
    }

    var authCode = $"auth_{Guid.NewGuid():N}";
    _intentStatuses[authCode] = "succeeded";

    return Task.FromResult(Result<PaymentGatewayResponse>.Ok(
        new PaymentGatewayResponse(GatewayConstants.Providers.Bogus,
            authorization: authCode,
            clientSecret: $"pi_fake_{Guid.NewGuid():N}_secret_{Guid.NewGuid():N}")));
}
```

- [ ] **Step 3: Add using statement**

Add at the top of `BogusGateway.cs`:

```csharp
using System.Collections.Concurrent;
```

- [ ] **Step 4: Build to verify no warnings**

```bash
dotnet build service/Api/src/Module
```
Expected: Build succeeded with 0 warnings.

- [ ] **Step 5: Run existing BogusGateway tests**

```bash
dotnet test service/Api/tests/Module.UnitTests --filter "FullyQualifiedName~BogusGateway"
```
Expected: All 3 tests pass.

---

### Task 5: Pass GatewayOptions to CancelAsync (GW-005)

**Files:**
- Modify: `service/Api/src/Module/Payment/Services/Processing/PaymentProcessingService.cs:212-231`

**Interfaces:**
- Produces: `CancelAsync` signature changes from `(PaymentCapture, IPaymentGatewayActionProvider, CancellationToken)` to `(PaymentCapture, IPaymentGatewayActionProvider, GatewayOptions, CancellationToken)`

- [ ] **Step 1: Change CancelAsync signature to accept GatewayOptions**

Change line 212-213 from:

```csharp
private async Task<Result<PaymentProcessingResult>> CancelAsync(PaymentCapture payment, IPaymentGatewayActionProvider gateway, CancellationToken ct = default)
{
    var gatewayResult = await gateway.VoidAsync(payment.ResponseCode, payment, new GatewayOptions
    {
        Email = string.Empty,
        Customer = string.Empty,
        OrderId = payment.OrderId.ToString(),
        PaymentId = payment.Number,
        IdempotencyKey = GatewayConstants.Idempotency.ForPayment(payment.Number)
    }, ct).ConfigureAwait(false);
```

to:

```csharp
private async Task<Result<PaymentProcessingResult>> CancelAsync(PaymentCapture payment, IPaymentGatewayActionProvider gateway, GatewayOptions options, CancellationToken ct = default)
{
    var gatewayResult = await gateway.VoidAsync(payment.ResponseCode, payment, options, ct).ConfigureAwait(false);
```

- [ ] **Step 2: Confirm CancelAsync has 0 callers (safe refactor)**

```bash
rg "\.CancelAsync\(" service/Api/src/Module/Payment/Services/Processing/
```
Expected: 0 matches (the method definition at line 212 is the only reference — it's dead code). No callers to update.

- [ ] **Step 3: Build to verify no warnings**

```bash
dotnet build service/Api/src/Module
```
Expected: Build succeeded with 0 warnings.

- [ ] **Step 4: Run existing PaymentProcessingService tests**

```bash
dotnet test service/Api/tests/Module.UnitTests --filter "FullyQualifiedName~PaymentProcessingService"
```
Expected: All tests pass.

---

### Task 6: Fix Complete() Comment (DOC-006)

**Files:**
- Modify: `service/Api/src/Module/Payment/Domain/PaymentCaptures/PaymentCapture.Method.State.cs:35`

**Interfaces:**
- Produces: No code change — comment only

- [ ] **Step 1: Fix the comment on Complete()**

Change line 35 in `PaymentCapture.Method.State.cs` from:

```csharp
// Update: Processing/Pending → Completed — idempotent if already completed
```

to:

```csharp
// Update: Processing/Pending → Completed — returns AlreadyCompleted error if already completed
```

- [ ] **Step 2: Build to verify no warnings**

```bash
dotnet build service/Api/src/Module
```
Expected: Build succeeded with 0 warnings (comment-only change).

---

### Task 7: Update Existing Tests for New Behavior

**Files:**
- Modify: `service/Api/tests/Module.UnitTests/Payment/Backgrounds/ProcessStripeWebhookEventJobTests.cs` — add 3 test methods
- Modify: `service/Api/tests/Module.UnitTests/Payment/Infrastructure/BogusGatewayTests.cs` — add 1 test method

**Interfaces:**
- Consumes: `ProcessStripeWebhookEventJob` now checks `Result.IsFailure` and logs
- Consumes: `BogusGateway.GetPaymentStatusAsync` now returns correct status

- [ ] **Step 1: Add test for Complete() failure in webhook job**

Add to `ProcessStripeWebhookEventJobTests` class, after the idempotency test (line 147):

```csharp
[Fact(DisplayName = "payment_intent.succeeded does not save when Complete returns failure")]
public async Task HandlePaymentIntentSucceeded_ShouldNotSave_WhenCompleteFails()
{
    var payment = PaymentCaptureMethod.Create(100m, Guid.NewGuid(), Guid.NewGuid()).Value;
    payment.State = PaymentRecordState.Completed; // Already completed — Complete() will fail
    payment.ResponseCode = "pi_already_done";
    _dbContext.Set<PaymentCapture>().Add(payment);
    await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

    _webhookMock.Setup(x => x.ParseEvent(It.IsAny<string>()))
        .Returns(new Event
        {
            Type = "payment_intent.succeeded",
            Data = new EventData
            {
                Object = new PaymentIntent { Id = "pi_already_done" }
            }
        });

    await _job.ExecuteAsync("{}", TestContext.Current.CancellationToken);

    // State should still be Completed, not changed by a failed Complete() call
    var updated = await _dbContext.Set<PaymentCapture>().FirstAsync(p => p.Id == payment.Id);
    updated.State.Should().Be(PaymentRecordState.Completed);
}
```

- [ ] **Step 2: Add test for Fail() pre-check on non-failable state**

Add to `ProcessStripeWebhookEventJobTests` class:

```csharp
[Fact(DisplayName = "payment_intent.payment_failed does not save when Fail returns failure")]
public async Task HandlePaymentIntentFailed_ShouldNotSave_WhenFailFails()
{
    var payment = PaymentCaptureMethod.Create(100m, Guid.NewGuid(), Guid.NewGuid()).Value;
    payment.State = PaymentRecordState.Completed; // Cannot fail a completed payment
    payment.ResponseCode = "pi_cant_fail";
    _dbContext.Set<PaymentCapture>().Add(payment);
    await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

    _webhookMock.Setup(x => x.ParseEvent(It.IsAny<string>()))
        .Returns(new Event
        {
            Type = "payment_intent.payment_failed",
            Data = new EventData
            {
                Object = new PaymentIntent { Id = "pi_cant_fail" }
            }
        });

    await _job.ExecuteAsync("{}", TestContext.Current.CancellationToken);

    var updated = await _dbContext.Set<PaymentCapture>().FirstAsync(p => p.Id == payment.Id);
    updated.State.Should().Be(PaymentRecordState.Completed); // Unchanged
}
```

- [ ] **Step 3: Add test for BogusGateway GetPaymentStatusAsync**

Add to `BogusGatewayTests` class:

```csharp
[Fact(DisplayName = "GetPaymentStatusAsync returns correct status after successful purchase")]
public async Task GetPaymentStatusAsync_ShouldReturnStatus_FromSimulatedIntent()
{
    var gateway = CreateGateway();
    var response = await gateway.PurchaseAsync(
        amount: 1000m,
        source: BogusGateway.TestCards.Success,
        options: CreateGatewayOptions());
    Assert.True(response.IsSuccess);

    var authorization = response.Value!.Authorization;
    var status = await gateway.GetPaymentStatusAsync(authorization);
    status.Should().Be("succeeded");
}
```

- [ ] **Step 4: Run all updated tests**

```bash
dotnet test service/Api/tests/Module.UnitTests --filter "FullyQualifiedName~ProcessStripeWebhookEventJob|FullyQualifiedName~BogusGateway"
```
Expected: All tests pass (existing + new).

---

### Task 8: Add Tests for VoidOrderPayments Transaction (TX-003)

**Files:**
- Modify: `service/Api/tests/Module.UnitTests/Payment/Features/Shared/Commands/VoidOrderPaymentsTests.cs` — add 2 test methods

**Interfaces:**
- Consumes: `VoidOrderPaymentsCommandHandler` now uses `BeginTransactionAsync` and fail-fast on unregistered gateway

Note: The in-memory database provider does not support `BeginTransactionAsync`. The existing `ApplicationDbContext` in tests wraps the real `Database.BeginTransactionAsync`. For in-memory DB, this throws `NotSupportedException`. The test must either:
- Mock `IApplicationDbContext` (instead of using real `ApplicationDbContext`)
- Or create a test double that returns a `NoOpTransaction`

Since existing tests use the real `ApplicationDbContext` with in-memory provider, create a separate test class that uses a mock `IApplicationDbContext`.

- [ ] **Step 1: Create test double for in-memory transaction support**

Create file `service/Api/tests/Module.UnitTests/Payment/Features/Shared/Commands/VoidOrderPaymentsTransactionTests.cs`:

```csharp
using System.Data;
using Module.Payment.Domain.PaymentCaptures;
using Module.Payment.Features.Shared.Commands;
using Module.Payment.Services.Provider;
using Module.Payment.Services.Processing;
using Shared.Operational.Persistence.Transactions;
using IPaymentGatewayActionProvider = Module.Payment.Services.Provider.IPaymentGatewayActionProvider;
using IGatewayRegistry = Module.Payment.Services.Provider.IGatewayRegistry;
using IPaymentProcessingService = Module.Payment.Services.Processing.IPaymentProcessingService;
using PaymentProcessingResult = Module.Payment.Services.Processing.PaymentProcessingResult;
using GatewayOptions = Module.Payment.Services.Provider.GatewayOptions;

namespace Module.UnitTests.Payment.Features.Shared.Commands;

[Trait("Category", "Unit")]
[Trait("Module", "Payment")]
[Trait("Feature", "VoidOrderPayments")]
public class VoidOrderPaymentsTransactionTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly Mock<IDatabaseTransaction> _transactionMock;
    private readonly Mock<IApplicationDbContext> _dbContextMock;
    private readonly Mock<IPaymentGatewayActionProvider> _gatewayMock;
    private readonly Mock<IGatewayRegistry> _gatewayRegistryMock;
    private readonly Mock<IPaymentProcessingService> _processingServiceMock;
    private readonly VoidOrderPaymentsCommandHandler _handler;

    public VoidOrderPaymentsTransactionTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(PaymentCapture).Assembly];
        _dbContext = new ApplicationDbContext(options);

        _transactionMock = new Mock<IDatabaseTransaction>();

        _dbContextMock = new Mock<IApplicationDbContext>();
        _dbContextMock.Setup(x => x.Set<PaymentCapture>())
            .Returns(_dbContext.Set<PaymentCapture>());
        _dbContextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(() => _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken));
        _dbContextMock.Setup(x => x.BeginTransactionAsync(It.IsAny<IsolationLevel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_transactionMock.Object);

        _gatewayMock = new Mock<IPaymentGatewayActionProvider>();
        _gatewayMock.Setup(x => x.AutoCapture).Returns(false);
        _gatewayMock.Setup(x => x.PaymentProfilesSupported).Returns(false);

        _gatewayRegistryMock = new Mock<IGatewayRegistry>();
        _gatewayRegistryMock.Setup(x => x.GetGateway(It.IsAny<string>()))
            .Returns(Result<IPaymentGatewayActionProvider>.Ok(_gatewayMock.Object));

        _processingServiceMock = new Mock<IPaymentProcessingService>();
        _handler = new VoidOrderPaymentsCommandHandler(_dbContextMock.Object, _gatewayRegistryMock.Object, _processingServiceMock.Object);
    }

    public void Dispose() { _dbContext.Dispose(); GC.SuppressFinalize(this); }

    [Fact(DisplayName = "Should fail when gateway is not registered")]
    public async Task Handle_ShouldFail_When_GatewayNotRegistered()
    {
        _gatewayRegistryMock
            .Setup(x => x.GetGateway("unknown"))
            .Returns(Result<IPaymentGatewayActionProvider>.Failure(
                Error.NotFound("Gateway.Provider.unknown.NotFound", "No gateway")));

        var orderId = Guid.NewGuid();
        var payment = PaymentCaptureMethod.Create(100m, Guid.NewGuid(), orderId).Value;
        payment.ProviderKey = "unknown";
        _dbContext.Set<PaymentCapture>().Add(payment);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(new VoidOrderPaymentsCommand { OrderId = orderId, Reason = "Cancellation" }, TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        _transactionMock.Verify(x => x.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "Should rollback when void transaction fails")]
    public async Task Handle_ShouldRollback_When_VoidFails()
    {
        _processingServiceMock
            .Setup(x => x.VoidTransactionAsync(It.IsAny<PaymentCapture>(), It.IsAny<IPaymentGatewayActionProvider>(), It.IsAny<GatewayOptions>(), It.IsAny<object?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Error.BadRequest("Gateway.Declined", "Void declined."));

        var orderId = Guid.NewGuid();
        var payment = PaymentCaptureMethod.Create(100m, Guid.NewGuid(), orderId).Value;
        payment.ProviderKey = "bogus";
        payment.ResponseCode = "auth-123";
        _dbContext.Set<PaymentCapture>().Add(payment);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(new VoidOrderPaymentsCommand { OrderId = orderId, Reason = "Cancellation" }, TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        _transactionMock.Verify(x => x.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
```

- [ ] **Step 2: Run new test file**

```bash
dotnet test service/Api/tests/Module.UnitTests --filter "FullyQualifiedName~VoidOrderPaymentsTransaction"
```
Expected: Both tests pass.

---

### Task 9: Full Build and Test Verification

**Files:** None — verification only

- [ ] **Step 1: Full solution build**

```bash
dotnet build
```
Expected: Build succeeded with 0 warnings across all projects.

- [ ] **Step 2: Run all Payment unit tests**

```bash
dotnet test service/Api/tests/Module.UnitTests --filter "FullyQualifiedName~Payment"
```
Expected: All existing + new tests pass (0 failures).

- [ ] **Step 3: Run grep to verify DOC-006**

```bash
rg "idempotent if already completed" service/Api/src/Module/Payment/
```
Expected: 0 matches.

- [ ] **Step 4: Run grep to verify FW-001 implementation**

```bash
rg "result\.IsFailure" service/Api/src/Module/Payment/Backgrounds/ProcessStripeWebhookEventJob.cs
```
Expected: 3 matches (one per handler method).

- [ ] **Step 5: Run grep to verify TX-003 implementation**

```bash
rg "BeginTransactionAsync" service/Api/src/Module/Payment/Features/Shared/Commands/VoidOrderPayments.cs
```
Expected: 1 match. Also verify no `continue` in the handler:

```bash
rg "continue" service/Api/src/Module/Payment/Features/Shared/Commands/VoidOrderPayments.cs
```
Expected: 0 matches.

- [ ] **Step 6: Commit all changes**

```bash
git add service/Api/src/Module/Payment/ service/Api/tests/Module.UnitTests/Payment/
git commit -m "fix(payment): check domain Result in webhook job, nullable FK, transaction scope in batch void, Bogus status override, CancelAsync options passthrough, fix misleading idempotency comment"
```
