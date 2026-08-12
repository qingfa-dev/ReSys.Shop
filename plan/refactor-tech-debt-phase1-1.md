---
goal: Fix High-Priority Tech Debt — SPA Tests, Dashboard, Security, Code Hygiene
version: 1.0
date_created: 2026-08-11
last_updated: 2026-08-11
owner: ReSys.Shop
status: 'Planned'
tags: [tech-debt, test, architecture, security, code-hygiene]
---

# Introduction

![Status: Planned](https://img.shields.io/badge/status-Planned-blue)

Fix 6 high-priority tech-debt items identified in the 2026-08-11 technical debt register. These items are the highest-scoring, lowest-effort, highest-impact improvements selected from a comprehensive codebase scan. All fixes are one-root-cause or one-commit-each — zero architectural risk.

## 1. Requirements & Constraints

- **REQ-001**: .NET 10, TreatWarningsAsErrors=true — zero warnings on `dotnet build`
- **REQ-002**: SPA `pnpm run build` must continue to pass
- **REQ-003**: SPA `pnpm run test:unit` — all 26 Pinia-related failures must resolve; remaining tests unchanged
- **REQ-004**: Dashboard module must render at `GET /api/admin/dashboard` after registration without affecting other modules
- **REQ-005**: SECURITY.md must follow GitHub's recommended template (disclosure policy, supported versions, reporting process)
- **SEC-001**: Dashboard endpoint must require authorization + permission check (already enforced — `DashboardFeature.Admin.Get.Permission` exists)
- **CON-001**: No new cross-module Domain references may be introduced
- **CON-002**: Do NOT change handler behavior — only structure/registration/test-harness fixes
- **GUD-001**: Follow existing codebase patterns (createTestingPinia, module extension methods, AC conventions)
- **PAT-001**: `createTestingPinia({ stubActions: true })` in Vue Test Utils mount (per AppHeader.spec.ts pattern)
- **PAT-002**: Module extension methods follow `ShippingExtension.AddShippingModule()` pattern (public static class, `this WebApplicationBuilder`)

## 2. Implementation Steps

### Implementation Phase 1

- GOAL-001: Fix 26 SPA unit test failures — all share a single root cause (Pinia not installed in test harness). Restore CI gate for the SPA test suite.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-001 | Fix `CheckoutView.spec.ts` (5 tests): add `import { createTestingPinia } from '@pinia/testing'` and `createTestingPinia({ stubActions: true })` to the mount plugins array. File: `app/Store/src/features/ordering/views/__tests__/CheckoutView.spec.ts`. | ⬜ | |
| TASK-002 | Fix `CartDrawer.spec.ts` (4 tests): same pattern as TASK-001. Also fix the vi.fn missing-type-parameter oxlint errors in that file if present. File: `app/Store/src/features/ordering/components/__tests__/CartDrawer.spec.ts`. | ⬜ | |
| TASK-003 | Fix `OrderListView.spec.ts` (2 tests): same pattern. File: `app/Store/src/features/ordering/views/__tests__/OrderListView.spec.ts`. | ⬜ | |
| TASK-004 | Fix `OrderDetailView.spec.ts` (2 tests): same pattern. Files: `app/Store/src/features/ordering/views/__tests__/OrderDetailView.spec.ts`. | ⬜ | |
| TASK-005 | Fix `ShopView.spec.ts` (1 test): same pattern. File: `app/Store/src/features/catalog/views/__tests__/ShopView.spec.ts`. | ⬜ | |
| TASK-006 | Fix `ProductGridCard.spec.ts` (2 tests): same pattern. File: `app/Store/src/features/catalog/components/__tests__/ProductGridCard.spec.ts`. | ⬜ | |
| TASK-007 | Fix `AppHeader.spec.ts` (1 test): already uses createTestingPinia — verify no Pinia-related failures and fix any remaining type-parameter oxlint errors. File: `app/Store/src/app/components/layout/__tests__/AppHeader.spec.ts`. | ⬜ | |
| TASK-008 | Fix any remaining failing test files beyond the 7 listed above (scan with `npx vitest run 2>&1 | grep 'FAIL'` and fix each using the same createTestingPinia pattern). Commit all test fixes together: `test(store-spa): add createTestingPinia to Vue component test harness` | ⬜ | |

### Implementation Phase 2

- GOAL-002: Register the Dashboard module — 9 feature files exist but `AddDashboardModule()` is never called. A one-line extension method + registration.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-009 | Create `service/Api/src/Module/Dashboard/Dashboard.Extension.cs` following the `Shipping.Extension.cs` pattern: `public static class DashboardExtension` with `public static WebApplicationBuilder AddDashboardModule(this WebApplicationBuilder builder) { return builder; }` (no seeders to register for now). File: `service/Api/src/Module/Dashboard/Dashboard.Extension.cs`. | ⬜ | |
| TASK-010 | In `service/Api/src/Api/Program.cs`, add `builder.AddDashboardModule();` after `builder.AddShippingModule();` (line 52). The Dashboard module's `DashboardFeature.Admin.Get.Route = "api/admin/dashboard"` with `.HasPermission(DashboardFeatureMetadata.Sales.List)` already exists — registration makes it live. Verify `dotnet build` 0/0. | ⬜ | |

### Implementation Phase 3

- GOAL-003: Create SECURITY.md and update CONCERNS.md with post-execution state.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-011 | Create `SECURITY.md` in the repo root following GitHub's recommended template: Supported Versions table, Reporting a Vulnerability section (email or private reporting), Security Update Process. Use `service/Api/src/Api/.env.template` as reference for placeholder values. File: `SECURITY.md`. | ⬜ | |
| TASK-012 | Update `docs/codebase/CONCERNS.md`: (a) cross-module refs 38→31 (update lines 9,20); (b) remove `eslint-plugin-boundaries` entry (lines 12,25 — already removed from package.json); (c) mark `.gitignore` Release pattern as resolved; (d) mark TotalAvailable clamp + StockLocationName as resolved; (e) mark `.http` fixtures as resolved. | ⬜ | |

### Implementation Phase 4

- GOAL-004: De-duplicate release loop in 3 cancel handlers and fix 5 AC-001 Command/Query violations.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-013 | De-duplicate the identical per-line-item `GetStockLocationIdForVariantAsync` + `AdjustStockAsync` loop in `CancelOrder.cs`, `CancelOrderAdmin.cs`, `UpdateOrderStatus.cs`. Extract to a private static method in the `CancelOrder` partial class (storefront, which is the canonical one), then have the other two call it. The method signature: `private static async Task<Result> ReturnStockForPlacedOrderAsync(IApplicationDbContext dbContext, IStockItemService stockItem, Order order, CancellationToken ct)`. Keep the `wasPlaced` guard in each caller (it differs slightly per handler). Verify `dotnet build` 0/0 and full test suite unchanged. | ⬜ | |
| TASK-014 | Fix AC-001 violation in `CreatePaymentIntent.cs`: change `public sealed record Command(Guid OrderId, Guid? PaymentMethodId = null, string? PaymentMethodToken = null, string? ReturnUrl = null, string? CardNumber = null, string? Currency = null) : ICommand<Response>` to `public sealed record Command(Request Request) : ICommand<Response>`. Move the inlined fields to a new `CreatePaymentIntent.Request.cs`: `public sealed record Request { Guid OrderId, Guid? PaymentMethodId = null, string? PaymentMethodToken = null, string? ReturnUrl = null, string? CardNumber = null, string? Currency = null }`. Update the endpoint to construct `new Command(new Request { OrderId = request.OrderId, ... })`. File: `Billing/Features/Storefront/Payment/CreateIntent/CreatePaymentIntent.cs` + new `CreatePaymentIntent.Request.cs`. | ⬜ | |
| TASK-015 | Fix AC-001 violation in `ConfirmPayment.cs`: change `public sealed record Command(Guid PaymentId, Guid? PaymentMethodId = null)` to `public sealed record Command(Request Request)`. Create `ConfirmPayment.Request.cs` with the 2 fields. Update the endpoint accordingly. File: `Billing/Features/Storefront/Payment/Confirm/ConfirmPayment.cs` + new Request.cs. | ⬜ | |
| TASK-016 | Fix AC-001 violation in `UpdateProfile.cs`: change `public sealed record Command(Guid UserId, Request Request, bool IsAdminBypass = false)` to `public sealed record Command(Parameters Parameters) : ICommand<Response>`. The `UserId` and `IsAdminBypass` are infrastructure params (not request data) — they belong in a `Parameters` class per the convention. File: `Customer/Features/Storefront/Profiles/Update/UpdateProfile.cs` + new Parameters.cs. NOTE: this is the most invasive of the 5 AC-001 fixes; verify `UpdateProfile.Endpoint.cs` sends the Command with the new shape and `dotnet build` passes. | ⬜ | |
| TASK-017 | Fix AC-001 violation in `DeleteWishlist.cs`: change `public sealed record Command(Guid UserId, Guid Id, string? DeletedBy = null)` to `public sealed record Command(Parameters Parameters)`. Create `DeleteWishlist.Parameters.cs`. File: `Customer/Features/Storefront/Wishlists/Delete/DeleteWishlist.cs` + new Parameters.cs. | ⬜ | |
| TASK-018 | Fix AC-001 violation in `GetSimilarProducts.cs`: change `public sealed record Query(Guid Id, int TopK = 20)` to `public sealed record Query(Parameters Parameters)`. Create `GetSimilarProducts.Parameters.cs`. File: `Catalog/Features/Storefront/Products/Get/Similar/GetSimilarProducts.cs` + new Parameters.cs. | ⬜ | |

## 3. Alternatives

- **ALT-001**: Fix SPA tests by adding `createTestingPinia` globally in `vitest.config.ts` instead of per-file. Rejected: global setup would affect all tests uniformly but real Pinia stores may interfere with tests that want different store states. Per-file setup gives explicit control and matches the existing pattern in passing test files (AppHeader, MobileNav, layouts).
- **ALT-002**: Delete the unregistered Dashboard module instead of registering it. Rejected: the 9 feature files are complete, routed, and permissioned — registering them is a one-liner and adds value. Deleting them would be wasteful.
- **ALT-003**: Leave the release loop duplicated. Rejected: copy-paste bugs have already cost debugging time (the Cancel path was the most-fixed handler during code review). Single-source truth reduces future defect rate.

## 4. Dependencies

- **DEP-001**: The storefront-spa-migration plan must be complete (it is — the SPA builds and the test failures predate that plan). The test failures are Pinia-env issues, not route-related.
- **DEP-002**: The inventory-services-consolidation plan must be complete (it is — `GetStockLocationIdForVariantAsync` exists for the release loop extraction).
- **DEP-003**: `createTestingPinia` from `@pinia/testing` is already a devDependency in `app/Store/package.json` (verified — it's used by passing tests).

## 5. Files

- **FILE-001**: `app/Store/src/features/ordering/views/__tests__/CheckoutView.spec.ts` — TASK-001 (add createTestingPinia)
- **FILE-002**: `app/Store/src/features/ordering/components/__tests__/CartDrawer.spec.ts` — TASK-002
- **FILE-003**: `app/Store/src/features/ordering/views/__tests__/OrderListView.spec.ts` — TASK-003
- **FILE-004**: `app/Store/src/features/ordering/views/__tests__/OrderDetailView.spec.ts` — TASK-004
- **FILE-005**: `app/Store/src/features/catalog/views/__tests__/ShopView.spec.ts` — TASK-005
- **FILE-006**: `app/Store/src/features/catalog/components/__tests__/ProductGridCard.spec.ts` — TASK-006
- **FILE-007**: `app/Store/src/app/components/layout/__tests__/AppHeader.spec.ts` — TASK-007
- **FILE-008**: `service/Api/src/Module/Dashboard/Dashboard.Extension.cs` — TASK-009 (new)
- **FILE-009**: `service/Api/src/Api/Program.cs` — TASK-010 (add Dashboard registration)
- **FILE-010**: `SECURITY.md` — TASK-011 (new)
- **FILE-011**: `docs/codebase/CONCERNS.md` — TASK-012 (update)
- **FILE-012**: `service/Api/src/Module/Ordering/Features/Storefront/Orders/Cancel/CancelOrder.cs` — TASK-013 (extract shared method)
- **FILE-013**: `service/Api/src/Module/Ordering/Features/Admin/Orders/Cancel/CancelOrderAdmin.cs` — TASK-013
- **FILE-014**: `service/Api/src/Module/Ordering/Features/Admin/Orders/UpdateStatus/UpdateOrderStatus.cs` — TASK-013
- **FILE-015**: `service/Api/src/Module/Billing/Features/Storefront/Payment/CreateIntent/CreatePaymentIntent.cs` + new `Request.cs` — TASK-014
- **FILE-016**: `service/Api/src/Module/Billing/Features/Storefront/Payment/Confirm/ConfirmPayment.cs` + new `Request.cs` — TASK-015
- **FILE-017**: `service/Api/src/Module/Customer/Features/Storefront/Profiles/Update/UpdateProfile.cs` + new `Parameters.cs` — TASK-016
- **FILE-018**: `service/Api/src/Module/Customer/Features/Storefront/Wishlists/Delete/DeleteWishlist.cs` + new `Parameters.cs` — TASK-017
- **FILE-019**: `service/Api/src/Module/Catalog/Features/Storefront/Products/Get/Similar/GetSimilarProducts.cs` + new `Parameters.cs` — TASK-018

## 6. Testing

- **TEST-001**: `cd app/Store && npx vitest run` — all 26 failures must resolve. Total passing tests must be 337/337 (0 failures). If any non-Pinia failures remain, document and assess separately (not this plan's scope).
- **TEST-002**: `cd app/Store && pnpm run lint` — zero NEW lint errors in touched files. Pre-existing oxlint `require-mock-type-parameters` errors in spec files (CartDrawer, CartView, OrderDetailView, ProductDetailView) are NOT in this plan's scope but may be partially resolved during TASK-002/004/007 editing.
- **TEST-003**: `dotnet build` must be 0 errors / 0 warnings after each task
- **TEST-004**: `dotnet test service/Api/tests/Module.UnitTests` — full suite must remain green (2593 passed, 0 failed)
- **TEST-005**: `bash scripts/check-feature-conventions.sh` — AC-001 count must decrease by 5 (from current 5 violations to 0). AC-002/AC-003 unchanged.
- **TEST-006**: `bash scripts/check-cross-module-refs.sh` — count unchanged at 31 (no new refs)
- **TEST-007**: Verify Dashboard registers: `GET /api/admin/dashboard` returns 200 after running `dotnet run` (manual verification via `.http` file, or confirm the route exists in OpenAPI/Swagger)

## 7. Risks & Assumptions

- **RISK-001**: TASK-016 (UpdateProfile Parameters refactor) is the most invasive AC-001 fix — the Command signature changes from `(Guid, Request, bool)` to `(Parameters)`, which affects callers. The `UpdateProfile.Endpoint.cs` sends the Command via `ISender`; updating the construction there is the primary risk.
- **RISK-002**: TASK-013 (release loop extraction) may surface subtle behavioral differences between the 3 handlers — the `wasPlaced` guard differs slightly between storefront and admin variants. Mitigation: keep the guard in each caller and extract only the per-line-item loop body.
- **ASSUMPTION-001**: All 26 SPA test failures share the single root cause (missing `createTestingPinia`). If any failures have DIFFERENT root causes, those are out of scope for this plan and will be documented as follow-up items.
- **ASSUMPTION-002**: `GetSimilarProducts` uses `IPagedQuery<Response>` and changing to `Query(Parameters)` won't break the handler's MediatR interface binding.
- **ASSUMPTION-003**: The Dashboard module's existing `DashboardFeature.cs` route constant `api/admin/dashboard` and permission `DashboardFeatureMetadata.Sales.List` are correct and the endpoint will bind immediately upon registration.

## 8. Related Specifications / Further Reading

- [Tech Debt Register (2026-08-11)](../docs/codebase/CONCERNS.md) — 30+ item comprehensive debt register
- [Storefront API Alignment Design Spec](../docs/superpowers/specs/2025-08-11-storefront-api-alignment-design.md) — originating design for the 5-plan alignment
- [AGENTS.md](../AGENTS.md) — non-negotiable rules, repository map, verification commands
- [Inventory Services Consolidation Plan](../docs/superpowers/plans/2025-08-11-inventory-services-consolidation.md) — introduced `GetStockLocationIdForVariantAsync` used in TASK-013
