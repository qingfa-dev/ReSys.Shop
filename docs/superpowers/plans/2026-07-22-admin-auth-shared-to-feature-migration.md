# Admin SPA — Migrate Auth from Shared to Feature Module Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use subagent-driven-development (recommended) or executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Move all auth-domain code from `shared/auth/` into `features/auth/` with correct subdirectory placement, delete dead code, and update the two consumer imports.

**Architecture:** `TokenService` → `features/auth/services/`; permissions + roles utils → `features/auth/utils/`; tests relocated alongside subjects. `AuthService` deleted (dead code). `session.ts` deleted (no-op re-export). `shared/auth/` directory removed entirely. Auth-adjacent HTTP interceptors stay in `shared/api/`.

**Tech Stack:** TypeScript, Vitest

## Global Constraints

- All imports must use project path aliases (`@/`) or relative paths consistently
- Zero test breakage — all 85 existing tests must pass after migration
- No remaining imports from `shared/auth` anywhere in `src/`
- Build and lint must remain clean
- No changes to `shared/api/interceptors/` or `shared/localization/`

---

### Task 1: Move `token.service` to `features/auth/services/`

**Files:**
- Create: `app/Admin/src/features/auth/services/token.service.ts`
- Create: `app/Admin/src/features/auth/services/__tests__/token.service.spec.ts`

**Interfaces:**
- Consumes: nothing from this plan
- Produces: `TokenService` class at `features/auth/services/token.service.ts` — exact same API as before

- [ ] **Step 1: Create `features/auth/services/` directory and move `token.service.ts`**

```bash
mkdir -p app/Admin/src/features/auth/services/__tests__
git mv app/Admin/src/shared/auth/token.service.ts app/Admin/src/features/auth/services/token.service.ts
git mv app/Admin/src/shared/auth/__tests__/token.service.spec.ts app/Admin/src/features/auth/services/__tests__/token.service.spec.ts
```

No code changes — file content is identical.

- [ ] **Step 2: Verify tests still pass from new location**

Run: `pnpm --filter admin test:unit -- src/features/auth/services/__tests__/token.service.spec.ts`
Expected: PASS (2 passed, 1 pending — `expired token` test)

- [ ] **Step 3: Commit**

```bash
git add app/Admin/src/features/auth/services/
git commit -m "refactor: move token.service and tests to features/auth/services/"
```

---

### Task 2: Move `permissions` and `roles` to `features/auth/utils/`

**Files:**
- Create: `app/Admin/src/features/auth/utils/permissions.ts`
- Create: `app/Admin/src/features/auth/utils/roles.ts`
- Create: `app/Admin/src/features/auth/utils/__tests__/permissions.spec.ts`

**Interfaces:**
- Consumes: nothing from this plan
- Produces:
  - `hasPermission(required: string, userPermissions: string[]): boolean`
  - `hasAnyPermission(required: string[], userPermissions: string[]): boolean`
  - `hasAllPermissions(required: string[], userPermissions: string[]): boolean`
  - `ROLES`, `ROLE_HIERARCHY`, `hasRole(userRole: string, requiredRole: Role): boolean`, `Role` type

- [ ] **Step 1: Create `features/auth/utils/` directory and move files**

```bash
mkdir -p app/Admin/src/features/auth/utils/__tests__
git mv app/Admin/src/shared/auth/permissions.ts app/Admin/src/features/auth/utils/permissions.ts
git mv app/Admin/src/shared/auth/roles.ts app/Admin/src/features/auth/utils/roles.ts
git mv app/Admin/src/shared/auth/__tests__/permissions.spec.ts app/Admin/src/features/auth/utils/__tests__/permissions.spec.ts
```

- [ ] **Step 2: Update test imports to reflect new paths**

In `features/auth/utils/__tests__/permissions.spec.ts`, change:

```typescript
import { hasPermission, hasAnyPermission, hasAllPermissions } from '../permissions'
```

This import is already relative and correct after the move. No change needed.

- [ ] **Step 3: Verify tests pass**

Run: `pnpm --filter admin test:unit -- src/features/auth/utils/__tests__/permissions.spec.ts`
Expected: PASS (5 passed)

- [ ] **Step 4: Commit**

```bash
git add app/Admin/src/features/auth/utils/
git commit -m "refactor: move permissions and roles to features/auth/utils/"
```

