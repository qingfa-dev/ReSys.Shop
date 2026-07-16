---
goal: Admin Dashboard Frontend Wiring Fix
version: 1.0
date_created: 2026-07-16
owner: Platform Team
status: Planned
tags: feature, admin, frontend, bugfix
---

# Introduction

![Status: Planned](https://img.shields.io/badge/status-Planned-blue)

The backend `GET /api/dashboard` endpoint is built (7 commits, `7ac1135e..bfef47ac`). The frontend service and store were wired in tasks 6-7 but contain 3 bugs that prevent data from rendering: double API prefix in the URL, wrong response unwrapping in the store, and incorrect return type. This plan fixes those bugs and cleans up dead code.

## 1. Requirements & Constraints

- **REQ-001**: Dashboard page at `/reports/dashboard` renders live data from `GET /api/dashboard`.
- **REQ-002**: Service layer calls correct backend URL — `apiClient` has `baseURL: '/api'` and Vite proxy preserves the `/api` prefix (no `rewrite`). All service paths must account for this.
- **REQ-003**: Response interceptor in `api.client.ts:48-53` already unwraps `Result<T>.value` — store must not double-unwrap.
- **REQ-004**: Admin SPA lint passes (`pnpm run lint`), no new errors.
- **CON-001**: Do not touch the response interceptor (`api.client.ts`) — it handles all API calls uniformly. Fix consumers instead.
- **CON-002**: The `DashboardResponse` interface in `report.service.ts` may duplicate `report.types.ts`. Do not consolidate types in this fix — scope is wiring only.

## 2. Implementation Steps

### Implementation Phase 1: Fix Dashboard Wiring

- GOAL-001: Fix 3 bugs so the dashboard view renders live data from the backend.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-001 | Fix double API prefix in `report.service.ts:38` — change `'/api/dashboard'` to `'/dashboard'` (baseURL `/api` already provides the prefix; Vite proxy preserves it without rewrite) | | |
| TASK-002 | Fix return type in `report.service.ts:37` — change `Promise<AxiosResponse<{ value: DashboardResponse }>>` to `Promise<AxiosResponse<DashboardResponse>>` (interceptor at `api.client.ts:48-53` already unwraps `value`) | | |
| TASK-003 | Fix response unwrapping in `report.store.ts:17-21` — replace `const value = data.value` with direct access (`data.sales`, `data.inventory`, `data.catalog`, `data.recentActivities`) since the interceptor already extracted `value` into `data` | | |
| TASK-004 | Remove dead file `reports.api.ts` (empty export, comment says "re-add when backend exists") — it's been replaced by `report.service.ts` | | |
| TASK-005 | Verify: lint Admin SPA (`cd app/Admin && pnpm run lint`), build backend (`dotnet build service/Api/src/Module/Module.csproj`), run dashboard tests (`dotnet test --filter "Dashboard"`) | | |

## 3. Alternatives

- **ALT-001**: Fix the Vite proxy by adding `rewrite: (path) => path.replace(/^\/api/, '')` so `baseURL + path` double-prefix is stripped. Rejected: this would break all existing service calls that already account for the current proxy behavior (or make the proxy a silent fix for a systemic issue — better to be explicit in each service).
- **ALT-002**: Consolidate `DashboardResponse` interface from `report.service.ts` into `report.types.ts` and re-export. Rejected: CON-002 — scope is wiring only, type consolidation is a separate refactor.

## 4. Dependencies

- **DEP-001**: Backend `GET /api/dashboard` endpoint (commits `7ac1135e..bfef47ac` on current branch) — already built.
- **DEP-002**: Axios response interceptor at `api.client.ts:48-53` — already unwraps `Result<T>.value`. No changes needed.

## 5. Files

- **FILE-001**: `app/Admin/src/features/reports/services/report.service.ts` — Modify L37-38 (URL and return type)
- **FILE-002**: `app/Admin/src/features/reports/stores/report.store.ts` — Modify L17-21 (response unwrapping)
- **FILE-003**: `app/Admin/src/features/reports/services/reports.api.ts` — Delete (dead empty file, replaced by report.service.ts)
- **FILE-004**: `app/Admin/src/shared/api/constants.ts` — Verify no `DASHBOARD` constant needed (service uses raw string path). No change.

## 6. Testing

- **TEST-001**: After fixes, run `cd app/Admin && pnpm run lint` — must pass with 0 new errors.
- **TEST-002**: Run `dotnet test --filter "Dashboard"` — 6/6 must still pass (no backend changes in this plan).
- **TEST-003**: Manual verification: start the API + Admin SPA, navigate to `/reports/dashboard`, confirm stat cards show non-zero values (requires seeded test data).
- **TEST-004**: Open browser DevTools Network tab, confirm request to `/api/dashboard` returns 200 with populated JSON body.

## 7. Risks & Assumptions

- **RISK-001**: The Vite proxy behavior (preserves `/api` prefix without rewrite) is confirmed by the docs. If the proxy is later configured with `rewrite`, the `'/dashboard'` path would become `localhost:5035/dashboard` (missing `/api` prefix) and break. Mitigation: document this dependency.
- **ASSUMPTION-001**: The response interceptor always fires and always unwraps `value`. If the backend returns a different envelope shape (e.g., error responses without `value`), the interceptor falls through to the default case (L56-59) which returns the raw body as `data`. The store handles this gracefully via `try/finally`.

## 8. Related Specifications / Further Reading

- [spec-design-dashboard-api.md](/spec/spec-design-dashboard-api.md) — Backend endpoint design
- [2026-07-16-dashboard-api.md](/docs/superpowers/plans/2026-07-16-dashboard-api.md) — Implementation plan (8 tasks, all complete)
- [spec-design-admin-api-services.md](/spec/spec-design-admin-api-services.md) — Admin SPA API service layer mappings
