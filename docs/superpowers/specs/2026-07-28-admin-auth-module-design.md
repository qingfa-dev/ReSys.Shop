# Admin Auth Module — Design Spec

**Date**: 2026-07-28
**Status**: Approved
**Context**: Implementing the first real feature module — authentication — following the established feature scaffold structure. Uses the storefront auth API (`api/store/identity/auth/...`) since admin users share the same auth system, differentiated by roles/permissions.

---

## Architecture

The auth feature follows the co-located module pattern: all auth-related code lives in `features/auth/`. Layers interact vertically: views dispatch store actions, the store calls services (API + token), and types/validations are shared across layers.

```
View → Store Action → API Service → Backend
                ↘ Token Service (localStorage)
```

Dependencies:
- `features/auth/services/tokenService` — pure TS, depends on `STORAGE_KEYS` constants
- `features/auth/services/authApi` — depends on `shared/api/client` (axios get/post), `shared/types/result`
- `features/auth/stores/authStore` — depends on tokenService, authApi, shared types
- `features/auth/validations/auth` — depends on Zod
- `features/auth/views/*` — depends on store, validations, shared/ui components

---

## Files

| File | Purpose | Action |
|------|---------|--------|
| `features/auth/types/auth.ts` | Request/response types + AuthUser + SessionInfo | Create |
| `features/auth/types/index.ts` | Barrel export | Modify |
| `features/auth/validations/auth.ts` | Reusable field validators + grouped Zod schemas | Create |
| `features/auth/validations/index.ts` | Barrel export | Modify |
| `features/auth/services/tokenService.ts` | localStorage wrapper: get/set/clear tokens + expiry | Create |
| `features/auth/services/authApi.ts` | API functions: login, logout, getSession, forgotPassword, resetPassword | Create |
| `features/auth/services/index.ts` | Barrel export | Modify |
| `features/auth/stores/authStore.ts` | Pinia store: state, getters, login/logout/init/fetchSession actions | Create |
| `features/auth/stores/index.ts` | Barrel export | Modify |
| `features/auth/routes/index.ts` | auth routes: login, forgot-password, reset-password | Modify |
| `features/auth/views/LoginPage.vue` | Real login form (replaces placeholder) | Modify |
| `features/auth/views/ForgotPasswordPage.vue` | Email → request password reset | Create |
| `features/auth/views/ResetPasswordPage.vue` | Token + new password → reset | Create |
| `features/auth/views/index.ts` | Barrel export | Modify |
| `features/auth/index.ts` | Root barrel | Modify |
| `shared/api/interceptors/refresh.ts` | Delegate refresh to tokenService + fix URL | Modify |
| `shared/api/client.ts` | Remove direct localStorage access for tokens | Modify |
| `app/router/guards.ts` | Call authStore.init() before resolving routes | Modify |

---

## Types (`features/auth/types/auth.ts`)

```ts
// Request types
export interface LoginRequest {
  credential: string
  password: string
}

export interface ForgotPasswordRequest {
  email: string
}

export interface LogoutRequest {
  refreshToken?: string
  revokeAll?: boolean
}

export interface ResetPasswordRequest {
  email: string
  userId: string
  token: string
  newPassword: string
}

// Response types
export interface TokenPair {
  accessToken: string
  accessTokenExpiresIn: number
  refreshToken: string
  refreshTokenExpiresIn: number
}

export interface SessionInfo {
  id: string
  roles: string[]
  permissions: string[]
}

export interface AuthUser {
  userId: string
  roles: string[]
  permissions: string[]
  isAuthenticated: boolean
}
```

`AuthUser` is the store's in-memory model — adds `isAuthenticated` flag not present in the API response. `SessionInfo` is the raw API response from `GET /sessions`. The store maps `SessionInfo → AuthUser` after fetching session.

---

## Validation Schemas (`features/auth/validations/auth.ts`)

### Reusable field validators

Exportable individually for reuse across schemas and composables:

