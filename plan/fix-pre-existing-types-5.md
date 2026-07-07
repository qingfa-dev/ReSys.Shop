---
goal: Eliminate the 17 pre-existing Cannot-find-module type errors by fixing pnpm dependency resolution and restoring the missing metadata-manager component
version: 1.0
date_created: 2026-07-07
status: 'Planned'
tags: fix, typecheck, primevue, build
---

# Introduction

![Status: Planned](https://img.shields.io/badge/status-Planned-blue)

The Admin app has 17 remaining `vue-tsc` errors from two root causes:

1. **`@primevue/core/api` module not found** (12 files): `@primevue/core` is a transitive dependency of `primevue` v4.5.5. pnpm does not hoist transitive dependencies, so `import { FilterMatchMode } from '@primevue/core/api'` fails module resolution. Fix: add `@primevue/core` as a direct dependency.

2. **`metadata-manager.component.vue` module not found** (5 files): The component file exists at the old `app/ReSys.Admin/` path (gitignored, never tracked on current branch) but is missing from the active `app/Admin/` directory. Fix: copy the existing file to the correct path.

## 1. Requirements & Constraints

- **REQ-001**: Add `@primevue/core` as a direct dependency in `app/Admin/package.json` to hoist it into the top-level `node_modules/@primevue/core/` symlink
- **REQ-002**: Restore `metadata-manager.component.vue` at `app/Admin/src/shared/components/metadata-manager.component.vue` — the file exists at `app/ReSys.Admin/src/shared/components/metadata-manager.component.vue` and must be copied
- **CON-001**: Do NOT modify any of the 17 importing `.vue` files — the import paths are correct; only the dependency/file availability is broken
- **CON-002**: After fix, run `pnpm type-check` and confirm the only remaining errors are the `@primevue/core/api` and `metadata-manager` ones (they should be zero); if new errors appear, stop and report

## 2. Implementation Steps

### Implementation Phase 1: Fix `@primevue/core/api` module resolution

- GOAL-001: Make `@primevue/core` a direct dependency so pnpm creates the required symlink at `node_modules/@primevue/core/`.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-001 | Run `pnpm add @primevue/core` in `app/Admin/` (this adds it to `package.json` dependencies and creates the `node_modules/@primevue/core/` symlink) | | |

### Implementation Phase 2: Restore `metadata-manager.component.vue`

- GOAL-002: Copy the existing component file from its old location to the path expected by the 5 importing views.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-002 | Copy `/home/qingfa/Repos/ReSys.Shop/app/ReSys.Admin/src/shared/components/metadata-manager.component.vue` to `/home/qingfa/Repos/ReSys.Shop/app/Admin/src/shared/components/metadata-manager.component.vue` | | |

### Implementation Phase 3: Verify

- GOAL-003: Run full verification — type-check must report zero errors.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-003 | Run `pnpm type-check` — must pass with zero errors | | |
| TASK-004 | Run `pnpm build-only` — must pass | | |
| TASK-005 | Run `pnpm test:unit` — all 107 tests must pass | | |

## 3. Alternatives

- **ALT-001**: Create a `.d.ts` declaration shim to suppress the `@primevue/core/api` type error — rejected because at runtime the import would still fail; `FilterMatchMode` etc. would be `undefined` and DataTable filtering would break
- **ALT-002**: Rewrite imports to use a different path from `primevue` — rejected because `primevue` 4.x does not re-export `FilterMatchMode`/`FilterOperator` from its main entry point; the subpath `@primevue/core/api` is the canonical source
- **ALT-003**: Recreate `metadata-manager` from scratch — rejected because the existing file at `app/ReSys.Admin/` compiles and works; copying is zero-risk

## 4. Dependencies

- **DEP-001**: pnpm must be the package manager (it is — confirmed via `package.json` `"packageManager": "pnpm@10.8.0"`)
- **DEP-002**: The file `app/ReSys.Admin/src/shared/components/metadata-manager.component.vue` must exist on disk (it does — confirmed by `ls`)

## 5. Files

- **FILE-001**: `app/Admin/package.json` — modified by `pnpm add` (adds `@primevue/core` to dependencies)
- **FILE-002**: `app/Admin/pnpm-lock.yaml` — updated by `pnpm add`
- **FILE-003**: `app/Admin/node_modules/@primevue/core/` — new symlink created by pnpm
- **FILE-004**: `app/Admin/src/shared/components/metadata-manager.component.vue` — new file (copy from `app/ReSys.Admin/...`)

## 6. Testing

- **TEST-001**: `pnpm type-check` — zero errors
- **TEST-002**: `pnpm build-only` — passes
- **TEST-003**: `pnpm test:unit` — all 107 tests pass

## 7. Risks & Assumptions

- **RISK-001**: None — `@primevue/core` at the same version (4.5.5) is already installed as a transitive dependency; adding it as a direct dependency only creates the symlink without changing the installed version
- **ASSUMPTION-001**: The `@primevue/core` version resolved by `pnpm add` will match `primevue@4.5.5`'s semver range (it will — the lockfile pins it)

## 8. Related Specifications / Further Reading

- `plan/fix-i18n-admin-types-4.md` — prior phase that fixed the GlobalSearch `$t` errors
