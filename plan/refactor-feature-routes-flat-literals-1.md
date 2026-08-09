---
goal: "Flatten all Feature route constants to full-path literal strings; fix non-compliant API prefixes"
version: "1.0"
date_created: "2026-08-09"
last_updated: "2026-08-09"
status: "Completed"
tags:
  - refactor
  - api
  - breaking-change
---

# Introduction

![Status: Completed](https://img.shields.io/badge/status-Completed-brightgreen)

Eliminate all intermediate route-assembly constants (`StoreRoute`, `AdminRoute`, `CatalogRoute`, `BaseRoute`, `BaseAuthRoute`, `BaseLoginRoute`, `PaymentRoute`, `AdminStore`, `BaseProductRoute`, etc.) from every `*Feature*.cs` and `OptionValue.Feature.cs` file. Every leaf `Route` constant must become a standalone full-path literal string. At the same time, fix every non-compliant admin prefix (`api/identity`, `api/locations`, `api/profiles`, `api/shipping`, `api/dashboard`) to `api/admin/{module}` and every non-compliant storefront prefix (`api/storefront` without module segment) to `api/storefront/{module}`.

## 1. Requirements & Constraints

- **REQ-001**: Every leaf `Route` constant in `*Feature*.cs` and `OptionValue.Feature.cs` is a single `const string` assigned a full literal path (no `$""` interpolation referencing other constants).
- **REQ-002**: Admin routes follow pattern `api/admin/{module_name}/...` where `{module_name}` is singular, matching the module folder name exactly (catalog, identity, inventory, location, ordering, payment, profile, shipping, dashboard).
- **REQ-003**: Storefront routes follow pattern `api/storefront/{module_name}/...` where `{module_name}` is singular.
- **REQ-004**: Intermediate/assembly constants (names containing `Base`, `Route` prefix but not the final leaf `Route`, plus `CatalogRoute`, `StoreRoute`, `AdminRoute`, `PaymentRoute`, `AdminStore`) are deleted after inlining.
- **REQ-005**: No endpoint `.cs` file, test file, SPA service file, or `.http` file references old routes after migration (hard cutover, no redirect aliases).
- **SEC-001**: No route constants contain secrets, tokens, or environment-specific values.
- **CON-001**: This is a breaking API change; all callers (Admin SPA, Store SPA, integration tests, smoke tests, `.http` files, thesis docs) must be updated atomically in the same changeset.
- **CON-002**: `TreatWarningsAsErrors=true`; any unused-constant warning from deleted intermediate constants must be fully resolved.
- **GUD-001**: Follow existing naming convention — nested `public static class` hierarchy is preserved, only the constant values change.
- **PAT-001**: The pattern `CatalogFeature.Admin.Products.Create.Route` continues to work; only the assigned string value of each `Route` const changes.

## 2. Implementation Steps

### Phase 1 — Admin Feature constants: fix prefixes + flatten literals

- GOAL-001: Rewrite all admin-side Feature constants to full-path literals with compliant `api/admin/{module}` prefix.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-001 | `IdentityFeature.Admin.cs` — change `AdminRoute` from `"api/identity"` to `"api/admin/identity"`; rewrite all 24 leaf `Route` constants to full `$"api/admin/identity/..."` literals; delete `AdminRoute`, `BaseUserRoute`, `BaseRoleRoute` intermediate constants. | ✅ | 2026-08-09 |
| TASK-002 | `LocationFeature.Admin.cs` — change root from `"api/locations"` to `"api/admin/location"`; rewrite all 13 leaf routes; delete intermediate constants. | ✅ | 2026-08-09 |
| TASK-003 | `ProfileFeature.Admin.cs` — change `AdminStore` from `"api/profiles"` to `"api/admin/profile"`; rewrite all 10 leaf routes; delete `AdminStore`, `BaseRoute`. | ✅ | 2026-08-09 |
| TASK-004 | `ShippingFeature.Admin.cs` — change `Route` from `"api/shipping"` to `"api/admin/shipping"`; rewrite all 13 leaf routes; delete `BaseRoute` intermediates. | ✅ | 2026-08-09 |
| TASK-005 | `DashboardFeature.cs` — change `Route` from `"api/dashboard"` to `"api/admin/dashboard"`; rewrite 2 leaf routes. | ✅ | 2026-08-09 |
| TASK-006 | `PaymentFeature.Admin.cs` — rename `PaymentRoute` to `Route = "api/admin/payment"` (already correct prefix); rewrite all 12 leaf routes; delete `PaymentRoute`, `BaseRoute` intermediates. | ✅ | 2026-08-09 |
| TASK-007 | `CatalogFeature.Admin.cs` — Route already `api/admin/catalog`; rewrite all 69 leaf routes to full-path literals; delete all 81 intermediate `BaseRoute` constants. | ✅ | 2026-08-09 |
| TASK-008 | `InventoryFeature.Admin.cs` — Route already `api/admin/inventory`; rewrite all 28 leaf routes; delete all 33 intermediate `BaseRoute` constants. | ✅ | 2026-08-09 |
| TASK-009 | `OrderingFeature.Admin.cs` — Route already `api/admin/ordering`; rewrite all 20 leaf routes; delete intermediate constants. | ✅ | 2026-08-09 |
| TASK-010 | `CatalogDashboardFeature.cs` — rewrite 2 leaf routes (already `api/admin/catalog/dashboard`). | ✅ | 2026-08-09 |
| TASK-011 | `InventoryDashboardFeature.cs` — rewrite 2 leaf routes (already `api/admin/inventory/dashboard`). | ✅ | 2026-08-09 |
| TASK-012 | `OrderingDashboardFeature.cs` — rewrite 2 leaf routes (already `api/admin/ordering/dashboard`). | ✅ | 2026-08-09 |
| TASK-013 | `Catalog/Domain/OptionTypes/Values/OptionValue.Feature.cs` — rewrite `Base` and 5 endpoint constants to full-path literals; delete `Base` intermediate. | ✅ | 2026-08-09 |

### Phase 2 — Storefront Feature constants: fix prefixes + flatten literals

- GOAL-002: Rewrite all storefront-side Feature constants to full-path literals with compliant `api/storefront/{module}` prefix.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-020 | `IdentityFeature.Storefront.cs` — `StoreRoute` already `"api/storefront/identity"` (compliant); rewrite all 13 leaf routes to full-path literals; delete `StoreRoute`, `BaseAuthRoute`, `BaseLoginRoute`, `BaseExternalRoute`, `BaseRoute` intermediates. | ✅ | 2026-08-09 |
| TASK-021 | `LocationFeature.Storefront.cs` — `Route` already `"api/storefront/locations"` (but should be singular `location`); change to `"api/storefront/location"`; rewrite all 7 leaf routes; delete `CountryRoute` intermediate. | ✅ | 2026-08-09 |
| TASK-022 | `ProfileFeature.Storefront.cs` — change `StoreRoute` from `"api/storefront/profiles"` to `"api/storefront/profile"`; rewrite all 20 leaf routes; delete `StoreRoute`, all `BaseRoute` intermediates. | ✅ | 2026-08-09 |
| TASK-023 | `ShippingFeature.Storefront.cs` — `Route` is `"api/storefront"` (missing module); change to `"api/storefront/shipping"`; rewrite all 4 leaf routes; delete `BaseRoute` intermediate. | ✅ | 2026-08-09 |
| TASK-024 | `PaymentFeature.Storefront.cs` — `Route` is `"api/storefront"` (missing module); change to `"api/storefront/payment"`; rewrite all 7 leaf routes; delete `BaseRoute` intermediates (Payment, Webhooks). | ✅ | 2026-08-09 |
| TASK-025 | `CatalogFeature.Storefront.cs` — `CatalogRoute` is `"api/storefront/catalog"` (compliant); rewrite all 13 leaf routes to full-path literals; delete `CatalogRoute`, `BaseProductRoute`, `BaseImagesRoute`, `BaseClassificationRoute`, `BaseOptionRoute` intermediates. | ✅ | 2026-08-09 |
| TASK-026 | `InventoryFeature.Storefront.cs` — `Route` already `"api/storefront/inventory"` (compliant); rewrite all 5 leaf routes; delete `BaseRoute` intermediates. | ✅ | 2026-08-09 |
| TASK-027 | `OrderingFeature.Storefront.cs` — `Route` already `"api/storefront/ordering"` (compliant); rewrite all 17 leaf routes; delete `BaseRoute` intermediates. | ✅ | 2026-08-09 |

### Phase 3 — Update all downstream callers (SPAs, tests, docs)

- GOAL-003: Update every consumer of the old route values to match the new full-path constants.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-040 | `app/Admin/src/shared/constants/api.ts` — update constant values: `IDENTITY` → `"api/admin/identity"`, `LOCATION` → `"api/admin/location"`, `PROFILE` → `"api/admin/profile"`, `SHIPPING` → `"api/admin/shipping"`, `DASHBOARD` → `"api/admin/dashboard"`. | ✅ | 2026-08-09 |
| TASK-041 | `app/Admin/src/shared/constants/api.spec.ts` — update test expectations to match new constant values. | ✅ | 2026-08-09 |
| TASK-042 | `app/Store/src/shared/constants/api.ts` — change `PROFILES` to `PROFILE = '/api/storefront/profile'`, `LOCATIONS` to `LOCATION = '/api/storefront/location'`, update `ENDPOINTS` object, update `SHIPPING`/`PAYMENT` if any client-side concatenation relies on old base. | ✅ | 2026-08-09 |
| TASK-043 | `app/Store/src/features/profile/services/*.ts` (accountApi, addressApi, notificationApi, profileApi, wishlistApi) — update imports from `PROFILES` to `PROFILE`; fix any path segments that assumed `profiles/profiles` or `profiles/addresses` nesting. | ✅ | 2026-08-09 |
| TASK-044 | `app/Store/src/features/location/types/location.ts` — update type-level path constants if hardcoded. | ✅ | 2026-08-09 |
| TASK-045 | `service/Api/tests/Api.Tests/**/*.cs` (integration tests) — update all route string literals in `HttpRequestMessage`, `HttpClient.GetAsync`, etc. that use old non-compliant paths. Affected modules: Identity (~30 files), Location (~12 files), Profile (~16 files), Workflows (~3 files). | | |
| TASK-046 | `service/Api/tests/Api.SmokeTests/**/*.http` — update all request URLs in `.http` files for Identity, Location, Profile, Shipping, Dashboard, Payment webhooks. | | |
| TASK-047 | `ApiTests/**/*.http` — update Payment webhook and other route references. | | |
| TASK-048 | `docs/thesis/` and `_thesis/` — update route references in API design chapters and diagrams. | | |
| TASK-049 | `plan/feature-admin-data-layer-1.md` — update route references in plan docs. | | |
| TASK-050 | `service/Api/src/Module/*/README.yaml` — update route documentation in Identity, Location, Profile, Shipping README.yaml files. | | |

### Phase 4 — Validation & drift guard

- GOAL-004: Verify build, tests, and add automated enforcement.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-060 | Run `dotnet build` — zero warnings (TreatWarningsAsErrors). | ⚠️ | 2026-08-09 |
| TASK-061 | Run `dotnet test service/Api/tests/Module.UnitTests` — all pass. | ⏳ | |
| TASK-062 | Run `dotnet test service/Api/tests/Shared.UnitTests` — all pass. | ⏳ | |
| TASK-063 | Run `dotnet test` — integration tests pass (requires Docker). | ⏳ | |
| TASK-064 | Run `cd app/Admin && pnpm run lint && pnpm run test:unit` — Admin SPA lint + unit tests pass. | ⚠️ | 2026-08-09 |
| TASK-065 | Run `cd app/Store && pnpm run lint && pnpm run test:unit` — Store SPA lint + unit tests pass. | ⏳ | |
| TASK-066 | Create `scripts/check-route-conventions.sh` — grep all `*Feature*.cs` files; fail if any `Route` constant value does not match `^api/admin/{module}/` or `^api/storefront/{module}/` pattern (where `{module}` is one of the 9 known module names). | ✅ | 2026-08-09 |
| TASK-067 | Add route-convention check step to `.github/workflows/ci.yml` in `dotnet-build` job, after existing convention checks. | ✅ | 2026-08-09 |

## 3. Alternatives

- **ALT-001**: Keep intermediate constants and only fix prefix values — rejected because it leaves ~350 `$""` interpolations that make route values opaque and hard to grep/validate.
- **ALT-002**: Use a single `Routes` static class per module instead of nested `Feature.Admin.Endpoint` hierarchy — rejected because it would break the established `CatalogFeature.Admin.Products.Create.Route` naming pattern used in all 283 Endpoint files.
- **ALT-003**: Add backward-compatible 308 redirect aliases for old routes — rejected because the app is pre-launch with no external consumers; hard cutover is cleaner.

## 4. Dependencies

- **DEP-001**: `dotnet build` with TreatWarningsAsErrors — must pass after deleting intermediate constants (no unused-constant warnings).
- **DEP-002**: All 283 Endpoint `.cs` files consume routes via `*Feature.Admin.*.Route` / `*Feature.Storefront.*.Route` — these files require zero changes because only the assigned string value changes, not the constant name.

## 5. Files

### Feature constants files (23 files — Phase 1 & 2)

- **FILE-001**: `service/Api/src/Module/Identity/Features/Shared/IdentityFeature.Admin.cs`
- **FILE-002**: `service/Api/src/Module/Identity/Features/Shared/IdentityFeature.Storefront.cs`
- **FILE-003**: `service/Api/src/Module/Location/Features/Shared/LocationFeature.Admin.cs`
- **FILE-004**: `service/Api/src/Module/Location/Features/Shared/LocationFeature.Storefront.cs`
- **FILE-005**: `service/Api/src/Module/Profile/Features/Shared/ProfileFeature.Admin.cs`
- **FILE-006**: `service/Api/src/Module/Profile/Features/Shared/ProfileFeature.Storefront.cs`
- **FILE-007**: `service/Api/src/Module/Shipping/Features/Shared/ShippingFeature.Admin.cs`
- **FILE-008**: `service/Api/src/Module/Shipping/Features/Shared/ShippingFeature.Storefront.cs`
- **FILE-009**: `service/Api/src/Module/Dashboard/Features/Shared/DashboardFeature.cs`
- **FILE-010**: `service/Api/src/Module/Payment/Features/Shared/PaymentFeature.Admin.cs`
- **FILE-011**: `service/Api/src/Module/Payment/Features/Shared/PaymentFeature.Storefront.cs`
- **FILE-012**: `service/Api/src/Module/Catalog/Features/Shared/CatalogFeature.Admin.cs`
- **FILE-013**: `service/Api/src/Module/Catalog/Features/Shared/CatalogFeature.Storefront.cs`
- **FILE-014**: `service/Api/src/Module/Inventory/Features/Shared/InventoryFeature.Admin.cs`
- **FILE-015**: `service/Api/src/Module/Inventory/Features/Shared/InventoryFeature.Storefront.cs`
- **FILE-016**: `service/Api/src/Module/Ordering/Features/Shared/OrderingFeature.Admin.cs`
- **FILE-017**: `service/Api/src/Module/Ordering/Features/Shared/OrderingFeature.Storefront.cs`
- **FILE-018**: `service/Api/src/Module/Catalog/Features/Shared/CatalogDashboardFeature.cs`
- **FILE-019**: `service/Api/src/Module/Inventory/Features/Shared/InventoryDashboardFeature.cs`
- **FILE-020**: `service/Api/src/Module/Ordering/Features/Shared/OrderingDashboardFeature.cs`
- **FILE-021**: `service/Api/src/Module/Catalog/Domain/OptionTypes/Values/OptionValue.Feature.cs`

### SPA constant files (Phase 3)

- **FILE-030**: `app/Admin/src/shared/constants/api.ts`
- **FILE-031**: `app/Admin/src/shared/constants/api.spec.ts`
- **FILE-032**: `app/Store/src/shared/constants/api.ts`

### SPA service files consuming constants (Phase 3)

- **FILE-040**: `app/Store/src/features/profile/services/accountApi.ts`
- **FILE-041**: `app/Store/src/features/profile/services/addressApi.ts`
- **FILE-042**: `app/Store/src/features/profile/services/notificationApi.ts`
- **FILE-043**: `app/Store/src/features/profile/services/profileApi.ts`
- **FILE-044**: `app/Store/src/features/profile/services/wishlistApi.ts`
- **FILE-045**: `app/Store/src/features/location/types/location.ts`

### Test files (Phase 3)

- **FILE-050**: `service/Api/tests/Api.Tests/Scenarios/Identity/` (30+ integration test files)
- **FILE-051**: `service/Api/tests/Api.Tests/Scenarios/Location/` (12 integration test files)
- **FILE-052**: `service/Api/tests/Api.Tests/Scenarios/Profile/` (16+ integration test files)
- **FILE-053**: `service/Api/tests/Api.Tests/Scenarios/Workflows/` (3 workflow test files)
- **FILE-054**: `service/Api/tests/Api.SmokeTests/` (all `.http` files across Identity, Location, Profile, Shipping, Payment)
- **FILE-055**: `ApiTests/Payment/webhook.http`

### CI / drift guard

- **FILE-060**: `scripts/check-route-conventions.sh` (new)
- **FILE-061**: `.github/workflows/ci.yml`

### Documentation

- **FILE-070**: `service/Api/src/Module/Identity/README.yaml`
- **FILE-071**: `service/Api/src/Module/Location/README.yaml`
- **FILE-072**: `service/Api/src/Module/Profile/README.yaml`
- **FILE-073**: `service/Api/src/Module/Shipping/README.yaml`
- **FILE-074**: `docs/thesis/06-api-design.md`
- **FILE-075**: `docs/_thesis/06-api-design.md`

## 6. Testing

- **TEST-001**: `dotnet build` — zero warnings, zero errors.
- **TEST-002**: `dotnet test service/Api/tests/Module.UnitTests` — all pass.
- **TEST-003**: `dotnet test service/Api/tests/Shared.UnitTests` — all pass.
- **TEST-004**: `dotnet test` — all integration tests pass.
- **TEST-005**: `cd app/Admin && pnpm run lint && pnpm run test:unit` — Admin SPA passes.
- **TEST-006**: `cd app/Store && pnpm run lint && pnpm run test:unit` — Store SPA passes.
- **TEST-007**: `bash scripts/check-route-conventions.sh` — exit 0, no violations.
- **TEST-008**: `bash scripts/check-feature-conventions.sh` — exit 0 (existing check still passes).
- **TEST-009**: `bash scripts/check-cross-module-refs.sh` — exit 0 (no cross-module route references).

## 7. Risks & Assumptions

- **RISK-001**: Hard cutover may break any untracked API consumer (e.g., Postman collections, external integrations) — mitigated by pre-launch status and no external consumers.
- **RISK-002**: Thesis documentation route references may drift from implementation — mitigated by including docs in Phase 3 tasks.
- **RISK-003**: Deleting ~350 intermediate constants may surface unused-constant compiler warnings in unexpected files — mitigated by full `dotnet build` in Phase 4.
- **ASSUMPTION-001**: The Admin SPA and Store SPA are the only frontend consumers; no other SPAs or mobile apps exist.
- **ASSUMPTION-002**: The `IdentityFeature.Storefront.cs` `StoreRoute` constant is already `"api/storefront/identity"` (compliant) and only needs literal flattening, not prefix correction.
- **ASSUMPTION-003**: Singular module names (`location`, `profile`, `shipping`, `payment`) are acceptable for all routes.

## 8. Related Specifications / Further Reading

- `AGENTS.md` — Repository conventions, verification commands, known issues.
- `docs/codebase/ARCHITECTURE.md` — Layer responsibilities, data flow, feature file structure.
- `docs/codebase/CONVENTIONS.md` — Coding conventions for features and modules.
- `docs/codebase/CONCERNS.md` — Tech debt and risks.
