# Stripe Enablement & Storefront API Correction

## Overview

Enable Stripe as a real, working payment gateway and correct the storefront
API to match the thesis (UC-STR-CHK checkout flow + mechanical conformance),
backend only. The frontend (`app/legacy/Storefront`) is explicitly out of
scope; a handoff document captures the API contract for a future Stripe.js
integration plan.

### Phasing

Seven sequenced phases in one spec / one implementation plan. Each earlier
phase is shippable before later phases begin. Phases 1 and 2 together
satisfy UC-STR-CHK + UC-STR-PAY end-to-end; Phases 3A–3G sweep the remaining
modules to thesis conformance.

- **Phase 1 — Stripe Enablement:** make the Stripe gateway reachable
  end-to-end. Fix the two bugs that block all `SourceRequired` gateways
  (Bogus is also broken today), enable Stripe in config, fill the webhook
  gap, verify with the user's test keys.
- **Phase 2 — Storefront API Correction:** mechanical conformance (contract
  drift, dead/stale endpoints, missing endpoints, `.http` tests) + UC-STR-CHK
  flow restructure (inventory reservation coupled with intent creation;
  E3 void-on-stock-failure; strict checkout state machine). **Phase 2B is
  expanded to consolidate the existing cross-module domain violations**
  (move MediatR contracts to `Shared.Application.Contracts`).
- **Phase 3A — Profile conformance + gaps:** `Store/` → `Storefront/` folder
  rename, duplicated `profiles/profiles` route fix, storefront Create
  endpoint, Address FK validation via MediatR.
- **Phase 3B — Inventory bugs:** ReleaseCartReservation authorization gap,
  CountOnHand double-count, remove the duplicate CheckStockAvailability
  fragment, DB-side paging, cartToken propagation.
- **Phase 3C — Shipping conformance + cross-module viols:** zone-filtered
  methods, MediatR cost calculator (eliminate `Order`/`Variant` direct
  references), wire Mapster mapping, paging math fix.
- **Phase 3D — Catalog gaps:** facet counts, Similar products variant
  selection, populate OptionValue1/OptionValue2, breadcrumb array, N+1 in
  availability calculator.
- **Phase 3E — Location minor bugs:** case-insensitive ISO matching,
  regex-constrain `{isoCode}` route, Address FK query (pairs with 3A).
- **Phase 3F — Identity cross-module + minor gaps:** move
  ExternalAuthenticate to Shared contract + ISender, remove GetSession's
  Profile reference, auth consistency, UserId rule, comment drift.
- **Phase 3G — Vertical-slice shared utility move:** shared
  mappings/models/validators used by both Storefront and Admin move to
  `Features/Shared/` rather than living under `Admin/` (Profile, Location).

### Demo success criterion

After all phases, the backend end-to-end demo (exercised via the new
`ApiTests/**/*.http` files, since frontend is out of scope) satisfies the
thesis use cases:

- **UC-STR-CRT (cart):** add-to-cart, update/remove, guest→user cart
  merge (`/api/storefront/cart/associate`), empty, delete.
- **UC-STR-CHK (checkout):** strict Address→Delivery→Payment→Confirm→
  Complete state machine; stock reserved when the payment intent is
  created; E3 void-on-post-gateway-failure; order placed; notification
  sent.
- **UC-STR-PAY (payment):** Stripe `create-intent` returns a real
  `clientSecret`; webhook-driven state transitions (idempotent); Bogus
  demo path still works end-to-end.
- **UC-STR-OHI (order history):** list/detail/cancel-with-void.
- **UC-ADM-PAY (admin payments):** capture/refund/void still reachable.

`.http` files cover the full flow: cart → update-checkout →
select-shipping-rate (zone-filtered) → create-intent (with reserved
stock) → confirm/checkout → webhook replay (Stripe test mode).

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
  validation), UC-STR-CHK step 3 + E3, plus removal of existing
  cross-module domain violations in `CreateOrderFromCart` /
  `AddToCart` / `ExternalAuthenticate` / `GetSession` /
  `CalculateShipping` (AGENTS.md rule 2).
- Phase 3A: Profile conformance (AGENTS rule 3 Storefront naming; route
  defect; missing Create endpoint; Address FK integrity via MediatR).
- Phase 3B: Inventory correctness + security (ReleaseCartReservation
  authorization; CountOnHand accounting; duplicate fragment removal;
  paging; cartToken propagation).
- Phase 3C: Shipping conformance (UC-STR-CHK delivery step: zone-filtered
  methods); cross-module viols in `CalculateShipping` removed; Mapster
  wired; paging math.
- Phase 3D: Catalog gaps (filter facet counts for the browse UI;
  Similar-products determinism; variant option-value mapping; product
  detail breadcrumb; availability calculator N+1).
- Phase 3E: Location case-insensitivity + route constraint (UC-STR-BRW
  taxonomy navigation by ISO code).
