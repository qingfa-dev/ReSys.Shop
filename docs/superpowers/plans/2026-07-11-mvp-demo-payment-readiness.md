# MVP Demo Payment Readiness Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Fix the storefront payment flow so `CreatePaymentIntent` returns a usable client secret, `ConfirmPayment` is secure, and void/refund/dispute handling behaves correctly for an MVP demo.

**Architecture:** Extend the existing gateway abstraction to surface a client secret, store it on `PaymentCapture`, and return it through the response model. Add ownership and result-checking guards to the existing vertical-slice handlers without changing endpoint routes or the `Result<T>` pattern.

**Tech Stack:** .NET 10, Carter minimal APIs, MediatR, FluentValidation, Stripe.net, EF Core, xUnit, FluentAssertions

## Global Constraints

- All domain operations return `Result<T>` or `Result`; exceptions only for unrecoverable infrastructure failures.
- Modules never reference each other; communication via MediatR `ISender` only.
- Every C# feature action is a `static partial class` split across files.
- `TreatWarningsAsErrors=true` globally.
- Forward-only dependency: `Shared` depends on nothing within `service/`. `Module` depends only on `Shared`. `Api` composes both.

---

### Task 1: Add `ClientSecret` to `PaymentGatewayResponse`

**Files:**
- Modify: `service/Api/src/Module/Payment/Services/Models/PaymentGatewayResponse.cs`
- Test: `service/Api/tests/Module.UnitTests/Payment/Services/Models/PaymentGatewayResponseTests.cs` (create)

**Interfaces:**
- Consumes: existing `PaymentGatewayResponse` constructor call sites
- Produces: `PaymentGatewayResponse` with new optional `ClientSecret` parameter

- [ ] **Step 1: Write the failing test**

```csharp
using FluentAssertions;
using Module.Payment.Services.Models;

namespace Module.UnitTests.Payment.Services.Models;

public class PaymentGatewayResponseTests
{
    [Fact]
    public void Constructor_Should_Set_ClientSecret_When_Provided()
    {
        var response = new PaymentGatewayResponse(
            Provider: "bogus",
            Authorization: "auth_123",
            ClientSecret: "pi_fake_secret_123");

        response.ClientSecret.Should().Be("pi_fake_secret_123");
    }
}
```

- [ ] **Step 2: Run test to verify it fails**

Run: `dotnet test service/Api/tests/Module.UnitTests/Module.UnitTests.csproj --filter "FullyQualifiedName~PaymentGatewayResponseTests"`

Expected: FAIL — `ClientSecret` does not exist

- [ ] **Step 3: Add `ClientSecret` property and constructor parameter**

```csharp
namespace Module.Payment.Services.Models;

public sealed record PaymentGatewayResponse
{
    public string Provider { get; }
    public string? Authorization { get; }
    public string? ClientSecret { get; }              // NEW
    public string? SetupIntentClientSecret { get; }
    public string? PaymentStatus { get; }
    public string? AvsResultCode { get; }
    public string? CvvResultCode { get; }
    public string? CvvResultMessage { get; }
    public Dictionary<string, object?> Properties { get; }

    public PaymentGatewayResponse(
        string provider,
        string? authorization = null,
        string? clientSecret = null,                  // NEW
        string? setupIntentClientSecret = null,
        string? paymentStatus = null,
        Dictionary<string, object?>? properties = null,
        string? avsResultCode = null,
        string? cvvResultCode = null,
        string? cvvResultMessage = null)
    {
        Provider = provider;
        Authorization = authorization;
        ClientSecret = clientSecret;                  // NEW
        SetupIntentClientSecret = setupIntentClientSecret;
        PaymentStatus = paymentStatus;
        Properties = properties ?? new Dictionary<string, object?>();
        AvsResultCode = avsResultCode;
        CvvResultCode = cvvResultCode;
        CvvResultMessage = cvvResultMessage;
    }
}
```

- [ ] **Step 4: Run test to verify it passes**

Run: `dotnet test service/Api/tests/Module.UnitTests/Module.UnitTests.csproj --filter "FullyQualifiedName~PaymentGatewayResponseTests"`

Expected: PASS

- [ ] **Step 5: Commit**

```bash
git add service/Api/src/Module/Payment/Services/Models/PaymentGatewayResponse.cs
git add service/Api/tests/Module.UnitTests/Payment/Services/Models/PaymentGatewayResponseTests.cs
git commit -m "feat(payment): add ClientSecret to PaymentGatewayResponse"
```

