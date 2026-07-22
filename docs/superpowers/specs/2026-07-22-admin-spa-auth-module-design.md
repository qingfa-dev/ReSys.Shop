# Admin SPA — Auth Feature Module Design

**Date:** 2026-07-22
**Status:** Draft
**Branch:** `feature/implement-admin-panel`

## Overview

Add a self-contained `features/auth/` module to the Admin SPA. The module wraps the existing shared auth infrastructure (`TokenService`, `AuthService`, `useSessionStore`, interceptors) and adds what's missing: typed API layer, Pinia auth store with form-action state, Zod validation schemas, page components, and route definitions.

Zero changes to `shared/auth/`, `shared/api/interceptors/`, `router/guards.ts`, or the API client.

## Architecture

```
features/auth/
  components/   → Vue SFCs (LoginForm, RegisterForm, etc.)
  store/        → useAuthStore (Pinia setup store)
  api/          → typed wrappers over AuthService + direct REST calls
  composables/  → useAuth() — single facade for page components
  types/        → request/response DTOs matching backend contracts
  models/       → Zod validation schemas
  routes.ts     → auth route definitions
  index.ts      → barrel: exports routes, store, schemas

↑ imports from ↑
  shared/auth/        (AuthService, TokenService, permissions)
  shared/api/         (apiClient for auth endpoints)
  shared/constants/   (ROUTES, API module names)
  shared/models/      (Result<T>, ApiProblemDetail)
  stores/             (useSessionStore — session persistence)

↓ consumed by ↓
  router/index.ts     (wires auth routes)
  router/guards.ts    (redirects unauthenticated to /login)
  app/layout/         (AuthLayout for public pages)
```

**Key rule:** the feature module *imports from* shared; shared never imports from features.

## Route Topology

Auth pages (login, register, forgot/reset password) are standalone top-level routes — each page wraps itself with `<AuthLayout>` (centered card, no sidebar/topbar/breadcrumb). Change-password requires a session so it lives as a child route under MainLayout.

```
/                          → (standalone, page wraps AuthLayout)
├── /login                → LoginPage
├── /register             → RegisterPage
├── /forgot-password      → ForgotPasswordPage
└── /reset-password       → ResetPasswordPage

/                          → MainLayout (authenticated)
├── /dashboard             → (existing)
├── /account
│   └── /change-password  → ChangePasswordPage
├── /catalog/...           → (existing)
└── ...
```

`AuthLayout` (`shared/components/layout/AuthLayout.vue`) is a centered white card with the app logo at top, responsive. Used inside each auth page, not as a route-level wrapper. This avoids conflicting with MainLayout's `path: '/'` and requires zero changes to `App.vue`.

## Data Flow (Login)

```
LoginForm.vue
  │  user fills credential + password, clicks submit
  ▼
useAuth().login(credential, password)
  │  Zod validates → sets isLoading=true in store
  ▼
auth.api.ts: login(credential, password)
  │  POST /api/store/identity/auth/login/password
  │  via the same apiClient (interceptors apply)
  ▼
AuthService.login() ← shared/
  │  success: TokenService.setTokens(tokens)
  │          useSessionStore.setUser(decoded)
  │  error:   error interceptor wraps into Result<T>.failure
  ▼
useAuthStore
  │  success: isAuthenticated→true, router.push(redirect)
  │  error:   fieldErrors + serverErrors populated
  ▼
LoginForm.vue
  │  shows inline field errors + toast for server errors
```

Registration, forgot-password, and reset-password follow the same pattern but don't persist tokens (register may auto-login after success).

## Store: `features/auth/store/auth.store.ts`

Single-file Pinia Setup store. Two state groups:

| State | Purpose | Lifecycle |
|-------|---------|-----------|
| Session (via `useSessionStore`) | user, roles, permissions | persists across navigation |
| Form action state | isLoading, serverErrors, fieldErrors | resets to initial on each action |

```typescript
export const useAuthStore = defineStore('auth', () => {
  const session = useSessionStore()
  const router = useRouter()

  const isLoading = ref(false)
  const serverErrors = ref<ApiProblemDetail[]>([])
  const fieldErrors = ref<Record<string, string[]>>({})

  function resetFormState() {
    isLoading.value = false
    serverErrors.value = []
    fieldErrors.value = {}
  }

  async function login(credential: string, password: string) {
    resetFormState()
    isLoading.value = true
    const result = await loginApi(credential, password)
    if (result.isSuccess) {
      session.setUser(fromTokenPayload(TokenService.getAccessTokenPayload()))
      const redirect = router.currentRoute.value.query.redirect as string ?? '/'
      router.push(redirect)
    } else {
      mapErrors(result, fieldErrors, serverErrors)
    }
    isLoading.value = false
  }

  // register, forgotPassword, resetPassword, changePassword, logout, initialize
  // follow the same pattern

  return {
    isLoading, serverErrors, fieldErrors,
    login, register, forgotPassword, resetPassword, changePassword, logout, initialize,
    isAuthenticated: session.isAuthenticated,
    currentUser: session.user,
  }
})
```