- Phase 3F: Identity cross-module cleanup (ExternalAuthenticate → Shared
  contract + ISender; GetSession Profile reference); auth consistency;
  validator gaps; comment drift.
- Phase 3G: Vertical-slice shared-utility move (AGENTS rule 3 isolation
  between Storefront and Admin slices; Profile + Location).

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

The handler loses its inline stock-deduction loop and replaces the four
existing direct cross-module domain references —
`CreateOrderFromCart.cs:5` (`Module.Catalog.Domain.Products.Variants`),
`:6-8` (three Inventory domain namespaces),
`:12` (`Module.Payment.Domain.PaymentCaptures`) — with mediated calls
through `Shared.Application.Contracts`. The handler keeps only the
`Module.Ordering.Domain.Orders` and ordering-local references.

Responsibilities after restructure:

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

### New MediatR contracts (cross-module via `ISender`)

The existing `ReserveCartStock` feature command types live in Inventory's
feature namespace (`Module.Inventory.Features.Storefront.CartReservations.
Reserve`), so `AddToCart.cs:6` and `CreateOrderFromCart.cs` reference
Inventory's feature namespace directly — a cross-module violation even
though dispatch is via MediatR. Phase 2B moves the request/response
contracts to `Shared.Application.Contracts.Inventory` (parallel to the
existing `Shared.Application.Contracts.Profile.CreateUserProfileCommand`
pattern that `ConfirmEmail.cs:117-125` already uses correctly).

To respect AGENTS.md rule 2, contracts live in `Shared.Application.Contracts`
and producing modules own the handlers. Consumers dispatch via `ISender`:

| Contract | Owned by | Consumed by | Purpose |
|---|---|---|---|
| `GetCartForCheckoutQuery` / Response | Ordering | Payment (`CreatePaymentIntent`) | Returns cart's `CheckoutState`, line items (variantId + quantity), total, email. Removes the existing `Module.Ordering.Domain` import from the Payment handler. |
| `ReserveCartStockCommand` / Response | Inventory | Ordering (`AddToCart`), Payment (`CreatePaymentIntent`) | Validates + reserves stock; returns reservation IDs or a stock error. Replaces the existing `ReserveCartStock.Command` referenced from `AddToCart.cs:83-93`. |
| `ReleaseCartStockReservationsCommand` | Inventory | Payment (`CreatePaymentIntent` on downstream failure) | Releases non-consumed reservations for a cart. Backed by the M5 endpoint behavior. |
| `ConsumeCartStockReservationsCommand` | Inventory | Ordering (`CreateOrderFromCart`) | Converts existing reservations into `Pick` + `StockMovement` + marks reservations consumed. Replaces the inline block `CreateOrderFromCart.cs:108-154`. Eliminates the four Inventory + the Catalog + the Payment domain references in that file. |
| `GetVariantDiscontinuedStatusesQuery` | Catalog | Ordering (`CreateOrderFromCart`) | Replaces `CreateOrderFromCart.cs:82-85` (`dbContext.Set<Variant>()`) direct Catalog reference. |
| `GetPaymentForCheckoutQuery` | Payment | Ordering (`CreateOrderFromCart`) | Replaces `CreateOrderFromCart.cs:64-67` (`dbContext.Set<PaymentCapture>()`) direct Payment reference. |
| `MarkPaymentPaidCommand` | Payment | Ordering (`CreateOrderFromCart`) | Replaces the `cart.MarkPaymentAsPaid()` direct-state reference if cart-state lives in Ordering; decision during impl. |

Each producing module's vertical-slice feature files live under
`Features/Storefront/{...}/{Action}/` per AGENTS.md rule 3. The four
internal commands (not HTTP-exposed) omit `Endpoint.cs`; the queries are
internal-only too (no `Endpoint.cs`) — they are dispatched only from other
modules. Validators stay.

Phase 2B removes these existing cross-module references
(`check-cross-module-refs.sh` count should drop measurably):

- `CreateOrderFromCart.cs:5-8,12` (4 Inventory + 1 Catalog + 1 Payment)
- `AddToCart.cs:5-6` (1 Inventory domain + 1 Inventory feature)
- `CreatePaymentIntent.cs:3` (`Module.Ordering.Domain.Orders.Order`)
- `ExternalAuthenticate.cs:3-4` (`Module.Profile.Domain` +
  `Module.Profile.Features.Store.Profiles.Create`) — moved to Phase 3F
- `GetSession.cs:3` (`Module.Profile.Domain`) — moved to Phase 3F
- `CalculateShipping.cs:1,43` (`Module.Ordering.Domain`,
  `Module.Catalog.Domain`) — moved to Phase 3C

### Phase 2B out of scope

- ORD-FR-06 (parallel decoupled payment-state and fulfillment-state
  counters per order) — not addressed; needs its own plan.
- ORD-FR-08 (sequential order numbers in `RepeatableRead`) — already
  implemented in `CreateOrderFromCart.cs:101`; no change.