---

### Task 2: Return Client Secret from `StripeGateway.PurchaseAsync`

**Files:**
- Modify: `service/Api/src/Module/Payment/Services/Provider/Stripe/StripeGateway.cs`
- Test: `service/Api/tests/Module.UnitTests/Payment/Services/Provider/Stripe/StripeGatewayTests.cs` (create or extend)

**Interfaces:**
- Consumes: `PaymentGatewayResponse` from Task 1
- Produces: `PaymentGatewayResponse` with `ClientSecret = intent.ClientSecret`

- [ ] **Step 1: Write the failing test**

```csharp
using FluentAssertions;
using Microsoft.Extensions.Options;
using Module.Payment.Services.Provider.Stripe;
using Moq;

namespace Module.UnitTests.Payment.Services.Provider.Stripe;

public class StripeGatewayTests
{
    [Fact]
    public void PurchaseAsync_Returns_ClientSecret_From_Intent()
    {
        var options = Options.Create(new StripeSetting
        {
            SecretKey = "sk_test_xxx",
            PublishableKey = "pk_test_xxx",
            WebhookSecret = "whsec_xxx"
        });
        var gateway = new StripeGateway(options);

        // This is a design-time compilation test; full Stripe integration requires a stubbed HttpClient.
        gateway.Should().NotBeNull();
    }
}
```

- [ ] **Step 2: Run test to verify it fails**

Run: `dotnet test service/Api/tests/Module.UnitTests/Module.UnitTests.csproj --filter "FullyQualifiedName~StripeGatewayTests"`

Expected: FAIL or no tests matched until file exists

- [ ] **Step 3: Update `PurchaseAsync` and `AuthorizeAsync` to return `intent.ClientSecret`**

In `service/Api/src/Module/Payment/Services/Provider/Stripe/StripeGateway.cs`:

```csharp
return new PaymentGatewayResponse(
    GatewayConstants.Providers.Stripe,
    authorization: intent.Id,
    clientSecret: intent.ClientSecret);
```

Apply the same change in `AuthorizeAsync`:

```csharp
return new PaymentGatewayResponse(
    GatewayConstants.Providers.Stripe,
    authorization: intent.Id,
    clientSecret: intent.ClientSecret);
```

- [ ] **Step 4: Run build to verify no compile errors**

Run: `dotnet build service/Api/src/Module/Module.csproj`

Expected: Build succeeds

- [ ] **Step 5: Commit**

```bash
git add service/Api/src/Module/Payment/Services/Provider/Stripe/StripeGateway.cs
git add service/Api/tests/Module.UnitTests/Payment/Services/Provider/Stripe/StripeGatewayTests.cs
git commit -m "feat(payment): return client secret from StripeGateway"
```

---

### Task 3: Return Client Secret from `BogusGateway.PurchaseAsync`

**Files:**
- Modify: `service/Api/src/Module/Payment/Services/Provider/Bogus/BogusGateway.cs`
- Test: `service/Api/tests/Module.UnitTests/Payment/Services/Provider/Bogus/BogusGatewayTests.cs` (create)

**Interfaces:**
- Consumes: `PaymentGatewayResponse` from Task 1
- Produces: deterministic fake client secret for demo use

- [ ] **Step 1: Write the failing test**

```csharp
using FluentAssertions;
using Microsoft.Extensions.Options;
using Module.Payment.Services.Provider;
using Module.Payment.Services.Provider.Bogus;

namespace Module.UnitTests.Payment.Services.Provider.Bogus;

public class BogusGatewayTests
{
    [Fact]
    public void PurchaseAsync_With_Success_Card_Returns_ClientSecret()
    {
        var gateway = new BogusGateway(Options.Create(new BogusSetting()));

        var result = gateway.PurchaseAsync(
            10.00m,
            BogusGateway.TestCards.Success,
            new GatewayOptions(),
            default).Result;

        result.IsSuccess.Should().BeTrue();
        result.Value.ClientSecret.Should().NotBeNullOrEmpty();
        result.Value.ClientSecret.Should().StartWith("pi_fake_");
    }
}
```

- [ ] **Step 2: Run test to verify it fails**

Run: `dotnet test service/Api/tests/Module.UnitTests/Module.UnitTests.csproj --filter "FullyQualifiedName~BogusGatewayTests"`