**Key decisions:**
- `initialize()` is called once at app mount in `main.ts`, after the router + Pinia are installed but before mounting the app. It reads the stored token via `TokenService.getAccessToken()`, checks expiry via `TokenService.isAccessTokenExpired()`, and if valid, calls `getSessionApi()` (GET `/me`) to hydrate `useSessionStore`. If the token is expired or the session call fails, tokens are cleared silently — the guard will redirect to `/login` on the first navigation.
- `logout()` calls `logoutApi()` with the stored refresh token, clears `useSessionStore` + `TokenService.clearTokens()`, and redirects to `/login`.
- `changePassword()` posts to `/api/store/identity/passwords/change` using the existing session token from the interceptor.
- `mapErrors()` is a pure helper in `auth.store.ts`: takes `Result.failure` and maps `ApiProblemDetail[]` errors by code prefix to `fieldErrors` (e.g. `User.Email.Duplicate` → `fieldErrors.email`) and un-mapped errors to `serverErrors` (shown as toast). Error codes follow the backend pattern: `User.{Field}.{Reason}` or `User.{Reason}`.
- `fromTokenPayload()` maps the decoded JWT claims `{ sub, email, name, role, permissions }` to `useSessionStore`'s `User` shape `{ id, email, name, role, permissions }`.

## API Layer: `features/auth/api/auth.api.ts`

Thin typed wrappers. All use the shared `apiClient` (axios instance with interceptors) so auth headers, camelCase transform, error wrapping, and token refresh all apply transparently.

```typescript
import { apiClient } from '@/shared/api'

export async function loginApi(credential: string, password: string): Promise<Result<TokenResponse>>
export async function registerApi(fields: RegisterRequest): Promise<Result<RegisterResponse>>
export async function forgotPasswordApi(email: string): Promise<Result<void>>
export async function resetPasswordApi(params: ResetPasswordRequest): Promise<Result<void>>
export async function changePasswordApi(params: ChangePasswordRequest): Promise<Result<void>>
export async function logoutApi(refreshToken: string): Promise<Result<void>>
export async function getSessionApi(): Promise<Result<SessionResponse>>
```

**Endpoint mapping** (from backend `Identity.Feature.cs`):

| API function | Endpoint | Existing in AuthService? |
|---|---|---|
| `loginApi` | `POST /api/store/identity/auth/login/password` | Yes — reuse |
| `registerApi` | `POST /api/store/identity/auth/register` | No |
| `forgotPasswordApi` | `POST /api/store/identity/passwords/forgot` | No |
| `resetPasswordApi` | `POST /api/store/identity/passwords/reset` | No |
| `changePasswordApi` | `POST /api/store/identity/passwords/change` | No |
| `logoutApi` | `POST /api/store/identity/auth/logout` | Yes — reuse |
| `getSessionApi` | `GET /api/store/identity/auth/sessions` | Yes — reuse |

For endpoints already in `AuthService`, `auth.api.ts` delegates. For new endpoints, it calls `apiClient` directly with typed params.

## Types: `features/auth/types/`

Request and response DTOs matching the backend contracts exactly (camelCase — the interceptor handles transform):

```typescript
// Requests
interface LoginRequest { credential: string; password: string }
interface RegisterRequest {
  email: string; userName: string; password: string
  firstName: string; lastName?: string; phone?: string; acceptTerm: boolean
}
interface ForgotPasswordRequest { email: string }
interface ResetPasswordRequest { email: string; userId: string; token: string; newPassword: string }
interface ChangePasswordRequest { email: string; currentPassword: string; newPassword: string }

// Responses
interface TokenResponse {
  accessToken: string; accessTokenExpiresIn: number
  refreshToken: string; refreshTokenExpiresIn: number
}
interface RegisterResponse { userId: string; email: string; message: string }
interface SessionResponse { id: string; roles: string[]; permissions: string[] }

// Form shapes (separate from API shapes — used by components)
interface LoginForm { credential: string; password: string }
interface RegisterForm { email: string; userName: string; password: string; confirmPassword: string; firstName: string; lastName: string; phone: string; acceptTerm: boolean }
interface ForgotPasswordForm { email: string }
interface ResetPasswordForm { password: string; confirmPassword: string }
interface ChangePasswordForm { currentPassword: string; newPassword: string; confirmPassword: string }
```