- Frontend updates — accepted out of scope; the demo path may break
  further because cart-state transitions now require strict ordering. The
  Handoff `.md` documents the new flow.
- M1 route-prefix unification — dropped (see Phase 2A out of scope).
- Identity and Shipping cross-module violations — handled by Phase 3F and
  Phase 3C respectively, not Phase 2B.

---

## Phase 3A — Profile conformance + gaps

### 3A.1 `Store/` → `Storefront/` folder rename

Profile's storefront features live under
`service/Api/src/Module/Profile/Features/Store/`. AGENTS.md rule 3 mandates
the subdirectory name is always `Storefront`. Identity and Location already
use `Storefront/`.

**Change:** mass move `Profile/Features/Store/**` → `Profile/Features/
Storefront/**`; update `ProfileFeature.cs` route-prefix file and all
`*.Endpoint.cs` namespace declarations. Build stays green. The
`api/store/profiles` route prefix is unchanged (route-prefix unification is
the dropped M1; the doubled `profiles/profiles` segment is fixed below).

### 3A.2 Duplicated `profiles/profiles` route segment

`ProfileFeature.cs:21,61,101,122,182,227` constructs
`Profiles.BaseRoute = "api/store/profiles" + "/profiles"`, producing
`api/store/profiles/profiles` for every Profile Get/Update/Delete
operation. **Defect.** Affects all six Profile storefront + admin Profile
endpoints (Addresses, Wishlists, NotificationPreferences sit one level
below and inherit the doubled segment).

**Change:** collapse the base route to `api/store/profiles` and make the
Profiles subroutes live under the empty `/` (or rename the module's
storefront surface to `Profile` singular). Decide during impl based on
which produces a cleaner route tree; the requirement is no duplicated
segment. Update the corresponding frontend constants in a future plan
(out of scope here).

### 3A.3 Missing storefront Create endpoint

`Store/Profiles/Create/CreateProfile.cs` exists with its handler and
validator but has **no `CreateProfile.Endpoint.cs`** — there is no
`POST /api/store/.../profiles`. Today profile creation happens only
indirectly from Identity flows (register, external login, email-confirm)
via `Shared.Application.Contracts.Profile.CreateUserProfileCommand`.

**Decision:** the thesis lists "Profiles CRUD" under the Profile module
(api-design.typ:68). Two readings:

- **Implicit-via-auth creation is acceptable** (thesis use case UC-STR-AUT
  creates the profile as a side-effect of register/external-login). The
  `CreateProfile.Endpoint.cs` is intentionally absent — keep current.
  Add a doc comment in `CreateProfile.cs` noting it is only consumed via
  the Identity flow, not an HTTP endpoint.
- **A storefront Create endpoint is required.** Add
  `CreateProfile.Endpoint.cs` exposing `POST /api/store/profiles` (after
  the 3A.2 route fix) guarded by `.RequireAuthorization()`. Use case:
  cosmetic / alternate creation path; lower priority than the auth flow.

Phase 3A picks the second so the storefront surface is CRUD-complete per
the thesis. Add `IdentityFeature.Store`-style route wiring + the
`.Endpoint.cs` file binding the existing handler. Validate-only auth-no-
extra-context; the existing `CreateProfile.Validator.cs` is reused.

### 3A.4 Address CountryCode/StateCode FK validation

`Admin/Addresses/Shared/Validators/Address.Validator.cs:11-15` validates
only `FirstName`, `Address1`, `City`, `CountryName`. `CountryCode` and
`StateCode` are persisted unchecked — a user can submit `CountryCode="ZZ"`
and the address saves. Module isolation (rule 2) forbids direct queries to
`Location.Country/State`, so validation must go via MediatR.

**Change:** introduce two `Shared.Application.Contracts.Location` queries:
- `CountryExistsByIsoQuery(string isoCode)` → bool
- `StateExistsByIsoQuery(string countryCode, string stateCode)` → bool

Owned by Location (handlers under `Location/Features/Shared/...`), consumed
by Profile's Address `Validator` via `ISender`. Validator calls them when
`CountryCode`/`StateCode` are non-null; returns RFC 7807 400 on invalid
FK.

Phase 3E owns the Location-side additions; 3A consumes them. Coordinate
the contract introduction in 3A, the handler implementation in 3E.

### Phase 3A out of scope

- Address Create's `MaxAddressesCount` / `MaxAddressesCountPerType`
  business rules are already enforced — no change.
- Wishlist `AddItem` VariantId FK validation against Catalog (acceptable
  under rule 2 today; flagged in CONCERNS.md, not addressed here).

---

## Phase 3B — Inventory bugs (security + correctness)

### 3B.1 ReleaseCartReservation authorization gap (SECURITY)

`ReleaseCartReservation.cs:17-18` looks up the reservation by `Id` only —
no `CartToken` filter, no auth check. **Anyone with a reservation Guid can
DELETE any other cart's reservation.**

