# Stripe Enablement & Storefront API Correction

## Overview

Enable Stripe as a real, working payment gateway and correct the storefront
API to match the thesis (UC-STR-CHK checkout flow + mechanical conformance),
backend only. The frontend (`app/legacy/Storefront`) is explicitly out of
scope; a handoff document captures the API contract for a future Stripe.js
integration plan.

### Phasing

Two sequenced phases in one spec / one implementation plan. Phase 1 is
shippable independently before Phase 2 begins.

- **Phase 1 — Stripe Enablement:** make the Stripe gateway reachable
  end-to-end. Fix the two bugs that block all `SourceRequired` gateways
  (Bogus is also broken today), enable Stripe in config, fill the webhook
  gap, verify with the user's test keys.
- **Phase 2 — Storefront API Correction:** mechanical conformance (contract
  drift, dead/stale endpoints, missing endpoints, `.http` tests) + UC-STR-CHK
  flow restructure (inventory reservation coupled with intent creation;
  E3 void-on-stock-failure; strict checkout state machine).

### Cross-cutting guardrails

- No frontend changes. `app/legacy/Storefront/` is untouched; the demo path
  may break further because cart-state transitions now require strict
  ordering. The Handoff `.md` documents the new flow so a future frontend
  plan can catch up.
- Secrets (Stripe keys) stay in `dotnet user-secrets`, never in tracked
  files.
- Bogus gateway stays enabled in Development (PAY-FR-06); Stripe becomes
  co-enabled.
- No new cross-module `using Module.X.Domain...` references
  (AGENTS.md rule 2). Payment ↔ Ordering ↔ Inventory communication uses
  MediatR `ISender` only.
- `dotnet build` stays green (warnings-as-errors).
- `scripts/check-cross-module-refs.sh` stays green.

### Thesis requirements satisfied

- Phase 1: PAY-FR-01 (intent creation), PAY-FR-02 (confirm), PAY-FR-04 +
  NFR-05c (webhook HMAC + idempotency), PAY-FR-07 (local state mirror),
  PAY-FR-08 (SetupIntent — kept).
- Phase 2A: PAY-FR-01 currency honoring; contract conformance.
- Phase 2B: ORD-FR-04 (checkout state machine), ORD-FR-11 (pre-payment
  validation), UC-STR-CHK step 3 + E3.

---

## Phase 1 — Stripe Enablement

### Problem

The current payment flow is broken for **all** `SourceRequired` gateways —
both Stripe and Bogus. Two bugs combine to block it:

1. **Source never set.** `CreatePaymentIntent.cs:50-53` calls
   `PaymentCaptureMethod.Create(amount, paymentMethodId, orderId)` with no
   `sourceId`/`sourceType`. `PaymentProcessingService.HandlePaymentPreconditions`
   (`:257-268`) then returns `ProcessingSourceRequired` because
   `gateway.SourceRequired` is true for both Stripe (`StripeGateway.cs:18`)
   and Bogus (`BogusGateway.cs:13`).
2. **Source shape mismatch.** Even if set, `GatewayActionAsync` (`:279`)
   passes `new { Id = payment.SourceId, Type = payment.SourceType }` as
   `source`, but `StripeGateway.Supports` (`:20`) and
   `StripeGateway.PurchaseAsync` (`:229`) only treat `source as string` as
   a Stripe PaymentMethod ID. The anonymous object is invisible to the
   gateway.

Plus the config gap: `appsettings.json:8` has `stripe:Enabled=false`;
`service/Api/scripts/setup-dev-secrets.sh:24-29` never sets
`stripe:Enabled` even when keys are present.

### 1A — Source plumbing: column type migration

**Decision (locked):** change `PaymentCapture.SourceId` from `Guid?` to
`string?`. Add an EF Core migration.

`SourceId: Guid?` was a leftover from an older model where sources were
internal references. Real payment tokens are strings for every Stripe-like
gateway (Stripe `pm_...`, Bogus test card numbers). The migration is a
column type change; all current rows have null `SourceId` (the bug blocked
real use), so backfill is trivial.

