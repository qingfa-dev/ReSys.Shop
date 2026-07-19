# Stripe Payment Integration Bug Fixes and Legacy Cleanup Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Fix state transition validator error messages, remove dead webhook code, delete 4 legacy duplicate classes, retag fake-Stripe-API unit tests, and correct README docs.

**Architecture:** Each task is a single-file change or deletion with independent testability. No new files created. All changes confined to `service/Api/src/Module/Payment/` and `service/Api/tests/Module.UnitTests/Payment/`.

**Tech Stack:** .NET 10, Stripe.net, xUnit, FluentAssertions, Moq.

## Global Constraints

- `TreatWarningsAsErrors=true` — zero warnings on `dotnet build`
- Result objects, not exceptions — domain failures return `Result.IsFailure`, never throw
- Modules never reference each other — all changes confined to Payment module
- No breaking interface changes to `IWebhookHandler` — delete the legacy implementation instead of changing the interface

---

### Task 1: Fix State Transition Validator Error Message (TRN-002)

**Files:**
- Modify: `service/Api/src/Module/Payment/Domain/PaymentCaptures/PaymentCapture.Validation.cs:23-24`

**Interfaces:**
- Consumes: `PaymentCaptureResult.Failure.InvalidStateTransition(PaymentRecordState from, PaymentRecordState to)` → `Error`
- Produces: No signature change — behavior only (error message now correctly reports target state)

- [ ] **Step 1: Fix the arguments to InvalidStateTransition**

Open `PaymentCapture.Validation.cs`. Change lines 23-24 from:

```csharp
.WithErrorCode(PaymentCaptureResult.Failure.InvalidStateTransition(currentState, currentState).Code)
.WithMessage(PaymentCaptureResult.Failure.InvalidStateTransition(currentState, currentState).Message);
```

to:

```csharp
.WithErrorCode(PaymentCaptureResult.Failure.InvalidStateTransition(currentState, target).Code)
.WithMessage(PaymentCaptureResult.Failure.InvalidStateTransition(currentState, target).Message);
```

- [ ] **Step 2: Build to verify no warnings**

```bash
dotnet build service/Api/src/Module
```
Expected: Build succeeded with 0 warnings.

- [ ] **Step 3: Run existing Payment validation tests**

```bash
dotnet test service/Api/tests/Module.UnitTests --filter "FullyQualifiedName~Payment.Validation"
```
Expected: All existing tests pass.

- [ ] **Step 4: Commit**

```bash
git add service/Api/src/Module/Payment/Domain/PaymentCaptures/PaymentCapture.Validation.cs
git commit -m "fix(payment): pass target state instead of currentState twice in state transition validator"
```

---

### Task 2: Delete Dead HandleAsync from StripeWebhookDispatcher (SIG-001)

**Files:**
- Modify: `service/Api/src/Module/Payment/Features/Storefront/Payment/Webhooks/StripeWebhookDispatcher.cs:43-57`

**Interfaces:**
- Produces: `HandleAsync` method removed. No interface change — method is not on `IStripeWebhookService`.

- [ ] **Step 1: Delete the HandleAsync method**

Remove lines 43-57 from `StripeWebhookDispatcher.cs` (the entire `HandleAsync` method — from the comment line `// Webhook: Dispatches Stripe event to handler...` through the closing brace before `ValidateSignature`):

```csharp
// DELETE LINES 43-57:
//
//     // Webhook: Dispatches Stripe event to handler via MediatR — signature validation happens in CommandHandler
//     public async Task<Result> HandleAsync(string eventType, string payload, CancellationToken ct = default)
//     {
//         // Check: Webhook secret must be configured
//         if (string.IsNullOrEmpty(_options.WebhookSecret))
//         {
//             return Error.Validation(
//                 "Stripe.WebhookSecret.NotConfigured",
//                 "Stripe webhook secret is not configured.");
//         }
//
//         // Assume: Stripe-Signature header is injected by gateway pipeline before reaching dispatcher
//         var result = await _sender.Send(new StripeWebhook.Command(payload, "stripe-signature"), ct);
//         return result;
//     }
```

- [ ] **Step 2: Build to verify no warnings**

