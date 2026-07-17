---
goal: Split monolithic repository files into per-entity repositories
version: 1.0
date_created: 2026-07-17
owner: feat/admin-app
status: 'Completed'
tags: refactor, repository, architecture
---

# Introduction

![Status: Completed](https://img.shields.io/badge/status-Completed-brightgreen)

Split monolithic `repository/*.repository.ts` files into per-entity files:

| Feature | Monolithic → Per-entity |
|---------|------------------------|
| catalog | `catalog.repository.ts` → `product.repository.ts`, `variant.repository.ts`, `option-type.repository.ts`, `option-value.repository.ts`, `property-type.repository.ts`, `taxonomy.repository.ts`, `taxon.repository.ts` |
| identity | `identity.repository.ts` → `user.repository.ts`, `role.repository.ts`, `permission.repository.ts` |
| inventory | `inventory.repository.ts` → `stock.repository.ts`, `location.repository.ts`, `reservation.repository.ts`, `transfer.repository.ts`, `movement.repository.ts` |
| ordering | `ordering.repository.ts` → `order.repository.ts`, `fulfillment.repository.ts` |

## 1. Requirements & Constraints

- **REQ-001**: Each repository file handles ONE entity domain
- **REQ-002**: Repository name matches the entity (e.g., `product.repository.ts`)
- **REQ-003**: All existing service public APIs preserved — only imports change
- **CON-001**: Auth, location, profile are already split — skip

## 2. Implementation Steps

### Phase 1 — Catalog (7 entities)

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-001 | Split `catalog.repository.ts` into product, variant, option-type, option-value, property-type, taxonomy, taxon | | |

### Phase 2 — Identity (3 entities)

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-002 | Split `identity.repository.ts` into user, role, permission | | |

### Phase 3 — Inventory (5 entities)

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-003 | Split `inventory.repository.ts` into stock, location, reservation, transfer, movement | | |

### Phase 4 — Ordering (2 entities)

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-004 | Split `ordering.repository.ts` into order, fulfillment | | |

### Phase 5 — Verify

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-005 | `pnpm run build` + `vue-tsc --build` — 0 errors | | |
