---
goal: Deliver real Stripe webhook events to the local dev API — managed natively by Aspire via the Stripe CLI — and add diagnostic logging across the payment flow so a checkout can be traced end-to-end.
version: 3.0
date_created: 2026-08-13
last_updated: 2026-08-13
owner: Billing / Ordering / Store SPA / Aspire
status: 'In progress'
tags: [feature, billing, stripe, webhook, logging, aspire, checkout, store]
---

# Introduction

![Status: In progress](https://img.shields.io/badge/status-In%20progress-yellow)

A Stripe Checkout payment succeeds on Stripe's hosted page, but the local app
never advances: the `PaymentCapture` stays `Processing` and the order is not
auto-placed, because Stripe's servers cannot reach `localhost` (no webhook
delivery). Per the thesis (PAY-FR-04: "verify and process incoming gateway
webhooks using HMAC signatures"; UC-STR-PAY E3: "webhook updates state"), the
**webhook is the single source of truth** for payment-state updates. This plan:

1. **Delivers the webhook locally via Aspire-managed `stripe listen`** — the
   AppHost starts the Stripe CLI as an executable resource that forwards real
   `checkout.session.completed` / `checkout.session.expired` events to the API's
   resolved HTTPS endpoint. In Development the API accepts forwarded events
   without wiring the CLI's ephemeral signing secret (production still verifies
   HMAC).
2. **Adds diagnostic structured logging** at every boundary of the payment flow
   (intent creation, webhook receipt + signature, session lookup, completion,
   order placement, expiry regression, SPA return polling) so the flow can be
   traced end-to-end and the failing stage identified immediately.

The success_url return page continues to poll local state (which the webhook
updates); no completion happens from the browser return. This matches the
thesis and keeps a single completion source.

**Execution status:** Phases 1-3 (manual CLI script, diagnostic logging, first
verification) are complete and committed. Phase 4 (Aspire-native CLI + dev-only
signature relaxation) is the active work.

## 1. Requirements & Constraints

- **REQ-001**: The AppHost starts a `stripe listen` executable resource that forwards real Stripe webhook events to the API webhook endpoint (`api/storefront/billing/webhooks/stripe`) using the API's resolved HTTPS endpoint.
- **REQ-002**: The `stripe listen` resource is added only when `STRIPE_SECRET_KEY` is set in the environment, so orchestration works with or without Stripe keys.
- **REQ-003**: In Development, webhook signature verification is skipped when no `WebhookSecret` is configured (logged warning); in all other environments the HMAC check is mandatory.
- **REQ-004**: Structured `[LoggerMessage]` logs exist at every payment-flow boundary: intent creation, webhook receipt, signature validation, session lookup, payment completion, order placement, expiry regression, and SPA return polling.
- **REQ-005**: The `/checkout/return` SPA page logs each polling attempt (order id, `isCompleted`, attempt count) to the browser console.
- **SEC-001**: Production (and any non-Development environment) always verifies webhook signatures via HMAC (`GatewayProviders:stripe:WebhookSecret`); the Development-only bypass is gated strictly on `IHostEnvironment.IsDevelopment()` and logged. No logging emits secrets, API keys, or `whsec_...` values.
- **CON-001**: `TreatWarningsAsErrors=true` — any C# warning fails the build.
- **CON-002**: Structured logs use the existing `[LoggerMessage]` source-generated partial-logger pattern (`*Loggers.cs`); new EventIds use free ranges (dispatcher 5020+, CreatePaymentIntent 6010+).
- **CON-003**: The API HTTPS port is pinned to 5001 (`.WithExternalHttpEndpoints()` retained) so the forward URL is stable and readable.
- **CON-004**: Store SPA comments follow `app/Store/AGENTS.md` (`// Label: Sentence.`); lines under 100 chars.
- **CON-005**: `StripeSettingValidation` requires `WebhookSecret` only outside Development, so Dev can start Stripe-enabled without a secret.
- **GUD-001**: The webhook remains the single source of truth for payment-state updates (thesis PAY-FR-04 / E3); no success_url completion path is added.
- **PAT-001**: Logging calls follow the existing `ProcessStripeWebhookEventJobLoggers` / `StripeWebhookDispatcherLoggers` partial-class pattern.