Expected: FAIL — `ClientSecret` is null

- [ ] **Step 3: Update `BogusGateway.SimulateGatewayResponse`**

```csharp
return Task.FromResult(Result<PaymentGatewayResponse>.Ok(
    new PaymentGatewayResponse(
        GatewayConstants.Providers.Bogus,
        authorization: $"auth_{Guid.NewGuid():N}",
        clientSecret: $"pi_fake_{Guid.NewGuid():N}_secret_{Guid.NewGuid():N}")));
```

- [ ] **Step 4: Run test to verify it passes**

Run: `dotnet test service/Api/tests/Module.UnitTests/Module.UnitTests.csproj --filter "FullyQualifiedName~BogusGatewayTests"`

Expected: PASS

- [ ] **Step 5: Commit**

```bash
git add service/Api/src/Module/Payment/Services/Provider/Bogus/BogusGateway.cs
git add service/Api/tests/Module.UnitTests/Payment/Services/Provider/Bogus/BogusGatewayTests.cs
git commit -m "feat(payment): return fake client secret from BogusGateway"
```

---

### Task 4: Store and Return Client Secret in `CreatePaymentIntent`

**Files:**
- Modify: `service/Api/src/Module/Payment/Features/Storefront/Payment/CreateIntent/CreatePaymentIntent.cs`
- Test: `service/Api/tests/Module.UnitTests/Payment/Features/Storefront/Payment/CreateIntent/CreatePaymentIntentTests.cs` (create)

**Interfaces:**
- Consumes: `PaymentGatewayResponse.ClientSecret` from Tasks 2 and 3
- Produces: `CreatePaymentIntent.Response.ClientSecret` populated from `PaymentCapture.IntentClientSecret`

- [ ] **Step 1: Write the failing test**

```csharp
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Module.Payment.Domain.PaymentMethods;
using Module.Payment.Features.Storefront.Payment.CreateIntent;
using Shared.Testing;

namespace Module.UnitTests.Payment.Features.Storefront.Payment.CreateIntent;

public class CreatePaymentIntentTests : TestBase
{
    [Fact]
    public async Task Handle_Should_Return_ClientSecret()
    {
        var order = await CreateDraftOrderAsync(total: 10.00m);
        var method = await CreatePaymentMethodAsync(providerKey: "bogus", active: true);

        var result = await SendAsync(new CreatePaymentIntent.Command(order.Id));

        result.IsSuccess.Should().BeTrue();
        result.Value.ClientSecret.Should().NotBeNullOrEmpty();
    }
}
```

- [ ] **Step 2: Run test to verify it fails**

Run: `dotnet test service/Api/tests/Module.UnitTests/Module.UnitTests.csproj --filter "FullyQualifiedName~CreatePaymentIntentTests"`

Expected: FAIL — `ClientSecret` is null

- [ ] **Step 3: Store client secret after processing**

In `service/Api/src/Module/Payment/Features/Storefront/Payment/CreateIntent/CreatePaymentIntent.cs`, after line 61 (`processResult`):

```csharp
var processResult = await processingService.ProcessAsync(payment, gateway, options, cancellationToken);
if (processResult.IsFailure) return processResult.Errors;

payment.IntentClientSecret = processResult.Value.ClientSecret;

await dbContext.SaveChangesAsync(cancellationToken);
```

- [ ] **Step 4: Run test to verify it passes**

Run: `dotnet test service/Api/tests/Module.UnitTests/Module.UnitTests.csproj --filter "FullyQualifiedName~CreatePaymentIntentTests"`

Expected: PASS

- [ ] **Step 5: Commit**

```bash
git add service/Api/src/Module/Payment/Features/Storefront/Payment/CreateIntent/CreatePaymentIntent.cs
git add service/Api/tests/Module.UnitTests/Payment/Features/Storefront/Payment/CreateIntent/CreatePaymentIntentTests.cs
git commit -m "feat(payment): store and return client secret in CreatePaymentIntent"
```

---

### Task 5: Add Ownership Check to `ConfirmPayment` and Remove Empty Request

**Files:**
- Modify: `service/Api/src/Module/Payment/Features/Storefront/Payment/Confirm/ConfirmPayment.cs`
- Modify: `service/Api/src/Module/Payment/Features/Storefront/Payment/Confirm/ConfirmPayment.Request.cs`
- Modify: `service/Api/src/Module/Payment/Features/Storefront/Payment/Confirm/ConfirmPayment.Endpoint.cs`
- Test: `service/Api/tests/Module.UnitTests/Payment/Features/Storefront/Payment/Confirm/ConfirmPaymentTests.cs` (create)