```bash
dotnet build service/Api/src/Module
```
Expected: Build succeeded with 0 warnings.

- [ ] **Step 3: Run webhook-related tests**

```bash
dotnet test service/Api/tests/Module.UnitTests --filter "FullyQualifiedName~StripeWebhook"
```
Expected: All tests pass.

- [ ] **Step 4: Commit**

```bash
git add service/Api/src/Module/Payment/Features/Storefront/Payment/Webhooks/StripeWebhookDispatcher.cs
git commit -m "fix(payment): delete dead HandleAsync with hardcoded signature literal from StripeWebhookDispatcher"
```

---

### Task 3: Delete Legacy StripeWebhookHandler (WEB-003)

**Files:**
- Delete: `service/Api/src/Module/Payment/Services/Webhook/StripeWebhookService.cs`
- Delete: `service/Api/src/Module/Payment/Services/Webhook/StripeWebhookService.Loggers.cs`
- Modify: `service/Api/src/Module/Payment/Payment.Extension.cs:78-81` (remove DI registrations)
- Modify (optional): `service/Api/tests/Module.UnitTests/Payment/Services/Webhook/StripeWebhookServiceParseEventLoggingTests.cs` (if it exists and references legacy class)

**Interfaces:**
- Consumes: `IWebhookHandler` registration at `Payment.Extension.cs:80` — to be removed or kept commented
- Produces: Legacy `StripeWebhookHandler` class deleted. `IStripeWebhookService` still available via `StripeWebhookDispatcher`.
- **What replaces it:** `StripeWebhookDispatcher` (already registered as `IStripeWebhookService` at line 78) handles `ValidateSignature` and `ParseEvent`. The Carter endpoint sends `StripeWebhook.Command` directly via MediatR — no `IWebhookHandler` needed.

- [ ] **Step 1: Verify no callers of StripeWebhookHandler**

```bash
rg "StripeWebhookHandler" service/Api/src/ --include "*.cs"
```
Expected: Only `StripeWebhookService.cs`, `StripeWebhookService.Loggers.cs`, and `Payment.Extension.cs` reference it.

- [ ] **Step 2: Remove DI registration from Payment.Extension.cs**

Open `Payment.Extension.cs`. Change lines 78-81 from:

```csharp
services.AddSingleton<IStripeWebhookService, StripeWebhookDispatcher>();
// TODO(follow-up): Remove legacy StripeWebhookHandler — StripeWebhookDispatcher is the current impl
services.AddSingleton<IWebhookHandler, StripeWebhookHandler>();
```

to:

```csharp
services.AddSingleton<IStripeWebhookService, StripeWebhookDispatcher>();
```

- [ ] **Step 3: Delete the two legacy files**

```bash
rm service/Api/src/Module/Payment/Services/Webhook/StripeWebhookService.cs
rm service/Api/src/Module/Payment/Services/Webhook/StripeWebhookService.Loggers.cs
```

- [ ] **Step 4: Delete the legacy test file (if it exists and references the deleted class)**

```bash
ls service/Api/tests/Module.UnitTests/Payment/Services/Webhook/StripeWebhookServiceParseEventLoggingTests.cs
```

If it exists, delete it:

```bash
rm service/Api/tests/Module.UnitTests/Payment/Services/Webhook/StripeWebhookServiceParseEventLoggingTests.cs
```

- [ ] **Step 5: Build to verify no warnings**

```bash
dotnet build service/Api/src/Module
```
Expected: Build succeeded with 0 warnings.

- [ ] **Step 6: Run all Payment tests**

```bash
dotnet test service/Api/tests/Module.UnitTests --filter "FullyQualifiedName~Payment"
```
Expected: All tests pass.

- [ ] **Step 7: Commit**

```bash
git add service/Api/src/Module/Payment/Payment.Extension.cs
git rm service/Api/src/Module/Payment/Services/Webhook/StripeWebhookService.cs
git rm service/Api/src/Module/Payment/Services/Webhook/StripeWebhookService.Loggers.cs
# If test file was deleted:
git rm service/Api/tests/Module.UnitTests/Payment/Services/Webhook/StripeWebhookServiceParseEventLoggingTests.cs
git commit -m "fix(payment): delete legacy StripeWebhookHandler — replaced by StripeWebhookDispatcher"
```