## 2. Implementation Steps

### Implementation Phase 1: Stripe CLI webhook delivery (manual) — DONE

- GOAL-001: Deliver real Stripe events to the local API via a manual `stripe listen` script.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-001 | Pin the API HTTPS port to 5001 in the Aspire AppHost. | ✅ | 2026-08-13 |
| TASK-002 | Add `scripts/dev-stripe-listen.sh` + README documenting the two-terminal flow and webhook-secret wiring. | ✅ | 2026-08-13 |

### Implementation Phase 2: Diagnostic logging — DONE

- GOAL-002: Add structured logs at every boundary so a checkout can be traced end-to-end.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-003 | Dispatcher logs: webhook-secret missing, event received, signature verified. | ✅ | 2026-08-13 |
| TASK-004 | Webhook-job logs: event routed, session lookup, completed, order placed, expired, cart regressed. | ✅ | 2026-08-13 |
| TASK-005 | `CreatePaymentIntent` logs: session created, COD created, retry voided stale. | ✅ | 2026-08-13 |
| TASK-006 | Auto-place logs in `CompleteCheckoutForPayment` + `CheckoutPlacementService`. | ✅ | 2026-08-13 |
| TASK-007 | `console.debug` polling logs in `CheckoutReturnView.vue`. | ✅ | 2026-08-13 |

### Implementation Phase 3: First verification — DONE

- GOAL-003: Verify the build/tests and the webhook log chain.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-008 | Run full backend + SPA + convention verification. | ✅ | 2026-08-13 |

### Implementation Phase 4: Aspire-managed Stripe CLI + dev signature relaxation

- GOAL-004: Start `stripe listen` from the AppHost and accept forwarded events in Development without manual secret wiring.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-009 | Add an `AddExecutable("stripe-listen")` resource to the AppHost. | | |
| TASK-010 | Add Development-only webhook signature relaxation (dispatcher + `StripeSettingValidation` + logger). | | |
| TASK-011 | Update README + script to make the Aspire-native flow primary. | | |
| TASK-012 | Add dev-relaxation tests + run full verification. | | |

#### TASK-009: AppHost `stripe-listen` executable resource

**Files:**
- Modify: `infra/Aspire/src/ReSys.AppHost/AppHost.cs` (after the `api` resource is defined)

**Consumes:** `api.GetEndpoint("https")` (the pinned 5001 endpoint from TASK-001), env `STRIPE_SECRET_KEY`.

- [ ] After the `api` resource definition, add (conditionally):
```csharp
var stripeApiKey = Environment.GetEnvironmentVariable("STRIPE_SECRET_KEY");
if (!string.IsNullOrEmpty(stripeApiKey))
{
    builder.AddExecutable("stripe-listen", "stripe",
            "listen",
            "--forward-to", $"{api.GetEndpoint("https")}/api/storefront/billing/webhooks/stripe")
        .WithEnvironment("STRIPE_API_KEY", stripeApiKey)
        .WaitFor(api);
}
```
- [ ] Verify: `dotnet build infra/Aspire/src/ReSys.AppHost` — 0 warnings; with `STRIPE_SECRET_KEY` set, `dotnet run --project infra/Aspire/src/ReSys.AppHost` shows a `stripe-listen` resource (the `stripe` CLI must be on `PATH`); without the env var, no such resource is created and the app still runs.

#### TASK-010: Development-only signature relaxation

**Files:**
- Modify: `service/Api/src/Module/Billing/Features/Storefront/Payment/Webhooks/StripeWebhookDispatcher.cs`
- Modify: `service/Api/src/Module/Billing/Features/Storefront/Payment/Webhooks/StripeWebhookDispatcher.Loggers.cs`
- Modify: `service/Api/src/Module/Billing/Services/Provider/Stripe/StripeSettingValidation.cs`