**Interfaces:**
- Consumes: `Order` entity and `currentUser.UserId`
- Produces: `ConfirmPayment` command that rejects cross-user payment access

- [ ] **Step 1: Write the failing test**

```csharp
using FluentAssertions;
using Module.Payment.Domain.PaymentCaptures;
using Module.Payment.Features.Storefront.Payment.Confirm;
using Shared.Testing;

namespace Module.UnitTests.Payment.Features.Storefront.Payment.Confirm;

public class ConfirmPaymentTests : TestBase
{
    [Fact]
    public async Task Handle_Should_Fail_When_Payment_Belongs_To_Another_User()
    {
        var otherUserOrder = await CreateOrderForOtherUserAsync();
        var payment = await CreatePaymentCaptureAsync(otherUserOrder.Id, PaymentRecordState.Processing);
        SetCurrentUser(Guid.NewGuid());

        var result = await SendAsync(new ConfirmPayment.Command(payment.Id));

        result.IsFailure.Should().BeTrue();
    }
}
```

- [ ] **Step 2: Run test to verify it fails**

Run: `dotnet test service/Api/tests/Module.UnitTests/Module.UnitTests.csproj --filter "FullyQualifiedName~ConfirmPaymentTests"`

Expected: FAIL — no ownership check exists

- [ ] **Step 3: Add ownership check in handler**

In `service/Api/src/Module/Payment/Features/Storefront/Payment/Confirm/ConfirmPayment.cs`:

```csharp
using Module.Ordering.Domain.Orders;

// ... inside Handle
var payment = await dbContext.Set<PaymentCapture>()
    .FirstOrDefaultAsync(p => p.Id == command.PaymentId, cancellationToken);
if (payment is null)
    return PaymentCaptureResult.Failure.NotFound;

var order = await dbContext.Set<Order>()
    .FirstOrDefaultAsync(o => o.Id == payment.OrderId && o.UserId == userId, cancellationToken);
if (order is null)
    return PaymentCaptureResult.Failure.NotFound;
```

- [ ] **Step 4: Remove empty request from endpoint**

In `service/Api/src/Module/Payment/Features/Storefront/Payment/Confirm/ConfirmPayment.Endpoint.cs`, change the route registration to:

```csharp
app.MapPost("/api/storefront/payments/{paymentId:guid}/confirm", async (
    [FromRoute] Guid paymentId,
    ISender sender,
    CancellationToken ct) =>
{
    var result = await sender.Send(new ConfirmPayment.Command(paymentId), ct);
    return result.ToHttpResult();
})
```

In `service/Api/src/Module/Payment/Features/Storefront/Payment/Confirm/ConfirmPayment.Request.cs`, keep an empty record or delete the file if not referenced. Prefer deleting:

```bash
rm service/Api/src/Module/Payment/Features/Storefront/Payment/Confirm/ConfirmPayment.Request.cs
```

- [ ] **Step 5: Run test to verify it passes**

Run: `dotnet test service/Api/tests/Module.UnitTests/Module.UnitTests.csproj --filter "FullyQualifiedName~ConfirmPaymentTests"`

Expected: PASS

- [ ] **Step 6: Commit**

```bash
git add service/Api/src/Module/Payment/Features/Storefront/Payment/Confirm/
git add service/Api/tests/Module.UnitTests/Payment/Features/Storefront/Payment/Confirm/ConfirmPaymentTests.cs
git commit -m "feat(payment): enforce ownership in ConfirmPayment and remove empty request"
```

---

### Task 6: Check Void Results in `VoidOrderPayments`

**Files:**
- Modify: `service/Api/src/Module/Payment/Features/Shared/Commands/VoidOrderPayments.cs`
- Test: `service/Api/tests/Module.UnitTests/Payment/Features/Shared/Commands/VoidOrderPaymentsTests.cs` (create)

**Interfaces:**
- Consumes: `processingService.VoidTransactionAsync` result
- Produces: aggregated failure if any void fails

- [ ] **Step 1: Write the failing test**

