---
goal: Add missing Sakai Vue components and error pages to Admin SPA
version: 1.0
date_created: 2026-07-06
status: 'Completed'
tags: feature, enhancement, sakai, components, pages
---

# Introduction

Add missing layout components and utility pages from the upstream Sakai Vue template (https://github.com/primefaces/sakai-vue) that are not yet present in the current Admin SPA.

Reference: `app/ReSys.Admin/` for adapted examples.

![Status: Completed](https://img.shields.io/badge/status-Completed-brightgreen)

## 1. Requirements & Constraints

- **REQ-001**: FloatingConfigurator must provide dark mode toggle and theme palette access from any page
- **REQ-002**: NotFound (404) page must match Sakai design with illustration, message, and navigation option
- **REQ-003**: Error (500) page must match Sakai design with illustration and retry/home options
- **REQ-004**: Access (403) page must match Sakai design with lock icon and back-to-home action
- **REQ-005**: Empty page template must follow Sakai's minimal centered layout
- **REQ-006**: Profile settings page must show user info, roles, and account preferences
- **REQ-007**: All pages must use PrimeVue components (Card, Button, Tag, etc.)
- **REQ-008**: All new pages must use existing shared composables (toast, formatter)
- **CON-001**: Must preserve existing route structure
- **CON-002**: Must pass `pnpm build-only`
- **CON-003**: Login page must remain with editorial identity design
- **PAT-001**: Error pages follow Sakai pattern: centered illustration + heading + description + action button
- **PAT-002**: Components use `composables/` folder pattern, views use `views/` folder pattern
- **PAT-003**: Page components use `<script setup lang="ts">` with PrimeVue auto-imports

## 2. Implementation Steps

### Phase 1: FloatingConfigurator + Utility

- GOAL-001: Add the FloatingConfigurator component and wire it into the main layout

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-001 | Create FloatingConfigurator.vue with dark mode + palette | ✅ | 2026-07-06 |
| TASK-002 | Wire FloatingConfigurator into main.layout.vue | ✅ | 2026-07-06 |

### Phase 2: Error & Status Pages

- GOAL-002: Add NotFound (404), Error (500), AccessDenied (403), and Empty page templates

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-003 | Create NotFound.view.vue — centered 404 with SVG + navigation | ✅ | 2026-07-06 |
| TASK-004 | Create ErrorPage.view.vue — centered 500 with retry/home | ✅ | 2026-07-06 |
| TASK-005 | Create AccessDenied.view.vue — centered 403 with back-to-home | ✅ | 2026-07-06 |
| TASK-006 | Create error.routes.ts with public error routes | ✅ | 2026-07-06 |
| TASK-007 | Wire error routes into router before auth guard | ✅ | 2026-07-06 |
| TASK-008 | Create EmptyPage.view.vue — centered placeholder | ✅ | 2026-07-06 |

### Phase 3: Profile Settings

- GOAL-003: Improve the Profile page to match Sakai's user settings pattern

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-009 | Rewrite Profile.view.vue with Card sections, avatar, roles Tag, password form, notification toggles | ✅ | 2026-07-06 |

### Phase 4: Verify

- GOAL-004: Build verification

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-010 | pnpm build-only, verify no regressions | ✅ | 2026-07-06 |

## 3. Alternatives

- **ALT-001**: Skip error pages and rely on generic browser error handling — rejected. Custom error pages are production-critical for user experience.
- **ALT-002**: Keep the Profile page basic — rejected. The current profile view is minimal and doesn't show user roles or settings.

## 4. Dependencies

- **DEP-001**: PrimeVue 4 components auto-imported via unplugin-vue-components + PrimeVueResolver
- **DEP-002**: PrimeIcons for all iconography
- **DEP-003**: `v-styleclass` directive registered in `main.ts`
- **DEP-004**: AppConfigurator at `@/app/layout/configurator.layout.vue`
- **DEP-005**: Auth store at `@/features/auth/stores/auth.store`

## 5. Files

- **FILE-001**: `app/Admin/src/app/layout/components/FloatingConfigurator.vue` — CREATE
- **FILE-002**: `app/Admin/src/app/layout/main.layout.vue` — MODIFY (add FloatingConfigurator)
- **FILE-003**: `app/Admin/src/features/error/pages/NotFound.view.vue` — CREATE
- **FILE-004**: `app/Admin/src/features/error/pages/ErrorPage.view.vue` — CREATE
- **FILE-005**: `app/Admin/src/features/error/pages/AccessDenied.view.vue` — CREATE
- **FILE-006**: `app/Admin/src/features/error/error.routes.ts` — CREATE
- **FILE-007**: `app/Admin/src/features/error/pages/EmptyPage.view.vue` — CREATE
- **FILE-008**: `app/Admin/src/app/router/index.ts` — MODIFY (add error routes)
- **FILE-009**: `app/Admin/src/features/auth/views/Profile.view.vue` — MODIFY

## 6. Testing

- **TEST-001**: `pnpm build-only` passes
- **TEST-002**: Navigate to `/error/404` — renders centered 404 page with illustration
- **TEST-003**: Navigate to `/error/500` — renders centered error page with retry button
- **TEST-004**: Navigate to `/error/403` — renders centered access denied page
- **TEST-005**: Floating configurator button appears in top-right corner on layout pages
- **TEST-006**: Dark mode toggle works from FloatingConfigurator
- **TEST-007**: Palette button opens AppConfigurator

## 7. Risks & Assumptions

- **RISK-001**: Error route order matters — must be before the auth-guarded layout route so unauthenticated users can see 404 pages
- **ASSUMPTION-001**: SVG illustrations for error pages can use simple inline SVGs (no external assets needed)
- **ASSUMPTION-002**: Profile page has access to `useAuthStore` for user data

## 8. Related Specifications / Further Reading

- Sakai Vue source: https://github.com/primefaces/sakai-vue
- `docs/superpowers/plans/2026-07-06-admin-saikai-migration.md`
- `docs/superpowers/specs/2026-07-06-admin-saikai-migration-design.md`
- `plan/refine-admin-sakai-design-1.md`