**Interfaces:**
- Produces: `StripeWebhookDispatcher` accepts forwarded events in Development when no secret is configured; `StripeSettingValidation` stops requiring `WebhookSecret` in Development.

- [ ] `StripeWebhookDispatcher.cs`: inject `IHostEnvironment _environment` (add to the constructor after `ILogger`); rewrite `ValidateSignature`:
```csharp
public bool ValidateSignature(string payload, string stripeSignature)
{
    if (string.IsNullOrEmpty(_options.WebhookSecret))
    {
        if (_environment.IsDevelopment())
        {
            StripeWebhookDispatcherLoggers.SignatureBypassedInDevelopment(_logger);
            return true;
        }
        StripeWebhookDispatcherLoggers.WebhookSecretMissing(_logger);
        return false;
    }
    try
    {
        EventUtility.ValidateSignature(payload, stripeSignature, _options.WebhookSecret);
        StripeWebhookDispatcherLoggers.SignatureVerified(_logger);
        return true;
    }
    catch (StripeException ex)
    {
        StripeWebhookDispatcherLoggers.SignatureValidationFailed(_logger, ex);
        return false;
    }
}
```
- [ ] `StripeWebhookDispatcher.Loggers.cs`: change `WebhookSecretMissing` message to reflect that rejection now only happens outside Development, e.g. `"Stripe webhook secret is not configured (GatewayProviders:stripe:WebhookSecret); webhooks rejected outside Development."`; add (EventId 5023, Warning):
```csharp
[LoggerMessage(
    EventId = 5023,
    Level = LogLevel.Warning,
    Message = "Development: accepting Stripe webhook without signature verification (stripe listen). Set GatewayProviders:stripe:WebhookSecret to verify.")]
public static partial void SignatureBypassedInDevelopment(ILogger logger);
```
- [ ] `StripeSettingValidation.cs`: inject `IHostEnvironment _environment` (constructor param) and skip the `WebhookSecret` requirement in Development:
```csharp
if (string.IsNullOrEmpty(options.SecretKey))
    errors.Add("GatewayProviders:stripe:SecretKey is required when Enabled=true.");
if (!_environment.IsDevelopment() && string.IsNullOrEmpty(options.WebhookSecret))
    errors.Add("GatewayProviders:stripe:WebhookSecret is required when Enabled=true (skipped in Development for stripe listen).");
```
- [ ] Verify: `dotnet build service/Api/src/Api/Api.csproj` — 0 warnings.

#### TASK-011: README + script update (Aspire-native primary)

**Files:**
- Modify: `service/Api/README.md`
- Modify: `scripts/dev-stripe-listen.sh`

- [ ] README "Stripe webhooks (local)" section: replace the manual two-terminal steps with the Aspire-native flow — set `STRIPE_SECRET_KEY=sk_test_...`, run `dotnet run --project infra/Aspire/src/ReSys.AppHost`, and the AppHost starts `stripe listen` automatically, forwarding to `https://localhost:5001/...`; no `whsec_...` wiring is needed in Development (signature is skipped there). Keep the manual script as an optional fallback.
- [ ] `scripts/dev-stripe-listen.sh`: add a header comment noting it is a manual fallback superseded by the AppHost `stripe-listen` resource (set `STRIPE_SECRET_KEY` and start via Aspire for the normal flow).

#### TASK-012: Dev-relaxation tests + full verification

**Files:**
- Create: `service/Api/tests/Module.UnitTests/Billing/Features/Storefront/Payment/Webhooks/StripeWebhookDispatcherTests.cs`
- Create: `service/Api/tests/Module.UnitTests/Billing/Services/Provider/Stripe/StripeSettingValidationTests.cs`