**Changes:**

- `PaymentCapture.cs:34` — `public Guid? SourceId` → `public string? SourceId`.
- `PaymentCaptureValidation` (if it constrains `SourceId` shape) — relax to
  string rules.
- New EF Core migration in `service/Migrations/` — `ALTER COLUMN` SourceId
  from `uuid` to `text` (or `varchar(N)`). PostgreSQL allows null→null
  implicitly.
- `PaymentRecordConfiguration.cs:28` — `builder.Property(x => x.SourceId)`
  becomes a string-mapped column. Adjust max length if it inherits one.

`SourceType: string?` stays unchanged.

### 1B — Gateway source pass-through

`PaymentProcessingService.GatewayActionAsync:279`:

```csharp
// Before
var source = payment.SourceType is not null
    ? new { Id = payment.SourceId, Type = payment.SourceType }
    : null;

// After
object? source = payment.SourceId;  // string? — the raw gateway token
```

`StripeGateway.Supports(object?) => source is string or null` accepts it.
`StripeGateway.PurchaseAsync:229-230` assigns `o.PaymentMethod = s` when
`source is string s && !string.IsNullOrEmpty(s)`. Bogus
`BogusGateway.Supports(object?) => source is string` matches too. **No
gateway code changes.**

### 1C — `CreatePaymentIntent` flow

Update the storefront `CreatePaymentIntent` handler to accept and propagate
the source token:

- `CreatePaymentIntent.Request.cs` — add `string? PaymentMethodToken`
  (the gateway token from the client, distinct from the local
  `PaymentMethodId: Guid?` which selects the local method entity). Keep
  `ReturnUrl` and `PaymentMethodId`.
- `CreatePaymentIntent.Validator.cs` — when the resolved `PaymentMethod`'s
  provider is Stripe (or any `SourceRequired` gateway), require
  `PaymentMethodToken` non-empty. Bogus in the demo uses known test card
  constants; a `SourceRequired` gateway with no token is a 400.
- `CreatePaymentIntent.cs:50-53` — pass the source into the factory:

  ```csharp
  var createResult = PaymentCaptureMethod.Create(
      amount: order.Total,
      paymentMethodId: paymentMethod.Id,
      orderId: order.Id,
      sourceId: command.Request.PaymentMethodToken,  // string? now
      sourceType: command.Request.PaymentMethodToken is null
          ? null
          : GatewayConstants.SourceTypes.PaymentMethod);
  ```

- Add `GatewayConstants.SourceTypes.PaymentMethod = "payment_method"` (and
  `"card"` for the Bogus path) so `SourceType` is a stable discriminator.
- `CreatePaymentIntent.cs:71` — honor `command.Request.ReturnUrl` for the
  Stripe 3DS redirect by setting `options.SuccessUrl = request.ReturnUrl`
  when non-null. (`StripeGateway.CreatePaymentIntentOptions:200-201`
  already wires `ReturnUrl` from `options.SuccessUrl`.)

`currency` honoring is deferred to Phase 2A-M3 (it is a mechanical
conformance item, not a Stripe-blocker).

### 1D — Stripe config enablement

**No tracked file edits for secrets.** All via `dotnet user-secrets`
(id `resys.shop.api`).

- Update `service/Api/scripts/setup-dev-secrets.sh:24-29` to **also** set
  `GatewayProviders:stripe:Enabled=true` when `STRIPE_SECRET_KEY` env var is
  present. This is a tracked script edit, not a secret. When the env var is
  absent the script leaves `Enabled` untouched (default false).
- Document the user-secret commands in a script header comment so the
  manual path is also clear:

  ```
  dotnet user-secrets set "GatewayProviders:stripe:Enabled"     "true"
  dotnet user-secrets set "GatewayProviders:stripe:SecretKey"   "sk_test_..."
  dotnet user-secrets set "GatewayProviders:stripe:WebhookSecret" "whsec_..."
  dotnet user-secrets set "GatewayProviders:stripe:PublishableKey" "pk_test_..."
  ```