```ts
import { z } from 'zod'

export const emailField = z.string().min(1, 'Email is required').email('Invalid email address')
export const credentialField = z.string().min(1, 'Email or username is required')
export const passwordField = z.string().min(1, 'Password is required')
export const newPasswordField = z.string().min(8, 'Password must be at least 8 characters')
export const userIdField = z.string().min(1, 'User ID is required')
export const tokenField = z.string().min(1, 'Reset token is required')
```

### Grouped schemas

Composed from the field validators above:

```ts
export const loginSchema = z.object({
  credential: credentialField,
  password: passwordField,
})

export const forgotPasswordSchema = z.object({
  email: emailField,
})

export const resetPasswordSchema = z.object({
  email: emailField,
  userId: userIdField,
  token: tokenField,
  newPassword: newPasswordField,
})

// Inferred types
export type LoginFormValues = z.infer<typeof loginSchema>
export type ForgotPasswordFormValues = z.infer<typeof forgotPasswordSchema>
export type ResetPasswordFormValues = z.infer<typeof resetPasswordSchema>
```

---

## Token Service (`features/auth/services/tokenService.ts`)

Pure localStorage wrapper with no framework dependencies. Bridges the new store and the existing axios auth interceptor.

```ts
export function getAccessToken(): string | null
export function getRefreshToken(): string | null
export function setTokens(pair: TokenPair): void
export function clearTokens(): void
export function hasValidAccessToken(): boolean
```

- `getAccessToken` / `getRefreshToken` — read from `STORAGE_KEYS.ACCESS_TOKEN` / `STORAGE_KEYS.REFRESH_TOKEN`
- `setTokens` — stores both tokens via localStorage, also stores expiry timestamps at `STORAGE_KEYS.ACCESS_TOKEN + '_expires_at'` / `STORAGE_KEYS.REFRESH_TOKEN + '_expires_at'`
- `clearTokens` — removes all four localStorage keys
- `hasValidAccessToken` — returns true if access token exists AND its expiry timestamp is in the future; false otherwise

### Integration with interceptor

After login, the store calls `setTokenGetter(tokenService.getAccessToken)` to hook the existing axios auth interceptor. The interceptor's `_tokenGetter` defaults to reading `localStorage` directly — this replaces it with the token service, keeping the interceptor code unchanged.

---

## API Service (`features/auth/services/authApi.ts`)

All endpoints use the storefront namespace `api/store/identity`:

```ts
const AUTH_BASE = 'api/store/identity/auth'
const PASSWORD_BASE = 'api/store/identity/passwords'

export function login(request: LoginRequest): Promise<Result<TokenPair>>
export function logout(request?: LogoutRequest): Promise<Result<void>>
export function getSession(): Promise<Result<SessionInfo>>
export function forgotPassword(request: ForgotPasswordRequest): Promise<void>
export function resetPassword(request: ResetPasswordRequest): Promise<Result<void>>
```

| Function | Method | Path |
|----------|--------|------|
| `login` | POST | `${AUTH_BASE}/login/password` |
| `logout` | POST | `${AUTH_BASE}/logout` |
| `getSession` | GET | `${AUTH_BASE}/sessions` |
| `forgotPassword` | POST | `${PASSWORD_BASE}/forgot` |
| `resetPassword` | POST | `${PASSWORD_BASE}/reset` |

**Response unwrapping**: The backend wraps responses in `Result<T>` envelopes with data under `.value`. `login` extracts `TokenPair` from `response.value`. `getSession` extracts `SessionInfo` from `response.value`. `forgotPassword` returns `void` because the backend always returns 204 NoContent (prevents email enumeration). The existing axios camelCase interceptor handles snake_case → camelCase conversion.

**Bug fix — refresh URL**: The existing `shared/api/interceptors/refresh.ts` hardcodes `/api/identity/auth/sessions/refresh` (missing `/store` segment). This is corrected by creating a shared refresh function that uses the correct path.

---

## Pinia Store (`features/auth/stores/authStore.ts`)