**Tests (xUnit + Moq, InMemory where needed):**
- [ ] `StripeWebhookDispatcherTests.ValidateSignature_DevelopmentNoSecret_Accepts` — `IOptions<StripeSetting>` with empty `WebhookSecret`, `IHostEnvironment` with `IsDevelopment=true` → returns `true` (no `EventUtility` call; a `Mock` of `IStripeWebhookService` is not needed — test the concrete `StripeWebhookDispatcher`).
- [ ] `StripeWebhookDispatcherTests.ValidateSignature_NonDevelopmentNoSecret_Rejects` — `IsDevelopment=false` → returns `false`.
- [ ] `StripeWebhookDispatcherTests.ValidateSignature_WithSecret_Verifies` — non-empty secret + a matching signature built with `EventUtility` or a valid test signature → returns `true`; a tampered signature → returns `false`.
- [ ] `StripeSettingValidationTests.Development_SkipsWebhookSecretRequirement` — `IsDevelopment=true`, `SecretKey` set, `WebhookSecret` empty → `ValidateOptionsResult.Success`.
- [ ] `StripeSettingValidationTests.NonDevelopment_RequiresWebhookSecret` — `IsDevelopment=false`, `SecretKey` set, `WebhookSecret` empty → `ValidateOptionsResult.Fail`; with secret set → `Success`.
- [ ] Run: `dotnet build` (0 warnings), `dotnet test service/Api/tests/Module.UnitTests` (all pass), `cd app/Store && pnpm run lint && pnpm run build-only`, `bash scripts/check-feature-conventions.sh` (all PASS).
- [ ] Manual smoke: set `STRIPE_SECRET_KEY`, start the AppHost, pay via a Checkout Session, and trace the log chain — `SessionCreated` → `EventRouted(checkout.session.completed)` → `SessionLookup(Found=true)` → `SignatureBypassedInDevelopment` (dev) → `CheckoutSessionCompleted` → `OrderPlaced`; abandon a session → `EventRouted(checkout.session.expired)` → `CheckoutSessionExpired` → `CartRegressedToDelivery`.
- [ ] Commit: `git add` the changed/new files then `git commit -m "feat: manage stripe listen via Aspire and relax dev webhook signature"`.

## 3. Alternatives

- **ALT-001**: success_url / return-page verification (completes + places from the browser return) — rejected: not described in the thesis (PAY-FR-04 mandates the webhook as the completion source; UC-STR-PAY E3 says "webhook updates state"), and it would create a second, divergent completion path.
- **ALT-002**: Manual two-terminal `stripe listen` + copy `whsec_...` (Phase 1) — retained as a fallback but superseded by the Aspire-managed resource (TASK-009), which removes the manual secret wiring.
- **ALT-003**: Public tunnel (ngrok/cloudflared) + dashboard webhook endpoint — rejected: exposes the dev API publicly and needs a stable public URL; `stripe listen` forwards signed events locally with no public exposure.
- **ALT-004**: Keep HMAC strict in Development and wire the ephemeral `whsec_...` per run — rejected: the secret changes each `stripe listen` start, which is incompatible with an Aspire-managed process lifecycle; the Development-only bypass (TASK-010) is gated on `IsDevelopment()` and logged.

## 4. Dependencies

- **DEP-001**: Stripe CLI (`stripe listen`) on `PATH` — started by Aspire as an executable resource.
- **DEP-002**: Aspire — `AddExecutable` resource + pinned API HTTPS port 5001 (`GetEndpoint("https")`).
- **DEP-003**: `IHostEnvironment` (ASP.NET Core) — Development gating for signature relaxation and validation.
- **DEP-004**: `ILogger` + `[LoggerMessage]` source generator — structured logging.
- **DEP-005**: `service/Api/scripts/setup-dev-secrets.sh` — `STRIPE_SECRET_KEY` env for `GatewayProviders:stripe:SecretKey`.

## 5. Files

