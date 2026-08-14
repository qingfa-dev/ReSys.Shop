---
goal: Correct and align shipping rates/adjustments, Cart request/response contracts, Cart/Order mappings, and the related Admin and Storefront UI so the server-side applied shipping adjustment is the single authoritative value end-to-end.
version: 1.0
date_created: 2026-08-15
last_updated: 2026-08-15
owner: Ordering team
status: 'Planned'
tags: [refactor, shipping, cart, ordering, store, admin, api, mapping, bug]
---

# Introduction

![Status: Planned](https://img.shields.io/badge/status-Planned-blue)

The storefront currently reconstructs the authoritative shipping amount on the client: `CheckoutView` computes `total = cart.subtotal + (rate.finalPrice ?? rate.cost)` from the raw rates endpoint (`app/Store/src/features/ordering/views/CheckoutView.vue:185-187,231`), so the delivery step can label a method `Free` (`isMethodFree`, `:177-182`) while the review panel shows the raw rate price (`:498`). Meanwhile the backend already computes the applied shipping cost via `ShippingRateCalculator.CalculateAsync` and persists it as an `Order.Adjustment` (`ReplaceShippingAdjustment`) feeding `ShipmentTotal` and `Total`, but the Cart API response exposes only a lumped `Total` (`Cart.Model.Response.Base.cs:13`) — no `ShipmentTotal`/`AdjustmentTotal`/adjustment metadata — so neither frontend can display the server's breakdown, and the Admin order flow (`UpdateOrderShippingMethod`) never replaces the shipping adjustment on method change, leaving `ShipmentTotal` stale. This plan creates ONE authoritative calculation path — `Shipping Rates → Server Calculation → Shipping Adjustment → ShipmentTotal → Cart/Order Total → API Response → Admin/Storefront UI` — and corrects the Cart/Order contracts, mappings, and UI to consume it.

## 1. Requirements & Constraints

- **REQ-001**: The server-side shipping calculation is the single authoritative source; `ShipmentTotal` must always equal the applied eligible shipping adjustment, and `Total` must equal `ItemTotal + ShipmentTotal + AdjustmentTotal` (server-computed).
- **REQ-002**: A free-shipping threshold met by the cart must produce an actual `0` shipping adjustment (`ShipmentTotal == 0`), not a raw-rate amount.
- **REQ-003**: Changing a shipping method on an order (Admin or Storefront) must replace the shipping adjustment with the newly calculated cost — never leave the previous method's amount applied.
- **REQ-004**: The Cart API response must expose `Id`, `ItemCount`, `Currency`, `ItemTotal`, `ShipmentTotal`, `AdjustmentTotal`, `Total`, `ShippingMethodId`, `ShipAddressId`, `CheckoutState`, `Email`, line items, and a `ShippingAdjustment` summary (id, label, amount, shipping method id).
- **REQ-005**: Order detail responses (Admin and Storefront) must expose the full financial state: `ItemTotal`, `ShipmentTotal`, `AdjustmentTotal`, `Total`, currency, shipping/billing address ids, shipping method id, statuses, timestamps, line items, and the applied `ShippingAdjustment` summary.
- **REQ-006**: The Storefront must consume server-authoritative cart/order totals; it must not reconstruct the final shipping amount from raw rate rows (`finalPrice ?? cost`) for any total shown as final.
- **REQ-007**: The review panel's shipping line must equal the applied `ShipmentTotal` (rendering `$0.00` when free), so it is consistent with the delivery-step `Free` indication.
- **REQ-008**: Cart summary UI (CartView, CartDrawer, Checkout review) must render `Subtotal → Shipping → Adjustments/Discounts → Total` from server fields.
- **REQ-009**: The delivery panel may show available rates for selection, but the authoritative per-method preview shown for the selected method must come from the existing `GET api/storefront/shipping/calculate` endpoint.
- **REQ-010**: Admin and Storefront must represent the same underlying business state in their order responses; no divergence in financial fields.
- **SEC-001**: No PII, card data, or secrets may be added to any response DTO.
- **CON-001**: Warnings-as-errors (`TreatWarningsAsErrors=true`) — every backend change must build with zero warnings.
- **CON-002**: Result objects, not exceptions — handlers and domain methods return `Result` / `Result<T>`.
- **CON-003**: Vertical slice feature files — features stay `static partial class` split across Handler/Request/Response/Endpoint/Validator files; domain logic stays in `Domain/Orders/Order.Method.*.cs` partial files; new shared helpers live under `Features/Storefront/Cart/Shared/`.
- **CON-004**: Store SPA comments follow the Store AGENTS.md standard (`// Label: Sentence.` in script; `<!-- Section: Title — purpose -->` in template); Admin SPA comments follow its own conventions.
- **CON-005**: Store SPA lint and unit tests must pass with zero NEW warnings (`pnpm run lint`, `pnpm run test:unit`, `pnpm run type-check`). Pre-existing dirty-tree failures in unrelated files are not this plan's.
- **CON-006**: No new cross-module references beyond existing patterns: `Ordering` already consumes `Module.Shipping.Domain.Calculators` and `Module.Catalog.Domain.Variants`; the shared `ShippingCostApplier` stays inside `Ordering` and reuses those existing usings only.
- **CON-007**: Never run `git stash`, `git restore`, `git revert`, `git checkout <ref> -- <path>`, or `git reset --hard`.
- **GUD-001**: Reuse existing domain methods (`ShippingRateCalculator.CalculateAsync`, `Order.ReplaceShippingAdjustment`, `Order.CalculateTotalWeight`, `Order.Method.Computation.RecalculateTotals`) verbatim; do not re-implement shipping math.
- **GUD-002**: Keep DTOs separated from domain entities — expose a `ShippingAdjustmentSummary` DTO, never the `Adjustment` entity.
- **GUD-003**: One authoritative calculation path; remove every parallel client-side shipping calculation that feeds a final total.
- **PAT-001**: `ShippingAdjustmentSummary` record — `Guid Id`, `string Label`, `decimal Amount`, `Guid? ShippingMethodId` — defined once in the Cart shared models and reused by Cart and Order responses.
- **PAT-002**: `ShippingCostApplier.ApplyAsync(dbContext, order, shippingMethodId, ct)` — the single method that computes weight, calls the calculator, and replaces the shipping adjustment; used by SelectShippingRate, UpdateCheckout, and UpdateOrderShippingMethod.
- **PAT-003**: Mapping populates server totals + adjustment summary from the `Order` entity's `Adjustments` collection (filter `a.Eligible && a.SourceType == AdjustmentConstant.SourceTypes.Shipping`).

## 2. Implementation Steps

### Implementation Phase 1: Domain/business logic — single authoritative calculation path

- GOAL-001: Consolidate shipping-cost calculation into one server-side applier used by every cart/order mutation so the applied adjustment is authoritative and never stale.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-001 | Create `service/Api/src/Module/Ordering/Features/Storefront/Cart/Shared/Services/ShippingCostApplier.cs`: `public static class ShippingCostApplier` with `public static async Task<Result> ApplyAsync(IApplicationDbContext dbContext, Order cart, Guid shippingMethodId, CancellationToken ct)`. Body (move verbatim from `SelectShippingRate.cs:45-69`, no logic change): load `Variant.Weight` for `cart.LineItems` variant ids (needs `using Module.Catalog.Domain.Variants;`), `var weightMap = variantWeights.ToDictionary(v => v.Id, v => v.Weight ?? 0m); var totalWeight = cart.CalculateTotalWeight(weightMap);`; `var calcResult = await ShippingRateCalculator.CalculateAsync(dbContext, shippingMethodId, totalWeight, cart.Total, ct); if (calcResult.IsFailure) return calcResult.Errors; var (cost, _) = calcResult.Value; return cart.ReplaceShippingAdjustment(cost, shippingMethodId);` (needs `using Module.Shipping.Domain.Calculators;`). Callers pass a cart with `LineItems` and `Adjustments` included. | |  |
| TASK-002 | Refactor `service/Api/src/Module/Ordering/Features/Storefront/Cart/SelectShippingRate/SelectShippingRate.cs`: replace the inline weight-lookup/`CalculateAsync`/`ReplaceShippingAdjustment` block (currently lines ~45-69) with `var costResult = await ShippingCostApplier.ApplyAsync(dbContext, cart, command.Request.ShippingMethodId, cancellationToken); if (costResult.IsFailure) return costResult.Errors;`. Keep unchanged: `previousMethodId`/`previousTotal` capture, `cart.SetShippingMethod`, `cart.RegressCheckoutIfAmountChanged(previousTotal)`, the `PickPaymentMethod → PickDeliveryMethod` regress on method change, the `Address → PickDeliveryMethod` advance, and `SaveChangesAsync`. Remove `using Module.Shipping.Domain.Calculators;` and `using Module.Catalog.Domain.Variants;` only if they become unused. | |  |
| TASK-003 | Refactor `service/Api/src/Module/Ordering/Features/Storefront/Cart/UpdateCheckout/UpdateCheckout.cs`: inside `if (addressChanged && cart.ShippingMethodId.HasValue)` (lines ~50-77) replace the inline weight/`CalculateAsync`/`ReplaceShippingAdjustment` with `var costResult = await ShippingCostApplier.ApplyAsync(dbContext, cart, cart.ShippingMethodId.Value, cancellationToken); if (costResult.IsFailure) return costResult.Errors;`. Keep `UpdateDetails`, `RecalculateTotals`, `RegressCheckoutIfAmountChanged(previousTotal)`, and the existing `PickPaymentMethod` regress on address change. | |  |
| TASK-004 | Fix `service/Api/src/Module/Ordering/Features/Admin/Orders/UpdateShippingMethod/UpdateOrderShippingMethod.cs`: (a) change the order load (line 21) to `.Include(o => o.LineItems).Include(o => o.Adjustments)` so the applier can compute weight and replace the adjustment; (b) after `order.SetShippingMethod(...)` (line 26) insert `var costResult = await ShippingCostApplier.ApplyAsync(dbContext, order, command.Request.ShippingMethodId, cancellationToken); if (costResult.IsFailure) return (Result<Response>)costResult.Errors;` before `SaveChangesAsync`. This fixes the stale-`ShipmentTotal` bug (currently the old method's adjustment persists after a change). | |  |

### Implementation Phase 2: Cart request/response contract

- GOAL-002: Expose the server-computed shipping/adjustment breakdown and applied shipping-adjustment metadata on the Cart contract so no client reconstructs it.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-005 | In `service/Api/src/Module/Ordering/Features/Storefront/Cart/Shared/Models/Cart.Model.Response.cs` add `public sealed record ShippingAdjustmentSummary { public Guid Id { get; init; } public string Label { get; init; } = string.Empty; public decimal Amount { get; init; } public Guid? ShippingMethodId { get; init; } }`. In `Cart.Model.Response.Base.cs` add to `CartResponseBase` after `Email`: `public decimal ShipmentTotal { get; init; }`, `public decimal AdjustmentTotal { get; init; }`, `public ShippingAdjustmentSummary? ShippingAdjustment { get; init; }`. | |  |
| TASK-006 | Extend `MapToDetail<T>` in `service/Api/src/Module/Ordering/Features/Storefront/Cart/Shared/Mappings/Cart.Mapping.Model.cs` with `ShipmentTotal = entity.ShipmentTotal`, `AdjustmentTotal = entity.AdjustmentTotal`, and `ShippingAdjustment = entity.Adjustments.FirstOrDefault(a => a.Eligible && a.SourceType == AdjustmentConstant.SourceTypes.Shipping) is { } sa ? new ShippingAdjustmentSummary { Id = sa.Id, Label = sa.Label, Amount = sa.Amount, ShippingMethodId = sa.SourceId } : null` (add `using Module.Ordering.Domain.Adjustments;` for `AdjustmentConstant`). `MapToDetailWithItems` inherits via `MapToDetail`. `EmptyCart<T>()` needs no change (defaults `0`/`null`). | |  |

### Implementation Phase 3: Order response contracts + mappings

- GOAL-003: Represent the same financial state (including the applied shipping adjustment) consistently on Admin and Storefront order responses.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-007 | In `service/Api/src/Module/Ordering/Features/Admin/Orders/Shared/Models/Order.Model.Response.cs` add `public ShippingAdjustmentSummary? ShippingAdjustment { get; init; }` to `OrderDetailResponse` (reference the record from the Cart shared models, `using Module.Ordering.Features.Storefront.Cart.Shared.Models;`). In `Admin/Orders/Shared/Mappings/Order.Mapping.Model.cs` `MapToDetail<T>` populate it identically to TASK-006 (filter `entity.Adjustments` for eligible Shipping source). Both Admin and Storefront order detail inherit via `GetCustomerOrder.Response : OrderDetailResponse`. | |  |
| TASK-008 | In `service/Api/src/Module/Ordering/Features/Storefront/Orders/ListOrders/ListCustomerOrders.Response.cs` (`StorefrontOrderListItemResponse`) add `public string Currency { get; init; } = string.Empty;` and `public int ItemCount { get; init; }`. In the list mapping (`Features/Storefront/Orders/Shared/Mappings/OrderStore.Mapping.cs` `MapToStoreListItem<T>`) populate `Currency = entity.Currency`, `ItemCount = entity.ItemCount`. | |  |

### Implementation Phase 4: Storefront SPA types + API clients

- GOAL-004: Mirror the corrected contracts in the Storefront SPA types, validations, and shipping API client.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-009 | `app/Store/src/features/ordering/types/cart.ts`: add `export interface ShippingAdjustmentSummary { id: string; label: string; amount: number; shippingMethodId: string \| null }`; add `shipmentTotal: number`, `adjustmentTotal: number`, `shippingAdjustment: ShippingAdjustmentSummary \| null` to `CartResponse`. `app/Store/src/features/ordering/validations/cart.ts`: add `shipmentTotal: z.number().min(0)`, `adjustmentTotal: z.number().min(0)`, `shippingAdjustment: z.object({ id: z.string(), label: z.string(), amount: z.number(), shippingMethodId: z.string().nullable() }).nullable()` to `CartResponseSchema`. Update every `CartResponse` literal fixture in `CheckoutView.spec.ts`, `CartView.spec.ts`, `CartDrawer.spec.ts` with the three new fields (use `shipmentTotal: 0, adjustmentTotal: 0, shippingAdjustment: null` unless the scenario needs a value). | |  |
| TASK-010 | `app/Store/src/features/ordering/types/order.ts`: add `shippingAdjustment: ShippingAdjustmentSummary \| null` to `OrderDetail`; add `itemCount: number`, `specialInstructions: string \| null` to `OrderDetail`; add `adjustmentTotal: number` to `OrderLineItem`; add `currency: string`, `itemCount: number` to `OrderListItem`; add `paymentProcessingAt: string \| null`, `paymentCompletedAt: string \| null`, `paymentFailedAt: string \| null`, `deliveryExceptionAt: string \| null` to `OrderTrackingResponse`. `app/Store/src/features/ordering/validations/order.ts`: mirror each addition. Update `OrderDetailView.spec.ts`/`OrderListView.spec.ts` fixtures. | |  |
| TASK-011 | `app/Store/src/features/shipping/composables/useShipping.ts`: change `fetchRates(orderId)` to `fetchRates()` and drop the `{ filter: \`orderId eq '\${orderId}'\` }` param (the backend `ListShippingRates` has no such filter; rates are the available set for selection). Add `const preview = ref<ShippingCost \| null>(null)` and `async function previewFor(methodId: string, orderId: string): Promise<void>` that calls `calculateShipping(methodId, orderId)` from `shippingApi.ts` and stores `preview.value` (clearing on failure). Export `preview`, `previewFor`, and keep `methods`/`rates`/`selectMethod`. Confirm `ShippingCost` in `app/Store/src/features/shipping/types/shipping.ts` matches `CalculateShipping.Response` (fields `shippingMethodId`, `methodName`, `cost`, `currency`, `isFreeShipping`); add any missing field. | |  |

### Implementation Phase 5: Storefront shared state/composables

- GOAL-005: Make the cart composable carry the server-authoritative totals.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-012 | `app/Store/src/features/ordering/composables/useCart.ts`: add module refs `shipmentTotal = ref(0)`, `adjustmentTotal = ref(0)`, `total = ref(0)`, `itemTotal = ref(0)`, `shippingAdjustment = ref<ShippingAdjustmentSummary \| null>(null)`; set them from `result.value` in `fetchCart`, `addItem`, `updateQuantity`, `removeItem` success blocks (mirroring the existing checkout-prefill sync); reset to defaults in `clearCart()` and `reset()`; add all five to the `createCart()` reactive return; add computeds `shipping = computed(() => shipmentTotal.value)` and `adjustments = computed(() => adjustmentTotal.value)`. Keep the existing `subtotal` (line-item sum). | |  |

### Implementation Phase 6: Cart UI

- GOAL-006: Cart view and drawer show the server breakdown.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-013 | `app/Store/src/features/ordering/views/CartView.vue`: rework the summary card — `Items (itemCount)` → `cart.subtotal`; `Shipping` → `formatCurrency(cart.shipping)` (remove the `Calculated at checkout` placeholder for Shipping; a free/un-applied shipment renders `$0.00`); add an `Adjustments/Discounts` row shown only when `cart.adjustments !== 0`; `Total` → `formatCurrency(cart.total)`. | |  |
| TASK-014 | `app/Store/src/features/ordering/components/CartDrawer.vue`: under the existing `Subtotal` line add a `Shipping` line (`formatCurrency(cart.shipping)`) and a `Total` line (`formatCurrency(cart.total)`), matching the file's existing summary markup. | |  |

### Implementation Phase 7: Checkout UI

- GOAL-007: Checkout uses server totals; the free-shipping indication is consistent across the delivery and review steps.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-015 | `app/Store/src/features/ordering/views/CheckoutView.vue`: (a) delete `const shippingCost = computed(...)` (`:185-187`) and replace `const total = computed(() => cart.subtotal + (shippingCost.value ?? 0))` (`:231`) with `const total = computed(() => cart.total)`; (b) review panel — `Subtotal` → `cart.subtotal`, `Shipping` → `formatCurrency(cart.shipping)` (remove the `shippingCost === null ? 'Calculated at checkout'` fallback at `:498`), add an `Adjustments` row when `cart.adjustments !== 0`, `Total` → `formatCurrency(cart.total)`; (c) delivery panel — for the SELECTED method display the server preview: call `shipping.previewFor(methodId, cart.id!)` in `continueToPayment` (and on `onMounted` for a pre-selected method), render `formatCurrency(preview.cost)` and a `Free` badge when `preview.isFreeShipping`; keep the rates list (`methodCost`) as the available-rate display for unselected methods; remove the client `isMethodFree(methodId)` free-threshold recomputation (`:177-182`) as the source of the delivery-step `Free` label (the review panel's `cart.shipping` is now the consistent server value). | |  |

### Implementation Phase 8: Order UI (Storefront) + Admin

- GOAL-008: Order detail views show the complete financial state; Admin shows the applied shipping adjustment.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-016 | `app/Store/src/features/ordering/views/OrderDetailView.vue`: add an `Adjustments` row under Shipping (`formatCurrency(order.adjustmentTotal)`); add `Paid` (`formatCurrency(order.paymentTotal)`) and `Outstanding` (`formatCurrency(order.outstandingBalance)`) rows when non-zero; display the applied shipping method by resolving `order.shippingMethodId` against `useShipping().methods` (fall back to the raw id when unresolvable); render `item.adjustmentTotal` in line items when present. | |  |
| TASK-017 | `app/Store/src/features/ordering/views/OrderListView.vue`: display the new list-item `itemCount` and `currency` columns if the enriched `OrderListItem` type provides them (display-only; do not change pagination logic). | |  |
| TASK-018 | Admin SPA: `app/Admin/src/features/ordering/types/order.ts` and its `validations/order.ts` — add `shippingAdjustment: { id, label, amount, shippingMethodId } \| null` to the Admin `OrderDetail` type/schema. `app/Admin/src/features/ordering/views/OrderDetail.vue` — add a summary line for the applied shipping adjustment (method id + amount) near the existing `shipmentTotal` block (lines ~367-370) and verify the Items→Shipping→Adjustments→Total summary (itemTotal/shipmentTotal/adjustmentTotal/total) renders correctly. | |  |

### Implementation Phase 9: Tests

- GOAL-009: Prove correctness of calculation, contracts, and UI totals end-to-end.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-019 | Backend unit tests in `service/Api/tests/Module.UnitTests` (Ordering + Shipping namespaces): `ShippingRateCalculator` — cheapest weight-matched rate selection, weight-tier fallback, free-shipping threshold returns `(0m, true)`, no applicable rate returns failure; `SelectShippingRate` handler — applied `ShipmentTotal` equals the calculated cost and `0` for the free case (zero adjustment); `UpdateOrderShippingMethod` handler — changing the method replaces the adjustment so `ShipmentTotal` reflects the NEW method's cost (regression for the stale bug); cart mapping — `MapToDetail` exposes `ShipmentTotal`, `AdjustmentTotal`, `ShippingAdjustment` (label/amount/method) and `Total == ItemTotal + ShipmentTotal + AdjustmentTotal`; order mapping — `MapToDetail` exposes `ShippingAdjustment`. Update existing fixtures that construct `CartResponseBase`-derived objects. | |  |
| TASK-020 | Storefront SPA tests: `CartView.spec`/`CartDrawer.spec` — Shipping and Total lines render `cart.shipmentTotal`/`cart.total`; `CheckoutView.spec` — review panel Shipping shows the server `shipmentTotal` and renders `$0.00` when `shipmentTotal === 0` (free case), `total` equals `cart.total`; `OrderDetailView.spec` — Adjustments/Paid/Outstanding rows render; a `useCart`-level test asserting the server totals sync from a cart response (and reset on `clearCart`). Update all cart/order fixtures with the new fields. | |  |
| TASK-021 | Verification run (repo root unless noted): `dotnet build` (0 warnings / 0 errors); `dotnet test service/Api/tests/Module.UnitTests --filter-namespace "Module.UnitTests.Ordering*"` and `--filter-namespace "Module.UnitTests.Shipping*"`; `cd app/Store && pnpm run lint && pnpm run test:unit && pnpm run type-check`; `bash scripts/check-feature-conventions.sh`. Confirm `Server Cart Total == Storefront Cart Total == Checkout/Order Total` for a same-state cart (manual smoke via `ApiTests/Ordering/demo-flow.http` optional). Report pre-existing dirty-tree failures separately; do not edit unrelated files. | |  |

### Implementation Phase 10: Cleanup of obsolete client-side calculations

- GOAL-010: Remove every parallel client-side shipping calculation that can diverge from the backend.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-022 | Remove obsolete client-side calculation: in `CheckoutView.vue` delete the `shippingCost` computed and the `cart.subtotal + shippingCost` total reconstruction (TASK-015) and the client `isMethodFree` threshold recomputation; in `useShipping.ts` delete the fake `orderId` filter param (TASK-011). Grep `app/Store/src` for any remaining arithmetic over `finalPrice ?? cost` used to build a final total and replace with server fields (`cart.shipping` / `cart.total`). Confirm no parallel client shipping calculation remains. | |  |

## 3. Alternatives

- **ALT-001**: Expose the full `Adjustments` collection on cart/order responses. Rejected: leaks the domain `Adjustment` entity and every adjustment's internals; a single `ShippingAdjustment` summary DTO satisfies the UI needs (PAT-001).
- **ALT-002**: Add the shipping breakdown only to the Storefront cart and keep Admin on the lumped total. Rejected: REQ-010 requires the same financial state on both sides; Admin needs the applied adjustment metadata to show Items → Shipping → Adjustments → Total.
- **ALT-003**: Have the SPA call `calculateShipping` for every method on the delivery panel. Rejected: N round-trips per render; only the selected method needs an authoritative preview — the rates list already serves as the available-rate selector (REQ-009).
- **ALT-004**: Keep the inline calculation in each handler and merely duplicate it into the admin handler. Rejected: the three existing copies are already diverging (Admin is stale, TASK-004); a single shared applier is the one authoritative path (GUD-003).
- **ALT-005**: Recompute the final shipping amount on the client from rates plus a free-threshold rule. Rejected: this is the current bug — parallel client logic can diverge from the server (free threshold, cheapest weight-matched rate).

## 4. Dependencies

- **DEP-001**: `Module.Shipping.Domain.Calculators.ShippingRateCalculator.CalculateAsync` — unchanged; consumed by `ShippingCostApplier` (TASK-001).
- **DEP-002**: `Order.ReplaceShippingAdjustment` and `Order.Method.Computation.RecalculateTotals` (Ordering domain) — unchanged; used by the applier and the totals formula.
- **DEP-003**: `Module.Catalog.Domain.Variants.Variant.Weight` — the weight lookup used by the applier (existing cross-module usage, permitted by CON-006).
- **DEP-004**: Existing `GET api/storefront/shipping/calculate` endpoint and `app/Store/src/features/shipping/services/shippingApi.ts` `calculateShipping` (currently unused) — activated by TASK-011/TASK-015.
- **DEP-005**: The merged `CheckoutState` rename (`PickDeliveryMethod`/`PickPaymentMethod`) and the checkout-state-sync commits (idempotent `AdvanceCheckoutState`, `RegressCheckoutIfAmountChanged`) — unchanged; `SelectShippingRate` keeps its existing regression/advance logic (TASK-002).
- **DEP-006**: `ShippingAdjustmentSummary` record (TASK-005) — referenced by both the Cart response (TASK-006) and the Order response (TASK-007) and by SPA types (TASK-009/TASK-010/TASK-018).
- **DEP-007**: `ShippingCost` type in `app/Store/src/features/shipping/types/shipping.ts` — must match `CalculateShipping.Response` for the preview (TASK-011).

## 5. Files

- **FILE-001**: `service/Api/src/Module/Ordering/Features/Storefront/Cart/Shared/Services/ShippingCostApplier.cs` — create (TASK-001).
- **FILE-002**: `service/Api/src/Module/Ordering/Features/Storefront/Cart/SelectShippingRate/SelectShippingRate.cs` — modify (TASK-002).
- **FILE-003**: `service/Api/src/Module/Ordering/Features/Storefront/Cart/UpdateCheckout/UpdateCheckout.cs` — modify (TASK-003).
- **FILE-004**: `service/Api/src/Module/Ordering/Features/Admin/Orders/UpdateShippingMethod/UpdateOrderShippingMethod.cs` — modify (TASK-004).
- **FILE-005**: `service/Api/src/Module/Ordering/Features/Storefront/Cart/Shared/Models/Cart.Model.Response.cs` — modify (TASK-005).
- **FILE-006**: `service/Api/src/Module/Ordering/Features/Storefront/Cart/Shared/Models/Cart.Model.Response.Base.cs` — modify (TASK-005).
- **FILE-007**: `service/Api/src/Module/Ordering/Features/Storefront/Cart/Shared/Mappings/Cart.Mapping.Model.cs` — modify (TASK-006).
- **FILE-008**: `service/Api/src/Module/Ordering/Features/Admin/Orders/Shared/Models/Order.Model.Response.cs` — modify (TASK-007).
- **FILE-009**: `service/Api/src/Module/Ordering/Features/Admin/Orders/Shared/Mappings/Order.Mapping.Model.cs` — modify (TASK-007).
- **FILE-010**: `service/Api/src/Module/Ordering/Features/Storefront/Orders/ListOrders/ListCustomerOrders.Response.cs` — modify (TASK-008).
- **FILE-011**: `service/Api/src/Module/Ordering/Features/Storefront/Orders/Shared/Mappings/OrderStore.Mapping.cs` — modify (TASK-008).
- **FILE-012**: `app/Store/src/features/ordering/types/cart.ts` — modify (TASK-009).
- **FILE-013**: `app/Store/src/features/ordering/validations/cart.ts` — modify (TASK-009).
- **FILE-014**: `app/Store/src/features/ordering/views/__tests__/CheckoutView.spec.ts` — modify (TASK-009/TASK-020).
- **FILE-015**: `app/Store/src/features/ordering/views/__tests__/CartView.spec.ts` — modify (TASK-009/TASK-020).
- **FILE-016**: `app/Store/src/features/ordering/components/__tests__/CartDrawer.spec.ts` — modify (TASK-009/TASK-020).
- **FILE-017**: `app/Store/src/features/ordering/types/order.ts` — modify (TASK-010).
- **FILE-018**: `app/Store/src/features/ordering/validations/order.ts` — modify (TASK-010).
- **FILE-019**: `app/Store/src/features/shipping/composables/useShipping.ts` — modify (TASK-011/TASK-022).
- **FILE-020**: `app/Store/src/features/shipping/types/shipping.ts` — modify (TASK-011).
- **FILE-021**: `app/Store/src/features/ordering/composables/useCart.ts` — modify (TASK-012).
- **FILE-022**: `app/Store/src/features/ordering/views/CartView.vue` — modify (TASK-013).
- **FILE-023**: `app/Store/src/features/ordering/components/CartDrawer.vue` — modify (TASK-014).
- **FILE-024**: `app/Store/src/features/ordering/views/CheckoutView.vue` — modify (TASK-015/TASK-022).
- **FILE-025**: `app/Store/src/features/ordering/views/OrderDetailView.vue` — modify (TASK-016).
- **FILE-026**: `app/Store/src/features/ordering/views/OrderListView.vue` — modify (TASK-017).
- **FILE-027**: `app/Admin/src/features/ordering/types/order.ts` — modify (TASK-018).
- **FILE-028**: `app/Admin/src/features/ordering/validations/order.ts` — modify (TASK-018).
- **FILE-029**: `app/Admin/src/features/ordering/views/OrderDetail.vue` — modify (TASK-018).
- **FILE-030**: `service/Api/tests/Module.UnitTests` (Ordering + Shipping test folders) — add/update (TASK-019).

## 6. Testing

- **TEST-001**: `ShippingRateCalculator` unit tests — normal cost, cheapest weight-matched selection, weight-tier fallback, free-shipping threshold → `(0m, true)`, no rate → failure (TASK-019).
- **TEST-002**: `SelectShippingRate` handler test — `ShipmentTotal` equals the calculated cost; free case produces a `0` shipping adjustment (TASK-019).
- **TEST-003**: `UpdateOrderShippingMethod` handler test — method change replaces the adjustment; `ShipmentTotal` reflects the new method's cost, not the old (stale-bug regression test) (TASK-019).
- **TEST-004**: Cart mapping test — `MapToDetail` exposes `ShipmentTotal`, `AdjustmentTotal`, `ShippingAdjustment` (label/amount/method) and `Total == ItemTotal + ShipmentTotal + AdjustmentTotal` (TASK-019).
- **TEST-005**: Order mapping test — `MapToDetail` exposes `ShippingAdjustment`; Storefront list item exposes `Currency` + `ItemCount` (TASK-019).
- **TEST-006**: Storefront `CartView.spec`/`CartDrawer.spec` — Shipping and Total lines render `cart.shipmentTotal`/`cart.total` from server fields (TASK-020).
- **TEST-007**: Storefront `CheckoutView.spec` — review panel Shipping equals server `shipmentTotal`; free case (`shipmentTotal === 0`) renders `$0.00`; `total` equals `cart.total` (TASK-020).
- **TEST-008**: Storefront `OrderDetailView.spec` — Adjustments/Paid/Outstanding rows render (TASK-020).
- **TEST-009**: `useCart` test — server totals sync from cart responses and reset on `clearCart` (TASK-020).
- **TEST-010**: Store SPA gates — `pnpm run lint`, `pnpm run test:unit`, `pnpm run type-check` pass for this plan's files (TASK-021).
- **TEST-011**: Backend gates — `dotnet build` 0W/0E; Ordering + Shipping unit suites pass (TASK-021).
- **TEST-012**: Feature conventions — `bash scripts/check-feature-conventions.sh` passes for this plan's files (TASK-021).
- **TEST-013**: Invariant check — for a same-state cart, `Server Cart Total == Storefront Cart Total == Checkout/Order Total` (TASK-021, manual smoke optional).

## 7. Risks & Assumptions

- **RISK-001**: Widening `CartResponseBase`/`OrderDetailResponse` could break existing unit tests and SPA fixtures that construct the DTOs — mitigated by updating fixtures in TASK-009/TASK-010/TASK-019/TASK-020.
- **RISK-002**: `UpdateOrderShippingMethod` (Admin) now recalculates shipping — for a non-Draft placed order `SetShippingMethod` already returns `NotDraftForShippingMethod`, so the applier runs only on Draft orders; no change to placed-order behavior (verify with TEST-003).
- **RISK-003**: The `ShippingCostApplier` uses `cart.Total` as the calculator's order-total input (existing behavior, preserved). Free-threshold comparisons therefore include the pre-application shipment amount; this is the pre-existing rule and is not changed by this plan (GUD-001).
- **RISK-004**: The SPA delivery panel still shows available rate prices for unselected methods; only the selected method's preview and all final totals are server-authoritative — accepted per REQ-009 (no N-call preview).
- **RISK-005**: The Store working tree contains pre-existing unrelated dirty files (e.g. `CartDrawer.vue`) that fail lint/unit independently; this plan does not fix them and gates only on its own files (CON-005).
- **ASSUMPTION-001**: The `CheckoutState` rename (`PickDeliveryMethod`/`PickPaymentMethod`) and the checkout-state-sync commits are already merged and correct; this plan does not touch the state machine.
- **ASSUMPTION-002**: `ShippingCost` in the SPA (`types/shipping.ts`) matches `CalculateShipping.Response`; any missing field is added in TASK-011.
- **ASSUMPTION-003**: Admin has no cart flow — only Storefront consumes the cart contract; Admin consumes the enriched order responses (REQ-010).
- **ASSUMPTION-004**: Only the selected shipping method needs a server preview on the delivery panel; the rates list remains the available-rate selector.
- **ASSUMPTION-005**: `ListShippingRates` intentionally returns all rates for a method (selection aid), not a weight-filtered set; the applied cost is always server-calculated at apply time.

## 8. Related Specifications / Further Reading

- [refactor-checkout-state-sync-1.md](./refactor-checkout-state-sync-1.md) — prior plan that established idempotent `AdvanceCheckoutState`, `RegressCheckoutIfAmountChanged`, and the backend-driven checkout step; unchanged by this plan.
- [docs/codebase/ARCHITECTURE.md](../docs/codebase/ARCHITECTURE.md)
- [docs/codebase/CONVENTIONS.md](../docs/codebase/CONVENTIONS.md)
- [AGENTS.md](../AGENTS.md) — non-negotiable rules (result objects, module isolation, vertical-slice features, no destructive git).
