---
title: MVP Demo Readiness Remediation — Critical API Service Fixes
date_created: 2026-07-11
owner: ReSys.Shop Team
tags: [process, mvp, api, payment, identity, ordering, architecture]
---

# Introduction

This specification defines the remediation work required to make the ReSys.Shop API services ready for an MVP demo. It is derived from a focused code review of the API surface and addresses only the highest-impact blockers: architectural rule violations, broken user flows, payment integration gaps, security configuration risks, and functional stubs that prevent a successful end-to-end demo.

## 1. Purpose & Scope

**Purpose:** Provide a self-contained, implementation-ready plan that brings the API from its current state to MVP demo readiness.

**Scope:**
- `service/Api/src/Module/` — Identity, Ordering, Payment, Profile modules
- `service/Api/src/Shared/` — Storage providers, Governance helpers
- `service/Api/src/Api/` — Host startup configuration
- `service/Api/tests/Module.UnitTests/` — Architecture tests

**Audience:** AI coding agents and human reviewers implementing the fixes.

**Assumptions:**
- Existing codebase conventions remain unchanged: vertical slices, `Result<T>`, MediatR CQRS, Carter endpoints, FluentValidation.
- The demo targets the `Bogus` payment gateway in development, with Stripe as the production target.
- Modules must not reference each other directly (AGENTS.md rule #2).

## 2. Definitions

| Term | Definition |
|------|-----------|
| **MVP** | Minimum Viable Product — the smallest set of features that can be demonstrated end-to-end. |
| **Vertical slice** | A feature implemented as a cohesive set of files (Request, Response, Command, Handler, Endpoint, Validator) in a single folder. |
| **Module isolation** | The architectural rule that business modules in `service/Api/src/Module/` must not have compile-time dependencies on each other. |
| **TOCTOU** | Time-of-Check-Time-of-Use race condition where a value is checked and then used in two non-atomic steps. |
| **`Result<T>`** | A readonly record struct representing success (`Value`) or failure (`Errors`), with implicit conversions from `T`, `Error`, and `Error[]`. |
| **Client secret** | In Stripe/Bogus flows, the short-lived secret returned to the frontend to confirm a payment intent. |

## 3. Requirements, Constraints & Guidelines

### 3.1 Architecture & Module Isolation

- **ARC-001**: The `ModuleIsolationTests.ModuleTypes_ShouldNotCrossReferenceOtherModules` test MUST pass. Direct type references between modules are forbidden.
- **ARC-002**: `Module.Catalog.Domain.Products.Variants.Variant` MUST NOT reference `Module.Inventory.Domain.StockLocations.StockItems.StockItem`. Use a shared identifier (`VariantId`) instead of a navigation property.
- **ARC-003**: `Module.Inventory.Domain.StockLocations.StockItems.StockItem` MUST NOT reference `Module.Catalog.Domain.Products.Variants.Variant`. Use `VariantId` only.
- **ARC-004**: `Module.Ordering.Domain.Orders.Order` MUST NOT reference `Module.Payment.Domain.PaymentCaptures.PaymentCapture`. Use `OrderId` lookup in handlers.
- **ARC-005**: `Module.Ordering.Domain.LineItems.LineItem` MUST NOT reference `Module.Catalog.Domain.Products.Variants.Variant`. Use `VariantId` only.
- **ARC-006**: `Module.Identity` MUST NOT reference `Module.Profile` types. Cross-module commands MUST be sent via `IMediator` using request/response records defined in the caller or `Shared`.
- **ARC-007**: `Module.Ordering` handlers that need inventory behavior MUST depend on an abstraction in `Shared` (e.g., `IStockQuantityService`) or use `IMediator`, not on concrete `Module.Inventory` services.

### 3.2 Payment Flow

- **PAY-001**: `CreatePaymentIntent` MUST return a non-null `ClientSecret` so the frontend can complete the payment.
- **PAY-002**: `StripeGateway.PurchaseAsync` MUST capture and return `intent.ClientSecret` in the `PaymentGatewayResponse`.
- **PAY-003**: `BogusGateway.PurchaseAsync` MUST return a deterministic fake client secret (e.g., `pi_fake_{guid}_secret_{guid}`) so the demo UI has a secret to confirm.
- **PAY-004**: `PaymentGatewayResponse` MUST expose a `ClientSecret` property if not already present.
- **PAY-005**: `CreatePaymentIntent` MUST store the returned client secret on `PaymentCapture.IntentClientSecret` before saving.
- **PAY-006**: `ConfirmPayment` MUST verify the payment belongs to the current user by resolving its order and checking `order.UserId == currentUserId`.
- **PAY-007**: `ConfirmPayment` endpoint MUST NOT require an empty `[FromBody] Request` parameter.
- **PAY-008**: `VoidOrderPayments` MUST check the result of each `VoidTransactionAsync` call and fail the command if any void fails.
- **PAY-009**: `RefundPayment` MUST honor the `Amount` request property for partial refunds instead of always refunding the full amount.
- **PAY-010**: `StripeWebhook.HandleChargeDisputeCreated` MUST log a warning and optionally suspend the order; silently returning `Result.Ok()` is not acceptable for demo observability.

### 3.3 Identity & Registration

- **IDN-001**: `EmailRegister.BuildVerificationPath` and `ConfirmEmail` MUST use the same Base64 variant. Both MUST use URL-safe Base64 (`ToBase64Url` / `TryFromBase64Url`).
- **IDN-002**: `PasswordLogin.FindUserByCredentialAsync` MUST NOT load all users into memory to match phone number. It MUST query the store directly via `FindByPhoneNumberAsync` or an indexed `Where(...).FirstOrDefaultAsync`.
- **IDN-003**: `EmailRegister` SHOULD use `FindByNameAsync` with Identity's normalized username instead of `FindByNameAsync(trimmedUsername.ToLowerInvariant())`.

### 3.4 Ordering & Stock

- **ORD-001**: `CreateOrderFromCart` MUST make stock availability checks and stock deductions atomic to prevent overselling. Use `ExecuteUpdateAsync` with a `WHERE CountOnHand >= @qty` guard or a serializable transaction with row locks.
- **ORD-002**: `AddToCart` MUST NOT hardcode `"USD"` or `Guid.Empty` when creating a new cart. Currency MUST come from configuration; the cart creation factory MUST allow a null/default ship address.
- **ORD-003**: `Order.Checkout.AssignDefaultAddresses` MUST resolve the user's default billing and shipping addresses from the Profile module via `IMediator` or leave the order in a state that requires explicit address selection.
- **ORD-004**: `EnsureLineItemVariantsAreNotDiscontinued` MUST query the catalog for discontinued status or be removed; returning `true` unconditionally is a functional bug.

### 3.5 Storage & Infrastructure

- **INF-001**: `S3StorageProvider` MUST either be fully implemented using the AWS SDK for .NET or explicitly disabled when `S3` is selected as the default provider. Returning fake success for upload and `NotImplemented` for download is inconsistent and demo-breaking.
- **INF-002**: `Program.cs` MUST NOT run EF Core migrations automatically in Production. Migrations in Production MUST be opt-in via configuration or run from a separate deployment step.
- **INF-003**: `appsettings.Development.json` MUST NOT contain a hardcoded JWT secret. The secret MUST be supplied via user secrets, environment variables, or a secret manager.

### 3.6 Code Quality

- **QAL-001**: Duplicate `StripeGateway`, `PaymentProcessingService`, and `StripeWebhookHandler` implementations MUST be consolidated. Only one canonical implementation per type may remain.
- **QAL-002**: Obsolete or always-true domain guards (`EnsureLineItemsAreInStock`, `EnsureAvailableShippingRates`) MUST be removed or implemented.

## 4. Interfaces & Data Contracts

### 4.1 Payment Gateway Response

The `PaymentGatewayResponse` record MUST include a client secret field:

```csharp
public sealed record PaymentGatewayResponse(
    string Provider,
    string? Authorization = null,
    string? ClientSecret = null,        // NEW
    string? SetupIntentClientSecret = null,
    string? PaymentStatus = null,
    Dictionary<string, object?>? Properties = null,
    string? AvsResultCode = null,
    string? CvvResultCode = null,
    string? CvvResultMessage = null);
```

### 4.2 Create Payment Intent Response

`CreatePaymentIntent.Response` (inherits `PaymentDetailResponse`) MUST expose `ClientSecret` populated from `PaymentCapture.IntentClientSecret`.

### 4.3 Cross-Module Profile Creation

Identity `ConfirmEmail` MUST dispatch a MediatR command instead of referencing `Module.Profile` directly:

```csharp
public sealed record CreateUserProfileCommand(
    Guid UserId,
    string FirstName,
    string? LastName,
    string Email) : ICommand<CreateUserProfileResult>;

public sealed record CreateUserProfileResult(Guid ProfileId);
```

The handler for this command lives in the Profile module. Identity only references the command type from `Shared.Application` or its own feature namespace.

### 4.4 Atomic Stock Adjustment

Stock deduction MUST use a single atomic statement. Example shape:

```csharp
var updated = await dbContext.Set<StockItem>()
    .Where(si => si.Id == stockItemId && si.CountOnHand >= quantity)
    .ExecuteUpdateAsync(setters => setters
        .SetProperty(si => si.CountOnHand, si => si.CountOnHand - quantity)
        .SetProperty(si => si.ModifiedAtUtc, DateTimeOffset.UtcNow),
        cancellationToken);

if (updated == 0)
    return StockItemResult.Errors.InsufficientStock;
```

## 5. Acceptance Criteria

- **AC-001**: Given the architecture test `ModuleTypes_ShouldNotCrossReferenceOtherModules`, when `dotnet test service/Api/tests/Module.UnitTests` runs, then it passes with zero violations.
- **AC-002**: Given a registered user with a cart, when `CreatePaymentIntent` is called, then the response contains a non-null, non-empty `ClientSecret`.
- **AC-003**: Given a newly registered user, when they click the email verification link, then `ConfirmEmail` succeeds and a profile is created.
- **AC-004**: Given a payment intent created by user A, when user B calls `ConfirmPayment` with that payment id, then the endpoint returns a `NotFound` or `Forbidden` error.
- **AC-005**: Given an order with captured payments, when `VoidOrderPayments` is invoked and the gateway void fails, then the command returns failure and no order state is updated.
- **AC-006**: Given a completed payment of $100, when an admin refunds $25, then the payment `RefundedAmount` is $25 and only $25 is refunded through the gateway.
- **AC-007**: Given a cart with 1 item in stock, when two concurrent checkout requests are made, then only one succeeds and total stock sold does not exceed availability.
- **AC-008**: Given `ASPNETCORE_ENVIRONMENT=Production`, when the API starts, then pending EF Core migrations are NOT applied unless explicitly opted in via configuration.
- **AC-009**: Given S3 is configured as the active storage provider, when a file is uploaded and then downloaded, then the downloaded bytes match the uploaded bytes.
- **AC-010**: Given duplicate `StripeGateway`/`PaymentProcessingService`/`StripeWebhookHandler` files exist before remediation, when the build runs, then only one implementation per type compiles and the stale files are removed.

## 6. Test Automation Strategy

- **Unit tests**: Update `Module.UnitTests.Architecture.ModuleIsolationTests` to continue enforcing module isolation as the canonical gate.
- **Integration tests**: Add tests for `CreatePaymentIntent` → `ConfirmPayment` using the `Bogus` gateway to verify client secret round-trip.
- **Integration tests**: Add a concurrent checkout test using a real PostgreSQL container to validate atomic stock deduction.
- **Manual demo tests**: Walk through registration, email confirmation, login, add-to-cart, checkout, payment intent creation, payment confirmation, and order placement.

## 7. Rationale & Context

The current API builds successfully but contains multiple blockers that would surface during a live MVP demo: email verification fails due to an encoding mismatch, payments return no client secret, stock can be oversold under concurrency, and the architecture test that enforces the project's most important modularity rule is failing. Consolidating these fixes into one specification prevents piecemeal patches that reintroduce cross-module coupling or leave stubbed paths half-implemented.

## 8. Dependencies & External Integrations

- **EXT-001**: PostgreSQL — required for integration tests that validate atomic stock operations (`ExecuteUpdateAsync` is not supported by EF Core InMemory).
- **EXT-002**: Stripe API — required for production payment processing; demo can use the `Bogus` gateway.
- **EXT-003**: S3-compatible object store (MinIO, AWS S3, Wasabi) — required if S3 is selected as the active storage provider.
- **EXT-004**: SMTP or SendGrid — required for email verification and order confirmation notifications.

## 9. Examples & Edge Cases

### 9.1 Bogus Gateway Client Secret

```csharp
// BogusGateway.PurchaseAsync
return Task.FromResult(Result<PaymentGatewayResponse>.Ok(
    new PaymentGatewayResponse(
        GatewayConstants.Providers.Bogus,
        authorization: $"auth_{Guid.NewGuid():N}",
        clientSecret: $"pi_fake_{Guid.NewGuid():N}_secret_{Guid.NewGuid():N}")));
```

### 9.2 Stripe Gateway Client Secret

```csharp
// StripeGateway.PurchaseAsync
return new PaymentGatewayResponse(
    GatewayConstants.Providers.Stripe,
    authorization: intent.Id,
    clientSecret: intent.ClientSecret);
```

### 9.3 ConfirmPayment Ownership Check

```csharp
var order = await dbContext.Set<Order>()
    .FirstOrDefaultAsync(o => o.Id == payment.OrderId && o.UserId == userId, cancellationToken);

if (order is null)
    return PaymentCaptureResult.Failure.NotFound;
```

## 10. Validation Criteria

- `dotnet build service/Api/src/Api/Api.csproj` succeeds with zero warnings.
- `dotnet test service/Api/tests/Module.UnitTests/Module.UnitTests.csproj` passes.
- `dotnet test service/Api/tests/Shared.UnitTests/Shared.UnitTests.csproj` passes.
- A manual end-to-end demo script executes successfully:
  1. Register user
  2. Confirm email
  3. Login
  4. Add product to cart
  5. Create payment intent
  6. Confirm payment
  7. Place order

## 11. Related Specifications / Further Reading

- `spec/spec-architecture-mvp-hardening.md` — broader hardening specification covering 82 findings across all modules
- `docs/codebase/ARCHITECTURE.md` — detailed architecture, layer responsibilities, data flow
- `docs/codebase/CONVENTIONS.md` — coding conventions
- `AGENTS.md` — project-specific agent instructions and non-negotiable rules