- **FILE-001**: `infra/Aspire/src/ReSys.AppHost/AppHost.cs` — pinned HTTPS port 5001 (done) + `AddExecutable("stripe-listen")`.
- **FILE-002**: `scripts/dev-stripe-listen.sh` — optional manual fallback (header note).
- **FILE-003**: `service/Api/README.md` — Aspire-native webhook flow.
- **FILE-004**: `service/Api/src/Module/Billing/Features/Storefront/Payment/Webhooks/StripeWebhookDispatcher.cs` — dev-only signature bypass.
- **FILE-005**: `service/Api/src/Module/Billing/Features/Storefront/Payment/Webhooks/StripeWebhookDispatcher.Loggers.cs` — `SignatureBypassedInDevelopment` (5023) + message update.
- **FILE-006**: `service/Api/src/Module/Billing/Services/Provider/Stripe/StripeSettingValidation.cs` — skip `WebhookSecret` requirement in Development.
- **FILE-007**: `service/Api/src/Module/Billing/Features/Storefront/Payment/Webhooks/StripeWebhookDispatcher.Loggers.cs` (existing) + `ProcessStripeWebhookEventJob.Loggers.cs` + `CreatePaymentIntent.Loggers.cs` — diagnostic logs (done).
- **FILE-008**: `app/Store/src/features/ordering/views/CheckoutReturnView.vue` — `console.debug` (done).
- **FILE-009**: `service/Api/tests/Module.UnitTests/Billing/Features/Storefront/Payment/Webhooks/StripeWebhookDispatcherTests.cs` (new).
- **FILE-010**: `service/Api/tests/Module.UnitTests/Billing/Services/Provider/Stripe/StripeSettingValidationTests.cs` (new).

## 6. Testing

- **TEST-001**: `StripeWebhookDispatcherTests` — dev-accept with empty secret, non-dev reject with empty secret, verify/tamper with a configured secret.
- **TEST-002**: `StripeSettingValidationTests` — dev skips `WebhookSecret` requirement; non-dev requires it.
- **TEST-003**: Full gates — `dotnet build` 0 warnings; `Module.UnitTests` all pass; SPA lint/build 0 warnings/errors; feature-conventions all PASS.
- **TEST-004**: Manual — start the AppHost with `STRIPE_SECRET_KEY`; trace the log chain for `checkout.session.completed` (auto-place) and `checkout.session.expired` (cart regression); confirm `SignatureBypassedInDevelopment` appears in Development.

## 7. Risks & Assumptions

- **RISK-001**: Development-only signature bypass weakens local verification; mitigated by the strict `IsDevelopment()` gate, a Warning log, and production/non-dev HMAC remaining mandatory.
- **RISK-002**: The `stripe` CLI must be on `PATH` for the AppHost resource to start; the resource is conditional on `STRIPE_SECRET_KEY` so a missing CLI/key does not break orchestration.
- **RISK-003**: The `StripeWebhookDispatcher` constructor change (adding `IHostEnvironment`) must not break DI resolution — it is a registered singleton service; existing tests that construct the dispatcher are updated with a mock env.
- **RISK-004**: Pinning port 5001 could collide with another local service; change the port (and the forward URL) if needed.
- **ASSUMPTION-001**: The Stripe CLI's ephemeral signing secret is accepted in Development via the bypass; Production always configures a real `WebhookSecret`.
- **ASSUMPTION-002**: The API HTTPS endpoint 5001 is acceptable for local dev.

## 8. Related Specifications / Further Reading

- [Payment method selection design](docs/superpowers/specs/2026-08-13-payment-method-selection-design.md)
- [Thesis PAY-FR-04 / UC-STR-PAY E3](thesis/chapters/part2/ch2-design/01-requirements/01-functional-requirements.typ)
- [Aspire AddExecutable](https://learn.microsoft.com/en-us/dotnet/aspire/overview)
- [Stripe CLI listen](https://docs.stripe.com/stripe-cli)
- [Stripe webhooks](https://docs.stripe.com/webhooks)