**Change:** filter by both `Id` and `CartToken` (pulled from `X-Cart-Token`
header / claim, same fallback chain as `ReserveCartStock.Endpoint.cs`).
Unknown reservation or token mismatch → 404 (avoid enumeration). Add
`ReleaseCartReservation.Validator.cs` and a `Request`/`Response` to make
the vertical slice complete (currently missing — AGENTS rule 3).

### 3B.2 CountOnHand double-count (CORRECTNESS)

`ReleaseCartReservation.cs:37` does `stockItem.CountOnHand +=
reservation.Quantity`, restoring stock that `ReserveCartStock.cs` **never
decremented** (Reserve only sums active reservations; it never calls
`StockItem.Pick` ormutating `CountOnHand`). Phantom stock restoration —
`StockItem.CountOnHand` will drift upward after each reserve→release
cycle.

**Change:** pick a single accounting model.

- **Option (recommended):** Reserve **does not** decrement `CountOnHand`
  (a reservation is a soft hold); Release **does not** increment it. The
  active-reservation sum is the source of truth for "available to
  reserve." Remove the `+= reservation.Quantity` line. Then
  `CreateOrderFromCart`'s `StockItem.Pick(take)` (which actually decrements
  `CountOnHand`) is the only writer; release never touches it.
- Alternative: Reserve **does** decrement `CountOnHand`.'s `StockItem.Pick`
  becomes idempotent on consume. More invasive; rejected for risk.

Document the chosen accounting in a `PaymentCapture.Constant.cs`-style
constant file and in `docs/codebase/ARCHITECTURE.md` (out of this plan's
file-edit scope only if a small addition — otherwise a CONCERNS.md note).

### 3B.3 Remove duplicate `CheckStockAvailability` fragment

`Inventory/Features/Storefront/StockAvailability/CheckStockAvailability/`
has a parallel half-built `CheckStockAvailability` (Query imports
`Module.Inventory.Services.Abstractions` while the active `Check/`
implementation imports `Module.Inventory.Services`). Two availability
features with drift.

**Change:** delete the `CheckStockAvailability/` directory tree after
confirming nothing references it (grep `CheckStockAvailability` across
`service/`). If referenced by tests, redirect to `Check/`. Otherwise raw
delete.

### 3B.4 GetCartReservations DB-side paging

`GetCartReservations.cs:21-44` materializes all active reservations
then applies paging in memory. Push paging to the DB (`Skip`/`Take`
before `ToListAsync`).

### 3B.5 cartToken propagation into availability calculator

`GetStockAvailability.Request.cs:8` captures `cartToken` but
`GetStockAvailability.cs` never forwards it to
`IStockAvailabilityCalculator.GetForVariantAsync`. The class doc says
"cart-specific holds" but they are not applied. Either forward the token
so already-reserved quantity for this cart is excluded from "available,"
or remove the unused field + doc. Phase 3B removes the unused field to
keep behavior explicit; future plan can re-introduce it with semantics.

### 3B.6 RepeatableRead is not serializable

`ReserveCartStock.cs:31` uses `IsolationLevel.RepeatableRead` with a
header comment saying "serializable." `RepeatableRead` does not take row
locks on reads in PostgreSQL/Npgsql + EF Core — concurrent reservations
between `SumAsync` and `SaveChangesAsync` can both pass the availability
check and both insert. **Oversell window.**

**Change:** switch to `IsolationLevel.Serializable` for the reserve
transaction, OR add `SELECT ... FOR UPDATE` semantics by issuing an
explicit row lock on the `StockItem` row (e.g. `FROM stock_items WHERE id
= ... FOR UPDATE` via raw SQL or `EF.Functions`). Decide during impl;
serializable is the simpler change, watch for deadlocks under high
concurrency and the existing retry loop extension.

### Phase 3B out of scope

- `GetStockAvailability` validator absence (read-only query, AC permits
  omission).
- Mapster for inventory storefront DTOs (inline projections are fine).

---

## Phase 3C — Shipping conformance + cross-module viols

### 3C.1 Zone-filtered methods (UC-STR-CHK delivery step)

`GetShippingMethods.cs:19` returns all `AvailableToUsers && !IsDeleted`
methods **regardless of buyer address**. Thesis UC-STR-CHK
(`ordering.typ:102-107`) says "System calculates available shipping methods
and rates based on address zone, cart weight, and cart value." Today none
of those dimensions filter the list.

**Change:** `GetShippingMethods.Parameters.cs` accepts an optional
`ZoneId`/`CountryCode` (or the existing shipAddressId from the cart) and
filters by zone availability. The exact filter predicate depends on
whether `ShippingMethod` has a `Zones` collection — verify during impl;
if no zone association exists yet, add it (migration + seeder). Fallback
to "all active methods" when no zone is supplied (admin / pre-address
cart browsing).

### 3C.2 MediatR cost calculator (eliminate cross-module viols)

`CalculateShipping.cs:1` has `using Module.Ordering.Domain.Orders` and
`:43` queries `dbContext.Set<Catalog.Domain.Products.Variants.Variant>`
inline — two clear cross-module violations. The handler reads the
buyer's cart (Ordering) and variant weights (Catalog) directly.