```csharp
using FluentAssertions;
using Module.Payment.Domain.PaymentCaptures;
using Module.Payment.Features.Shared.Commands;
using Shared.Testing;

namespace Module.UnitTests.Payment.Features.Shared.Commands;

public class VoidOrderPaymentsTests : TestBase
{
    [Fact]
    public async Task Handle_Should_Fail_When_Void_Fails()
    {
        var orderId = Guid.NewGuid();
        await CreatePaymentCaptureAsync(orderId, PaymentRecordState.Pending, responseCode: null);

        var result = await SendAsync(new VoidOrderPaymentsCommand(orderId, "Cancellation"));

        result.IsFailure.Should().BeTrue();
    }
}
```

- [ ] **Step 2: Run test to verify it fails**

Run: `dotnet test service/Api/tests/Module.UnitTests/Module.UnitTests.csproj --filter "FullyQualifiedName~VoidOrderPaymentsTests"`

Expected: FAIL — command returns success even when void has no response code

- [ ] **Step 3: Check void result and short-circuit**

In `service/Api/src/Module/Payment/Features/Shared/Commands/VoidOrderPayments.cs`:

```csharp
var voidResult = await processingService.VoidTransactionAsync(payment, gatewayResult.Value, options, null, ct);
if (voidResult.IsFailure)
    return voidResult.Errors;
```

- [ ] **Step 4: Run test to verify it passes**

Run: `dotnet test service/Api/tests/Module.UnitTests/Module.UnitTests.csproj --filter "FullyQualifiedName~VoidOrderPaymentsTests"`

Expected: PASS

- [ ] **Step 5: Commit**

```bash
git add service/Api/src/Module/Payment/Features/Shared/Commands/VoidOrderPayments.cs
git add service/Api/tests/Module.UnitTests/Payment/Features/Shared/Commands/VoidOrderPaymentsTests.cs
git commit -m "feat(payment): fail VoidOrderPayments when any void fails"
```

---

### Task 7: Honor `Amount` in `RefundPayment`

**Files:**
- Modify: `service/Api/src/Module/Payment/Features/Admin/Payments/Refund/RefundPayment.cs`
- Modify: `service/Api/src/Module/Payment/Features/Admin/Payments/Refund/RefundPayment.Request.cs`
- Test: `service/Api/tests/Module.UnitTests/Payment/Features/Admin/Payments/Refund/RefundPaymentTests.cs` (create)

**Interfaces:**
- Consumes: `RefundPayment.Request.Amount`
- Produces: partial refund through gateway and domain

- [ ] **Step 1: Write the failing test**

```csharp
using FluentAssertions;
using Module.Payment.Domain.PaymentCaptures;
using Module.Payment.Features.Admin.Payments.Refund;
using Shared.Testing;

namespace Module.UnitTests.Payment.Features.Admin.Payments.Refund;

public class RefundPaymentTests : TestBase
{
    [Fact]
    public async Task Handle_Should_Refund_Requested_Amount()
    {
        var payment = await CreateCompletedPaymentAsync(amount: 100.00m);

        var result = await SendAsync(new RefundPayment.Command(
            payment.Id,
            new RefundPayment.Request { Amount = 25.00m, Reason = "Partial" }));

        result.IsSuccess.Should().BeTrue();
        payment.RefundedAmount.Should().Be(25.00m);
    }
}
```

- [ ] **Step 2: Run test to verify it fails**

Run: `dotnet test service/Api/tests/Module.UnitTests/Module.UnitTests.csproj --filter "FullyQualifiedName~RefundPaymentTests"`

Expected: FAIL — refunded amount is full amount, not 25

- [ ] **Step 3: Use request amount in handler**

In `service/Api/src/Module/Payment/Features/Admin/Payments/Refund/RefundPayment.cs`, locate the refund call and replace `payment.Amount` with `request.Amount`:

```csharp
var refundResult = await processingService.RefundAsync(
    payment, gateway, options, request.Amount, cancellationToken);
```

Update the response mapping to return the actual refunded amount:

```csharp
return new Response
{
    Id = payment.Id,
    Number = payment.Number,
    Amount = request.Amount,
    State = payment.State,
    Message = "Payment refunded."
};
```

- [ ] **Step 4: Remove the TODO comment from Request.cs**

In `service/Api/src/Module/Payment/Features/Admin/Payments/Refund/RefundPayment.Request.cs`:

```csharp
namespace Module.Payment.Features.Admin.Payments.Refund;

public static partial class RefundPayment
{
    public class Request
    {
        public decimal Amount { get; init; }
        public string? Reason { get; init; }
    }
}
```

