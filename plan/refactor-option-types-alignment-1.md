---
goal: Align frontend OptionType models, mappers, form, and store with backend API contracts
version: 1.0
date_created: 2026-07-21
owner: Platform Team
status: Planned
tags: refactoring, alignment, option-types, api-contract, bugfix
---

# Introduction

![Status: Planned](https://img.shields.io/badge/status-Planned-yellow)

The frontend `option-types` module has diverged from the backend API in several ways: response types use an alias where backend returns different shapes (`OptionTypeDetailResponse` vs `OptionTypeListItemResponse`), the mapper reads fields the detail endpoint doesn't send, validation allows position -1 (backend accepts it) but schema says `min(0)`, the Zod schema validates a `description` field that doesn't exist on the backend, the form submits `publicMetadata`/`privateMetadata` the backend ignores, and delete errors are swallowed without contextual messaging. This plan fixes all divergences to match the actual API contracts at `service/Api/src/Module/Catalog/Features/Admin/OptionTypes/`.

## 1. Requirements & Constraints

- **REQ-001**: Frontend `OptionTypeDetail` must match backend `OptionTypeDetailResponse` — no `optionValuesCount`, no `productsCount`
- **REQ-002**: Frontend `OptionTypeListItem` must match backend `OptionTypeListItemResponse` — has `optionValuesCount`, `productsCount`
- **REQ-003**: Frontend `toDetail` mapper must only read fields present in `GET /option-types/{id}` response (which returns `OptionTypeDetailResponse`)
- **REQ-004**: Frontend `toListItem` mapper must read fields from `GET /option-types` paged response (which returns `OptionTypeListItemResponse`)
- **REQ-005**: Position validation must accept min -1, matching `OptionTypeConstant.Constraints.MinPosition = -1`
- **REQ-006**: Delete handler must surface 409 Conflict with the `CannotDeleteWithValues` message from the backend
- **CON-001**: Do not change backend code — only fix frontend to match existing backend contracts
- **CON-002**: Do not change public API of the store (`fetchList`, `fetchById`, `create`, `update`, `remove`, `clearCurrent`) — only change internals and response types
- **CON-003**: All form submission payloads must type-check against `CreateOptionTypeRequest` / `UpdateOptionTypeRequest` — no extra unmodeled fields
- **PAT-001**: Follow the existing `ProductMapper.toSummary`/`ProductMapper.toDetail` pattern where list and detail have different mappers
- **PAT-002**: Use `dto.field as type | null ?? null` pattern for nullable fields, matching `variant.mapper.ts`

## 2. Implementation Steps

### Implementation Phase 1: Fix response model types

- GOAL-001: Split `OptionTypeDetail` from `OptionTypeListItem` — they are distinct types on the backend

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-001 | In `models/option-type.response.ts`, define `OptionTypeDetail` as a separate interface with `id`, `name`, `presentation` (`string \| null`), `position`, `filterable`, `createdAtUtc`, `modifiedAtUtc` (`string \| null`). Remove the `OptionTypeDetail = OptionTypeListItem` alias. | | |
| TASK-002 | Update `OptionTypeListItem` — change `presentation` from `string` to `string \| null` (backend `OptionTypeParameters.Presentation` is `string?`), change `modifiedAtUtc` from `string` to `string \| null` (backend `DateTimeOffset?`). | | |
| TASK-003 | Update all consumers that destructure `OptionTypeDetail` (store `currentItem`, `OptionTypeFormPage.vue`) to verify they don't access `optionValuesCount` or `productsCount` on the detail type. | | |

### Implementation Phase 2: Fix mapper — separate detail and list mapping

- GOAL-002: Create proper `toDetail` that doesn't read fields absent from the detail endpoint response

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-004 | In `api/option-type.mapper.ts`, rewrite `toDetail(dto)` — return only fields present in `OptionTypeDetailResponse`: `id`, `name`, `presentation`, `position`, `filterable`, `createdAtUtc`, `modifiedAtUtc`. Use `dto.presentation as string \| null ?? null` and `dto.modifiedAtUtc as string \| null ?? null` for nullable fields. | | |
| TASK-005 | In `api/option-type.mapper.ts`, verify `toListItem(dto)` still reads `optionValuesCount` and `productsCount` — these are only present in the paged list response. Keep the `Number(dto.optionValuesCount ?? 0)` coercion. | | |
| TASK-006 | Update `api/option-type.api.ts` `getById` method (L21-26): replace `OptionTypeMapper.toListItem(result.value)` with `OptionTypeMapper.toDetail(result.value)`. | | |
| TASK-007 | Update `api/option-type.api.ts` `create` method (L28-33): replace `OptionTypeMapper.toListItem(result.value)` with `OptionTypeMapper.toDetail(result.value)`. | | |
| TASK-008 | Update `api/option-type.api.ts` `update` method (L35-43): replace `OptionTypeMapper.toListItem(result.value)` with `OptionTypeMapper.toDetail(result.value)`. | | |

### Implementation Phase 3: Fix Zod validation schema

- GOAL-003: Remove fields with no backend counterpart; align position constraint with backend

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-009 | In `types/option-type.field.ts`, remove `descriptionSchema` function (L26-31). Remove `description: descriptionSchema(t)` from `createOptionTypeSchema` (L38). | | |
| TASK-010 | In `types/option-type.field.ts`, change `positionSchema` min from `.min(0)` to `.min(-1)` (L17). | | |

### Implementation Phase 4: Fix form page — remove unmodeled fields, handle delete 409

- GOAL-004: Stop submitting fields the backend doesn't accept; surface 409 Conflict on delete with actionable message

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-011 | In `pages/OptionTypeFormPage.vue`, remove lines 34-35 (`publicMetadata` and `privateMetadata` ref declarations). Remove lines 132-133 (`publicMetadata.value = {}` / `privateMetadata.value = {}` in `loadItem`). Remove lines 156-159 (`publicMetadata: publicMetadata.value, privateMetadata: privateMetadata.value` from the submit payload). Remove the `MetadataManager` import and the `Divider` import. Remove the "Metadata" tab panel (L288-293) and the `Tab :value="2"` entry (L217). | | |
| TASK-012 | In `pages/OptionTypeFormPage.vue` L178, wrap `store.fetchList({ pageSize: 100 })` inside `if (handled && ...)` block — only refetch on successful create/update. | | |

### Implementation Phase 5: Fix store — handle 409 on delete

- GOAL-005: Surface backend `CannotDeleteWithValues` to the user; stop mutating `totalRecords`

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-013 | In `store/option-type.store.ts`, change `remove` function signature — add return type `Promise<ServerResult<void>>` (already correct). Update the implementation: remove `totalRecords.value--` (L69); after successful delete, call `fetchList()` to refetch from server instead of mutating local `items`/`totalRecords`. | | |
| TASK-014 | In `pages/OptionTypeManagerPage.vue`, update `deleteOptionType` handler — check `result.statusCode === 409` and show a contextual error: `"Cannot delete: this option type has existing option values. Remove all values first."`. Use `handleApiResult(result)` for all other errors (L42). | | |
| TASK-015 | In `pages/OptionTypeListPage.vue`, update `deleteOptionType` handler — same 409 check as TASK-014. | | |

### Implementation Phase 6: Fix nullable typing in store

- GOAL-006: Fix store return types to match the new split detail/list types

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-016 | In `store/option-type.store.ts`, change `currentItem` type from `ref<OptionTypeDetail \| null>` (already correct after split). Update `fetchById` return type — currently inferred as `Promise<ServerResult<OptionTypeListItem>>`. The underlying repo now returns `ServerResult<OptionTypeDetail>` via `toDetail`. Add explicit return annotation or verify inference. | | |
| TASK-017 | In `store/option-type.store.ts`, update `create` return type — it should return `ServerResult<OptionTypeDetail>` matching `CreateOptionType.Response` on the backend. Update the fallback path for non-success: return `result as ServerResult<OptionTypeDetail>` (or the original result, which will have `value` typed as `OptionTypeDetail` after the API layer maps it). | | |
| TASK-018 | In `store/option-type.store.ts`, update `update` return type — same as TASK-017. | | |

### Implementation Phase 7: Run lint, type-check, and tests

- GOAL-007: Verify all changes compile and pass validation

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-019 | Run `pnpm run lint` from `app/Admin/` — fix any lint errors. | | |
| TASK-020 | Run `pnpm run type-check` (or `vue-tsc --noEmit`) from `app/Admin/` — verify no type errors from the model split or nullable type changes. | | |
| TASK-021 | Run `pnpm run test:unit` from `app/Admin/` — all tests pass. | | |

## 3. Alternatives

- **ALT-001**: Keep `OptionTypeDetail = OptionTypeListItem` and add `optionValuesCount`/`productsCount` to the backend detail response — rejected because it would widen the detail response with unnecessary aggregates and requires backend changes.
- **ALT-002**: Keep `publicMetadata`/`privateMetadata` and implement backend support (add to `OptionTypeRequest`, EF column, migration) — rejected because it's a new feature, not a fix. Scope is alignment, not feature addition.
- **ALT-003**: Keep `totalRecords.value--` in store and push delete bugs to the list — rejected because the current approach desyncs `totalRecords` when filters change or when the deleted item is the last on a page. A server refetch is more reliable.

## 4. Dependencies

- **DEP-001**: `pnpm` installed and `app/Admin/` workspace configured
- **DEP-002**: `app/Admin/src/features/catalog/option-types/` directory with existing files
- **DEP-003**: Backend already deployed with the contracts described — no backend changes needed

## 5. Files

- **FILE-001**: `models/option-type.response.ts` — `OptionTypeDetail` separated from `OptionTypeListItem`; `presentation` and `modifiedAtUtc` made nullable
- **FILE-002**: `api/option-type.mapper.ts` — `toDetail` rewritten to not read count fields; `getById`/`create`/`update` API methods use `toDetail` instead of `toListItem`
- **FILE-003**: `api/option-type.api.ts` — L21-43: update mapper calls from `toListItem` to `toDetail` for single-entity endpoints
- **FILE-004**: `types/option-type.field.ts` — `descriptionSchema` removed; `positionSchema` min changed to -1
- **FILE-005**: `pages/OptionTypeFormPage.vue` — `publicMetadata`/`privateMetadata` removed; metadata tab panel removed; `MetadataManager`/`Divider` imports removed; `store.fetchList` moved inside success block
- **FILE-006**: `store/option-type.store.ts` — `totalRecords.value--` removed; `remove` calls `fetchList()` on success; type annotations updated
- **FILE-007**: `pages/OptionTypeManagerPage.vue` — `deleteOptionType` handles 409 specifically
- **FILE-008**: `pages/OptionTypeListPage.vue` — `deleteOptionType` handles 409 specifically

## 6. Testing

- **TEST-001**: `pnpm run test:unit` from `app/Admin/` — all unit tests pass
- **TEST-002**: `pnpm run type-check` (vue-tsc --noEmit) — no type errors
- **TEST-003**: `pnpm run lint` from `app/Admin/` — no lint errors
- **TEST-004**: Manual smoke test: open OptionTypeManager, create/edit/delete an option type, verify no console errors and toast messages are correct
- **TEST-005**: Manual smoke test: try deleting an option type with existing values — verify 409 message appears

## 7. Risks & Assumptions

- **RISK-001**: Changing `presentation` from `string` to `string \| null` may cause type errors in consumers that assume it's always present. Run a grep for `OptionTypeListItem` usage across the codebase before merging.
- **RISK-002**: Changing `modifiedAtUtc` from `string` to `string \| null` similarly affects consumers. Backend `ModifiedAtUtc` is `DateTimeOffset?` — can be null on initial creation.
- **RISK-003**: Removing `totalRecords.value--` and replacing with `fetchList()` adds a network round-trip on delete. Acceptable for data integrity.
- **ASSUMPTION-001**: The `optionValuesCount` and `productsCount` fields are not consumed anywhere on the detail view (GET by ID / edit page). Verify by checking `OptionTypeFormPage.vue` and `OptionTypeDetail` consumers — no existing usage was found during audit.

## 8. Related Specifications / Further Reading

- `service/Api/src/Module/Catalog/Features/Admin/OptionTypes/Shared/Models/OptionType.Model.Response.cs` — backend detail vs list response definitions
- `service/Api/src/Module/Catalog/Domain/OptionTypes/OptionType.Constant.cs` — backend validation constants (MinPosition = -1)
- `service/Api/src/Module/Catalog/Features/Admin/OptionTypes/Delete/DeleteOptionType.cs` — backend delete logic with `CannotDeleteWithValues` at L36-37
- `plan/refactor-catalog-mappers-1.md` — existing mapper refactoring this plan builds upon
