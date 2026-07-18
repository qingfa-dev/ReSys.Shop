---
goal: Rename all PascalCase non-UI files to kebab-case for consistent naming
version: 1.0
date_created: 2026-07-18
status: 'Completed'
tags: refactor, naming, convention
---

# Introduction

![Status: Completed](https://img.shields.io/badge/status-Completed-brightgreen)

Rename all 140 PascalCase non-UI files under `app/Admin/src/features/` to kebab-case, and update all 321 import statements across 205 consumer files. The target convention matches already-kebab files like `shipping-rate.api.ts`.

## 1. Requirements & Constraints

- **REQ-001**: All `*.Schema.ts` files → `*.schema.ts`
- **REQ-002**: All `*.Request.Type.ts` files → `*.request.type.ts`
- **REQ-003**: All `*.Response.Type.ts` files → `*.response.type.ts`
- **REQ-004**: All `*.Query.Type.ts` files → `*.query.type.ts`
- **REQ-005**: All `*.Parameters.Type.ts` files → `*.parameters.type.ts`
- **REQ-006**: All 321 import references to old names must be updated
- **CON-001**: `app/Store` is a separate app with no `features/` directory — no changes needed
- **CON-002**: Vue `.Component.vue` and `.View.vue` files are UI — excluded from rename
- **CON-003**: Already-kebab files (`*.api.ts`, `*.store.ts`, `*.service.ts`, etc.) — excluded

## 2. Implementation Steps

### Phase 1: Rename 140 files + update 321 imports + verify

- GOAL-001: Rename all PascalCase files to kebab-case and update all consumer imports

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| FILE-001 | Rename 33 `*.Schema.ts` → `*.schema.ts` | ✅ | 2026-07-18 |
| FILE-002 | Rename 30 `*.Request.Type.ts` → `*.request.type.ts` | ✅ | 2026-07-18 |
| FILE-003 | Rename 32 `*.Response.Type.ts` → `*.response.type.ts` | ✅ | 2026-07-18 |
| FILE-004 | Rename 19 `*.Query.Type.ts` → `*.query.type.ts` | ✅ | 2026-07-18 |
| FILE-005 | Rename 26 `*.Parameters.Type.ts` → `*.parameters.type.ts` | ✅ | 2026-07-18 |
| IMPORT-001 | Update all 321 import paths in 205 consumer files | ✅ | 2026-07-18 |
| INDEX-001 | Update 6 barrel index.ts re-exports | ✅ | 2026-07-18 |
| VUE-001 | Update 81 Vue component import paths | ✅ | 2026-07-18 |
| VERIFY-001 | Run `pnpm run type-check` — verify 0 new errors | ✅ | 2026-07-18 |

## 3. Alternatives

- **ALT-001**: Skip barrel files — would leave broken re-exports
- **ALT-002**: Batch by domain — slower, more commits, no benefit

## 4. Dependencies

- **DEP-001**: `git` for file renames
- **DEP-002**: `sed` for bulk import path replacement
- **DEP-003**: `pnpm run type-check` for verification

## 5. Files

See `plan/refactor-file-naming-convention-1.md` §Appendix A for the full 140-file rename mapping.

## 6. Testing

- **TEST-001**: `pnpm run type-check` — zero new TS errors beyond pre-existing baseline (~8 TreeNode issues)
- **TEST-002**: `rg 'Record<string, unknown>' src/features/ --type ts | grep -i '/types/\|/schemas/'` — zero

## 7. Risks & Assumptions

- **RISK-001**: Sed `--include` flag availability differs across platforms — use `-r` on Linux
- **ASSUMPTION-001**: PascalCase → kebab-case mapping is 1:1 with no collisions