```ts
// State
interface AuthState {
  user: AuthUser | null
  status: 'idle' | 'loading' | 'authenticated' | 'error'
  error: string | null
}

// Getters
isAuthenticated: boolean          // status === 'authenticated' && user !== null
currentUser: AuthUser | null      // the user object
hasRole(role: string): boolean    // user.roles?.includes(role) ?? false
hasPermission(perm: string): boolean // user.permissions?.includes(perm) ?? false

// Actions
login(credential: string, password: string): Promise<Result<void>>
logout(revokeAll?: boolean): Promise<void>
init(): Promise<void>
fetchSession(): Promise<Result<void>>
```

### Action flow — `login`
1. Set `status = 'loading'`, clear `error`
2. Call `authApi.login({ credential, password })`
3. On `Result.isSuccess`:
   a. `tokenService.setTokens(response.value)`
   b. `setTokenGetter(tokenService.getAccessToken)` — hook axios interceptor
   c. `await fetchSession()` — get user session
   d. Set `status = 'authenticated'`
   e. Return `Result.ok()`
4. On `Result.isFailure`:
   a. Set `status = 'error'`, `error = response.error.message`
   b. Return the failure Result

### Action flow — `init` (app startup)
1. If `!tokenService.hasValidAccessToken()`: set `status = 'idle'`, return
2. Call `authApi.getSession()` with existing token
3. On success: map `SessionInfo → AuthUser`, set `status = 'authenticated'`
4. On failure (token expired/invalid): `tokenService.clearTokens()`, set `status = 'idle'`
5. Does NOT redirect — the router guard handles that

### Action flow — `logout`
1. Call `authApi.logout({ revokeAll })` — fire-and-forget (ignore network errors)
2. `tokenService.clearTokens()`
3. Reset state: `{ user: null, status: 'idle', error: null }`

### Action flow — `fetchSession`
1. Call `authApi.getSession()`
2. On success: map to `AuthUser { userId: id, roles, permissions, isAuthenticated: true }`
3. On failure: return failure Result

---

## Routes (`features/auth/routes/index.ts`)

Three child routes under `/auth` (AuthLayout parent). All `requiresAuth: false` since they're pre-authentication pages.

```ts
export const authRoutes: RouteRecordRaw[] = [
  {
    path: 'login',
    name: 'login',
    component: () => import('../views/LoginPage.vue'),
    meta: { title: 'Sign In', requiresAuth: false },
  },
  {
    path: 'forgot-password',
    name: 'forgot-password',
    component: () => import('../views/ForgotPasswordPage.vue'),
    meta: { title: 'Forgot Password', requiresAuth: false },
  },
  {
    path: 'reset-password',
    name: 'reset-password',
    component: () => import('../views/ResetPasswordPage.vue'),
    meta: { title: 'Reset Password', requiresAuth: false },
  },
]
```

No menu items exported — auth pages don't appear in the sidebar.

---

## Views

All views use the `PageShell` shared component and are reactive-form-bound to validation schemas.

### LoginPage.vue

```
+----------------------------------+
| PageShell title="Sign In"        |
|                                  |
| [credential input................]|
| [password input.................]|
|                                  |
| [error alert — if authStore.error]|
|                                  |
| [Sign In button — loading spinner]|
|                                  |
| Forgot password? → link           |
+----------------------------------+
```

- Reactive form bound to `loginSchema` via `zodResolver`
- `credential` text input (autocomplete: "username")
- `password` input with `InputText` + toggle visibility via vee-validate `Field`
- Submit button: `Button` with severity "primary", loading state when `authStore.status === 'loading'`
- Error: `Message` component with severity "error" when `authStore.error` is truthy
- Link: `router-link` to `/auth/forgot-password`
- On success: `router.replace('/')` — navigate to dashboard

### ForgotPasswordPage.vue

```
+----------------------------------+
| ← Back to login                  |
| PageShell title="Forgot Password"|
|                                  |
| [email input....................]|
|                                  |
| [Send Reset Link button]         |
|                                  |
| Success: "If an account exists..."|
+----------------------------------+
```

