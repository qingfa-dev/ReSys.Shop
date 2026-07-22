# Admin SPA — Migrate Auth from Shared to Feature Module

**Date:** 2026-07-22
**Status:** Draft
**Branch:** `feature/implement-admin-panel`

## Overview

Move all auth-domain code from `shared/auth/` into `features/auth/` with correct subdirectory placement, delete dead code (`AuthService`, `session.ts` re-export), and update the two consumer imports. Auth-adjacent HTTP infrastructure (`shared/api/interceptors/auth.interceptor.ts`, `shared/api/handlers/refresh-handler.ts`) stays in the shared API layer — those are Axios plumbing, not auth domain logic. Localization (`shared/localization/`) similarly stays put — it's cross-cutting.

## Scope

### Moves to `features/auth/`

| Source | Target | Notes |
|---|---|---|
| `shared/auth/token.service.ts` | `features/auth/services/token.service.ts` | Static class for JWT storage/decoding |
| `shared/auth/__tests__/token.service.spec.ts` | `features/auth/services/__tests__/token.service.spec.ts` | — |
| `shared/auth/permissions.ts` | `features/auth/utils/permissions.ts` | 3 pure helper functions |
| `shared/auth/roles.ts` | `features/auth/utils/roles.ts` | Constants + `hasRole()` |
| `shared/auth/__tests__/permissions.spec.ts` | `features/auth/utils/__tests__/permissions.spec.ts` | — |

### Deletions

| File | Reason |
|---|---|
| `shared/auth/auth.service.ts` | Dead code — zero imports since direct `apiClient` calls replaced it in `auth.api.ts` |
| `shared/auth/session.ts` | No-op re-export of `@/stores/useSessionStore`; the store imports the source directly |
| `shared/auth/index.ts` | Directory barrel — entire `shared/auth/` directory is deleted |
| `shared/auth/__tests__/` (directory) | Tests relocated alongside their subjects |

### Stays in place

| Path | Reason |
|---|---|
| `shared/api/interceptors/auth.interceptor.ts` | Axios request interceptor — HTTP plumbing |
| `shared/api/handlers/refresh-handler.ts` | Axios response interceptor helper — HTTP plumbing |
| `shared/localization/` | Cross-cutting i18n for all features |
| `router/guards.ts` | Navigation guard — app infrastructure that imports `TokenService` from the new location |

### Consumer import changes

| File | Old import | New import |
|---|---|---|
| `router/guards.ts` | `@/shared/auth/token.service` | `@/features/auth/services/token.service` |
| `features/auth/store/auth.store.ts` | `@/shared/auth/token.service` | `../services/token.service` |

### Barrel additions (`features/auth/index.ts`)

Add exports for relocated modules that other features may consume (primarily permission/role guards):

```typescript
export { TokenService } from './services/token.service'
export { hasPermission, hasAnyPermission, hasAllPermissions } from './utils/permissions'
export { ROLES, ROLE_HIERARCHY, hasRole } from './utils/roles'
export type { Role } from './utils/roles'
```

Existing exports (routes, store, composable, types) remain unchanged.

## Resulting structure

```
features/auth/
  api/                  ← unchanged
  components/           ← unchanged
  composables/          ← unchanged
  models/               ← unchanged
  pages/                ← unchanged
  store/                ← unchanged
  types/                ← unchanged
  services/             ← NEW
    token.service.ts
    __tests__/
      token.service.spec.ts
  utils/                ← NEW
    permissions.ts
    roles.ts
    __tests__/
      permissions.spec.ts
  index.ts              ← extended barrel
  routes.ts             ← unchanged
```

## Verification

```bash
pnpm run lint          # clean
pnpm run build         # clean (no TS errors from changed imports)
pnpm run test:unit     # all 85+ tests pass (no relocation regressions)
```

Also confirm no remaining imports from `shared/auth` across the codebase:

```bash
grep -r "shared/auth" src/  # should return nothing
```
