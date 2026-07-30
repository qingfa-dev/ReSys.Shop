---
goal: Admin Panel CRUD UI Modernization — List and Detail Pages
version: 1.0
date_created: 2026-07-31
status: Completed
tags: [refactor, ui, admin, modernization]
---

# Introduction

![Status: In progress](https://img.shields.io/badge/status-In_progress-yellow)

Refactor all 12 CRUD list/detail pages (catalog + location modules) to follow
modern enterprise admin patterns: flex-based full-height layouts, scrollable
tables/forms only, fixed headers, consistent spacing, no page-level scrolling.

## 1. Requirements & Constraints

- **REQ-001**: Remove outer `<Card>` wrapper from all list and detail pages
- **REQ-002**: Layout fills the entire content area — no page-level scroll ever
- **REQ-003**: List pages: fixed title/description/toolbar/filters, scrollable DataTable only
- **REQ-004**: Detail pages: fixed title/description, scrollable form Card only
- **REQ-005**: Use flex layout: `flex-col`, `flex-1`, `min-h-0`, `overflow-auto`
- **REQ-006**: Consistent toolbar: primary actions left (New, Delete), secondary right (Export)
- **REQ-007**: Save/Cancel buttons inside form Card, sticky at bottom, visible on scroll
- **REQ-008**: DataTable fills available height, sticky header, paginator always visible
- **REQ-009**: Preserve all existing functionality (CRUD, validation, PickList, TreeTable)
- **CON-001**: Do not modify global assets, CSS, layout, router, composables, APIs, stores
- **CON-002**: 570 existing tests must pass
- **CON-003**: Build must pass with 0 errors
- **GUD-001**: `h-full` + `flex flex-col` on root, `flex-1 min-h-0 overflow-auto` on scroll areas

## 2. Implementation Steps

### Phase 1: List View Layout (6 files)

- GOAL-001: Refactor all list views to flex-based full-height layout with scrollable DataTable

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-001 | Refactor `CountriesList.vue`: remove outer Card, use `<div class="flex flex-col h-full">`. Fixed header (title + toolbar), scrollable `<div class="flex-1 min-h-0 overflow-auto"><DataTable/></div>` | | |
| TASK-002 | Refactor `StatesList.vue`: same pattern. Fixed header with country filter dropdown in toolbar area. Scrollable DataTable. | | |
| TASK-003 | Refactor `ProductsList.vue`: same flex layout pattern | | |
| TASK-004 | Refactor `OptionTypesList.vue`: same flex layout pattern | | |
| TASK-005 | Refactor `TaxonomiesList.vue`: same flex layout pattern | | |
| TASK-006 | Refactor `TaxonsList.vue`: same flex layout pattern, dual mode (table/tree). Both views scroll independently. | | |

### Phase 2: Detail View Layout (6 files)

- GOAL-002: Refactor all detail views to flex-based full-height layout with scrollable form

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-007 | Refactor `CountryDetail.vue`: remove outer Card, use `flex flex-col h-full`. Fixed title+description, scrollable `<div class="flex-1 min-h-0 overflow-auto"><Card><Form/></Card></div>`. Action buttons at bottom of form Card. | | |
| TASK-008 | Refactor `StateDetail.vue`: same pattern | | |
| TASK-009 | Refactor `ProductDetail.vue`: same pattern with tabs inside scrollable area. Form wraps tabs. Action buttons outside tabs, at bottom. | | |
| TASK-010 | Refactor `OptionTypeDetail.vue`: same pattern with tabs | | |
| TASK-011 | Refactor `TaxonomyDetail.vue`: same pattern, flat layout (no tabs). TreeTable after form in scrollable area. | | |
| TASK-012 | Refactor `TaxonDetail.vue`: same pattern with tabs | | |

### Phase 3: Verify & Cleanup

- GOAL-003: Verify all views, tests, build

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-013 | Run `pnpm run build-only` (0 errors) | | |
| TASK-014 | Run `pnpm run test:unit -- run` (570 pass) | | |
| TASK-015 | Run `pnpm run type-check` (0 errors) | | |

## 3. Alternatives

- **ALT-001**: Extract a shared `PageLayout` component — adds a wrapper, user prefers direct composition
- **ALT-002**: Use CSS grid instead of flex — flex is simpler and PrimeVue admin templates use flex

## 4. Dependencies

- **DEP-001**: PrimeVue `Card`, `Toolbar`, `DataTable`, `Message`, `Form`, `FormField` — already in use
- **DEP-002**: Tailwind CSS utility classes — already configured

## 5. Files

### List Views (6)
- **FILE-001**: `app/Admin/src/features/location/views/CountriesList.vue`
- **FILE-002**: `app/Admin/src/features/location/views/StatesList.vue`
- **FILE-003**: `app/Admin/src/features/catalog/views/ProductsList.vue`
- **FILE-004**: `app/Admin/src/features/catalog/views/OptionTypesList.vue`
- **FILE-005**: `app/Admin/src/features/catalog/views/TaxonomiesList.vue`
- **FILE-006**: `app/Admin/src/features/catalog/views/TaxonsList.vue`

### Detail Views (6)
- **FILE-007**: `app/Admin/src/features/location/views/CountryDetail.vue`
- **FILE-008**: `app/Admin/src/features/location/views/StateDetail.vue`
- **FILE-009**: `app/Admin/src/features/catalog/views/ProductDetail.vue`
- **FILE-010**: `app/Admin/src/features/catalog/views/OptionTypeDetail.vue`
- **FILE-011**: `app/Admin/src/features/catalog/views/TaxonomyDetail.vue`
- **FILE-012**: `app/Admin/src/features/catalog/views/TaxonDetail.vue`

## 6. Testing

- **TEST-001**: All 570 existing unit tests pass
- **TEST-002**: Build with 0 errors
- **TEST-003**: Type-check with 0 errors
- **TEST-004**: Manual smoke — list page fills viewport, DataTable scrolls, footer visible

## 7. Risks & Assumptions

- **RISK-001**: `h-full` layout depends on parent container providing height — AppLayout must have `h-[calc(100vh-4rem)]` similar setup
- **ASSUMPTION-001**: Tailwind `h-full` class works with `flex flex-col` on parent containers
- **ASSUMPTION-002**: DataTable `scrollable` and `scrollHeight="flex"` props are supported in PrimeVue 5

## 8. Related Specifications

- User-provided spec in this conversation thread
