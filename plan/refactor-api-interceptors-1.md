---
goal: Split monolithic api.client.ts into separate interceptor/handler files by concern
version: 1.0
date_created: 2026-07-18
status: 'Completed'
tags: refactor, api-client, interceptors, architecture
---

# Introduction

![Status: Planned](https://img.shields.io/badge/status-Planned-blue)

The current `http/api.client.ts` conflates 3 concerns in one file: auth token injection (request), camelCase conversion (response success), and error wrapping + 401 refresh + never-reject (response error). Split into dedicated interceptor files. Create a clean `api.client.ts` that composes them.

## 1. Requirements & Constraints

- **REQ-001**: Each interceptor is a standalone file in `http/interceptors/`
- **REQ-002**: `api.client.ts` imports and registers all interceptors — no logic in the file itself
- **REQ-003**: Each interceptor can be individually tested
- **REQ-004**: `refresh-handler.ts` moves to `handlers/refresh-handler.ts`
- **REQ-005**: `parseApiError` utility stays in `utils/api.utils.ts` (not an interceptor concern)
- **REQ-006**: Zero behavioral changes — existing spec files must pass unchanged
- **CON-001**: Axios interceptor registration order matters: request (auth) → response success (camelCase) → response error (wrapper)
- **CON-002**: The 401 refresh logic stays coupled with the error wrapper (same concern)

## 2. Implementation Steps

### Phase 1: Create interceptor and handler files

- GOAL-001: Extract each concern into its own file

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-001 | Create `http/interceptors/auth.interceptor.ts` — extracts the request interceptor that reads `localStorage` token and sets `Authorization: Bearer` header | | |

```typescript
import type { InternalAxiosRequestConfig } from 'axios'

export function authInterceptor(config: InternalAxiosRequestConfig): InternalAxiosRequestConfig {
  const token = localStorage.getItem('accessToken')
  if (token && config.headers) {
    config.headers.Authorization = `Bearer ${token}`
  }
  return config
}
```

| TASK-002 | Create `http/interceptors/camelcase.interceptor.ts` — extracts the response success interceptor that converts snake_case keys to camelCase | | |

```typescript
import type { AxiosResponse } from 'axios'
import { toCamelCaseKeys } from '@/shared/mapper/mapper.utils'

export function camelCaseInterceptor(response: AxiosResponse): AxiosResponse {
  if (response.data && typeof response.data === 'object') {
    response.data = toCamelCaseKeys(response.data as Record<string, unknown>)
  }
  return response
}
```

| TASK-003 | Create `http/interceptors/error-wrapper.interceptor.ts` — extracts the response error interceptor. This is the largest piece. It handles: (1) never-reject wrapping into `ServerResult<null>`, (2) 401 detection → token refresh → retry, (3) auth-endpoint short-circuit | | |

```typescript
import { type AxiosError, type AxiosResponse, type InternalAxiosRequestConfig } from 'axios'
import type { ServerResult } from '../../types/result.types'
import { parseApiError } from '../../utils/api.utils'
import { refreshTokens } from '../handlers/refresh-handler'
import apiClient from '../api.client'

export async function errorWrapperInterceptor(error: AxiosError): Promise<AxiosResponse> {
  const originalRequest = error.config as InternalAxiosRequestConfig & { _retry?: boolean }
  const apiError = parseApiError(error)

  if (apiError.statusCode === 401 && originalRequest && !originalRequest._retry) {
    if (originalRequest.url?.includes('/auth/session/refresh')) {
      return Promise.resolve({
        data: {
          isSuccess: false, statusCode: 401,
          errors: [{ code: 'UNAUTHORIZED', message: apiError.detail || 'Unauthorized', type: 0, metadata: null }],
          message: apiError.title, metadata: null, value: null,
        } as ServerResult<null>,
      } as AxiosResponse)
    }

    originalRequest._retry = true
    const refreshed = await refreshTokens()
    if (refreshed) {
      const newToken = localStorage.getItem('accessToken')
      if (newToken && originalRequest.headers) {
        originalRequest.headers.Authorization = `Bearer ${newToken}`
      }
      return apiClient(originalRequest)
    }
  }

  return Promise.resolve({
    data: {
      isSuccess: false, statusCode: apiError.statusCode,
      errors: [{ code: apiError.errorCode || 'ERROR', message: apiError.detail || apiError.title || 'Request failed', type: 0, metadata: null }],
      message: apiError.title, metadata: null, value: null,
    } as ServerResult<null>,
  } as AxiosResponse)
}
```

| TASK-004 | Create `http/handlers/error-handler.ts` — extract `parseApiError` + `convertServerErrors` from `utils/api.utils.ts` into a handler. Update `utils/api.utils.ts` to re-export from handler (backward compat). | | |

### Phase 2: Refactor api.client.ts

- GOAL-002: Compose interceptors cleanly

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-005 | Rewrite `http/api.client.ts`: remove inline interceptor logic, import and register from interceptor files | | |

```typescript
import axios, { type AxiosInstance } from 'axios'
import { authInterceptor } from './interceptors/auth.interceptor'
import { camelCaseInterceptor } from './interceptors/camelcase.interceptor'
import { errorWrapperInterceptor } from './interceptors/error-wrapper.interceptor'

const apiBaseUrl = import.meta.env.VITE_API_URL
  ? `${import.meta.env.VITE_API_URL}/api`
  : '/api'

const apiClient: AxiosInstance = axios.create({
  baseURL: apiBaseUrl,
  headers: { 'Content-Type': 'application/json' },
  paramsSerializer: { indexes: null },
})

apiClient.interceptors.request.use(authInterceptor)
apiClient.interceptors.response.use(camelCaseInterceptor, errorWrapperInterceptor)

export default apiClient
```

| TASK-006 | Move `http/refresh-handler.ts` → `http/handlers/refresh-handler.ts` — update import in `error-wrapper.interceptor.ts` | | |

### Phase 3: Create interceptor tests

- GOAL-003: Test each interceptor in isolation

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-007 | Create `http/interceptors/__tests__/auth.interceptor.spec.ts` — test token injection, missing token, custom header handling | | |
| TASK-008 | Create `http/interceptors/__tests__/camelcase.interceptor.spec.ts` — test snake_case→camelCase conversion on response data | | |
| TASK-009 | Create `http/interceptors/__tests__/error-wrapper.interceptor.spec.ts` — test 401 refresh, never-reject wrapping, auth-endpoint short-circuit | | |

### Phase 4: Update spec file for api.client.ts

- GOAL-004: `api.client.spec.ts` should test interceptor composition, not individual interceptor logic

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-010 | Update `http/api.client.spec.ts` — simplify to test that axios instance is created with correct baseURL and that interceptors are registered | | |
| TASK-011 | Verify: `pnpm run type-check` — zero new errors | | |
| TASK-012 | Verify: `pnpm run test:unit` — all existing tests pass (including moved handlers) | | |

## 3. Alternatives

- **ALT-001**: Keep everything in `api.client.ts` — rejected, violates single-responsibility and makes testing harder
- **ALT-002**: Merge refresh-handler into error-wrapper — rejected, the refresh logic is reusable (used by auth store)
- **ALT-003**: Use a DI/interceptor registry pattern — overengineered for 3 interceptors

## 4. Dependencies

- **DEP-001**: `toCamelCaseKeys` in `shared/mapper/mapper.utils.ts` (unchanged)
- **DEP-002**: `parseApiError` in `utils/api.utils.ts` (unchanged, optionally extracted)
- **DEP-003**: `ServerResult` type in `types/result.types.ts` (unchanged)

## 5. Files

| File | Status |
|------|--------|
| `http/api.client.ts` | Rewritten — compose interceptors only |
| `http/interceptors/auth.interceptor.ts` | New — token injection |
| `http/interceptors/camelcase.interceptor.ts` | New — key conversion |
| `http/interceptors/error-wrapper.interceptor.ts` | New — error wrapping + refresh |
| `http/handlers/refresh-handler.ts` | Moved from `http/refresh-handler.ts` |
| `http/handlers/error-handler.ts` | New — extracted from utils |
| `http/interceptors/__tests__/auth.interceptor.spec.ts` | New |
| `http/interceptors/__tests__/camelcase.interceptor.spec.ts` | New |
| `http/interceptors/__tests__/error-wrapper.interceptor.spec.ts` | New |
| `utils/api.utils.ts` | May re-export from handler if extracted |

## 6. Testing

- **TEST-001**: Existing `api.client.spec.ts` passes (updated for composition)
- **TEST-002**: New interceptor spec files cover: token present/missing, camelCase conversion, 401 refresh success/failure, standard error wrapping

## 7. Risks & Assumptions

- **RISK-001**: Circular import: `error-wrapper.interceptor.ts` imports `apiClient` (for retry), and `api.client.ts` imports `error-wrapper.interceptor.ts`. **Mitigation**: Axios allows using the same instance inside its own interceptor — the retry call goes through the full chain again. This is the EXISTING behavior and must be preserved.
- **RISK-002**: The `refresh-handler.ts` move may break imports. **Verify**: check all files importing `refresh-handler` before/after.
- **ASSUMPTION-001**: The `createModuleApi` factory and `identity.api.ts` anomaly are out of scope for this plan.
