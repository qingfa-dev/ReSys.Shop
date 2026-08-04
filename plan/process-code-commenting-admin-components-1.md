---
goal: Add full and necessary code commentation to Admin view .vue and .ts files
version: 1.2
date_created: 2026-08-04
last_updated: 2026-08-04
owner: Admin SPA squad
status: 'Planned'
tags: [process, docs, admin, views, commenting]
---

# Introduction

![Status: Planned](https://img.shields.io/badge/status-Planned-blue)

This plan adds human- and agent-readable comments to the **view** `.vue` and
`.ts` files under `app/Admin/src/features/*/views/`, using the repository Code
Commenting Standard v3.0 (`guide/code-commenting/`). Its goal is **full but
necessary code commentation**: every major block inside each `<template>` is
demarcated with a section comment (page header, error state, table, filters,
row actions, empty state, action footer, ...) and every non-obvious `<script
setup>` block carries the correct inline label (CAT-1..CAT-10) — so the code
is fully navigable — while trivial, self-evident lines receive no comment
(semantic density, AP-1/AP-3 anti-patterns). "Full" means nothing meaningful
is left uncommented; "necessary" means no comment that merely restates the
code is added. No runtime behaviour is changed.

## 1. Requirements & Constraints

- **REQ-001**: Apply the Code Commenting Standard v3.0 from `guide/code-commenting/` (`CommentingRules.xml`, `README.md`, `SKILL.md`) to all view `.vue` and `.ts` files in `app/Admin/src/features/*/views/`.
- **REQ-002**: Add a template section comment (the full "Section code commentation") above each major block in every `<template>`.
- **REQ-003**: Annotate `<script setup>` logic with the correct CAT-1..CAT-10 label at validation, mapping/computed, trigger/async, and API-call sites.
- **REQ-004**: Annotate every feature `views/index.ts` barrel with a `Boundary:` header comment.
- **REQ-005**: Provision the convention in `app/Admin/AGENTS.md` and `app/Admin/README.md` so future view files follow the same section format.
- **REQ-006**: Provide "full and necessary" commentation in every view: every major `<template>` section AND every non-obvious `<script setup>` block is commented; self-evident lines are left uncommented.
- **SEC-001**: Comments must never contain secrets, API tokens, credentials, tenant/IP addresses, or PII — any such content is forbidden.
- **CON-001**: Do not change markup, props, emits, `v-if`/`v-else` structure, bindings, handlers, or any runtime logic; comments are the only additions.
- **CON-002**: Do not rename, move, or delete any file or directory; generated files (`components.d.ts`) are untouched.
- **CON-003**: Comments must match actual current behaviour to avoid AP-5 stale comments; verify before writing.
- **CON-004**: Keep every comment line under 100 characters (formatting rule F3).
- **CON-005**: Do not add multi-line `/* ... */` blocks in `<script setup>`; prefer single-line `//` labels to avoid `warnings-as-errors` lint failures.
- **GUD-001**: Before writing each comment, traverse the Label Decision Tree (CAT-1..CAT-10) to pick the correct label; do not invent labels.
- **GUD-002**: Format each script comment as `// Label: Capitalised imperative sentence.` on its own line (F2, F8, F10).
- **GUD-003**: Follow semantic density (P6) — comment the WHY, never the WHAT; skip comments where naming/structure already carry intent.
- **GUD-004**: Format each template section comment as `<!-- Section: <Title> — <purpose> -->` on its own line, indented to match its block, kept under 100 chars.
- **GUD-005**: "Necessary" is a quality gate, not a count target: comment a line/block only when the WHY is non-obvious (P3); never comment what the code already expresses (AP-1 redundancy), never comment trivial lines (AP-3 over-commenting).
- **PAT-001**: Follow the existing per-view `views/index.ts` barrel re-export (e.g. `src/features/catalog/views/index.ts`).
- **PAT-002**: Canonical List-view template section order — decorate with `Section:` markers in this sequence: (1) `Page Header`, (2) `Scrollable Content`, (3) `Error State`, (4) `Data Table`, (5) `Search & Filters` (table `#header`), (6) `Table Columns`, (7) `Row Actions`, (8) `Empty State`.
- **PAT-003**: Canonical Detail-view template section order — `Section:` markers: (1) `Page Header`, (2) `Content Card`, (3) `Tabs`, (4) `Form Fields` (one per headline field group), (5) `Action Footer`.

## 2. Implementation Steps

### Implementation Phase 1 — Provision the View Commenting Standard

- GOAL-001: Document the template-section and script-label convention for Admin views.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-001 | Create `app/Admin/AGENTS.md` declaring the effective view-commenting rules: link `guide/code-commenting/README.md`, `guide/code-commenting/SKILL.md`, `guide/code-commenting/CommentingRules.xml`, restate `// Label: Capitalised sentence.` (F2/F8/F10), the 100-char limit (F3), and the template Section format from GUD-004/PAT-002/PAT-003| ✓ | |
| TASK-002 | In `app/Admin/README.md` append a `## Template Section Commenting Standard` section (before the project-setup block) embedding the `<!-- Section: ... -->` format and the canonical List/Detail section orders (PAT-002, PAT-003)| ✓ | |
| TASK-003 | In `app/Admin/README.md` append a `## View Code-Commenting Rules` section listing the required script labels per view operation (validate → `Validate:`, computed → `Compute:`/`Transform:`, API → `Call:`/`Load:`, confirm/flush → `Trigger:`/`Handle:`) and referencing GUD-001/GUD-002| ✓ | |

### Implementation Phase 2 — Annotate View Templates & Script (by feature)

- GOAL-002: Add full and necessary commentation (template Section markers + script labels) to all view files.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-004 | Annotate the **auth** views `LoginPage.vue`, `ForgotPasswordPage.vue`, `ResetPasswordPage.vue`: add Section markers per PAT-002 (form-oriented: Header, Validation, Action Footer), add `Validate:` before guard/validation and `Call:`/`Transform:` around `Submit` handler calls. Leave self-evident bindings uncommented (GUD-005)| ✓ | |
| TASK-005 | Annotate the **catalog** list/detail views `ProductsList.vue`, `OptionTypesList.vue`, `TaxonomiesList.vue`, `TaxonsList.vue`, `VariantsList.vue`, `ProductDetail.vue`, `OptionTypeDetail.vue`, `TaxonomyDetail.vue`, `TaxonDetail.vue`, `VariantDetail.vue`: apply PAT-002 Section markers to all list templates (mirror ProductsList.vue: Header, Scrollable Content, Error State, Data Table, Filters, Columns, Row Actions, Empty) and script labels — `Map:` in `first` computed, `Filter:` in `applyFilters`, `Trigger:` in `confirmStatusChange`/`confirmDelete`. Do not comment plain column bindings (GUD-005)| ✓ | |
| TASK-006 | Annotate the **dashboard** view `DashboardPage.vue`: add PAT-002 Section markers (Header, KPI Row, Chart/Table content) and `Compute:`/`Aggregate:` labels on derived metrics| ✓ | |
| TASK-007 | Annotate the **identity** views `PermissionsList.vue`, `RolesList.vue`, `RoleDetail.vue`, `UsersList.vue`, `UserDetail.vue`: apply PAT-002/PAT-003 Section markers and script labels (`Check:` for permission checks, `Enforce:` for tenant/role rules)| ✓ | |
| TASK-008 | Annotate the **inventory** views `StockItemsList.vue`, `StockItemDetail.vue`, `StockLocationsList.vue`, `StockLocationDetail.vue`, `StockMovementsList.vue`, `StockReservationsList.vue`, `StockTransfersList.vue`, `StockTransferDetail.vue`: apply PAT-002/PAT-003 Section markers and script labels (`Call:` for movement APIs, `Trigger:` for transfer/status flushes)| ✓ | |
| TASK-009 | Annotate the **location** views `CountriesList.vue`, `StatesList.vue`, `CountryDetail.vue`, `StateDetail.vue`: apply PAT-002/PAT-003 Section markers and script labels| ✓ | |
| TASK-010 | Annotate the **ordering** views `OrdersList.vue`, `OrderDetail.vue`: apply PAT-002/PAT-003 Section markers (list table; detail: Header, Steps/Tabs, Line Items, Action Footer) and script labels (`Verify:` for totals, `Trigger:` for status flushes)| ✓ | |
| TASK-011 | Annotate the **payment** views `PaymentsList.vue`, `PaymentMethodsList.vue`, `PaymentMethodDetail.vue`: apply PAT-002/PAT-003 Section markers and script labels (`Validate:` for method input, `Call:` for charge/refund)| ✓ | |
| TASK-012 | Annotate the **profile** views `ProfilesList.vue`, `ProfileDetail.vue`, `AddressesList.vue`, `AddressDetail.vue`: apply PAT-002/PAT-003 Section markers and script labels (`Map:` for address default computation)| ✓ | |
| TASK-013 | Annotate the **shipping** views `ShippingMethodsList.vue`, `ShippingMethodDetail.vue`, `ShippingRatesList.vue`, `ShippingRateDetail.vue`: apply PAT-002/PAT-003 Section markers and script labels (`Compute:` for rate calculations)| ✓ | |

### Implementation Phase 3 — Barrels & Script-Label Audit

- GOAL-003: Annotate view barrels and audit script logic labels for completeness.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-014 | Add a `// Boundary: views barrel — re-export only; do not add view logic here` header comment on the first `export {}` line of every `views/index.ts`: auth, catalog, dashboard, identity, inventory, location, ordering, payment, profile, shipping| ✓ | |
| TASK-015 | Audit every annotated view for full-and-necessary compliance: (a) confirm every major template section carries a `Section:` marker and every `computed` has a `Compute:`/`Transform:` comment, every `await ServiceApi.*` has a `Call:` comment, every `confirm.require`/status-flush has a `Trigger:` comment, and every guard has a `Validate:` comment — adding any missing single-line labels; (b) remove or reword any comment that merely restates code (AP-1) or comments a trivial line (AP-3)| ✓ | |

### Implementation Phase 4 — Verify

- GOAL-004: Confirm lint, type-check, build, and unit tests pass with zero regressions.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-016 | Run from `app/Admin`: `pnpm run lint` (0 errors), `pnpm run build-only` (0 errors), `pnpm run type-check` (0 errors)| ✓ | |
| TASK-017 | Run from `app/Admin`: `pnpm run test:unit -- run` — all existing unit tests pass unchanged| ✓ | |
| TASK-018 | Validate that every existing unit test under `src/features/**/__tests__` and `src/shared/**/__tests__` passes unchanged| ✓ | |
| TASK-019 | Grep-audit all edited `.vue` files: (a) confirm each `<template>` has at least one `<!-- Section:` marker; (b) confirm no comment line exceeds 100 chars (F3); (c) confirm no commented-out code (AP-6) is introduced; (d) spot-check for over-commenting (AP-1/AP-3) — flag any comment that restates an adjacent line for removal| ✓ | |

## 3. Alternatives

- **ALT-001**: Enforce comment rules via an ESLint plugin — rejected because it cannot enforce the label/semantics of GUD-001/GUD-002 and would fail the build under `warnings-as-errors`; a documented convention with PR review is chosen.
- **ALT-002**: Put all template-section markers as `//` script-style comments — rejected because Vue `<template>` only renders HTML comments; `<!-- Section: ... -->` is the correct mechanism for template regions.
- **ALT-003**: Annotate shared components separately in the same pass — deferred; this plan scopes strictly to view `.vue` and `.ts` files per the requirement.

## 4. Dependencies

- **DEP-001**: `guide/code-commenting/SKILL.md` — authoritative label decision tree and comment workflow.
- **DEP-002**: `guide/code-commenting/CommentingRules.xml` — machine-readable CAT-1..CAT-10, formatting F1..F10, temporal markers.
- **DEP-003**: `app/Admin/eslint.config.ts`, `.oxlintrc.json`, and the `pnpm` toolchain — used to run verification; not modified.
- **DEP-004**: `app/Admin/src/features/catalog/views/ProductsList.vue` — canonical exemplar template for the section-marker pattern (PAT-002).

## 5. Files

- **FILE-001**: `app/Admin/AGENTS.md` (new) — view commenting standard provisioning.
- **FILE-002**: `app/Admin/README.md` — add `## Template Section Commenting Standard` + `## View Code-Commenting Rules`.
- **FILE-003**: `src/features/auth/views/*` — LoginPage.vue, ForgotPasswordPage.vue, ResetPasswordPage.vue, index.ts.
- **FILE-004**: `src/features/catalog/views/ProductsList.vue`, OptionTypesList.vue, TaxonomiesList.vue, TaxonsList.vue, VariantsList.vue, index.ts.
- **FILE-005**: `src/features/catalog/views/ProductDetail.vue`, OptionTypeDetail.vue, TaxonomyDetail.vue, TaxonDetail.vue, VariantDetail.vue.
- **FILE-006**: `src/features/dashboard/views/DashboardPage.vue`, index.ts.
- **FILE-007**: `src/features/identity/views/PermissionsList.vue`, RolesList.vue, RoleDetail.vue, UsersList.vue, UserDetail.vue, index.ts.
- **FILE-008**: `src/features/inventory/views/StockItemsList.vue`, StockItemDetail.vue, StockLocationsList.vue, StockLocationDetail.vue, StockMovementsList.vue, StockReservationsList.vue, StockTransfersList.vue, StockTransferDetail.vue, index.ts.
- **FILE-009**: `src/features/location/views/CountriesList.vue`, StatesList.vue, CountryDetail.vue, StateDetail.vue, index.ts.
- **FILE-010**: `src/features/ordering/views/OrdersList.vue`, OrderDetail.vue, index.ts.
- **FILE-011**: `src/features/payment/views/PaymentsList.vue`, PaymentMethodsList.vue, PaymentMethodDetail.vue, index.ts.
- **FILE-012**: `src/features/profile/views/ProfilesList.vue`, ProfileDetail.vue, AddressesList.vue, AddressDetail.vue, index.ts.
- **FILE-013**: `src/features/shipping/views/ShippingMethodsList.vue`, ShippingMethodDetail.vue, ShippingRatesList.vue, ShippingRateDetail.vue, index.ts.

## 6. Testing

- **TEST-001**: `pnpm run lint` from `app/Admin` exits 0.
- **TEST-002**: `pnpm run build-only` exits 0 (production build passes `warnings-as-errors`).
- **TEST-003**: `pnpm run type-check` exits 0.
- **TEST-004**: `pnpm run test:unit -- run` — all existing unit tests pass (no regressions, no new tests).
- **TEST-005**: Grep check: every edited `.vue` has at least one `<!-- Section:` marker; no comment line exceeds 100 chars; no commented-out code (CON-004, F3, AP-6).
- **TEST-006**: Necessity spot-check: sample 3 annotated views and confirm their comments explain WHY (non-obvious) and no comment restates code verbatim (AP-1) or annotates every trivial line (AP-3).

## 7. Risks & Assumptions

- **RISK-001**: Some view templates are small/trivial (e.g. auth pages, `DashboardPage.vue`) with fewer sections; apply at least Header + one content Section and skip only where no distinct block exists.
- **RISK-002**: Adding comments may trip `warnings-as-errors` lint if multi-line — restrict all script comments to single line `//` under 100 chars (CON-005).
- **RISK-003**: "Necessary" (GUD-005) is a judgment call; a reviewer and implementer may disagree on whether a comment is warranted. Treat the plan's explicit marker/label mandates as required and emergent over-commenting (AP-1/AP-3) as fixable during review loops.
- **ASSUMPTION-001**: `app/Admin` auto-imports components via `components.d.ts`, so comment-only edits never affect registration.
- **ASSUMPTION-002**: List/detail views consistently follow the canonical template order observed in `ProductsList.vue`, making the Section markers safe to apply at fixed block boundaries.

## 8. Related Specifications / Further Reading

- `guide/code-commenting/README.md`
- `guide/code-commenting/CommentingRules.xml`
- `guide/code-commenting/SKILL.md`
- `app/Admin/src/features/catalog/views/ProductsList.vue` (exemplar template)