**Change:** introduce two contracts in `Shared.Application.Contracts`:

- `GetCartForShippingQuery(Guid cartId)` → `{ TotalWeight, TotalValue,
  ShipAddressId, Currency }` owned by Ordering.
- `GetVariantWeightsQuery(IEnumerable<Guid> variantIds)` →
  `Dictionary<Guid, (Weight, WeightUnit)>` owned by Catalog.

`CalculateShipping` dispatches both via `ISender`, never touching either
module's domain entities. Eliminates lines 1 and 43. (The
`CreateOrderFromCart.cs:5` Catalog reference is removed by Phase 2B's
`GetVariantDiscontinuedStatusesQuery`; the CalculateShipping Catalog
reference is the second instance and is removed here.)

### 3C.3 Wire the defined-but-unused Mapster mapping

`Shared/Mappings/ShippingMethod.Mapping.Model.cs:8-36` defines
`MapToDetail` and `MapToListItem` but `GetShippingMethods.cs:25-33`
builds the DTO via inline `Select`. Wire the mapping instead of the
inline rebuild. Delete the empty stub
`ShippingMethod.Mapping.Domain.cs:4-6` ("go here in future updates"
comment) if the Domain → Model mapping is genuinely unused — or fill it
with the inverse mapping if it has a purpose.

### 3C.4 GetShippingMethods paging math bug

`GetShippingMethods.cs:37` passes `Math.Max(1, items.Count)` as the
page count when `pageModel.IsEmpty`. Should be `(items.Count + pageSize -
1) / pageSize` or the no-page path should set `TotalPages = 1` when
`items.Count > 0`. Fix the math; minor but visible.

### Phase 3C out of scope

- Routing of shipping rates to a buyer-selected method (already handled by
  `POST /api/storefront/cart/shipping-rate` in Ordering).
- The shipping rate calculation algorithm itself — the thesis leaves it
  open; this phase only ensures the API conforms, not a new rate engine.

---

## Phase 3D — Catalog gaps (facets + similar + mappings)

### 3D.1 Facet counts for the browse UI

`GetStorefrontProducts.Response.cs:7` returns `PagedResult<Response>`
only — no facet aggregation. The thesis filter UI (`catalog.typ:36-37`,
`frontend-ux.typ` filter panel) surfaces per-`optionValueId` and per-
`taxonId` counts so disabled facets render. Grep across `service/Api/src`
returns zero `facet|Facet` matches — the capability is absent.

**Change:** add an optional `IncludeFacets` flag to the request. When
true, the response includes a `Facets` aggregate: per-active filter value,
the result count if that filter were applied剔除the current values of
the same facet. The straightforward implementation: re-run the same
query with all current filters except facet X, grouped by X's values.
Cost is N+1 queries per N facets; acceptable for a list endpoint with a
small N (taxonomy + option types). Cache with HybridCache if perf
warrants; defer the cache here, ship the aggregation first.

### 3D.2 Similar-products determinism

`GetSimilarProducts.cs:27-30` picks the first variant by
`FirstOrDefaultAsync(x => x.ProductId == request.Id && !x.IsDeleted)` —
arbitrary without ordering or `IsMaster`/`Position`. If the first
variant lacks a `Search`-type image embedding the search returns empty
(line 46). **Non-deterministic result.**

**Change:** order by `IsMaster descending, Position asc` (or whichever
field expresses "primary variant"); pick the variant with a `Search`-
type image embedding if one exists, else fall back to master. Response
DTO optionally includes which variant's embedding was used (debug aid).

### 3D.3 Populate `OptionValue1`/`OptionValue2` in variant DTO

`StoreProductVariantResponse` (ProductStorefront.Model.Response.cs:21-31)
declares `OptionValue1` / `OptionValue2` but `MapToStoreVariant`
(`ProductStore.Mapping.cs:46-62`) never assigns them. The handler
already includes `OptionValueVariants.ThenInclude(ov => ov.OptionValue)`
(`GetProductDetail.cs:30-31`) — the data is loaded, the mapping drops it.

**Change:** populate the two fields from the loaded
`OptionValueVariants` collection. Use deterministic positioning
(OptionValue1 ← first by OptionType.Position, OptionValue2 ← second);
consider a `OptionValues: []` array for >2 option values if the thesis
spec calls for arbitrary-depth variant dimensions — check
`catalog.typ:42-48` (UC-STR-BRW variant selection) during impl.

### 3D.4 Product detail breadcrumb

`StoreProductTaxonResponse` (ProductStorefront.Model.Response.cs:42-48)
has `Id/Name/Permalink/Depth` but no parent-chain breadcrumb. The
thesis product-detail view (`catalog.typ:43`) shows a taxonomy
breadcrumb. Client currently reassembles from `GetAllTaxons` (Depth +
ParentId).

