---
goal: Migrate all feature API layers to Repository + Mapper + Service pattern
version: 1.0
date_created: 2026-07-17
owner: feat/admin-app
status: 'Completed'
tags: refactor, architecture, pattern
---

# Introduction

![Status: Completed](https://img.shields.io/badge/status-Completed-brightgreen)

Migrate all 8 feature API/service layers to the new layered pattern:

```
Schema (Zod) → Types → Repository (HTTP) → Mapper (transform) → Service (logic) → Store (state)
```

## Naming Rules

| Artifact | Suffix | Example |
|----------|--------|---------|
| Zod validation | `Schema` | `LoginSchema` |
| Inferred form type | `FormData` | `LoginFormData` |
| API request body | `Request` | `CreateProductRequest` |
| API response body | `Response` | `AuthenticationResponse` |
| Search/filter params | `Params` | `ProductSearchParams` |
| Domain entity | _(none)_ | `Product`, `User` |
| Repository class | `Repository` | `CatalogRepository` |
| Mapper function | `map*` | `mapProductResponse` |

## 1. Requirements & Constraints

- **REQ-001**: Every `.api.ts` file becomes a `repository/*.repository.ts` extending `BaseRepository`
- **REQ-002**: Every `service.ts` uses repository + mapper — no direct apiClient calls
- **REQ-003**: Snake_case API JSON is mapped to camelCase frontend models in mapper layer
- **REQ-004**: All existing component imports remain valid — service public API unchanged
- **CON-001**: Auth already migrated as reference — skip
- **CON-002**: Keep existing `.api.ts` files until all consumers are updated, then delete

## 2. Implementation Steps

### Phase 1 — Identity

- GOAL-001: Migrate identity.api.ts → IdentityRepository + mapper

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-001 | Create `repository/identity.repository.ts` from `services/identity.api.ts` | ✅ | 2026-07-17 |
| TASK-002 | Create `mapper/identity.mapper.ts` for user/role/permission responses | ✅ | 2026-07-17 |
| TASK-003 | Refactor `services/user.service.ts` to use repository + mapper | ✅ | 2026-07-17 |
| TASK-004 | Refactor `services/role.service.ts` to use repository + mapper | ✅ | 2026-07-17 |
| TASK-005 | Refactor `services/permission.service.ts` to use repository + mapper | ✅ | 2026-07-17 |
| TASK-006 | Delete `services/identity.api.ts` after all consumers updated | ✅ | 2026-07-17 |

### Phase 2 — Location

- GOAL-002: Migrate location.api.ts → LocationRepository + mapper

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-007 | Create `repository/location.repository.ts` from `services/location.api.ts` | ✅ | 2026-07-17 |
| TASK-008 | Create `mapper/location.mapper.ts` for country/state responses | ✅ | 2026-07-17 |
| TASK-009 | Refactor `services/country.service.ts` and `services/state.service.ts` | ✅ | 2026-07-17 |
| TASK-010 | Delete `services/location.api.ts` | ✅ | 2026-07-17 |

### Phase 3 — Catalog

- GOAL-003: Migrate catalog.api.ts → CatalogRepository + mapper (largest feature)

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-011 | Create `repository/catalog.repository.ts` from `services/catalog.api.ts` | ✅ | 2026-07-17 |
| TASK-012 | Create `mapper/catalog.mapper.ts` for product/variant responses | ✅ | 2026-07-17 |
| TASK-013 | Refactor `services/product.service.ts` | ✅ | 2026-07-17 |
| TASK-014 | Refactor `services/variant.service.ts` | ✅ | 2026-07-17 |
| TASK-015 | Refactor `services/option-type.service.ts` and `option-value.service.ts` | ✅ | 2026-07-17 |
| TASK-016 | Refactor `services/property-type.service.ts` | ✅ | 2026-07-17 |
| TASK-017 | Refactor `services/taxonomy.service.ts` and `taxon.service.ts` | ✅ | 2026-07-17 |
| TASK-018 | Refactor `services/catalog-dashboard.service.ts` | ✅ | 2026-07-17 |
| TASK-019 | Delete `services/catalog.api.ts` | ✅ | 2026-07-17 |

### Phase 4 — Inventory

- GOAL-004: Migrate inventory.api.ts → InventoryRepository + mapper

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-020 | Create `repository/inventory.repository.ts` from `services/inventory.api.ts` | ✅ | 2026-07-17 |
| TASK-021 | Create `mapper/inventory.mapper.ts` | ✅ | 2026-07-17 |
| TASK-022 | Refactor `services/inventory.service.ts` | ✅ | 2026-07-17 |
| TASK-023 | Delete `services/inventory.api.ts` | ✅ | 2026-07-17 |

### Phase 5 — Ordering

- GOAL-005: Migrate ordering.api.ts → OrderingRepository + mapper

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-024 | Create `repository/ordering.repository.ts` from `services/ordering.api.ts` | ✅ | 2026-07-17 |
| TASK-025 | Create `mapper/ordering.mapper.ts` | ✅ | 2026-07-17 |
| TASK-026 | Refactor `services/order.service.ts` | ✅ | 2026-07-17 |
| TASK-027 | Refactor `fulfillment/services/fulfillment.service.ts` | ✅ | 2026-07-17 |
| TASK-028 | Delete `services/ordering.api.ts` | ✅ | 2026-07-17 |

### Phase 6 — Profile

- GOAL-006: Migrate profile.api.ts → ProfileRepository + mapper

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-029 | Create `repository/profile.repository.ts` from `services/profile.api.ts` | ✅ | 2026-07-17 |
| TASK-030 | Create `mapper/profile.mapper.ts` | ✅ | 2026-07-17 |
| TASK-031 | Refactor `services/profile.service.ts` | ✅ | 2026-07-17 |
| TASK-032 | Delete `services/profile.api.ts` | ✅ | 2026-07-17 |

## 3. Dependencies

- **DEP-001**: `BaseRepository` at `src/shared/repository/base.repository.ts`
- **DEP-002**: `mapper.utils.ts` at `src/shared/mapper/mapper.utils.ts`

## 4. Files

| File Pattern | Count |
|-------------|-------|
| `features/*/repository/*.repository.ts` | 7 new |
| `features/*/mapper/*.mapper.ts` | 7 new |
| `features/*/services/*.service.ts` | 20 refactored |
| `features/*/services/*.api.ts` | 6 deleted |
| `src/shared/repository/base.repository.ts` | exists |
| `src/shared/mapper/mapper.utils.ts` | exists |

## 5. Testing

- **TEST-001**: `npx vite build` passes
- **TEST-002**: `vue-tsc --build` — 0 errors
