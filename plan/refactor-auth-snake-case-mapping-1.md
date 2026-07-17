---
goal: Fix Auth API Token Refactoring — Consistent snake_case Backend → camelCase Client Mapping
version: 1.0
date_created: 2026-07-17
last_updated: 2026-07-17
owner: Admin
status: Completed
tags: refactor, auth, token, admin
---

# Introduction

![Status: Completed](https://img.shields.io/badge/status-Completed-brightgreen)

The backend auth API returns token fields in snake_case (`access_token`, `refresh_token`, `access_token_expires_in`, `refresh_token_expires_in`). The client-side code uses camelCase (`accessToken`, `refreshToken`, `accessTokenExpiresIn`, `refreshTokenExpiresIn`). An `auth.mapper.ts` converts snake_case DTOs to camelCase via `mapAuthResponse()`, but `refresh-handler.ts` bypasses the mapper and accesses camelCase property names directly on the raw server response — causing token refresh to silently fail. This plan fixes the inconsistency and eliminates the duplicate `AuthDto` interface.

## 1. Requirements & Constraints

- **REQ-001**: Backend returns `access_token`, `refresh_token`, `access_token_expires_in`, `refresh_token_expires_in` (snake_case)
- **REQ-002**: Client code uses `accessToken`, `refreshToken`, `accessTokenExpiresIn`, `refreshTokenExpiresIn` (camelCase) via `AuthenticationResponse` type
- **REQ-003**: `mapAuthResponse()` in `auth.mapper.ts` is the single source of truth for snake_case → camelCase conversion
- **REQ-004**: `refresh-handler.ts` must use the mapper or access snake_case fields directly
- **REQ-005**: Duplicate `AuthDto` interface must be eliminated — repository imports from mapper
- **CON-001**: TreatWarningsAsErrors: true — lint must pass
- **CON-002**: All imports use `@/` alias where applicable
- **PAT-001**: Repository → Service → Store pattern; mappers convert DTO ↔ domain types

## 2. Implementation Steps

### Implementation Phase 1 — Fix refresh-handler.ts snake_case access

- GOAL-001: Fix token refresh to correctly read snake_case fields from the backend response

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-001 | Change `body.value.accessToken` → `body.value.access_token` in `src/shared/api/http/refresh-handler.ts:18` | ✅ | 2026-07-17 |
| TASK-002 | Change `body.value.refreshToken` → `body.value.refresh_token` in `src/shared/api/http/refresh-handler.ts:18` | ✅ | 2026-07-17 |
| TASK-003 | Run `pnpm run lint` to verify zero new errors | ✅ | 2026-07-17 |
| TASK-004 | Commit: `fix(admin): use snake_case field access in refresh handler` | ✅ | 2026-07-17 |

### Implementation Phase 2 — Eliminate duplicate AuthDto

- GOAL-002: Remove duplicate `AuthDto` interface from repository, import from mapper

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-005 | Add `import type { AuthDto } from '../mappers/auth.mapper'` to `src/features/auth/repositories/auth.repository.ts` | ✅ | 2026-07-17 |
| TASK-006 | Remove local `interface AuthDto` definition (lines 6-11) from repository | ✅ | 2026-07-17 |
| TASK-007 | Verify `pnpm run test:unit -- src/features/auth/` passes | ✅ | 2026-07-17 |
| TASK-008 | Commit: `refactor(admin): import AuthDto from mapper, remove duplicate` | ✅ | 2026-07-17 |

## 3. Alternatives

- **ALT-001**: Have the refresh handler also use `mapAuthResponse()` — would require importing the mapper into a shared utility, adding a dependency on the auth feature from the shared layer. Rejected because the refresh handler intentionally stays in the shared layer to avoid coupling auth with the HTTP client.
- **ALT-002**: Change the backend to return camelCase — would require backend changes and break existing consumers. Rejected because snake_case is the C# convention.

## 4. Dependencies

- **DEP-001**: `auth.mapper.ts` `AuthDto` interface (exported for Phase 2)
- **DEP-002**: `refresh-handler.ts` — called by `api.client.ts` response interceptor on 401

## 5. Files

- **FILE-001**: `app/Admin/src/shared/api/http/refresh-handler.ts` — fix line 18 property access
- **FILE-002**: `app/Admin/src/features/auth/repositories/auth.repository.ts` — import AuthDto from mapper
- **FILE-003**: `app/Admin/src/features/auth/mappers/auth.mapper.ts` — export AuthDto (already exported)

## 6. Testing

- **TEST-001**: Existing auth service tests in `src/features/auth/_tests/auth.service.spec.ts` — verify they continue to pass
- **TEST-002**: Manual smoke test: login → wait for token expiry → verify auto-refresh works without redirecting to /login

## 7. Risks & Assumptions

- **RISK-001**: If the backend response shape changes from `{ value: { access_token: ... } }` to a different structure, the refresh handler will still fail. Currently assumes `ServerResult<T>` wrapper with `.value` property.
- **ASSUMPTION-001**: The backend consistently returns `{ isSuccess: true, value: { access_token, refresh_token, ... } }` for refresh endpoint responses.
- **ASSUMPTION-002**: No other code paths read snake_case fields from ServerResult.value directly — only refresh-handler.ts had this bug.

## 8. Related Specifications / Further Reading

- `docs/superpowers/specs/2026-07-17-admin-layout-migration-design.md`
- `app/Admin/src/features/auth/_tests/auth.service.spec.ts` — existing auth tests