**Change:** add `Breadcrumb: TaxonBreadcrumbItem[]` to the taxon DTO.
Backend resolves the chain server-side by walking the taxon's parent
chain (the taxonomy tree is small; N+1 is acceptable here, or
pre-loadable in a single query). Each breadcrumb item carries
`{ Id, Name, Permalink }`.

### 3D.5 GetAvailability N+1

`GetAvailability.cs:95-97` calls
`calculator.GetForVariantAsync` per out-of-stock variant sequentially
inside a `foreach` loop — N inventory calls for N OOS variants.

**Change:** add a batch entry to `IStockAvailabilityCalculator`
(`GetForVariantsAsync(IEnumerable<Guid> variantIds, ...)`) that fetches
metrics for the variant set in one query. Fall back to the per-variant
call only for variants whose metrics the batch call could not cover.
Performance-only, no behavioral change.

### Phase 3D out of scope

- Catalog admin endpoints (out of thesis storefront scope).
- The deeper schema design of `OptionValueVariants` position semantics
  (the existing shape is enough; this phase only wires the mapping).

---

## Phase 3E — Location minor bugs

### 3E.1 Case-insensitive ISO matching

`GetStorefrontCountryByIso.cs:23-24` compares `c.IsoCode ==
request.IsoCode` with ordinal case-sensitive equality. ISO 3166-1 codes
are conventionally uppercase; a client passing `"us"` gets a 404 instead
of finding `"US"`. Same in `GetStorefrontStateByIso.cs:23-24`
(`s.Abbreviation == request.IsoCode`).

**Change:** upper-case both sides before comparison (`c.IsoCode.ToUpper()
== request.IsoCode.ToUpper()`) or use `EF.Functions.ILike` for case-
insensitive matching. Apply consistently to country and state ISO
endpoints.

### 3E.2 Regex-constrain `{isoCode}` route

`GetStorefrontCountryByIso.Endpoint.cs:13` and `...StateByIso...`
declare `{isoCode}` unconstrained; a 50-char payload hits the DB query.

**Change:** constrain to `regex("^[A-Za-z]{2,3}$")`
(`{isoCode:regex("^[A-Za-z]{2,3}$")}`) for countries (ISO 3166-1 alpha-2
or alpha-3) and `^[A-Za-z0-9]{1,5}$` for states (ISO 3166-2 subdivision
codes vary). 400-route-match failure for invalid lengths; no DB hit.

### 3E.3 CountryExistsByIso + StateExistsByIso query handlers

Owned by Location, consumed by Profile's Address validator (Phase 3A.4).
Live under `Location/Features/Shared/...` (no `Endpoint.cs`; internal
MediatR queries).

### Phase 3E out of scope

- Renaming `Abbreviation` to `IsoCode` on the State domain entity —
  consistency improvement but a wider schema change; defer.

---

## Phase 3F — Identity cross-module + minor gaps

### 3F.1 ExternalAuthenticate → Shared contract + ISender

`ExternalAuthenticate.cs:3-4` has `using Module.Profile.Domain; using
Module.Profile.Features.Store.Profiles.Create;` and `:30,162` injects
`IMediator` + sends `CreateProfile.Command` directly. This is a
gratuitous Rule 2 violation — the existing `Shared.Application.Contracts.
Profile.CreateUserProfileCommand` pattern (consumed by `ConfirmEmail.cs:
22,117-125` via `IMediator`) is the right shape. Note `IMediator` vs
`ISender`: AGENTS specifies `ISender` as the dispatch surface; the
ConfirmEmail file also uses `IMediator` so the codebase is inconsistent
on that point. Phase 3F standardizes on `ISender`.

**Change:**
- Delete the two `using Module.Profile.*` lines.
- Replace `IMediator.Send(new CreateProfile.Command(...))` with
  `ISender.Send(new CreateUserProfileCommand(...))` (the existing
  contract). Inject `ISender` in the constructor alongside the others.
- Profile-side `CreateProfile.cs` retains its existing handler
  (`CreateUserProfileCommandHandler`) as the message handler bound in
  MediatR's assembly scan.

### 3F.2 Remove `GetSession.cs:3` Profile reference

`GetSession.cs:3` has `using Module.Profile.Domain` to use
`UserProfileResult.Failure.AuthRequired`. Move that error into
`Shared.Application.Contracts.Identity` (or have Identity return its own
`AuthRequired` error and drop the cross-module reference entirely).

### 3F.3 Auth consistency

- `Logout.Endpoint.cs:20` is `.AllowAnonymous()` — logout is an
  authenticated action. Change to `.RequireAuthorization()`. The handler
  already re-checks `currentUser.IsAuthenticated` (`Logout.cs:42`).
- `GetSession.Endpoint.cs:19` is `.AllowAnonymous()` for the same
  reason. Change to `.RequireAuthorization()`; clients fetching the
  session endpoint are by definition the authenticated user.

### 3F.4 ResetPassword validator gap

