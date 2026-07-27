# Axios API Client Layer

## Problem

The Admin SPA currently uses a raw `fetch()`-based API client (`shared/api/client.ts`). It lacks:
- Automatic token refresh on 401 (with concurrent request queue)
- Response camelCase transformation (API returns snake_case)
- Structured error normalization
- Toast notification integration for errors

Axios (`^1.18.1`) is already a dependency but unused.

## Solution

Replace the fetch-based client with an axios-based one, preserving the same public API signatures (`get<T>`, `post<T>`, `put<T>`, `patch<T>`, `del<T>`, `getPaged<T>`, `HttpError`). Add interceptors for auth, camelCase, and error handling. Add a `useApiErrorHandler` composable for toast notifications.

## Architecture

```
Component / Store
   │
   ├─ usePagedQuery<T>(url)           ← unchanged
   │    └─ getPaged<T>(url, params)   ← minor: axios cancel passthrough
   │         └─ get<T>(url, signal)   ← rewired to axios
   │
   ├─ useApiErrorHandler()            ← NEW
   │    └─ handleError(error) / handleResult(result)
   │         └─ PrimeVue useToast()
   │
   └─ get<T>, post<T>, put<T>...      ← client.ts (axios-based, same signatures)
        └─ apiClient (axios instance)
             ├─ request: auth.ts       ← reads token from localStorage getter
             └─ response:
                  ├─ camelcase.ts       ← snake_case → camelCase on data
                  └─ error.ts           ← HttpError normalization + 401 refresh
                       └─ refresh.ts    ← queue concurrent 401s, refresh once, retry
```

## Files

### New files
- `shared/api/axios.ts` — `createApiClient()`: AxiosInstance singleton
- `shared/api/types.ts` — shared constants (refresh endpoint, etc.)
- `shared/api/interceptors/auth.ts` — request interceptor
- `shared/api/interceptors/camelcase.ts` — response interceptor
- `shared/api/interceptors/error.ts` — response error interceptor
- `shared/api/interceptors/refresh.ts` — token refresh queue + retry
- `shared/composables/useApiErrorHandler.ts` — toast-only error handler

### Rewritten files
- `shared/api/client.ts` — replace fetch with axios wrappers

### Minor updates
- `shared/api/paged.ts` — add `axios.isCancel(e)` passthrough
- `shared/api/index.ts` — update exports
- `shared/composables/index.ts` — add useApiErrorHandler export

### Test files
- `shared/api/__tests__/client.spec.ts` — rewrite for axios
- `shared/api/__tests__/paged.spec.ts` — add cancel test
- `shared/api/__tests__/auth.spec.ts` — new
- `shared/api/__tests__/camelcase.spec.ts` — new
- `shared/api/__tests__/error.spec.ts` — new
- `shared/api/__tests__/refresh.spec.ts` — new
- `shared/composables/__tests__/useApiErrorHandler.spec.ts` — new

## Key Interfaces

### HttpError (unchanged)
```typescript
class HttpError extends Error {
  statusCode: number
  errors: Array<{ code: string; message: string }>
}
```

### client.ts public API (same signatures)
```typescript
get<T>(url: string, signal?: AbortSignal): Promise<T>
post<T>(url: string, body?: unknown, signal?: AbortSignal): Promise<T>
put<T>(url: string, body?: unknown, signal?: AbortSignal): Promise<T>
patch<T>(url: string, body?: unknown, signal?: AbortSignal): Promise<T>
del<T>(url: string, signal?: AbortSignal): Promise<T>
setBaseUrl(url: string): void
setAuthToken(token: string | null): void
```

## Interceptor Details

### auth.ts (request)
- Reads token via `_tokenGetter()` (default: `localStorage.getItem('auth_token')`)
- Sets `Authorization: Bearer <token>`
- Skips if no token or URL is in whitelist (login, refresh)

### camelcase.ts (response fulfilled)
- Recursively transforms all object keys: `snake_case` → `camelCase`
- Handles nested objects and arrays of objects
- Skips non-object responses, null, primitives

### error.ts (response rejected)
- `CanceledError` → re-throw (passthrough)
- 401 + has refresh token → delegate to `refresh.ts`
- axios error with response → extract `errors[]` from body, wrap as `HttpError`
- axios error without response (network) → `HttpError` with `NetworkError` code
- Unknown error → `HttpError` with `Unexpected` code

### refresh.ts
- Mutex flag `isRefreshing` prevents concurrent refresh calls
- Queue of pending requests waiting for refresh to complete
- On refresh success: store new tokens, retry original request (return response), resolve queue
- On refresh failure: clear tokens, reject queue, redirect to `/login`
- Refresh URL: `POST /api/identity/auth/refresh` with `{ refreshToken }` body

## Composable

### useApiErrorHandler
```typescript
useApiErrorHandler(): {
  handleError(error: unknown): void
  handleResult(result: Result<unknown> | PagedResult<unknown>): void
}
```
- `handleError`: accepts `HttpError`, `Error`, or string → shows PrimeVue toast
  - 4xx → severity `warn`, 5xx → severity `error`
- `handleResult`: if `!isSuccess` → shows toast from errors
- Life: 5000ms, closable

## Test Strategy

| Test file | What |
|---|---|
| `client.spec.ts` | Methods call axios with correct args, response.data unwrapped, HttpError propagation |
| `auth.spec.ts` | Token from localStorage → header set, no token → no header, getter override |
| `camelcase.spec.ts` | `first_name` → `firstName`, nested objects, arrays, null safety |
| `error.spec.ts` | Network error → HttpError, 4xx/5xx → HttpError, Cancel passthrough, refresh delegation |
| `refresh.spec.ts` | Single POST for concurrent 401s, retry original request, redirect on failure |
| `paged.spec.ts` | Cancel passthrough through getPaged (one new test) |
| `useApiErrorHandler.spec.ts` | Toast on HttpError, toast on failed Result, no toast on success |

## Coverage

All new files fall under the existing `src/shared/api/**` and `src/shared/composables/**` coverage includes in vitest.config.ts. Thresholds remain 65% statements/branches/functions/lines.

## Backward Compatibility

- All `client.ts` function signatures identical — zero changes needed in consumers
- `HttpError` class and behavior identical
- `getPaged` behavior identical (adds only cancel passthrough)
- `usePagedQuery` unchanged — continues to work via `getPaged`