---

### Task 4: Delete 4 Legacy Duplicate Files (CLN-006 to CLN-009)

**Files:**
- Delete: `service/Api/src/Module/Payment/Services/Models/GatewayConstants.cs`
- Delete: `service/Api/src/Module/Payment/Services/Abstractions/Gateway.cs`
- Delete: `service/Api/src/Module/Payment/Services/Models/StripeOptions.cs`
- Delete: `service/Api/src/Module/Payment/Services/Abstractions/IWebhookHandler.cs`

**Interfaces:**
- Produces: 4 files deleted. All consumers already reference the non-legacy copies.
- Risk: `ConfirmPayment.cs` imports `Module.Payment.Services.Models.GatewayConstants` — must be redirected.

- [ ] **Step 1: Find all imports of the Models copy of GatewayConstants**

```bash
rg "using Module.Payment.Services.Models.GatewayConstants\|Services.Models.GatewayConstants" service/Api/src/Module/Payment/ --include "*.cs"
```

- [ ] **Step 2: Fix any imports of Models.GatewayConstants to use Provider**

If any imports of `Module.Payment.Services.Models.GatewayConstants` are found, change them to `Module.Payment.Services.Provider.GatewayConstants`. For example:

```bash
# If ConfirmPayment.cs imports the Models copy, read it first
rg "GatewayConstants" service/Api/src/Module/Payment/Features/Storefront/Payment/Confirm/ConfirmPayment.cs
```

If the import uses `using GatewayConstants = Module.Payment.Services.Models.GatewayConstants`, change to:

```csharp
using GatewayConstants = Module.Payment.Services.Provider.GatewayConstants;
```

- [ ] **Step 3: Delete the 4 duplicate files**

```bash
rm service/Api/src/Module/Payment/Services/Models/GatewayConstants.cs
rm service/Api/src/Module/Payment/Services/Abstractions/Gateway.cs
rm service/Api/src/Module/Payment/Services/Models/StripeOptions.cs
rm service/Api/src/Module/Payment/Services/Abstractions/IWebhookHandler.cs
```

- [ ] **Step 4: Build to verify no warnings**

```bash
dotnet build service/Api/src/Module
```
Expected: Build succeeded with 0 warnings. If compile errors about missing types, fix remaining imports.

- [ ] **Step 5: Run all Payment tests**

```bash
dotnet test service/Api/tests/Module.UnitTests --filter "FullyQualifiedName~Payment"
```
Expected: All tests pass.

- [ ] **Step 6: Commit**

```bash
git rm service/Api/src/Module/Payment/Services/Models/GatewayConstants.cs
git rm service/Api/src/Module/Payment/Services/Abstractions/Gateway.cs
git rm service/Api/src/Module/Payment/Services/Models/StripeOptions.cs
git rm service/Api/src/Module/Payment/Services/Abstractions/IWebhookHandler.cs
# If imports were fixed:
git add service/Api/src/Module/Payment/Features/Storefront/Payment/Confirm/ConfirmPayment.cs
git commit -m "chore(payment): delete 4 legacy duplicate files — GatewayConstants, Gateway, StripeSetting, IWebhookHandler"
```

---

### Task 5: Retag StripeGatewayAuthorizeTests as Integration (TST-004)

**Files:**
- Modify: `service/Api/tests/Module.UnitTests/Payment/Infrastructure/Gateways/Stripe/StripeGatewayAuthorizeTests.cs:8`

**Interfaces:**
- Produces: Test trait changed from `"Unit"` to `"Integration"`. No behavior change.

- [ ] **Step 1: Change the test trait**

Open `StripeGatewayAuthorizeTests.cs`. Change line 8 from:

```csharp
[Trait("Category", "Unit")]
```

to:

```csharp
[Trait("Category", "Integration")]
```

- [ ] **Step 2: Build and verify**

```bash
dotnet build service/Api/tests/Module.UnitTests
```
Expected: Build succeeded with 0 warnings.

- [ ] **Step 3: Commit**