## Models: `features/auth/models/`

Zod validation schemas (one per form), using `vue-i18n` `t()` for error messages:

```typescript
// Zod schemas
export const loginSchema = (t: TFunction) => z.object({
  credential: z.string().min(1, t('auth.validation.credentialRequired')),
  password: z.string().min(1, t('auth.validation.passwordRequired')),
})

export const registerSchema = (t: TFunction) => z.object({
  email: z.string().email(t('auth.validation.invalidEmail')),
  userName: z.string().min(3).max(50),
  password: z.string()
    .min(8, t('auth.validation.passwordMinLength'))
    .regex(/[A-Z]/, t('auth.validation.passwordUppercase'))
    .regex(/[a-z]/, t('auth.validation.passwordLowercase'))
    .regex(/[0-9]/, t('auth.validation.passwordDigit'))
    .regex(/[^A-Za-z0-9]/, t('auth.validation.passwordSpecial')),
  confirmPassword: z.string(),
  firstName: z.string().min(1),
  lastName: z.string().optional(),
  phone: z.string().optional(),
  acceptTerm: z.literal(true, { errorMap: () => ({ message: t('auth.validation.acceptTerms') }) }),
}).refine(data => data.password === data.confirmPassword, {
  message: t('auth.validation.passwordMismatch'),
  path: ['confirmPassword'],
})

// Similar for forgotPasswordSchema, resetPasswordSchema, changePasswordSchema
```

## Composables: `features/auth/composables/useAuth.ts`

Single facade for page components. Wraps the store and exposes reactive state + actions + lifecycle hooks.

```typescript
export function useAuth() {
  const store = useAuthStore()
  const { t } = useI18n()

  return {
    // Reactive state (via storeToRefs)
    isLoading: computed(() => store.isLoading),
    isAuthenticated: computed(() => store.isAuthenticated),
    serverErrors: computed(() => store.serverErrors),
    fieldErrors: computed(() => store.fieldErrors),
    currentUser: computed(() => store.currentUser),

    // Actions
    login: (credential: string, password: string) => store.login(credential, password),
    register: (fields: RegisterRequest) => store.register(fields),
    forgotPassword: (email: string) => store.forgotPassword(email),
    resetPassword: (params: ResetPasswordRequest) => store.resetPassword(params),
    changePassword: (params: ChangePasswordRequest) => store.changePassword(params),
    logout: () => store.logout(),

    // Schemas (with i18n wired in)
    loginSchema: loginSchema(t),
    registerSchema: registerSchema(t),
    forgotPasswordSchema: forgotPasswordSchema(t),
    resetPasswordSchema: resetPasswordSchema(t),
    changePasswordSchema: changePasswordSchema(t),

    // Lifecycle
    initialize: () => store.initialize(),
  }
}
```

## Routes: `features/auth/routes.ts`

Auth routes are standalone (no parent `path: '/'` wrapper — that would conflict with MainLayout). Each auth page includes `<AuthLayout>` as its own wrapper. In `router/index.ts`, `authRoutes` are spread as siblings of the MainLayout route.

```typescript
import type { RouteRecordRaw } from 'vue-router'

export const authRoutes: RouteRecordRaw[] = [
  { path: '/login', name: 'auth.login', component: () => import('@/features/auth/pages/LoginPage.vue') },
  { path: '/register', name: 'auth.register', component: () => import('@/features/auth/pages/RegisterPage.vue') },
  { path: '/forgot-password', name: 'auth.forgotPassword', component: () => import('@/features/auth/pages/ForgotPasswordPage.vue') },
  { path: '/reset-password', name: 'auth.resetPassword', component: () => import('@/features/auth/pages/ResetPasswordPage.vue') },
]

// Change-password is a child route under existing MainLayout (driver is logged in).
// Since MainLayout wraps `/`, this route is added to MainLayout's `children` array:
export const changePasswordRoute: RouteRecordRaw = {
  path: '/account/change-password',
  name: 'auth.changePassword',
  component: () => import('@/features/auth/pages/ChangePasswordPage.vue'),
}
```

**Router registration** in `router/index.ts`:

```typescript
import { authRoutes, changePasswordRoute } from '@/features/auth'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    ...authRoutes,    // standalone — no layout wrapper, each page self-wraps with AuthLayout
    {
      path: '/',
      component: MainLayout,
      children: [
        { path: '', redirect: { name: 'reports.dashboard' } },
        changePasswordRoute,
        profileRoutes, reportsRoutes, catalogRoutes, // ...existing
      ],
    },
  ],
})
```

## Index: `features/auth/index.ts`

```typescript
export { authRoutes } from './routes'
export { useAuthStore } from './store/auth.store'
export { useAuth } from './composables/useAuth'
export * from './types'
export * from './models'
```

## Files to Create

| File | Type | Effort |
|------|------|--------|
| `features/auth/types/index.ts` | Type definitions | Small |
| `features/auth/models/index.ts` | Zod schemas | Medium |
| `features/auth/api/auth.api.ts` | API wrappers | Medium |
| `features/auth/store/auth.store.ts` | Pinia store | Medium |
| `features/auth/composables/useAuth.ts` | Facade composable | Small |
| `features/auth/routes.ts` | Route definitions | Small |
| `features/auth/index.ts` | Barrel | Trivial |
| `shared/components/layout/AuthLayout.vue` | Auth page layout | Small |
| `features/auth/pages/LoginPage.vue` | Login page — composes AuthLayout + LoginForm | Medium |
| `features/auth/pages/RegisterPage.vue` | Register page — composes AuthLayout + RegisterForm | Medium |
| `features/auth/pages/ForgotPasswordPage.vue` | Forgot password page — composes AuthLayout + forgot form | Small |
| `features/auth/pages/ResetPasswordPage.vue` | Reset password page — composes AuthLayout + reset form | Small |
| `features/auth/pages/ChangePasswordPage.vue` | Change password page — uses MainLayout | Small |
| `features/auth/components/LoginForm.vue` | Login form with VeeValidate + Zod | Small |
| `features/auth/components/RegisterForm.vue` | Register form with VeeValidate + Zod | Small |
| `features/auth/components/SocialLogin.vue` | Social login — placeholder stub | Trivial |
| `features/auth/components/PasswordStrength.vue` | Password strength indicator | Small |

**PasswordStrength** accepts a `password` string prop and shows a real-time strength bar + rules checklist as the user types. It checks against the same 5 rules as the Zod schema (min 8 chars, uppercase, lowercase, digit, special char). Each rule shows a check/cross icon and turns green when satisfied. The strength bar has 5 segments (0-2 rules: weak/red, 3-4: medium/yellow, 5: strong/green). Used by both `RegisterForm` and `ResetPasswordForm`.

## Files to Modify

| File | Change | Reason |
|------|--------|--------|
| `router/index.ts` | Import and add `authRoutes` at top level, `changePasswordRoute` to MainLayout children | Wire auth routes |
| `main.ts` | Call `useAuthStore().initialize()` after `app.use(router)` and `app.use(createPinia())`, before `app.mount('#app')` | Hydrate session on app load |
| `shared/constants/routes.ts` | Add `LOGIN: '/login'` if missing | Reference for guard + interceptor |
| `shared/localization/messages/en/auth.json` | Add i18n keys for validation messages | Error messages in forms |

## Files to Remove

None.

## Dependencies

Already installed and used in the project:
- `zod` + `@vee-validate/zod` — form schemas (used in legacy admin)
- `vee-validate` — form state management
- `vue-i18n` — internationalization (auth.json already exists)
- `jwt-decode` — token decoding (used by TokenService)
- `pinia` — state management (useSessionStore exists)
- `axios` — HTTP client (apiClient exists)

No new dependencies.

## Non-Goals

- Social login UI (placeholder only — external OAuth endpoint exists but UX is out of scope)
- Email verification UI (endpoint exists but flow requires server-side email delivery)
- Permission-based menu filtering (separate task)
- Removing the legacy admin apps (separate task)
- Role management UI (already stubbed in `features/users/pages/`)
- `rememberMe` checkbox (can be added later)
- Progressive profiling after initial registration

## Quality Gates

- `cd app/Admin && pnpm run lint` — 0 errors
- `cd app/Admin && pnpm run build` — clean
- `cd app/Admin && pnpm run test:unit` — all existing 60 tests pass, new tests added for store + API
- Router guard activated: unauthenticated users redirected to `/login`
- Login completes, stores tokens, hydrates session, redirects to dashboard
