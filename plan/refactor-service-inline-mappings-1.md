---
goal: Move all inline response mappings from services into proper mapper functions and eliminate `as unknown as` casts
version: 1.0
date_created: 2026-07-18
status: 'Completed'
tags: refactor, mappers, services, type-safety
---

# Introduction

![Status: Planned](https://img.shields.io/badge/status-Planned-blue)

Audit found 25+ locations across services where raw API responses are mapped inline (inside `value: { ... }` blocks) or cast via `as unknown as T` instead of going through the proper mapper layer. This bypasses type safety and makes the mapper layer incomplete. Every inline mapping must move to a `*.mapper.ts` file.

## 1. Requirements & Constraints

- **REQ-001**: Zero inline `value: { ... }` object construction in service files — all moved to mappers
- **REQ-002**: Zero `as unknown as` casts in service files — replaced with proper `as` or mapper-return-type
- **REQ-003**: Every mapper function has a typed signature — no `Record<string, unknown>` return
- **CON-001**: `ServerResult<T>` / `ServerPagedResult<T>` generics may need explicit type params
- **CON-002**: `as unknown as` on delete/create-void endpoints is acceptable only if service wraps with mapper first
- **PAT-001**: Service pattern: `const result = await repo.xxx(params); return { ...result, value: mapXxx(result.value) };`

## 2. Implementation Steps

### Phase 1: Auth — Move inline profile mapping to mapper

- GOAL-001: Replace inline `getProfile()` value construction with a mapper function

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-001 | Add `mapProfileResponse(value: Record<string, unknown>): Partial<UserProfile>` to `auth/mappers/auth.mapper.ts` — move the inline `{ id, email, fullName, roles }` logic | | |
| TASK-002 | Add `mapSessionResponse(value: { id: string; roles: string[] }): { id: string; roles: string[]; permissions: string[] }` to handle `fetchSession()` inline mapping | | |
| TASK-003 | Update `auth/services/auth.service.ts` — replace inline `value: { id: String(...)... }` with `mapProfileResponse(result.value)`, replace `fetchSession` inline with `mapSessionResponse` | | |
| TASK-004 | Remove `as unknown as Record<string, unknown>` cast in `getProfile()` | | |
| TASK-005 | Verify: `pnpm run type-check` — zero new errors | | |

### Phase 2: Users — Replace `as unknown as` casts with proper mapper typing

- GOAL-002: Fix `users/services/user.service.ts` — 4 `as unknown as` casts

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-006 | Read `users/services/user.service.ts` — identify all 4 `as unknown as` locations | | |
| TASK-007 | Fix each: `res as unknown as ServerResult<AdminUserSummaryModel>` → destructure and apply mapper. Pattern: `{ ...res, value: res.value ? toAdminUserSummaryModel(res.value) : undefined }` | | |
| TASK-008 | Verify: `pnpm run type-check` — zero new errors | | |

### Phase 3: Option-Value — Replace `as unknown as` cast

- GOAL-003: Fix `option-value.service.ts`

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-009 | Read `catalog/option-types/option-values/services/option-value.service.ts` — fix `as unknown as Promise<...>` | | |
| TASK-010 | Replace with proper `await` + `as` or typed variable | | |
| TASK-011 | Verify | | |

### Phase 4: Inventory — Clean up `as unknown as` casts

- GOAL-004: Fix `reservation.service.ts`, `movement.service.ts`, `transfer.service.ts`, `inventory.service.ts`

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-012 | Read `inventories/inventory-units/services/reservation.service.ts` — replace 4 `as unknown as` casts. Where mapper is applied, ensure return type matches mapper output directly | | |
| TASK-013 | Read `inventories/stock-movements/services/movement.service.ts` — replace 3 `as unknown as` casts | | |
| TASK-014 | Read `inventories/stock-transfers/services/transfer.service.ts` — replace `as unknown as` casts where mapper is applied with proper typing | | |
| TASK-015 | Read `inventories/services/inventory.service.ts` — replace 10+ `as unknown as` casts. Note: many void-return endpoints (`delete`, `restock`, `bulkAdjust`, `setDefault`) can use plain `as` since no data transformation needed | | |
| TASK-016 | Verify: `pnpm run type-check` | | |

### Phase 5: Catalog — Fix inline value + as unknown as

- GOAL-005: Fix `taxon.service.ts` inline and `product.service.ts` stub

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-017 | Read `catalog/taxonomies/taxa/services/taxon.service.ts` — replace `value: { items: [], totalCount: 0 }` with properly typed return | | |
| TASK-018 | Read `catalog/products/services/product.service.ts` — replace `null as unknown as ProductImage` with `null as ProductImage \| null` | | |
| TASK-019 | Verify | | |

### Phase 6: Location — Fix void-return casts

- GOAL-006: Fix `country.service.ts` and `state.service.ts` delete casts

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-020 | Read `location/countries/services/country.service.ts` — plain `as Promise<ServerResult<void>>` is fine, but ensure it matches | | |
| TASK-021 | Read `location/states/services/state.service.ts` — same check | | |
| TASK-022 | Verify | | |

## 3. Alternatives

- **ALT-001**: Leave `as unknown as` — rejected because it signals type unsafety and the user explicitly flagged it
- **ALT-002**: Create a `wrapResult` helper function — would handle the common `{ ...result, value: mapper(result.value) }` pattern generically

## 4. Dependencies

- **DEP-001**: All existing `*.mapper.ts` files (Phase 2-4 refactors)
- **DEP-002**: `ServerResult<T>` / `ServerPagedResult<T>` type definitions in `shared/api/types/`

## 5. Files

- **FILE-001**: `auth/mappers/auth.mapper.ts` — add `mapProfileResponse`, `mapSessionResponse`
- **FILE-002**: `auth/services/auth.service.ts` — use mappers, remove inline + unknown cast
- **FILE-003**: `users/services/user.service.ts` — fix 4 casts
- **FILE-004**: `catalog/option-types/option-values/services/option-value.service.ts` — fix cast
- **FILE-005**: `inventories/inventory-units/services/reservation.service.ts` — fix 4 casts
- **FILE-006**: `inventories/stock-movements/services/movement.service.ts` — fix 3 casts
- **FILE-007**: `inventories/stock-transfers/services/transfer.service.ts` — fix casts
- **FILE-008**: `inventories/services/inventory.service.ts` — fix 10+ casts
- **FILE-009**: `catalog/taxonomies/taxa/services/taxon.service.ts` — fix inline value
- **FILE-010**: `catalog/products/services/product.service.ts` — fix null cast

## 6. Testing

- **TEST-001**: `pnpm run type-check` — zero errors (baseline ~8 TreeNode)
- **TEST-002**: `pnpm run test:unit` — all existing tests pass

## 7. Risks & Assumptions

- **RISK-001**: Some `as unknown as` casts may hide genuine type mismatches that surface when removed — fix the root cause, don't re-add casts
- **ASSUMPTION-001**: All repository methods return `ServerResult<T>` with a `.value` property that can be safely spread