`ResetPassword.Validator.cs:10-16` validates `Token` + `NewPassword` but
not `UserId`. `ResetPassword.cs:37` reads `Request.UserId` (non-nullable
`Guid`); model binding rejects missing values, but an explicit
`RuleFor(x => x.UserId).NotEmpty()` matches `ConfirmEmail.Validator.cs:
14` and documents the contract. Add the rule.

### 3F.5 Comment drift

`ResendEmailVerification.Endpoint.cs:10` comments `/resend-verification`
but the route constant resolves to `/resend`. Fix the comment.

### 3F.6 Token security options verification (not a code change —

configuration check)

Confirm `TokenSecurityOptions.RotationEnabled` and `ReuseDetectionEnabled`
are `true` in `appsettings.json` / Development for IDN-FR-04/05 to be
live. The Phase 3F deliverable is a single one-line bump if either is
currently false (most likely already true; verify). Document the expected
values in the Handoff `.md` so production deploys do not accidentally
ship with rotation/reuse detection off.

### Phase 3F out of scope

- Phones change/confirm/resend (not part of the thesis storefront auth
  contract per `api-design.typ:53`; flagged in CONCERNS.md separately).

---

## Phase 3G — Vertical-slice shared utility move

The `Admin/.../Shared/` folders contain mappings/models/validators used
by both `Admin` and `Storefront` slices. Per AGENTS rule 3 the
vertical-slice isolation is between actions, but `Shared` utilities
couple the storefront slice to the admin namespace.

Concrete instances (non-exhaustive, same pattern in every affected
module):

- Profile: `CreateAddress.cs:3`, `UpdateProfile.cs:2`,
  `GetNotificationPreferences.Response.cs:1` import
  `Module.Profile.Features.Admin.Addresses.Shared.*` / `Admin.Profiles.
  Shared.*`.
- Location: `GetStorefrontCountryById.cs:2`,
  `GetStorefrontStateByIso.Response.cs:1` import
  `Module.Location.Features.Admin.{Countries,States}.Shared.*`.

**Change:** for each module, move the cross-slice `Shared/` utilities to
`{Module}/Features/Shared/...` (or
`{Module}/Features/{Domain}/Shared/...` if that better matches the
existing shape). Update all imports. The `Admin/` and `Storefront/`
slices both then reference the shared root, never each other.

Risk: large diff (mechanical namespace moves); test coverage on both
slices is the safety net. Keep the move to Profile + Location in this
phase (the two confirmed instances). Other modules may share the same
pattern — check during impl and apply the same move opportunistically,
but do not expand scope beyond Profile + Location without approval.

### Phase 3G out of scope

