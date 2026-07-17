---
goal: Establish layered architecture: types → schema → repository → mapper → service → store
version: 1.0
date_created: 2026-07-17
owner: feat/admin-app
status: 'Completed'
tags: architecture, pattern, refactor
---

# Introduction

![Status: Completed](https://img.shields.io/badge/status-Completed-brightgreen)

Define and implement a clean layered data-flow pattern for all Admin SPA features. Each feature follows a strict dependency chain where each layer only depends on the layer below it:

```
Schema (Zod validation) ──┐
                           ├──> Types (domain models)
Repository (HTTP layer) ──┤
                           ├──> Service (business logic)
Mapper (DTO transforms) ──┘
                              └──> Store (Pinia state)
```

## 1. Requirements & Constraints

- **REQ-001**: Zod schemas are the single source of truth for form data types via `z.infer<>`
- **REQ-002**: Types extend/inherit from inferred schema types where applicable
- **REQ-003**: Repository layer handles HTTP only — no business logic, no state
- **REQ-004**: Mapper layer transforms API snake_case DTOs to frontend camelCase models
- **REQ-005**: Service layer orchestrates repo calls + mapper transforms + business rules
- **REQ-006**: Store layer uses service only — never calls repo directly
- **PAT-001**: File naming: `{entity}.{layer}.ts` (e.g., `auth.repository.ts`, `auth.mapper.ts`)
- **CON-001**: Existing `.api.ts` files remain as-is until migrated; new features use the new pattern

## 2. Implementation Steps

### Phase 1 — Create shared infrastructure

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-001 | Create `src/shared/repository/base.repository.ts` — generic CRUD repository class | | |
| TASK-002 | Create `src/shared/mapper/mapper.utils.ts` — camelCase/snake_case conversion + mapping helpers | | |

### Phase 2 — Implement auth feature with new pattern

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-003 | Create `auth.repository.ts` — login, refresh, logout, getProfile HTTP methods | | |
| TASK-004 | Create `auth.mapper.ts` — map AuthenticationResponse → UserProfile | | |
| TASK-005 | Refactor `auth.service.ts` — use repository + mapper, expose typed methods | | |
| TASK-006 | Refactor `auth.store.ts` — use service, maintain auth state | | |

## 3. Files

| File | Action |
|------|--------|
| `src/shared/repository/base.repository.ts` | Create — generic CRUD |
| `src/shared/mapper/mapper.utils.ts` | Create — transform utilities |
| `src/features/auth/repository/auth.repository.ts` | Create — auth HTTP layer |
| `src/features/auth/mapper/auth.mapper.ts` | Create — DTO→model mapping |
| `src/features/auth/services/auth.service.ts` | Refactor — use repo + mapper |
| `src/features/auth/stores/auth.store.ts` | Refactor — use service |
| `src/features/auth/types/auth.types.ts` | Keep — domain models |
| `src/features/auth/schemas/auth.schema.ts` | Keep — Zod validation |

## 4. Testing

- **TEST-001**: `npx vite build` passes (0 errors)
- **TEST-002**: `vue-tsc --build` passes (0 errors)