- `StripeSettingValidation.cs:8-22` already enforces `SecretKey` +
  `WebhookSecret` required when `Enabled=true` — no change.
- Bogus stays `Enabled=true` in `appsettings.Development.json` (PAY-FR-06).
  Both gateways co-enabled in Development.

### 1E — Webhook endpoint gap

Audit `StripeWebhook.cs`, `StripeWebhookDispatcher.cs`,
`ProcessStripeWebhookEventJob.cs`, and `IStripeWebhookService` against
UC-SYS-MNT (thesis `system.typ:126-132`) and NFR-05c:

1. **HMAC signature verification** of `Stripe-Signature` against the raw
   request body using `WebhookSecret`, before any state mutation.
2. **Idempotency** by Stripe event ID — duplicate events return 200
   without re-applying state. (A2 in the thesis: "Duplicate webhook →
   return success without changes; duplicate logged.")
3. **State transition + order state update** on `payment_intent.succeeded`
   / `.payment_failed` / `.canceled` events.

Gaps discovered during audit become Phase 1 sub-tasks. The existing
route is `api/storefront/webhooks/stripe`
(`PaymentFeature.Storefront.cs:42-49`); only the stale XML doc on
`StripeWebhook.Endpoint.cs:11` (`api/payments/stripe/webhook`) needs
correcting — that fix lives in Phase 2A-M4 alongside other doc fixes.

### 1F — Verification

- **Unit tests** (`service/Api/tests/Module.UnitTests`):
  - `HandlePaymentPreconditions`: passes with a string source, fails with
    null when `SourceRequired`, passes with null when `!SourceRequired`.
  - `GatewayActionAsync` source pass-through: a string source reaches
    `StripeGateway.PurchaseAsync` as `o.PaymentMethod`.
  - `StripeWebhook` HMAC: valid signature → processed; invalid signature →
    401; duplicate event ID → 200 no-op.
- **`.http` integration tests** (new `ApiTests/Payment/*.http` — see
  Phase 2A-M6):
  - `create-intent` with a Stripe test token (`pm_card_visa` via Stripe.js
    token endpoint, or a test `tok_visa`).
  - `confirm` on the returned payment.
  - `methods` listing.
  - `setup-intent` create.
  - `webhook` replay via `stripe trigger payment_intent.succeeded`.
- **End-to-end smoke (user runs):** with user-secrets set,
  `dotnet test --filter Payment`, then `stripe listen --forward-to
  localhost:5000/api/storefront/webhooks/stripe` and
  `stripe trigger payment_intent.succeeded`. Confirms the full path
  including webhook-driven state transition.

### Phase 1 out of scope

- No frontend Stripe.js. The `.http` tests use synthetic tokens to exercise
  the backend; the UI stays broken until a follow-up plan.
- No UC-STR-CHK restructure (Phase 2B).
- No `currency` honoring (Phase 2A-M3).
- No route-prefix unification (Phase 2A drops M1, see below).

---

## Phase 2A — Mechanical conformance

Low-risk changes, no domain model restructure.

### M2 — `ConfirmPayment` contract drift

Frontend sends `{ paymentMethodId }` body; backend
(`ConfirmPayment.Endpoint.cs:13-18`) takes only `paymentId` from the route
and `ConfirmPayment.cs:12` has no Request body — the client body is
silently ignored. No `Request.cs` exists.

**Fix:** add `ConfirmPayment.Request.cs` with `Guid? PaymentMethodId`.
The handler captures it for audit but is not required for the confirm
action (state transition only). The endpoint binds the body. The
validator accepts null.

### M3 — `CreatePaymentIntent.Request` honors currency

Current handler ignores client-sent `amount` and `currency`; uses
`order.Total` and a hardcoded `GatewayOptions.Currency`. Thesis PAY-FR-01
says intent creation accepts "amount, currency, order reference, gateway
target."

**Decision (locked):**

- `amount` stays server-derived from `order.Total` (trust server over
  client for money — avoids a client underpaying).
- `currency` is honored from the request, falling back to
  `GatewayOptions.Currency` default when absent.

**Changes:** add `string? Currency` to `StorePaymentRequest` (the shared
request model) if not present; thread it into `GatewayOptions` inside
`CreatePaymentIntent.cs`.

### M4 — Dead/stale endpoints

- `CreateSetupIntent` stays wired (PAY-FR-08 is Low priority but
  thesis-listed). Add an `ApiTests/Payment/setup-intent.http` entry (see
  M6). No frontend caller yet — the Handoff `.md` documents that the future
  Stripe.js plan must consume it for saved-payment-method flows.
- Stale XML doc on `StripeWebhook.Endpoint.cs:11` says
  `api/payments/stripe/webhook`; the live route is
  `api/storefront/webhooks/stripe`. Fix the comment.

### M5 — Missing `Inventory CartReservations.Release` endpoint

Route declared in `InventoryFeature.Storefront.cs:34`
(`DELETE api/storefront/cart/reserve/{reservationId:guid}`) but no matching
`*.Endpoint.cs` exists. Needed for Phase 2B's reservation-release path
(`CreatePaymentIntent` releases reservations on downstream gateway failure).

**Fix:** add `Features/Storefront/CartReservations/Release/Release.Endpoint.cs`
+ minimal handler that deletes a `StockReservation` by ID (or marks it
released if the entity has a status). Conform to the existing Carter
module pattern.

### M6 — `ApiTests/Payment/*.http`

`ApiTests/Payment/` is currently empty (zero bytes). Add `.http` files for
all five storefront payment endpoints, matching the parity every other
module folder has:

- `methods.http` — `GET /api/storefront/payment/methods`
- `create-intent.http` — `POST /api/storefront/payment/create-intent`
  with `{ orderId, paymentMethodId, paymentMethodToken, currency, returnUrl }`.
- `confirm.http` — `POST /api/storefront/payment/confirm/{paymentId}` with
  `{ paymentMethodId }`.
- `setup-intent.http` — `POST /api/storefront/payment/setup-intent`.
- `webhook.http` — `POST /api/storefront/webhooks/stripe` with a sample
  Stripe event payload + `Stripe-Signature` header.

### M7 — `PaymentMethod` seeder autoCapture mismatch

`PaymentMethod.Seeder.cs:28` seeds `autoCapture:false` for the Bogus
method, but runtime behavior comes from `BogusGateway.AutoCapture=true`
(`BogusGateway.cs:12`). The seeded value is misleading in the admin UI.

**Fix:** correct the seed to `true`. If a quick audit shows nothing reads
the `PaymentMethod.AutoCapture` column outside display, leave the column
in place (a wider schema cleanup is out of scope). If something does read
it, leave the seed alone and add a doc note instead — decide during
implementation.

### Phase 2A out of scope

- **M1 (route-prefix unification for Identity/Profile/Location) is
  dropped.** The prior shipped spec
  `2026-07-28-storefront-api-path-alignment-design.md` deliberately aligned
  the *frontend* to the existing `api/store/{identity,profiles,locations}`
  prefixes rather than unifying the backend. Reversing it would contradict
  shipped work and force the frontend to update again. The thesis route
  convention at `api-design.typ:46-89` lists per-surface endpoint counts,
  not the literal prefix for every module — so this is a documentation-
  level wish, not a hard binding requirement. Document the divergence in
  `docs/codebase/CONCERNS.md` instead (a separate task, possibly outside
  this plan).

---

## Phase 2B — UC-STR-CHK flow restructure

The substantive change. Thesis requirement (UC-STR-CHK step 3,
`ordering.typ:112`): *"System creates a payment intent **and reserves
inventory** for each line item."* And exception E3 (`ordering.typ:128`):
*"Payment captured but inventory reservation fails → system voids
payment."*

### Current behavior (broken vs. thesis)

1. `CreatePaymentIntent.cs` — creates PaymentCapture, calls gateway, **no
   inventory reservation**.
2. `CreateOrderFromCart.cs:152-153` — returns `InsufficientStock` **after**
   payment is already captured (Bogus auto-capture), and **never voids**
   the captured payment. E3 violation.
3. No strict Address→Delivery→Payment→Confirm→Complete state-machine
   enforcement; `ValidateCheckoutPrerequisites()` checks fields but does
   not model sequential progression.

### Target flow

| Step | Endpoint | Action |
|---|---|---|
| Address | `PUT /api/storefront/cart` | `UpdateCheckout` sets `shipAddressId`. Cart state → `Address`. |
| Delivery | `POST /api/storefront/cart/shipping-rate` | `SelectShippingRate` applies method. Cart state → `Delivery`. |
| Payment | `POST /api/storefront/payment/create-intent` | `CreatePaymentIntent`: validate cart is in `Delivery` state; atomically (a) verify stock availability for all line items, (b) create stock reservations, (c) create PaymentCapture, (d) call gateway. If (a)/(b) fail → return stock error, no payment created. If (d) fails → release reservations, return gateway error. Cart state → `Payment`. |
| Confirm | `POST /api/storefront/cart/checkout` | `CreateOrderFromCart`: verify cart is in `Payment` state; verify payment is `Completed`; convert existing reservations into stock deductions (`Pick`); place order. Cart state → `Complete`. No new stock check — already reserved in step 3. |
| Complete | (implicit) | Order placed, notification sent. |

**Key restructure:** stock reservation moves from `CreateOrderFromCart` →
`CreatePaymentIntent`. Existing reservations are *consumed* (picked) at
checkout, not re-checked.

### E3 handling

Stock-failure now happens *before* the gateway call in
`CreatePaymentIntent`, so the "void payment after capture" scenario is
eliminated by construction — if stock fails, no payment is created. The E3
void path is only needed for the residual case: gateway call succeeds but
a *later* failure (e.g. save throws) requires voiding. Add a `try/catch`
around the gateway call in `CreatePaymentIntent` that calls
`processingService.VoidAsync` on any post-gateway failure — narrow,
defensive. The released reservations go via
`ReleaseCartStockReservations` (see below).

### CheckoutState state machine

**Decision (locked):** add a persisted `CheckoutState` enum column to
`Order`.

- New `CheckoutState` enum: `Address | Delivery | Payment | Confirm |
  Complete`. Initial cart state is `Address` (or `None` before any
  checkout step starts; decide during implementation based on existing
  Order construction).
- Domain method `Order.AdvanceCheckoutState(CheckoutState target)` enforces
  strict transitions; out-of-order calls return
  `OrderResult.Errors.InvalidCheckoutTransition(current, target)`.
- Each storefront step validates and advances the state:
  - `UpdateCheckout` → `Address`
  - `SelectShippingRate` → `Delivery`
  - `CreatePaymentIntent` → `Payment`
  - `CreateOrderFromCart` → `Complete` (skipping `Confirm` since the
    confirm endpoint is optional in the auto-capture path; Bogus
    auto-captures, Stripe webhook-completes)
- EF Core migration adds the column; existing Draft orders backfill to
  `Address` (the earliest valid state — they re-walk the pipeline).

### `CreatePaymentIntent` restructure

The handler grows three new responsibilities, all via MediatR `ISender`
(no direct cross-module references):

1. **Validate cart state** — `ISender.Send(new GetCartState(cartId))`
   returns the current `CheckoutState`. If not `Delivery`, return
   `OrderResult.Errors.InvalidCheckoutTransition`.
   - *Alternative:* inject `IApplicationDbContext` (already present in the
     handler) and read the `Order.CheckoutState` directly — this is the
     Ordering module's own `Order` entity, **not** a cross-module access.
     The Payment module loading `Module.Ordering.Domain.Orders.Order` is
     *already* a cross-module violation (existing —
     `CreatePaymentIntent.cs:3`). This should be fixed, but the fix is a
     separate refactor (the existing 39 violations tracked in
     `docs/codebase/CONCERNS.md`). For this plan, **introduce a MediatR
     query `GetCartForCheckout`** that returns the cart's state + line
     items + totals, and have `CreatePaymentIntent` consume it via
     `ISender`. This removes the existing `Module.Ordering.Domain` import
     from the Payment handler — a targeted improvement.
2. **Reserve stock** — `ISender.Send(new ReserveCartStock(cartId))`
   validates stock availability across all line items and creates
   `StockReservation` rows with the same TTL behavior as today
   (`StockReservationExpiryMinutes`, default 30). Returns reservation IDs
   or a stock-error `Result`. On failure, return early — no
   PaymentCapture, no gateway call.
3. **Release reservations on downstream failure** — if the gateway call
   throws or `SaveChangesAsync` fails after the gateway succeeded, send
   `ReleaseCartStockReservations(cartId)` and re-throw / return the
   error. This is the E3 void path.

The gateway call, PaymentCapture creation, and `GatewayOptions`
construction stay as-is except for the source plumbing (Phase 1) and the
currency honoring (M3).

### `CreateOrderFromCart` restructure

The handler loses its inline stock-deduction loop and gains two new
responsibilities:

1. **Validate cart state** — must be `Payment`. Advance to `Complete` only
   after successful placement. (Direct domain access — `Order` is the
   Ordering module's own entity.)
2. **Consume existing reservations** — instead of querying `StockItem` and
   deducting live, read the `StockReservation` rows created in
   `CreatePaymentIntent`, call `StockItem.Pick(reservation.Quantity)` for
   each, mark reservations as consumed, and log `StockMovement`s. The
   `RepeatableRead` transaction + 3-retry loop stays (concurrent checkout
   still races on the same stock rows).

The existing variant-discontinued check, order-number generation,
`ValidateCheckoutPrerequisites`, payment-Completed check,
`MarkPaymentAsPaid`, `Place()`, notification send, and event publish all
stay. Only the stock sourcing changes: from "check + reserve + deduct"
all-at-once to "consume prior reservations."

**Edge case:** if a cart reaches checkout with no reservations (e.g.
reservations expired between intent-creation and checkout), the handler
returns `StockItemResult.Errors.ReservationExpired` (new error) — the
frontend will need to re-run the payment step. Document this in the
Handoff `.md`.

### New MediatR commands (cross-module via `ISender`)

To respect AGENTS.md rule 2, three new commands live in their owning
module and are consumed by the Payment handler via `ISender`:

| Command | Owning module | Purpose |
|---|---|---|
| `GetCartForCheckout` (query) | Ordering | Returns cart's `CheckoutState`, line items (variantId + quantity), total, email. Removes the existing `Module.Ordering.Domain` import from `CreatePaymentIntent`. |
| `ReserveCartStock` (command) | Inventory | Validates stock for all line items; creates `StockReservation` rows; returns reservation IDs or a stock error. |
| `ReleaseCartStockReservations` (command) | Inventory | Releases all non-consumed reservations for a cart by cart ID. Backed by the M5 endpoint behavior. |

Each command has its own vertical-slice feature files
(`Features/{Admin|Storefront}/{...}/{Action}/`) per AGENTS.md rule 3.
`ReserveCartStock` and `ReleaseCartStockReservations` are not
HTTP-exposed (internal services) — they omit `Endpoint.cs`.

### Phase 2B out of scope

- ORD-FR-06 (parallel decoupled payment-state and fulfillment-state
  counters per order) — not addressed; needs its own plan.
- ORD-FR-08 (sequential order numbers in `RepeatableRead`) — already
  implemented in `CreateOrderFromCart.cs:101`; no change.
- Frontend updates — accepted out of scope; the demo path may break
  further because cart-state transitions now require strict ordering. The
  Handoff `.md` documents the new flow.
- M1 route-prefix unification — dropped (see Phase 2A out of scope).

---

## Frontend Integration Handoff document

A standalone `.md` (path: `docs/superpowers/specs/2026-08-04-stripe-frontend-integration-handoff.md`)
captures every API contract the future Stripe.js + storefront UI will
need. Written as part of Phase 1's deliverables so it is ready when
frontend work starts.

Contents:

1. **Storefront payment endpoint reference** — methods, create-intent,
   confirm, setup-intent, webhook; HTTP method, route, request/response
   body shapes, error codes (RFC 7807 Problem Details: 400/401/403/404/
   409/500).
2. **Source-token semantics** — the client obtains a Stripe PaymentMethod
   ID (`pm_...`) via Stripe.js, sends it as `paymentMethodToken` in
   `create-intent`. Clarify the distinction from the local
   `paymentMethodId: Guid` (which selects the local PaymentMethod entity).
3. **`clientSecret` flow** — what the backend returns, how the client uses
   it to confirm/3DS-redirect, where the `ReturnUrl` plugs in.
4. **3DS redirect** — the `ReturnUrl` field, the
   `requires_action`/`PaymentStatus` response field, how the client
   resumes.
5. **New checkout flow** (after Phase 2B) — the strict
   Address→Delivery→Payment→Confirm→Complete ordering, the cart-state
   field, the new error responses for out-of-order transitions and
   expired reservations.
6. **Webhook-driven state transitions** — what the client should poll or
   re-fetch after a 3DS redirect (since capture completion is
   asynchronous via webhook).
7. **Rate limiting** — 30 req/min on payment processing (security-design
   `:23`); clients should back off on 429.
8. **Known gaps the frontend plan must close** — no Stripe.js today,
   `confirm` unused in the active flow, `setup-intent` unused, the
   save-payment-method UI is stubbed (`CheckoutView.vue:262`), transaction
   repository targets a non-existent endpoint (dead code).

---

## Verification

### Phase 1

```bash
dotnet build
dotnet test service/Api/tests/Module.UnitTests --filter Payment
bash scripts/check-cross-module-refs.sh
# End-to-end smoke (user runs, with user-secrets set):
dotnet test --filter Payment
stripe listen --forward-to localhost:5000/api/storefront/webhooks/stripe
stripe trigger payment_intent.succeeded
```

### Phase 2A

```bash
dotnet build
dotnet test service/Api/tests/Module.UnitTests
bash scripts/check-feature-conventions.sh
bash scripts/check-cross-module-refs.sh
```

### Phase 2B

```bash
dotnet build
dotnet test service/Api/tests/Module.UnitTests --filter "Payment|Ordering|Inventory"
dotnet test                                          # incl. integration (requires Docker)
bash scripts/check-feature-conventions.sh
bash scripts/check-cross-module-refs.sh
```

Manual smoke via the new `ApiTests/Payment/*.http` files against a
running API (Aspire orchestration).

### Success criteria

- `GatewayProviders:stripe:Enabled=true` (via user-secrets) → Stripe
  `create-intent` returns a real `clientSecret`, `confirm` transitions
  state to `Completed`, webhook update is idempotent.
- Bogus demo path still works end-to-end (reservations created in
  `create-intent`, consumed in checkout).
- `CreateOrderFromCart` no longer returns `InsufficientStock` *after* a
  captured payment — stock failure surfaces in `create-intent` before any
  gateway call.
- No new `using Module.X.Domain...` violations — `check-cross-module-refs.sh`
  count does not increase (ideally decreases by 1, from removing the
  `Module.Ordering.Domain` import in `CreatePaymentIntent`).
- `dotnet build` stays green (warnings-as-errors).

---

## Open questions to resolve during implementation

1. Where exactly do the `ReserveCartStock` / `ReleaseCartStockReservations`
   commands live — Inventory or Ordering? Tentatively Inventory (they own
   `StockReservation`), but the existing `ReserveCartStock` was wired
   into `AddToCart` (`cc8d1370`) — check whether that path already
   created a `ReserveCartStock` command to reuse.
2. Does `StockReservation` have a "consumed"/"released" status, or are
   rows deleted? Decide the consume/release semantics during
   implementation based on the existing entity shape.
3. Initial `CheckoutState` for a fresh Draft cart — `Address` or a
   `None`/default. Decide based on existing Order construction.
4. Whether `PaymentMethod.AutoCapture` column is read anywhere; if not,
   the M7 seed fix is safe.

These are implementation details discoverable from the codebase, not
design-level forks.