- Moving the `Admin.Orders.Shared.Mappings` that `CreateOrderFromCart`
  currently imports (intra-module Admin↔Storefront — same pattern, but
  in Ordering; deferred to keep this phase bounded).

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
bash scripts/check-cross-module-refs.sh              # count must DROP vs. baseline
```

Manual smoke via the new `ApiTests/Payment/*.http` files against a
running API (Aspire orchestration).

### Phase 3A–3G

```bash
dotnet build
dotnet test service/Api/tests/Module.UnitTests
bash scripts/check-feature-conventions.sh
bash scripts/check-cross-module-refs.sh              # count must keep dropping
cd app/Admin && pnpm run lint && pnpm run test:unit  # if any shared types are referenced
```
Phase-specific:
- **3A** `dotnet test --filter Profile` + verify Address validator rejects invalid ISO codes via the new MediatR queries.
- **3B** `dotnet test --filter Inventory`; integration test for concurrent `ReserveCartStock` calls (serializable, no oversell).
- **3C** `dotnet test --filter Shipping`; the cost calculator handler no longer imports `Module.Ordering.Domain` or `Module.Catalog.Domain`.
- **3D** `dotnet test --filter Catalog`; verification: response includes `Facets` when `IncludeFacets=true`; Similar endpoint returns deterministic variant; `MapToStoreVariant` populates `OptionValue1/2`.
- **3E** `dotnet test --filter Location`; `.http` test: lowercase `"us"` resolves; `/by-iso/ab` (3 chars) resolves; `/by-iso/abcd` returns 400 route-match.
- **3F** `dotnet test --filter Identity`; cross-module-refs: Identity module count drops by the two known references.
- **3G** `dotnet build` after the namespace moves; grep `Module.Profile.Features.Admin` and `Module.Location.Features.Admin` inside `Storefront/` directories returns zero matches.

### Demo success criterion

After all phases, the backend end-to-end demo — exercised via the new
`ApiTests/**/*.http` files (frontend out of scope) — satisfies the thesis
use cases:

- **UC-STR-CRT (cart):** add-to-cart, update/remove, guest→user cart
  merge (`POST /api/storefront/cart/associate`), empty, delete.
- **UC-STR-CHK (checkout):** strict Address→Delivery→Payment→Confirm→
  Complete state machine; stock reserved when the payment intent is
  created; E3 void-on-post-gateway-failure; order placed; notification
  sent. Out-of-order transitions return 409 with a clear state error.
- **UC-STR-PAY (payment):** Stripe `create-intent` (with a Stripe test
  PaymentMethod token) returns a real `clientSecret`; webhook-driven
  state transitions (idempotent); Bogus demo path still works
  end-to-end. 30 req/min rate limit enforced.
- **UC-STR-OHI (order history):** list (paged, reverse-chrono),
  detail (line items, payment state, shipment state, status timeline),
  cancel (releases inventory + voids payment).
- **UC-STR-BRW / UC-STR-SRC (catalog):** browse returns facet counts;
  keyword search; product detail carries breadcrumb + populated variant
  option values; similar products returns deterministic variant; product
  by slug resolves.
- **UC-STR-AUT / UC-STR-SES (identity):** password login issues rotated
  refresh + access; reuse of a revoked refresh token revokes all;
  Google OAuth creates the profile via Shared contract; logout
  requires auth and invalidates refresh tokens. Country/State by ISO
  resolves case-insensitively.
- **UC-ADM-PAY (admin payments):** capture/refund/void still reachable
  with idempotency keys and state-machine guards.

A demo `.http` runbook exercises the full flow in order: login →
create-cart → add-items → ship-address → update-checkout →
select-shipping-rate (zone-filtered) → create-intent (with reserved
stock) → confirm-or-checkout → webhook replay (Stripe test mode) →
list-orders → cancel-order.

### Success criteria

- `GatewayProviders:stripe:Enabled=true` (via user-secrets) → Stripe
  `create-intent` returns a real `clientSecret`, `confirm` transitions
  state to `Completed`, webhook update is idempotent.
- Bogus demo path still works end-to-end (reservations created in
  `create-intent`, consumed in checkout).
- `CreateOrderFromCart` no longer returns `InsufficientStock` *after* a
  captured payment — stock failure surfaces in `create-intent` before any
  gateway call.
- `check-cross-module-refs.sh` count drops vs. baseline; no new
  `using Module.X.Domain...` violations.
- `dotnet build` stays green (warnings-as-errors).
- ReleaseCartReservation no longer accepts arbitrary reservation GUIDs
  (cartToken-filtered + auth).
- Catalog `GetStorefrontProducts?includeFacets=true` returns the
  `Facets` aggregate; `GetSimilarProducts` is deterministic;
  `MapToStoreVariant` populates `OptionValue1/2`.
- Shipping `GetShippingMethods` honors destination zone;
  `CalculateShipping` has no `Module.Ordering.Domain` or
  `Module.Catalog.Domain` imports.
- Location ISO-code resolution is case-insensitive + route-constrained.
- Identity `ExternalAuthenticate` has no `Module.Profile.*` imports;
  `Logout` / `GetSession` are `.RequireAuthorization()`.
- Profile `Features/Store/` no longer exists (renamed to `Storefront/`);
  store→admin shared-utility imports gone (Profile, Location).

---

## Open questions to resolve during implementation

1. Where exactly do the `ReserveCartStock` / `ReleaseCartStockReservations`
   / `ConsumeCartStockReservations` contracts live — `Shared.Application.
   Contracts.Inventory` (preferred; parallel to the existing
   `Profile.CreateUserProfileCommand`). The existing `ReserveCartStock`
   was wired into `AddToCart` (commit `cc8d1370`) and currently lives in
   Inventory's feature namespace — its request/response types move to
   Shared; the handler stays in Inventory.
2. Does `StockReservation` have a "consumed"/"released" status, or are
   rows deleted? Decide the Consume/Release semantics during
   implementation based on the existing entity shape. Likely a state
   column given the existing `Reserved` filter in `GetCartReservations`;
   add `Consumed`/`Released` if absent.
3. Initial `CheckoutState` for a fresh Draft cart — `Address` or a
   `None`/default. Decide based on existing Order construction in the
   cart-creation handler.
4. Whether `PaymentMethod.AutoCapture` column is read anywhere; if not,
   the M7 seed fix is safe.
5. Whether `ShippingMethod` has a `Zones` collection (3C.1 depends on it);
   if not, add the association table + seeder in Phase 3C.
6. How many Catalog `OptionTypes` a variant can carry (3D.3). Today the
   DTO has `OptionValue1/2`; if the thesis implies arbitrary-depth
   variant dimensions, replace with an `OptionValues: []` array. Confirm
   against `catalog.typ:42-48` (UC-STR-BRW variant selection) during
   impl.
7. `IMediator` vs `ISender` (3F.1) — AGENTS specifies `ISender`; existing
   `ConfirmEmail.cs` uses `IMediator`. Phase 3F standardizes on `ISender`
   for the two cross-module paths it touches; a wider sweep is out of
   scope.
8. `TokenSecurityOptions.RotationEnabled` / `ReuseDetectionEnabled`
   values in `appsettings` — verify and document in the Handoff `.md`.

These are implementation details discoverable from the codebase, not
design-level forks.