```bash
git add service/Api/tests/Module.UnitTests/Payment/Infrastructure/Gateways/Stripe/StripeGatewayAuthorizeTests.cs
git commit -m "test(payment): retag StripeGatewayAuthorizeTests as Integration (makes real HTTP calls to Stripe)"
```

---

### Task 6: Fix README.yaml Documentation (DOC-005)

**Files:**
- Modify: `service/Api/src/Module/Payment/README.yaml:263,302-303,719`

**Interfaces:**
- Produces: Documentation only — no code change.

- [ ] **Step 1: Update PaymentMethodId type to nullable**

Change line 263 from:

```yaml
      - PaymentMethodId (Guid) — FK to payment method
```

to:

```yaml
      - PaymentMethodId (Guid?) — FK to payment method (nullable)
```

- [ ] **Step 2: Remove phantom WebhookUrl and WebhookSecret properties**

Delete lines 302-303:

```yaml
# Delete these two lines:
      - WebhookUrl (string?) — webhook endpoint URL
      - WebhookSecret (string?) — webhook signing secret
```

The properties after deletion should read:

```yaml
      - Active (bool) — whether the method is available
      - AutoCapture (bool) — whether payments auto-capture
```

(Ensure the deleted lines are removed without leaving blank lines that break YAML structure.)

- [ ] **Step 3: Fix webhook registration example route**

Change line 719 from:

```yaml
app.MapPost("/api/storefront/webhooks/stripe", ...
```

to:

```yaml
app.MapPost("/api/payments/stripe/webhook", ...
```

- [ ] **Step 4: Verify the README is valid YAML**

```bash
python3 -c "import yaml; yaml.safe_load(open('service/Api/src/Module/Payment/README.yaml'))" && echo "Valid YAML" || echo "Invalid YAML"
```

Requires PyYAML. If not available, do a visual check.

- [ ] **Step 5: Commit**

```bash
git add service/Api/src/Module/Payment/README.yaml
git commit -m "docs(payment): fix README.yaml — nullable PaymentMethodId, remove phantom properties, correct webhook route"
```

---

### Task 7: Full Build and Test Verification

**Files:** None — verification only.

- [ ] **Step 1: Full Payment module build**

```bash
dotnet build service/Api/src/Module
```
Expected: Build succeeded with 0 warnings.

- [ ] **Step 2: Run all Payment unit tests (excluding integration)**

```bash
dotnet test service/Api/tests/Module.UnitTests 2>&1 | tail -15
```
Expected: All tests pass (not just Payment — the full suite to catch broken imports). The StripeGatewayAuthorizeTests that make real HTTP calls will now be skipped when filtering by Unit trait, but the `dotnet test` with no filter will run them (they will fail without network — that's expected for Integration tests).

- [ ] **Step 3: Run validation greps**

```bash
rg "currentState, currentState\)" service/Api/src/Module/Payment/Domain/PaymentCaptures/PaymentCapture.Validation.cs && echo "FAIL: bug still present" || echo "PASS: bug fixed"
```

```bash
rg "hardcoded.*stripe-signature\|\\\"stripe-signature\\\"" service/Api/src/Module/Payment/ && echo "FAIL: hardcoded literal found" || echo "PASS: no hardcoded literal"
```

```bash
ls service/Api/src/Module/Payment/Services/Models/GatewayConstants.cs service/Api/src/Module/Payment/Services/Abstractions/Gateway.cs service/Api/src/Module/Payment/Services/Models/StripeOptions.cs service/Api/src/Module/Payment/Services/Abstractions/IWebhookHandler.cs 2>&1 | grep "No such file" | wc -l
```
Expected: 4 (all 4 files deleted).

```bash
rg "WebhookUrl|WebhookSecret" service/Api/src/Module/Payment/README.yaml | head -5
```
Expected: No matches (or only matches in non-PaymentMethod contexts).

```bash
rg "PaymentMethodId \(Guid\)" service/Api/src/Module/Payment/README.yaml && echo "FAIL: still says Guid" || echo "PASS: says Guid?"
```

- [ ] **Step 4: Commit (if any straggling changes)**

```bash
git status
git diff
```
If clean, done. If straggling changes, commit them.