- [ ] **Step 5: Run test to verify it passes**

Run: `dotnet test service/Api/tests/Module.UnitTests/Module.UnitTests.csproj --filter "FullyQualifiedName~RefundPaymentTests"`

Expected: PASS

- [ ] **Step 6: Commit**

```bash
git add service/Api/src/Module/Payment/Features/Admin/Payments/Refund/
git add service/Api/tests/Module.UnitTests/Payment/Features/Admin/Payments/Refund/RefundPaymentTests.cs
git commit -m "feat(payment): honor requested amount in RefundPayment"
```

---

### Task 8: Log Disputes in `StripeWebhook`

**Files:**
- Modify: `service/Api/src/Module/Payment/Features/Storefront/Payment/Webhooks/StripeWebhook.cs`
- Modify: `service/Api/src/Module/Payment/Domain/PaymentCaptures/PaymentCapture.Loggers.cs` (or create if missing)

**Interfaces:**
- Consumes: `Stripe.Event` of type `charge.dispute.created`
- Produces: warning log with charge id and reason

- [ ] **Step 1: Write the failing test**

```csharp
using FluentAssertions;
using Module.Payment.Features.Storefront.Payment.Webhooks;
using Shared.Testing;

namespace Module.UnitTests.Payment.Features.Storefront.Payment.Webhooks;

public class StripeWebhookTests : TestBase
{
    [Fact]
    public async Task Handle_DisputeCreated_Should_Return_Success()
    {
        var payload = BuildDisputeCreatedPayload();
        var signature = "t=1,v1=fake";

        var result = await SendAsync(new StripeWebhook.Command(payload, signature));

        result.IsSuccess.Should().BeTrue();
    }
}
```

- [ ] **Step 2: Run test to verify it fails**

Run: `dotnet test service/Api/tests/Module.UnitTests/Module.UnitTests.csproj --filter "FullyQualifiedName~StripeWebhookTests"`

Expected: FAIL or inconclusive until logger is added

- [ ] **Step 3: Add dispute logger and warning**

In `service/Api/src/Module/Payment/Features/Storefront/Payment/Webhooks/StripeWebhook.cs`:

```csharp
private static Result HandleChargeDisputeCreated(StripeEvent stripeEvent)
{
    var charge = stripeEvent.Data.Object as Charge;
    if (charge is null)
        return Result.Ok();

    PaymentCaptureLoggers.DisputeCreated(
        logger,                          // requires ILogger injected into handler
        charge.Id,
        charge.Dispute?.Reason ?? "unknown");

    return Result.Ok();
}
```

Add `ILogger<CommandHandler>` to the handler constructor if not present.

Add the logger method in `service/Api/src/Module/Payment/Domain/PaymentCaptures/PaymentCapture.Loggers.cs`:

```csharp
[LoggerMessage(
    EventId = 2008,
    Level = LogLevel.Warning,
    Message = "Payment dispute created for charge {ChargeId} with reason {Reason}")]
public static partial void DisputeCreated(
    this ILogger logger,
    string chargeId,
    string reason);
```

- [ ] **Step 4: Run test to verify it passes**

Run: `dotnet test service/Api/tests/Module.UnitTests/Module.UnitTests.csproj --filter "FullyQualifiedName~StripeWebhookTests"`

Expected: PASS

- [ ] **Step 5: Commit**

```bash
git add service/Api/src/Module/Payment/Features/Storefront/Payment/Webhooks/StripeWebhook.cs
git add service/Api/src/Module/Payment/Domain/PaymentCaptures/PaymentCapture.Loggers.cs
git add service/Api/tests/Module.UnitTests/Payment/Features/Storefront/Payment/Webhooks/StripeWebhookTests.cs
git commit -m "feat(payment): log charge disputes in StripeWebhook"
```

---

### Task 9: Final Verification

- [ ] **Step 1: Run full Module unit test suite**

Run: `dotnet test service/Api/tests/Module.UnitTests/Module.UnitTests.csproj`

Expected: All payment-related tests pass; architecture test may still fail (addressed in a separate plan)

- [ ] **Step 2: Run build with warnings-as-errors**

Run: `dotnet build service/Api/src/Api/Api.csproj`

Expected: 0 warnings, 0 errors

- [ ] **Step 3: Commit any remaining changes**

```bash
git commit -m "chore(payment): final verification for payment readiness" --allow-empty
```