---

### Task 3: Update consumer imports and barrel

**Files:**
- Modify: `app/Admin/src/router/guards.ts`
- Modify: `app/Admin/src/features/auth/store/auth.store.ts`
- Modify: `app/Admin/src/features/auth/index.ts`

- [ ] **Step 1: Update `router/guards.ts` import**

In `app/Admin/src/router/guards.ts:2`, change:

```typescript
import { TokenService } from '@/features/auth/services/token.service'
```

- [ ] **Step 2: Update `auth.store.ts` import**

In `app/Admin/src/features/auth/store/auth.store.ts:5`, change:

```typescript
import { TokenService } from '../services/token.service'
```

- [ ] **Step 3: Extend `features/auth/index.ts` barrel**

Write to `app/Admin/src/features/auth/index.ts`:

```typescript
export { authRoutes, changePasswordRoute } from './routes'
export { useAuthStore } from './store/auth.store'
export { useAuth } from './composables/useAuth'
export type * from './types'
export { TokenService } from './services/token.service'
export { hasPermission, hasAnyPermission, hasAllPermissions } from './utils/permissions'
export { ROLES, ROLE_HIERARCHY, hasRole } from './utils/roles'
export type { Role } from './utils/roles'
```

- [ ] **Step 4: Build check**

Run: `pnpm --filter admin run build`
Expected: Clean (no TS errors)

- [ ] **Step 5: Commit**

```bash
git add app/Admin/src/router/guards.ts app/Admin/src/features/auth/store/auth.store.ts app/Admin/src/features/auth/index.ts
git commit -m "refactor: update imports and barrel for relocated auth modules"
```

---

### Task 4: Delete dead code and old `shared/auth/` directory

**Files:**
- Delete: `app/Admin/src/shared/auth/auth.service.ts`
- Delete: `app/Admin/src/shared/auth/session.ts`
- Delete: `app/Admin/src/shared/auth/index.ts`
- Delete: `app/Admin/src/shared/auth/__tests__/` (directory, now empty)
- Delete: `app/Admin/src/shared/auth/` (directory, now empty — may need `git rm -r` if git tracks the directory)

- [ ] **Step 1: Delete `AuthService` (dead code)**

```bash
git rm app/Admin/src/shared/auth/auth.service.ts
```

- [ ] **Step 2: Delete `session.ts` (no-op re-export)**

```bash
git rm app/Admin/src/shared/auth/session.ts
```

- [ ] **Step 3: Delete remaining old barrel + test directory**

```bash
git rm app/Admin/src/shared/auth/index.ts
# __tests__ dir should already be empty after Tasks 1-2 moves
git rm app/Admin/src/shared/auth/__tests__/permissions.spec.ts 2>/dev/null || true
git rm app/Admin/src/shared/auth/__tests__/token.service.spec.ts 2>/dev/null || true
```

If any files remain in `shared/auth/` (e.g., stubs the moves didn't cover), remove the entire directory:

```bash
git rm -r app/Admin/src/shared/auth/
```

- [ ] **Step 4: Build + test check**

```bash
pnpm --filter admin run build && pnpm --filter admin run test:unit
```

Expected: Build clean, all 85 tests pass.

- [ ] **Step 5: Commit**

```bash
git commit -m "refactor: remove shared/auth/ (dead code and relocated files)"
```

Note: use `--allow-empty` if all changes were already staged in prior steps.

---

### Task 5: Final verification

**Files:** none — verification only

- [ ] **Step 1: Full build, lint, test**

```bash
pnpm --filter admin run lint
pnpm --filter admin run build
pnpm --filter admin run test:unit
```

Expected: All clean (lint, build, 85 tests).

- [ ] **Step 2: Grep for stale imports**

```bash
grep -r "shared/auth" app/Admin/src/ --include='*.ts' --include='*.vue'
```

Expected: No output (zero remaining imports from shared/auth).

- [ ] **Step 3: Verify no accidental leftover files**

```bash
ls app/Admin/src/shared/auth/ 2>&1
```

Expected: `ls: cannot access '.../shared/auth/': No such file or directory`

- [ ] **Step 4: Final commit (if any verification fix was needed)**

```bash
git add -A && git commit -m "chore: final cleanup after auth shared-to-feature migration"
```
