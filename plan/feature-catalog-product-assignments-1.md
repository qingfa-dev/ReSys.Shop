---
goal: Implement admin catalog Product OptionTypes and Product Classifications frontend API services
version: 1.0
date_created: 2026-07-25
status: Planned
tags: feature, api, catalog, admin
---

# Introduction

![Status: Planned](https://img.shields.io/badge/status-Planned-blue)

Implement frontend API layers for Product OptionTypes (4 endpoints) and Product Classifications (4 endpoints) in `app/Admin/src/features/catalog/`. Both follow the same assign/revoke/sync pattern: GET list of items (with isAssigned flag), POST assign/revoke with ID+Position pairs, PUT sync with full replacement list.

## 1. Requirements & Constraints

- **REQ-001**: All 8 backend endpoints must have corresponding frontend API methods
- **REQ-002**: API methods take productId as parent ID parameter
- **REQ-003**: GET response includes `isAssigned` boolean — UI shows all available options, marks which are assigned
- **REQ-004**: Assign/Revoke/Sync send `IEnumerable<{Id, Position}>` payload
- **REQ-005**: No form validation needed (no free-form text fields)
- **CON-001**: Follow identical pattern for both OptionTypes and Classifications
- **CON-002**: Zero TypeScript errors
- **PAT-001**: Assign/Revoke/Sync return `Result<void>` (no response body on success)

## 2. Implementation Steps

### Phase 1: Product OptionTypes

- GOAL-001: Implement Product OptionTypes API + inline manager component

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-001 | Create `types/product-option-type.response.ts` — `ProductOptionTypeItem`: optionTypeId, position, name, presentation?, isAssigned; `ProductOptionTypesResponse`: items: ProductOptionTypeItem[] | | |
| TASK-002 | Create `types/product-option-type.request.ts` — `ProductOptionTypeAssignmentItem`: optionTypeId: string, position: number; `OptionTypeItemsRequest`: items: ProductOptionTypeAssignmentItem[] | | |
| TASK-003 | Create `api/product-option-type.api.ts` — `ProductOptionTypeApi`: get(productId) GET `/catalog/products/${productId}/option-types`; assign POST `.../assign`; revoke POST `.../revoke`; sync PUT `.../sync` | | |
| TASK-004 | Create `components/ProductOptionTypeManager.vue` — inline inside ProductForm; DataTable or checkbox list of all option types with position input; assign/revoke on toggle; sync on save | | |
| TASK-005 | Update `composables/useProduct.ts` — add `optionTypeApi: ProductOptionTypeApi` | | |
| TASK-006 | Integrate into ProductForm.vue as section, update barrel exports | | |
| TASK-007 | Verify: `pnpm build` passes | | |

### Phase 2: Product Classifications

- GOAL-002: Implement Product Classifications API + inline manager component

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-008 | Create `types/product-classification.response.ts` — `ClassificationItem`: taxonId, position, name, prettyName?, isAssigned; `ProductClassificationsResponse`: items: ClassificationItem[] | | |
| TASK-009 | Create `types/product-classification.request.ts` — `ProductClassificationAssignmentItem`: taxonId: string, position: number; `ClassificationItemsRequest`: items: ProductClassificationAssignmentItem[] | | |
| TASK-010 | Create `api/product-classification.api.ts` — `ProductClassificationApi`: get(productId) GET `/catalog/products/${productId}/classifications`; assign POST `.../assign`; revoke POST `.../revoke`; sync PUT `/catalog/products/${productId}/classifications` (no /sync suffix) | | |
| TASK-011 | Create `components/ProductClassificationManager.vue` — inline inside ProductForm; taxon tree browser with checkboxes + position; assign/revoke on toggle; sync on save | | |
| TASK-012 | Update `composables/useProduct.ts` — add `classificationApi: ProductClassificationApi` | | |
| TASK-013 | Integrate into ProductForm.vue, update barrel exports | | |
| TASK-014 | Verify: `pnpm build` passes | | |

## 3. Alternatives

- **ALT-001**: Single combined "assignments" API file — rejected: backend separates OptionTypes vs Classifications, frontend should mirror

## 4. Dependencies

- **DEP-001**: Existing `ProductForm.vue` — integration target
- **DEP-002**: Existing `OptionTypeApi.getMany` — ProductOptionTypeManager needs list of all option types
- **DEP-003**: Existing `TaxonApi.getTree` (to be implemented in Phase 7) — ClassificationManager needs taxon tree

## 5. Files

- **FILE-001**: `types/product-option-type.response.ts`, `types/product-option-type.request.ts`
- **FILE-002**: `api/product-option-type.api.ts`
- **FILE-003**: `components/ProductOptionTypeManager.vue`
- **FILE-004**: `types/product-classification.response.ts`, `types/product-classification.request.ts`
- **FILE-005**: `api/product-classification.api.ts`
- **FILE-006**: `components/ProductClassificationManager.vue`
- **FILE-007**: `composables/useProduct.ts` (updated)
- **FILE-008**: `components/ProductForm.vue` (updated)
- **FILE-009**: Barrel files (updated)

## 6. Testing

- **TEST-001**: `api/__tests__/product-option-types.spec.ts` — mock apiClient, verify get/assign/revoke/sync
- **TEST-002**: `api/__tests__/product-classifications.spec.ts` — mock apiClient, verify get/assign/revoke/sync

## 7. Risks & Assumptions

- **ASSUMPTION-001**: Both assign/revoke use POST with body `{ items: [{ optionTypeId/taxonId, position }] }` — verify payload shape matches backend expectation
- **RISK-001**: Backend sync for Classifications uses PUT without /sync suffix (`/classifications` not `/classifications/sync`) — different from OptionTypes which uses PUT with /sync suffix; double-check URL pattern
- **RISK-002**: `ProductClassificationManager` depends on taxon tree endpoint which is not yet frontend-implemented — mark as integration dependency

## 8. Related Specifications / Further Reading

Backend OptionTypes: `service/Api/src/Module/Catalog/Features/Admin/Products/OptionTypes/`
Backend Classifications: `service/Api/src/Module/Catalog/Features/Admin/Products/Classifications/`
