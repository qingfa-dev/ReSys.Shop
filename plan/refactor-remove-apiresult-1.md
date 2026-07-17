---
goal: Replace ApiResult with ServerResult/ServerPagedResult + result mappers
version: 1.0
date_created: 2026-07-17
owner: feat/admin-app
status: 'Completed'
tags: refactor, api, types
---

# Introduction

![Status: Completed](https://img.shields.io/badge/status-Completed-brightgreen)

Remove the `ApiResult<T>` abstraction layer. The apiClient response interceptor currently transforms raw `ServerResult<T>` / `ServerPagedResult<T>` into `ApiResult`. Instead, pass through the raw server types and let repositories/services extract data via result mapper utilities.

## 1. Requirements & Constraints

- **REQ-001**: Remove `ApiResult` type entirely
- **REQ-002**: apiClient response interceptor returns raw `ServerResult<T>` / `ServerPagedResult<T>`
- **REQ-003**: Create `result.mapper.ts` with `unwrapResult<T>`, `unwrapPagedResult<T>` helpers
- **REQ-004**: All repositories return `ServerResult<T>` / `ServerPagedResult<T>`
- **REQ-005**: Services use result mappers to extract data

## 2. Implementation Steps

### Phase 1 — Infrastructure

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-001 | Create `result.mapper.ts` — `unwrapResult`, `unwrapPagedResult`, `isSuccess`, `isFailure` | | |
| TASK-002 | Update `apiClient` response interceptor to pass through raw ServerResult/ServerPagedResult | | |
| TASK-003 | Delete `ApiResult` from `api.types.ts` | | |
| TASK-004 | Update `BaseRepository` to use `ServerResult`/`ServerPagedResult` | | |
| TASK-005 | Create `api/index.ts` re-exports for new result types | | |

### Phase 2 — Update all 51 consumer files

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-006 | Update all repositories (6 files) | | |
| TASK-007 | Update all services (10 files) | | |
| TASK-008 | Update all stores (8 files) | | |
| TASK-009 | Update all components/views (20 files) | | |
| TASK-010 | Update shared infrastructure (5 files) | | |

## 3. Files

| File | Action |
|------|--------|
| `src/shared/api/utils/result.mapper.ts` | Create |
| `src/shared/api/http/api.client.ts` | Edit — remove ApiResult transform |
| `src/shared/api/types/api.types.ts` | Edit — remove ApiResult |
| `src/shared/repository/base.repository.ts` | Edit — new return types |
| 47 consumer files | Edit — update imports and usage |

## 4. Testing

- **TEST-001**: `pnpm run build` passes
- **TEST-002**: `vue-tsc --build` — 0 errors
