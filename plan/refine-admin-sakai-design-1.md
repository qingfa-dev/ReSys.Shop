---
goal: Refine entire Admin SPA layout and UI to match Sakai Vue premium template quality
version: 1.0
date_created: 2026-07-06
status: 'In progress'
tags: design, migration, refinement, sakai
---

# Introduction

Refine all layout components, pages, and shared UI to match the Sakai Vue (PrimeVue) premium admin template design quality. The current state is functional but visually bare.

Reference source: `app/ReSys.Admin/src/layout/`, `app/ReSys.Admin/src/assets/scss/layout/`

![Status: In progress](https://img.shields.io/badge/status-In%20progress-yellow)

## 1. Requirements & Constraints

- **REQ-001**: All layout components must support static/overlay menu modes
- **REQ-002**: Dark mode toggle must use View Transition API
- **REQ-003**: Theme configurator must allow preset/primary/surface/menu-mode switching
- **REQ-004**: SCSS layout files must be used (not inline Tailwind layout classes)
- **REQ-005**: Menu tree must match ReSys.Admin's full nav structure with grouped items
- **REQ-006**: Mobile responsive with overlay menu + mask
- **REQ-007**: Login page must remain as redesigned (editorial identity)
- **CON-001**: Must preserve all existing route configs and feature pages
- **CON-002**: Must pass `pnpm build-only`

## 2. Implementation Steps

### Phase 1: Restore Sakai Layout System

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-001 | Rewrite layout.composable with LayoutConfig/LayoutState, menu modes, View Transition dark mode | ✅ | 2026-07-06 |
| TASK-002 | Rewrite main.layout.vue with containerClass (static/overlay/mobile), mask, outside-click | ✅ | 2026-07-06 |
| TASK-003 | Rewrite topbar.layout.vue with logo SVG, menu toggle, search, dark mode, configurator, user menu | ✅ | 2026-07-06 |
| TASK-004 | Rewrite sidebar.layout.vue with logo block, scroll, menu render | ✅ | 2026-07-06 |
| TASK-005 | Rewrite menu.layout.vue with full grouped model (Dashboard/Catalog/Inventory/Sales/Identity) | ✅ | 2026-07-06 |
| TASK-006 | Rewrite menu-item.layout.vue with root/child, active path, badge support, transitions | ✅ | 2026-07-06 |
| TASK-007 | Rewrite configurator.layout.vue with preset/primary/surface/menu-mode selectors, dark mode | ✅ | 2026-07-06 |
| TASK-008 | Restore SCSS layout files to match Sakai styling | ✅ | 2026-07-06 |

### Phase 2: Wire + Polish Pages

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-009 | Wire breadcrumb into layout | ✅ | 2026-07-06 |
| TASK-010 | Polish dashboard page with PrimeVue Card grid | | |
| TASK-011 | Polish catalog pages | | |
| TASK-012 | Polish users/roles/permissions pages | | |
| TASK-013 | Polish inventory/ordering/reports pages | | |

### Phase 3: Verify

| Task | Description |
|------|-------------|
| TASK-014 | `pnpm build-only`, fix issues, verify all routes render |

## 3. Alternatives

- **ALT-001**: Keep current Tailwind layout — rejected. Can't achieve proper menu modes, transitions, or responsive behavior.

## 4. Dependencies

- **DEP-001**: PrimeVue 4 components auto-imported
- **DEP-002**: SCSS layout files already in `app/Admin/src/assets/scss/layout/`
- **DEP-003**: Logo SVG from ReSys.Admin

## 5. Files

- `app/Admin/src/app/layout/composables/layout.composable.ts`
- `app/Admin/src/app/layout/main.layout.vue`
- `app/Admin/src/app/layout/topbar.layout.vue`
- `app/Admin/src/app/layout/sidebar.layout.vue`
- `app/Admin/src/app/layout/menu.layout.vue`
- `app/Admin/src/app/layout/menu-item.layout.vue`
- `app/Admin/src/app/layout/footer.layout.vue`
- `app/Admin/src/app/layout/configurator.layout.vue`
- `app/Admin/src/assets/scss/layout/*.scss` (8 files)

## 6. Testing

- **TEST-001**: `pnpm build-only` passes
- **TEST-002**: Layout renders in static/overlay modes
- **TEST-003**: Dark mode toggle works
- **TEST-004**: Configurator opens

## 7. Risks

- **RISK-001**: Menu route names may not match current router configs — adapt during implementation

## 8. Related Specs

- `docs/superpowers/plans/2026-07-06-admin-saikai-migration.md`
- `docs/superpowers/specs/2026-07-06-admin-saikai-migration-design.md`
- ReSys.Admin reference: `app/ReSys.Admin/src/layout/`
