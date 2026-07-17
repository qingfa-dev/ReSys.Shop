---
goal: Decompose feature types into separated concern files (domain, model, request, response)
version: 1.0
date_created: 2026-07-17
owner: feat/admin-app
status: 'Completed'
tags: refactor, types, architecture
---

# Introduction

![Status: Completed](https://img.shields.io/badge/status-Completed-brightgreen)

Split monolithic `types/*.types.ts` files into concern-specific files per feature:

| File | Content |
|------|---------|
| `{feature}.domain.types.ts` | Domain entities (UserProfile, Product, etc.) |
| `{feature}.model.types.ts` | Form/model types via `z.infer<typeof Schema>` |
| `{feature}.request.types.ts` | API request bodies |
| `{feature}.response.types.ts` | API response bodies (snake_case from server, mapped to camelCase) |

Schema files stay Zod-only — only fields needing validation rules.

## 1. Requirements & Constraints

- **REQ-001**: Schema defines validation rules only — no type exports unless `z.infer`
- **REQ-002**: Model types extend schema-inferred types via `z.infer<typeof XxxSchema>`
- **REQ-003**: Domain types are pure interfaces — no API concerns
- **REQ-004**: Request types extend or compose model types
- **REQ-005**: Response types mirror API JSON shape (transformed by mappers)
- **CON-001**: Keep existing import paths working — update all consumers

## 2. Implementation Steps

### Phase 1 — Auth (reference)

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-001 | Create `auth.model.types.ts` — LoginFormData, ChangePasswordFormData from schemas | | |
| TASK-002 | Create `auth.domain.types.ts` — UserProfile domain entity | | |
| TASK-003 | Create `auth.request.types.ts` — LoginRequest, RefreshRequest | | |
| TASK-004 | Create `auth.response.types.ts` — AuthenticationResponse | | |
| TASK-005 | Delete `auth.types.ts` — replaced by above | | |
| TASK-006 | Update `auth.schema.ts` — remove type exports, keep only Zod schemas | | |
| TASK-007 | Update all imports across repository, mapper, service, store, views | | |

### Phase 2 — Location

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-008 | Create `location.domain.types.ts` — Country, State | | |
| TASK-009 | Create `location.model.types.ts` — CountryFormData, StateFormData | | |
| TASK-010 | Create `location.request.types.ts` — CountryCreateRequest, CountryUpdateRequest | | |
| TASK-011 | Delete `country.types.ts`, `state.types.ts` | | |
| TASK-012 | Update schemas — remove type-only exports | | |
| TASK-013 | Update all imports | | |

### Phase 3 — Catalog entities

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-014 | Refactor catalog/option-types types | | |
| TASK-015 | Refactor catalog/option-values types | | |
| TASK-016 | Refactor catalog/products types | | |
| TASK-017 | Refactor catalog/property-types types | | |
| TASK-018 | Refactor catalog/taxonomies types | | |
| TASK-019 | Refactor catalog/taxa types | | |

## 3. Files

| File | Action |
|------|--------|
| `features/*/types/*.domain.types.ts` | Create per feature |
| `features/*/types/*.model.types.ts` | Create per feature |
| `features/*/types/*.request.types.ts` | Create per feature |
| `features/*/types/*.response.types.ts` | Create per feature |
| `features/*/types/*.types.ts` | Delete (old monolithic) |
| `features/*/schemas/*.schema.ts` | Edit — remove type exports |

## 4. Testing

- **TEST-001**: `vue-tsc --build` — 0 errors
- **TEST-002**: `npx vite build` — passes
