---
goal: Capture 46 thesis frontend-UX screenshots (Option C) and wire them into the frontend-ux chapter files
version: 1.0
date_created: 2026-08-20
last_updated: 2026-08-20
owner: Thesis author (ngtphat)
status: 'Planned'
tags: [feature, thesis, screenshots, playwright, typst, frontend-ux]
---

# Introduction

![Status: Planned](https://img.shields.io/badge/status-Planned-blue)

This plan captures the agreed **Option C** set of 46 real-UI screenshots (35 kept
from the original 50 commented-out references + 3 new + 8 restored) for the thesis
frontend-UX chapter (`chapters/part2/ch2-design/04-implementations/05-frontend-ux/`)
using Playwright against the running Vue 3 Admin/Store SPAs and the Hangfire
dashboard, then rewires the `f1`-`f10` Typst files to reference the captured PNGs
(with corrected captions). Six screenshots whose captions describe nonexistent UI
are permanently removed. The work ends with a green `typst compile main.typ` at
~169 pages and a fully-populated `thesis/figures/chapters/part2/ch2-design/04-implementations/screenshots/`
directory (46 PNGs).

## 1. Requirements & Constraints

- **REQ-001**: Capture exactly 46 screenshots: 22 storefront (Batch 1) + 21 admin (Batch 2) + 3 Hangfire (Batch 3), as enumerated in the task tables in Section 2.
- **REQ-002**: Each screenshot must be a PNG at viewport `1920x1200`, `deviceScaleFactor: 2.0`, `fullPage: true`, saved to `thesis/figures/chapters/part2/ch2-design/04-implementations/screenshots/<shot-name>.png`.
- **REQ-003**: Every screenshot must depict real, rendered UI from the running Admin SPA (`app/Admin`, port 5173), Storefront SPA (`app/Store`, port 5174), or Hangfire dashboard (served by `service/Api`, path `/hangfire`). No screenshots of nonexistent components may be captured or referenced.
- **REQ-004**: The 6 fabricated screenshots must be permanently removed from the thesis source: `storefront-payment-stripe`, `storefront-cbir-loading`, `storefront-cbir-empty-results`, `admin-reference-data`, `admin-taxonomy-tree`, `admin-option-types`.
- **REQ-005**: Caption text for all referenced screenshots must match the real UI described in the per-shot captions table below (Section 5), incorporating the verified fixes for `admin-product-create` (PickList dual-transfer), `admin-orders-grid` (status + checkout-state filters), `storefront-cbir-results` ("Results (N)" grid + similarity badges), and all `storefront-checkout-*` ("Stepper" wizard).
- **REQ-006**: Final `typst compile main.typ` from `thesis/` must exit 0 with no missing-image errors, and the compiled PDF must be <= 170 pages total (est. ~169). Page headroom to 170 is ~28; the target is to stay within budget.
- **SEC-001**: No real customer PII, passwords, JWT tokens, or Stripe test keys may appear in any captured screenshot. Use seeded test data only.
- **CON-001**: All screenshots must come from actual rendered UI. Do not hand-craft or edit PNGs.
- **CON-002**: The thesis build must remain green after edits; do not reference any image file that does not exist on disk.
- **CON-003**: Do not delete the `04-implementation/` (singular) directory or any referenced diagram; the `P2S2.2.2_cbir-search-sequence.png` reference in `04-implementations/03-data-persistence`/`04-ml-sidecar` may be left as-is if it resolves, else fix only the caption mismatch already logged (see REQ-007).
- **REQ-007**: Fix the known path mismatch for `P2S2.2.2_cbir-search-sequence.png` (referenced but exists as `P2S2.2.4_cbir-search-sequence.png` in the `diagrams/` subdir) only if the referenced file is missing; otherwise leave unchanged.
- **REQ-008**: Extend `thesis/spec/verify_remediation.py` with a screenshot-verification mode that checks the 46 expected PNG filenames exist in the screenshots dir, the 6 fabricated screenshot names are absent from `thesis/chapters/`, and reports the remediation-log row as verified.
- **GUD-001**: Work root-cause-first: capture all screenshots before editing any `.typ` file, so every uncommented `image(...)` points at an existing file.
- **GUD-002**: Follow the existing chapter pattern: edit the numbered `fN-*.typ` files only, never the aggregator `frontend-ux.typ`.
- **PAT-001**: New storefront screenshots `storefront-home` and `storefront-cbir-params` go in `f1-visual-search.typ`/`f2-catalog-cart.typ`; new `admin-dashboard` goes in `f7-order-payment.typ` (chosen as the analytics/dashboard home; alternative `f10-system-processes.typ` documented in ALT-001).

## 2. Implementation Steps

### Implementation Phase 1 — Capture storefront screenshots (Batch 1, 22 shots)

- GOAL-001: Capture all 22 storefront screenshots from the Storefront SPA at `http://localhost:5174`.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-001 | Run `pnpm install` and start the Storefront SPA (`app/Store`, Vite dev server port 5174) and API (`dotnet run` via Aspire AppHost in `infra/Aspire`) so `http://localhost:5174` responds. | | |
| TASK-002 | Seed test data via existing API smoke tests (`service/Api/tests/Api.SmokeTests/`) or `DatabaseInitializer` so the storefront shows products, a populated cart, orders, addresses, wishlists, and notifications. | | |
| TASK-003 | Capture `storefront-home.png` (NEW) from `/` (HomeView.vue:131 hero + carousel). | | |
| TASK-004 | Capture `storefront-catalog-grid.png` from `/shop` (catalog grid). | | |
| TASK-005 | Capture `storefront-product-detail.png` from `/products/:id` (product detail). | | |
| TASK-006 | Capture `storefront-cart.png` from `/cart` with at least one line item. | | |
| TASK-007 | Capture `storefront-cart-empty.png` (RESTORED) from `/cart` after clearing items (empty-state view). | | |
| TASK-008 | Capture `storefront-cbir-empty.png` from `/recommendations` before upload (empty/initial state). | | |
| TASK-009 | Capture `storefront-cbir-upload.png` from `/recommendations` after selecting an image (upload state). | | |
| TASK-010 | Capture `storefront-cbir-results.png` from `/recommendations` with results grid + similarity badges ("Results (N)"). | | |
| TASK-011 | Capture `storefront-cbir-params.png` (NEW) from `/recommendations` showing the ML model selector + 3 sliders (VisualSearchView.vue:78). | | |
| TASK-012 | Capture `storefront-checkout-address.png` from `/checkout` step 1 (Stepper wizard). | | |
| TASK-013 | Capture `storefront-checkout-delivery.png` from `/checkout` step 2 (Stepper wizard). | | |
| TASK-014 | Capture `storefront-checkout-payment.png` from `/checkout` step 3 (Stepper wizard). | | |
| TASK-015 | Capture `storefront-checkout-confirm.png` from `/checkout` step 4 (Stepper wizard). | | |
| TASK-016 | Capture `storefront-checkout-complete.png` (RESTORED) from `/checkout` step 5 (order-complete). | | |
| TASK-017 | Capture `storefront-order-history.png` from `/account/orders`. | | |
| TASK-018 | Capture `storefront-order-detail.png` from `/account/orders/:id` (RESTORED; OrderDetailView.vue:428 with 2x Timeline + Pay-now). | | |
| TASK-019 | Capture `storefront-login.png` from `/login`. | | |
| TASK-020 | Capture `storefront-register.png` from `/register`. | | |
| TASK-021 | Capture `storefront-sessions.png` from `/sessions`. | | |
| TASK-022 | Capture `storefront-profile-addresses.png` from `/account/addresses`. | | |
| TASK-023 | Capture `storefront-profile-wishlists.png` from `/account/wishlists`. | | |
| TASK-024 | Capture `storefront-profile-notifications.png` from `/account/notifications`. | | |
| TASK-025 | Verify all 22 PNGs exist in `thesis/figures/chapters/part2/ch2-design/04-implementations/screenshots/` and are non-empty. | | |

### Implementation Phase 2 — Capture admin screenshots (Batch 2, 21 shots)

- GOAL-002: Capture all 21 admin screenshots from the Admin SPA at `http://localhost:5173`.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-026 | Start the Admin SPA (`app/Admin`, Vite dev server port 5173) and log in as an admin user (seed via `Identity` seeder / smoke tests). | | |
| TASK-027 | Capture `admin-dashboard.png` (NEW) from `/dashboard` (DashboardPage.vue:85, 4 KPI cards). | | |
| TASK-028 | Capture `admin-product-list.png` from `/catalog/products` (data table). | | |
| TASK-029 | Capture `admin-product-create.png` from `/catalog/products/:id` (PickList dual-transfer Basic Info/Fashion/SEO form). | | |
| TASK-030 | Capture `admin-product-variants.png` from `/catalog/products/:id` (variants tab). | | |
| TASK-031 | Capture `admin-variant-pricing.png` from `/catalog/variants/:id` (pricing fields). | | |
| TASK-032 | Capture `admin-product-images.png` from `/catalog/variants/:id` (images/gallery). | | |
| TASK-033 | Capture `admin-orders-grid.png` from `/ordering/orders` (status + checkout-state filters). | | |
| TASK-034 | Capture `admin-order-detail.png` from `/ordering/orders/:id` (order detail). | | |
| TASK-035 | Capture `admin-payment-detail.png` from `/payment/payments` (payment table). | | |
| TASK-036 | Capture `admin-payment-methods.png` (RESTORED) from `/payment/payment-methods`. | | |
| TASK-037 | Capture `admin-user-list.png` from `/identity/users`. | | |
| TASK-038 | Capture `admin-user-edit.png` from `/identity/users/:id`. | | |
| TASK-039 | Capture `admin-role-list.png` (RESTORED) from `/identity/roles`. | | |
| TASK-040 | Capture `admin-role-permissions.png` from `/identity/roles/:id` (permission matrix). | | |
| TASK-041 | Capture `admin-inventory-stock.png` from `/inventory/stock-items`. | | |
| TASK-042 | Capture `admin-inventory-movements.png` from `/inventory/stock-movements`. | | |
| TASK-043 | Capture `admin-stock-locations.png` from `/inventory/stock-locations`. | | |
| TASK-044 | Capture `admin-inventory-restock.png` (RESTORED) from `/inventory/stock-locations/:id` (restock dialog). | | |
| TASK-045 | Capture `admin-inventory-transfer.png` (RESTORED) from `/inventory/stock-transfers` (transfer dialog). | | |
| TASK-046 | Capture `admin-shipping-methods.png` from `/shipping/shipping-methods`. | | |
| TASK-047 | Capture `admin-shipping-rates.png` from `/shipping/shipping-rates`. | | |
| TASK-048 | Verify all 21 PNGs exist in the screenshots dir and are non-empty. | | |

### Implementation Phase 3 — Capture Hangfire dashboard screenshots (Batch 3, 3 shots)

- GOAL-003: Capture the 3 Hangfire dashboard screenshots from the API-hosted dashboard.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-049 | Confirm Hangfire dashboard is reachable at `http://localhost:5000/hangfire` (served by the .NET API middleware, not the Admin SPA). | | |
| TASK-050 | Capture `hangfire-dashboard-overview.png` from `/hangfire`. | | |
| TASK-051 | Capture `hangfire-queues.png` (RESTORED) from `/hangfire/queues`. | | |
| TASK-052 | Capture `hangfire-job-detail.png` (RESTORED) from `/hangfire/jobs/:id`. | | |
| TASK-053 | Verify all 3 PNGs exist in the screenshots dir and are non-empty. | | |

### Implementation Phase 4 — Rewire frontend-ux chapter files

- GOAL-004: Update the `f1`-`f10` Typst files so every screenshot reference points at an existing PNG with an accurate caption.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-054 | Grep `grep -rn "//\\s*image(" thesis/chapters/part2/ch2-design/04-implementations/05-frontend-ux/*.typ` to enumerate the 50 commented refs and map each to its target screenshot filename. | | |
| TASK-055 | Uncomment the 35 kept screenshot `image(...)` references (those not in the REQ-004 removal list), updating the caption text per REQ-005. | | |
| TASK-056 | Add the 3 new screenshot blocks: `storefront-home` (f2-catalog-cart.typ), `storefront-cbir-params` (f1-visual-search.typ), `admin-dashboard` (f7-order-payment.typ). | | |
| TASK-057 | Restore the 8 removed-then-restored references: `storefront-cart-empty`, `storefront-checkout-complete`, `storefront-order-detail`, `admin-role-list`, `admin-inventory-restock`, `admin-inventory-transfer`, `admin-payment-methods`, `hangfire-queues`, `hangfire-job-detail`. | | |
| TASK-058 | Delete the 6 fabricated screenshot blocks per REQ-004 (remove from f1-f7 files). | | |
| TASK-059 | Fix caption of `admin-product-create` to describe the PickList dual-transfer form (not "Basic Info/Fashion/SEO form"). | | |
| TASK-060 | Fix caption of `admin-orders-grid` to describe status + checkout-state filters (drop "Summary bar"). | | |
| TASK-061 | Fix caption of `storefront-cbir-results` to "Results (N)" grid + similarity badges (drop sidebar/metadata bar). | | |
| TASK-062 | Fix captions of all `storefront-checkout-*` to reference the Stepper wizard (not "progress bar"). | | |
| TASK-063 | Verify per REQ-007: check whether `P2S2.2.2_cbir-search-sequence.png` resolves; fix mismatch only if missing. | | |
| TASK-064 | Confirm `grep -rn "storefront-payment-stripe\\|storefront-cbir-loading\\|storefront-cbir-empty-results\\|admin-reference-data\\|admin-taxonomy-tree\\|admin-option-types" thesis/chapters/` returns zero hits. | | |

### Implementation Phase 5 — Verify build and page budget

- GOAL-005: Produce a green thesis build within the page budget.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-065 | Run `typst compile main.typ` from `thesis/`; confirm exit 0 and no missing-image errors. | | |
| TASK-066 | Confirm total page count <= 170 and count figures added to the List of Figures (est. ~169 pages). | | |
| TASK-067 | Confirm exactly 46 PNGs exist in the screenshots dir. | | |
| TASK-068 | Append the completed screenshot-capture row to `thesis/spec/remediation-log.md` and run `python thesis/spec/verify_remediation.py` (extended per REQ-008) to confirm all edits verify. | | |

## 3. Alternatives

- **ALT-001**: Place `admin-dashboard` in `f10-system-processes.typ` instead of `f7-order-payment.typ`. Chosen alternative: f7 (dashboard is order/payment-adjacent analytics); f10 is the fallback if the dashboard screenshot is judged to belong with system processes.
- **ALT-002**: Hand-write screenshot PNGs or reuse prior mockup images instead of capturing live UI. Rejected: violates CON-001 (real UI only) and would misrepresent the actual application.
- **ALT-003**: Keep all 50 original screenshot references (Option A) instead of removing the 6 fabricated ones. Rejected: 6 captions describe nonexistent UI (Stripe iframe, skeleton grid, empty-results illustration, reference-data panel, taxonomy tree, option-type drag handles), which would misrepresent the system.
- **ALT-004**: Reduce to a minimal ~15-20 shot set (Option B). Rejected: would omit essential views (storefront home, CBIR params, admin dashboard) and key states (empty cart, checkout complete, order detail) that the thesis audit identified as required.

## 4. Dependencies

- **DEP-001**: Running backend via Aspire AppHost (`infra/Aspire`) with PostgreSQL + Redis, API on `http://localhost:5000`.
- **DEP-002**: Storefront SPA dev server (`app/Store`) on `http://localhost:5174`.
- **DEP-003**: Admin SPA dev server (`app/Admin`) on `http://localhost:5173`.
- **DEP-004**: Seeded test data: catalog products with images, cart line items, orders (one with a full checkout history), addresses, wishlists, notifications, users/roles/permissions, stock items/movements/locations, shipping methods/rates (via `DatabaseInitializer` or `Api.SmokeTests`).
- **DEP-005**: Playwright (any version compatible with the installed Node; add `@playwright/test` + Chromium) — not currently in the repo; install in a scratch dir or add as dev dep.
- **DEP-006**: Typst 0.15.1 (`typst compile main.typ` from `thesis/`), per `thesis/AGENTS.md`.
- **DEP-007**: Existing chapter files `chapters/part2/ch2-design/04-implementations/05-frontend-ux/f1-visual-search.typ` ... `f10-system-processes.typ` (the 50 commented refs live here).

## 5. Files

- **FILE-001**: `thesis/figures/chapters/part2/ch2-design/04-implementations/screenshots/` — target directory (created; currently absent).
- **FILE-002**: `app/Store/` — Storefront SPA source (HomeView.vue:131, VisualSearchView.vue:78/185/206, CheckoutView.vue:582, OrderDetailView.vue:428).
- **FILE-003**: `app/Admin/` — Admin SPA source (DashboardPage.vue:85, ProductDetail.vue:526, VariantDetail.vue, etc.).
- **FILE-004**: `service/Api/` — Hangfire dashboard host, seeders, smoke tests.
- **FILE-005**: `chapters/part2/ch2-design/04-implementations/05-frontend-ux/f1-visual-search.typ` — CBIR screenshot refs (incl. `storefront-cbir-params` NEW).
- **FILE-006**: `chapters/part2/ch2-design/04-implementations/05-frontend-ux/f2-catalog-cart.typ` — catalog/cart/home refs (incl. `storefront-home` NEW).
- **FILE-007**: `chapters/part2/ch2-design/04-implementations/05-frontend-ux/f3-checkout.typ` — checkout Stepper refs.
- **FILE-008**: `chapters/part2/ch2-design/04-implementations/05-frontend-ux/f4-order-auth-payment.typ` — order/auth/payment refs.
- **FILE-009**: `chapters/part2/ch2-design/04-implementations/05-frontend-ux/f5-profile.typ` — profile refs.
- **FILE-010**: `chapters/part2/ch2-design/04-implementations/05-frontend-ux/f6-product-management.typ` — product-management refs (incl. `admin-product-create` caption fix).
- **FILE-011**: `chapters/part2/ch2-design/04-implementations/05-frontend-ux/f7-order-payment.typ` — order/payment/dashboard refs (incl. `admin-dashboard` NEW; delete `admin-reference-data`).
- **FILE-012**: `chapters/part2/ch2-design/04-implementations/05-frontend-ux/f8-inventory.typ` — inventory refs.
- **FILE-013**: `chapters/part2/ch2-design/04-implementations/05-frontend-ux/f9-user-shipping.typ` — user/shipping refs.
- **FILE-014**: `chapters/part2/ch2-design/04-implementations/05-frontend-ux/f10-system-processes.typ` — Hangfire refs.
- **FILE-015**: `thesis/main.typ` — top-level include for the chapter files (no structural change expected).
- **FILE-016**: `thesis/spec/remediation-log.md` — remediation log (append screenshot-capture row).
- **FILE-017**: `thesis/spec/verify_remediation.py` — verification oracle (extend per REQ-008).
- **FILE-018**: `infra/Aspire/` — orchestration AppHost to start the backend stack.

## 6. Testing

- **TEST-001**: `typst compile main.typ` exits 0 with no missing-image errors (REQ-006, CON-002).
- **TEST-002**: Exactly 46 PNG files exist and are non-empty in the screenshots dir (REQ-001, REQ-002).
- **TEST-003**: `grep -rn "storefront-payment-stripe\|storefront-cbir-loading\|storefront-cbir-empty-results\|admin-reference-data\|admin-taxonomy-tree\|admin-option-types" thesis/chapters/` returns zero hits (REQ-004).
- **TEST-004**: `grep -rn "//\\s*image(" thesis/chapters/part2/ch2-design/04-implementations/05-frontend-ux/` returns zero hits after Phase 4 (no dangling commented refs; every screenshot is either active or deleted) — except any intentionally retained non-screenshot comment.
- **TEST-005**: Every active `image(...)` path in the `f1`-`f10` files resolves to an existing file on disk (checked by a script that parses `image("@...")`/`image("figures/...")` refs).
- **TEST-006**: PDF page count <= 170 after compile (REQ-006; est. ~169).
- **TEST-007**: `python thesis/spec/verify_remediation.py` runs to completion and reports the screenshot-capture remediation row as verified (REQ-008).

## 7. Risks & Assumptions

- **RISK-001**: Some storefront routes require an authenticated customer session (orders, wishlists, notifications). Mitigation: reuse the smoke-test login flow to obtain a token and pass it to Playwright's `storageState`.
- **RISK-002**: Admin routes require admin claims (roles/permissions). Mitigation: seed an admin user with full permissions before capture (Identity seeder / smoke tests).
- **RISK-003**: The `/hangfire` dashboard may require Hangfire authorization; if so, run the API with Hangfire auth disabled in a dev profile for capture.
- **RISK-004**: Page budget could exceed 170 if more screenshots than estimated are added. Mitigation: 46 shots is fixed; if compile shows >170, compress or reduce caption length rather than drop shots.
- **RISK-005**: Playwright is not yet in the repo; installing it may require network access. Mitigation: install into a scratch directory (e.g. `thesis/tools/screenshots/`) so it does not touch the repo's `pnpm` workspaces; add to `package.json` only if the repo convention allows.
- **ASSUMPTION-001**: The `screenshots/` directory name and per-shot filenames exactly match the `image(...)` paths already written in the `f1`-`f10` files (the 50 refs use these names), so uncommenting needs no path edits.
- **ASSUMPTION-002**: The current compiled PDF is 159 pages total (142 main body + 17 appendix), and the 46-shot set adds ~27 pages for ~169 total — within the 170 budget. Verify at TEST-006.
- **ASSUMPTION-003**: `verify_remediation.py` will be extended with a screenshot-verification mode (REQ-008) rather than remaining benchmark-only.
- **ASSUMPTION-004**: The apps and API run locally under Aspire; no cloud/staging environment is needed for capture.

## 8. Related Specifications / Further Reading

- `thesis/AGENTS.md` — thesis build/structure conventions (Typst 0.15.1, chapter pattern, figure conventions).
- `thesis/spec/spec-process-thesis-review-remediation.md` — remediation process spec (REQ/CON numbering, log schema, verify-before-fix).
- `thesis/spec/remediation-log.md` — remediation log (this plan appends the screenshot-capture row).
- `thesis/spec/verify_remediation.py` — verification oracle (extended per REQ-008).
- `/home/ngtphat/Projects/ReSys.Shop/AGENTS.md` — platform-level rules and verification commands.
- `thesis/chapters/part2/ch2-design/04-implementations/05-frontend-ux/*.typ` — the f1-f10 chapter files edited by this plan.