- Back link: `router-link` to `/auth/login`
- Reactive form bound to `forgotPasswordSchema`
- `email` text input (autocomplete: "email")
- Submit calls `authApi.forgotPassword()` directly (no store action needed — stateless)
- Loading state: local `ref<boolean>` (not in store)
- Success state: local `ref<boolean>`, shows message "If an account exists with that email, a reset link has been sent"
- Error: network failure only (backend always returns 204)

### ResetPasswordPage.vue

```
+----------------------------------+
| PageShell title="Set New Password"|
|                                  |
| [email — readonly, from query]   |
| [userId — readonly, from query]  |
| [token input....................]|
| [newPassword input..............]|
|                                  |
| [error alert — token invalid]    |
|                                  |
| [Reset Password button]          |
|                                  |
| Success: toast → redirect to login|
+----------------------------------+
```

- Route query params: `?userId=...&email=...&token=...` (from email reset link)
- Reactive form bound to `resetPasswordSchema`
- `email`, `userId` fields: pre-filled from `route.query`, readonly/disabled
- `token` text input
- `newPassword` input with toggle visibility
- Submit calls `authApi.resetPassword()`
- Loading state: local `ref<boolean>`
- Error: "Invalid or expired reset link" message
- Success: `useToast().add({ severity: 'success', summary: 'Password reset successful' })`, then `router.push('/auth/login')`

---

## Cross-Cutting Changes

### Fix refresh interceptor URL
`shared/api/interceptors/refresh.ts` hardcodes `/api/identity/auth/sessions/refresh` — missing the `/store` segment. Correct the URL to `/api/store/identity/auth/sessions/refresh`.

### Remove direct localStorage writes from client.ts
`shared/api/client.ts:8-14` (`setAuthToken` function) directly writes to `localStorage.setItem('accessToken', ...)`. Replace with calls to `tokenService.setTokens()` and `tokenService.clearTokens()`.

### Wire init into router guard
`app/router/guards.ts` currently has a commented-out `beforeEach`. When the guard is re-enabled:
1. On first navigation: `await authStore.init()` to restore session from stored tokens
2. Then check `authStore.isAuthenticated` before granting access to `requiresAuth` routes
3. State machine handles case where init is called multiple times (idempotent)

---

## Testing Strategy

| Layer | Approach | Key cases |
|-------|----------|-----------|
| `types/` | No tests | Type-only definitions |
| `validations/` | Unit (Vitest) | Each field: valid input passes, invalid input fails with correct message. Each schema: pass with valid data, reject with errors. Inferred types compile correctly. |
| `tokenService` | Unit (Vitest) | Mock `localStorage`: setTokens writes 4 keys, clearTokens removes them, getAccessToken returns null for missing key, hasValidAccessToken false for expired token. |
| `authApi` | Unit (Vitest) | Mock `getApiClient`: correct URL and method for each endpoint, correct body serialization, extracts `.value` from Result envelope, error propagation on rejection. |
| `authStore` | Unit (Vitest + Pinia testing) | Mock services: login success sets tokens + user, login failure sets error state, logout clears state + tokens, init with valid token fetches session, init with invalid token stays idle, getters reflect state. |
| `views/` | Component (Vitest + Vue Test Utils) | Mount with `createTestingPinia`: form validation errors on blur, submit dispatches store action, loading spinner visible during `status === 'loading'`, error alert visible when `error` is set, success redirects. Navigation links point to correct routes. |

---

## Dependencies

| Dependency | Source | Purpose |
|------------|--------|---------|
| Zod | Already in codebase | Validation schemas |
| Pinia | Already in codebase | State management |
| Vue Router | Already in codebase | Navigation, route query params |
| Axios | Already in codebase (via shared/api/client) | HTTP requests |
| PrimeVue (Button, InputText, Message, Toast) | Already in codebase | UI components |
| PageShell | `shared/components/ui/PageShell.vue` | Page layout wrapper |
