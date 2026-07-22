---
title: Admin SPA — Auth Feature Module
version: 1.0
date_created: 2026-07-22
owner: Admin SPA team
tags: [design, app, admin-spa, vue, primevue, auth, identity]
---

# Introduction

Add a self-contained `features/auth/` module to the Admin SPA providing login,
registration, password reset, and session management. The module wraps the
existing shared auth infrastructure (`TokenService`, `AuthService`,
`useSessionStore`, interceptors) and adds what is missing: typed API layer,
Pinia auth store with form-action state, Zod + VeeValidate form schemas, page
components, and route definitions.

Zero changes to `shared/auth/`, `shared/api/interceptors/`, `router/guards.ts`,
or the API client.

## 1. Purpose & Scope

**Purpose:** Define the architecture, data contracts, UI patterns, and
implementation requirements for the Admin SPA auth feature module.

**Scope:** All authentication and session management pages accessible from the
Admin SPA — login, register, forgot/reset password, change password. Session
initialization at app boot. Activation of the existing router auth guard.

**Audience:** Frontend developers implementing the auth module.

**Assumptions:**
- Backend Identity endpoints already exist (13 Store endpoints under
  `/api/store/identity/`)
- Shared auth infrastructure (`TokenService`, `AuthService`, interceptors,
  `useSessionStore`) is already implemented in `shared/auth/` and `stores/`
- PrimeVue v5, Tailwind CSS, and the existing `AdminPreset` (Aura-based
  Indigo theme) are used
- `vee-validate` + `@vee-validate/zod` are available (used in legacy admin)
- No new npm dependencies are needed

## 2. Definitions

- **Auth page** — A standalone route page that does NOT render inside
  `MainLayout`. It self-contains a centered card layout (no sidebar, topbar,
  breadcrumb). Pattern derived from [Sakai Vue reference](#12-related-specifications--further-reading).
- **Session state** — Persistent per-user data stored in `useSessionStore`
  (Pinia): `{ id, email, name, role, permissions }`. Survives navigation.
- **Form action state** — Transient per-action state stored in `useAuthStore`
  (Pinia): `{ isLoading, serverErrors, fieldErrors }`. Resets between actions.
- **Token-based Tailwind class** — A Tailwind utility class whose value derives
  from the PrimeVue theme design tokens via the `tailwindcss-primeui` plugin
  (e.g. `bg-surface-0`, `text-primary`, `text-muted-color`). See
  [PrimeVue Tailwind docs](#12-related-specifications--further-reading).
- **API client** — The shared axios instance at `shared/api/client.ts` with
  preconfigured interceptors (auth header, camelCase transform, error wrapping,
  silent token refresh).
- **Result\<T\>** — The backend response envelope unwrapped by the error
  interceptor: `{ isSuccess: boolean, statusCode: number, value: T, errors:
  ApiProblemDetail[], message: string }`.

## 3. Requirements, Constraints & Guidelines

### Architecture

- **REQ-001**: The `features/auth/` module SHALL import from `shared/` only;
  shared MUST NOT import from features.
- **REQ-002**: No changes to existing interceptors, `TokenService`, or
  `AuthService`. Feature API layer wraps them.
- **REQ-003**: Auth pages are standalone top-level routes, not children of
  `MainLayout`. Each page wraps itself with `<AuthLayout>`.
- **REQ-004**: The change-password route is a child of `MainLayout` since the
  user is authenticated.

### Store

- **REQ-005**: `useAuthStore` SHALL be a single-file Pinia Setup store (not
  split into actions/getters files).
- **REQ-006**: Form action state (`isLoading`, `serverErrors`, `fieldErrors`)
  SHALL reset to initial values on each new action call.
- **REQ-007**: `initialize()` SHALL be called once at app boot (in `main.ts`
  after router + Pinia, before mount) to hydrate session from stored token.
- **REQ-008**: `logout()` SHALL clear `useSessionStore`, clear tokens via
  `TokenService.clearTokens()`, call the server logout endpoint, and redirect
  to `/login`.

### Forms & Validation

- **REQ-009**: Form validation SHALL use `vee-validate` + Zod schemas with
  `vue-i18n` for error messages.
- **REQ-010**: Password rules: minimum 8 characters, at least one uppercase,
  one lowercase, one digit, one special character.
- **REQ-011**: `PasswordStrength` component SHALL show real-time strength bar
  + rules checklist as the user types, matching the Zod schema rules.

### Router Guard

- **REQ-012**: The existing `registerAuthGuard` scaffolding SHALL be activated:
  unauthenticated users navigating to any non-auth route are redirected to
  `/login?redirect=<original_path>`. Authenticated users on `/login` are
  redirected to `/`.

### UI Design System

- **PAT-001**: Auth page UI SHALL follow the [Sakai Vue](#12-related-specifications--further-reading)
  pattern: full-screen centered card, gradient border on the card wrapper,
  `FloatingConfigurator` for dark mode toggle in top-right corner, no topbar
  or sidebar.
- **PAT-002**: Use Token-based Tailwind classes from `tailwindcss-primeui`
  for colors and surfaces (`bg-surface-0`, `bg-surface-50`, `text-primary`,
  `text-muted-color`, `text-surface-900`, etc.) with `dark:` variants.
- **PAT-003**: PrimeVue components used in forms: `InputText`, `Password`
  (with `:toggleMask="true"`, `:feedback="false"`), `Checkbox`, `Button`.
- **PAT-004**: Component imports SHALL use the existing auto-import pattern
  (PrimeVue components are globally available via `unplugin-vue-components`).
- **PAT-005**: The existing `AdminPreset` (Aura-based Indigo theme) SHALL
  remain the single source of truth for design tokens. No new presets.
- **PAT-006**: Dark mode SHALL use the existing `.app-dark` selector
  mechanism and `useDarkMode()` composable. Auth pages respect the current
  dark mode state.

### Constraints

- **CON-001**: No new npm dependencies.
- **CON-002**: Feature module SHALL NOT reference other feature modules.
- **CON-003**: The `ApiProblemDetail` error code format from the backend is
  `{code}: string, {message}: string, {type}: int` — store error mapping
  must parse the dot-separated code prefix.

## 4. Interfaces & Data Contracts

### 4.1 Module Structure

```
features/auth/
├── api/
│   └── auth.api.ts            # Typed wrappers over apiClient
├── components/
│   ├── LoginForm.vue           # VeeValidate + Zod login form
│   ├── RegisterForm.vue        # VeeValidate + Zod register form
│   ├── SocialLogin.vue         # Placeholder stub
│   └── PasswordStrength.vue    # Real-time password strength indicator
├── composables/
│   └── useAuth.ts              # Single facade for page components
├── models/
│   └── index.ts                # Zod validation schemas (login, register, etc.)
├── pages/
│   ├── LoginPage.vue           # Login page
│   ├── RegisterPage.vue        # Register page
│   ├── ForgotPasswordPage.vue  # Forgot password page
│   ├── ResetPasswordPage.vue   # Reset password page
│   └── ChangePasswordPage.vue  # Change password page (under MainLayout)
├── store/
│   └── auth.store.ts           # useAuthStore (Pinia Setup store)
├── types/
│   └── index.ts                # Request/response DTOs + form shapes
├── routes.ts                   # Route definitions
└── index.ts                    # Barrel export
```

### 4.2 API Endpoints

All endpoints delegate to the shared `apiClient` (axios instance with
interceptors). The error interceptor already wraps responses into `Result<T>`.

| Function | Method | Endpoint | Reuses AuthService? |
|---|---|---|---|
| `loginApi` | POST | `/api/store/identity/auth/login/password` | Yes |
| `registerApi` | POST | `/api/store/identity/auth/register` | No |
| `forgotPasswordApi` | POST | `/api/store/identity/passwords/forgot` | No |
| `resetPasswordApi` | POST | `/api/store/identity/passwords/reset` | No |
| `changePasswordApi` | POST | `/api/store/identity/passwords/change` | No |
| `logoutApi` | POST | `/api/store/identity/auth/logout` | Yes |
| `getSessionApi` | GET | `/api/store/identity/auth/sessions` | Yes |

### 4.3 Request/Response Types

```typescript
// --- Requests (match backend contracts) ---
interface LoginRequest { credential: string; password: string }
interface RegisterRequest {
  email: string; userName: string; password: string
  firstName: string; lastName?: string; phone?: string; acceptTerm: boolean
}
interface ForgotPasswordRequest { email: string }
interface ResetPasswordRequest { email: string; userId: string; token: string; newPassword: string }
interface ChangePasswordRequest { email: string; currentPassword: string; newPassword: string }

// --- Responses ---
interface TokenResponse {
  accessToken: string; accessTokenExpiresIn: number
  refreshToken: string; refreshTokenExpiresIn: number
}
interface RegisterResponse { userId: string; email: string; message: string }
interface SessionResponse { id: string; roles: string[]; permissions: string[] }

// --- Form shapes (separate from API shapes) ---
interface LoginForm { credential: string; password: string }
interface RegisterForm { email: string; userName: string; password: string; confirmPassword: string; firstName: string; lastName: string; phone: string; acceptTerm: boolean }
interface ForgotPasswordForm { email: string }
interface ResetPasswordForm { password: string; confirmPassword: string }
interface ChangePasswordForm { currentPassword: string; newPassword: string; confirmPassword: string }
```

### 4.4 Store Contract (`useAuthStore`)

```typescript
// State
isLoading: Ref<boolean>
serverErrors: Ref<ApiProblemDetail[]>
fieldErrors: Ref<Record<string, string[]>>

// Actions (all async, return void — callers check store state)
login(credential: string, password: string): Promise<void>
register(fields: RegisterRequest): Promise<void>
forgotPassword(email: string): Promise<void>
resetPassword(params: ResetPasswordRequest): Promise<void>
changePassword(params: ChangePasswordRequest): Promise<void>
logout(): Promise<void>
initialize(): Promise<void>

// Getters (delegated from useSessionStore)
isAuthenticated: ComputedRef<boolean>
currentUser: ComputedRef<User | null>
```

### 4.5 Composables Contract (`useAuth`)

Single facade for page components. Wraps the store and wires `vue-i18n` into
Zod schemas:

```typescript
function useAuth() {
  return {
    // Reactive state
    isLoading, isAuthenticated, serverErrors, fieldErrors, currentUser,
    // Actions
    login, register, forgotPassword, resetPassword, changePassword, logout,
    // i18n-wired Zod schemas
    loginSchema, registerSchema, forgotPasswordSchema, resetPasswordSchema, changePasswordSchema,
    // Lifecycle
    initialize,
  }
}
```

### 4.6 Zod Schemas

Factory functions accepting `t: TFunction` for i18n error messages:

- `loginSchema(t)` — credential + password required
- `registerSchema(t)` — email format, username 3-50 chars, password strength
  (5 rules), confirm match, firstName required, acceptTerm true
- `forgotPasswordSchema(t)` — email required
- `resetPasswordSchema(t)` — password strength + confirm match
- `changePasswordSchema(t)` — current password required, new password strength
  + confirm match

### 4.7 Route Definitions

```typescript
// Standalone auth routes (no layout wrapper)
export const authRoutes: RouteRecordRaw[] = [
  { path: '/login', name: 'auth.login', component: LoginPage },
  { path: '/register', name: 'auth.register', component: RegisterPage },
  { path: '/forgot-password', name: 'auth.forgotPassword', component: ForgotPasswordPage },
  { path: '/reset-password', name: 'auth.resetPassword', component: ResetPasswordPage },
]

// Child route under MainLayout
export const changePasswordRoute: RouteRecordRaw = {
  path: '/account/change-password', name: 'auth.changePassword', component: ChangePasswordPage,
}
```

### 4.8 Error Mapping Contract

The `mapErrors()` helper in `auth.store.ts` maps `Result.failure.errors:
ApiProblemDetail[]` to `fieldErrors` and `serverErrors`:

| Error code pattern | Mapping |
|---|---|
| `User.Email.Duplicate` | `fieldErrors.email.push(message)` |
| `User.Password.Mismatch` | `fieldErrors[currentPassword].push(message)` |
| `User.Credentials.Invalid` | No field — goes to `serverErrors` |
| `User.Token.Invalid` | No field — goes to `serverErrors` |
| Any unmapped code | `serverErrors.push(error)` |

Field name is extracted from the second segment of the dot-separated code,
lowercased (e.g. `User.Email.Duplicate` → `email`).

## 5. Acceptance Criteria

### Functional

- **AC-001**: Given an unauthenticated user, When they navigate to any admin
  page (`/catalog/products`, `/dashboard`), Then they are redirected to
  `/login?redirect=<original_path>`.
- **AC-002**: Given an authenticated user on `/login`, When they navigate to
  the page, Then they are redirected to `/`.
- **AC-003**: Given valid credentials on the login form, When the user submits,
  Then tokens are stored in localStorage, session is hydrated, and the user
  is redirected to the original path or dashboard.
- **AC-004**: Given invalid credentials, When login fails, Then field errors
  display inline on the form and server errors display as a toast.
- **AC-005**: Given a stored valid access token at app boot, When the app
  mounts, Then `initialize()` fetches `/me` and hydrates the session store
  without showing the login page.
- **AC-006**: Given a stored expired token at app boot, When the app mounts,
  Then the token is cleared silently and the login page is shown.
- **AC-007**: Given valid registration data, When submitted, Then a 201
  response triggers auto-login and redirects to the dashboard.
- **AC-008**: Given the forgot-password form, When ANY email is submitted,
  Then a success message is shown (prevents email enumeration).
- **AC-009**: Given the password field, When the user types, Then
  `PasswordStrength` updates in real-time showing which rules are satisfied.
- **AC-010**: Given the logout action, When triggered, Then the session is
  cleared, the server logout endpoint is called, and the user is redirected
  to `/login`.
- **AC-011**: Given the change-password form (authenticated), When submitted,
  Then the current password is validated and the new password is set.

### Visual (Sakai reference parity)

- **AC-012**: Given an auth page, When rendered in light mode, Then it matches
  the Sakai Login pattern: full-screen centered card with gradient border,
  surface colors from the existing Indigo Aura preset.
- **AC-013**: Given an auth page, When dark mode is active, Then all surface
  and text colors switch via `dark:` Tailwind variants.
- **AC-014**: Given any auth page, Then the `FloatingConfigurator` (dark mode
  toggle) is visible in the top-right corner.

### Integration

- **AC-015**: Given the token refresh interceptor, When a 401 is received and
  refresh fails, Then the user is redirected to `/login` (already implemented,
  must still work after this module is added).

## 6. Test Automation Strategy

- **Test Levels**: Unit tests for store actions and API functions. No
  integration or E2E tests in scope.
- **Frameworks**: Vitest + @vue/test-utils (existing project setup).
- **Test Data Management**: Mock `apiClient` with `vi.mock()`. Pinia stores
  created fresh per test with `setActivePinia(createPinia())`.
- **Store tests**: Test each action's state transitions — loading flag,
  error population, session hydration. Test `mapErrors()` edge cases
  (unknown code, multiple errors, empty errors).
- **API tests**: Test that each API function calls the correct endpoint with
  correct params. Mock the axios instance.
- **Component tests**: Test form validation — invalid fields show errors,
  valid submission calls store action. Test `PasswordStrength` rule checking.
- **Coverage Requirements**: Store and API functions at 100% statement
  coverage. Component tests at happy-path coverage (no edge case matrix).
- **CI/CD Integration**: Tests run via `pnpm run test:unit` in CI (already
  configured). All existing 60 tests must continue to pass.

## 7. Rationale & Context

### Why a feature module instead of extending shared?

The existing shared auth infrastructure (`TokenService`, `AuthService`,
interceptors, `useSessionStore`) is cross-cutting — consumed by the error
interceptor, the router guard, and the API client. Moving auth into a feature
module would make infrastructure depend on a feature, which is backwards.

The feature module adds what's missing — form-level orchestration, Zod schemas,
VeeValidate wiring, and page components — without touching the shared layer.

### Why standalone auth routes (no MainLayout wrapper)?

The Sakai Vue reference uses standalone auth routes — each page self-contains
its layout. This avoids the complexity of conditional layout switching in
`App.vue` and allows auth pages to render without the sidebar/topbar overhead.

The Admin SPA's `App.vue` uses a single `<RouterView />`. When a standalone
auth route is matched, the `<RouterView />` renders the auth page directly
(not `MainLayout`). No changes to `App.vue` are needed.

### Why Token-based Tailwind classes?

The `tailwindcss-primeui` plugin (referenced in PrimeVue docs and used in
Sakai) generates Tailwind utility classes from PrimeVue design tokens. This
means `bg-surface-0` always maps to the current theme's surface-0 color,
and `dark:bg-surface-900` maps to the dark mode variant automatically. Using
these classes ensures the auth pages stay consistent with theme changes and
match the rest of the admin UI.

### Why VeeValidate + Zod instead of plain validation?

The legacy admin already uses this combination. The Admin SPA has `vee-validate`
and `@vee-validate/zod` available. Zod schemas are declarative, composable,
and produce typed parse results — better than inline `if (!email) showError()`.

### Why single-file Pinia store?

The existing `useSessionStore` and all project stores use single-file Pinia
Setup stores. Splitting into separate actions/getters files adds indirection
without benefit — Setup stores already separate concerns via the composition
API.

## 8. Dependencies & External Integrations

### Existing Infrastructure (no changes required)
- **INF-001**: `shared/auth/AuthService` — login, logout, getCurrentUser
  static methods
- **INF-002**: `shared/auth/TokenService` — JWT storage, decode, expiry check
- **INF-003**: `stores/useSessionStore` — Pinia session persistence
- **INF-004**: `shared/api/interceptors/` — auth header, camelCase transform,
  error wrapping, silent token refresh
- **INF-005**: `shared/api/client.ts` — configured axios instance
- **INF-006**: `router/guards.ts` — existing `registerAuthGuard` scaffolding
  (activated by this module)
- **INF-007**: `assets/presets/admin-preset.ts` — Aura-based Indigo theme

### Backend API Dependencies
- **API-001**: Identity Store endpoints under `/api/store/identity/` — login,
  register, logout, refresh, sessions, passwords, emails

### Technology Platform Dependencies
- **PLT-001**: Vue 3 + TypeScript — SFC Composition API with `<script setup>`
- **PLT-002**: PrimeVue v5 with Aura preset — `InputText`, `Password`,
  `Checkbox`, `Button`, `Toast` components
- **PLT-003**: Tailwind CSS — utility classes for layout, spacing, typography
- **PLT-004**: `tailwindcss-primeui` plugin — Token-based Tailwind classes
  for surfaces, text colors, borders
- **PLT-005**: Pinia — state management
- **PLT-006**: Vue Router — routing with `createWebHistory`
- **PLT-007**: `vee-validate` + `@vee-validate/zod` — form validation
- **PLT-008**: `vue-i18n` — internationalization (existing `auth.json` locale)

## 9. Examples & Edge Cases

### Login Page Structure (Sakai pattern)

```vue
<!-- LoginPage.vue — matches Sakai reference pattern -->
<template>
  <div class="bg-surface-50 dark:bg-surface-950 flex items-center justify-center min-h-screen min-w-[100vw] overflow-hidden">
    <FloatingConfigurator />

    <div style="border-radius:56px; padding:0.3rem;
         background:linear-gradient(180deg, var(--p-primary-color) 10%, rgba(33,150,243,0) 30%)">
      <div class="w-full bg-surface-0 dark:bg-surface-900 py-20 px-8 sm:px-20" style="border-radius:53px">
        <div class="text-center mb-8">
          <div class="text-surface-900 dark:text-surface-0 text-3xl font-medium mb-4">
            {{ t('auth.login.welcome') }}
          </div>
          <span class="text-muted-color font-medium">
            {{ t('auth.login.subtitle') }}
          </span>
        </div>

        <LoginForm />
      </div>
    </div>
  </div>
</template>
```

### Token Expiry at Boot (Edge Case)

```
1. App mounts → main.ts calls useAuthStore().initialize()
2. initialize() reads accessToken from localStorage
3. TokenService.isAccessTokenExpired() → true
4. TokenService.clearTokens() → removes both tokens
5. initialize() returns (no error, no redirect)
6. Router resolves to current route (e.g. /catalog/products)
7. registerAuthGuard fires → TokenService.hasValidAccessToken() → false
8. Guard redirects to /login?redirect=/catalog/products
```

### Server-Side Validation Errors (Edge Case)

```
1. Login form submits → useAuthStore.login('user@test.com', 'weak')
2. POST /api/store/identity/auth/login/password
3. Server returns 400 with errors: [
     { code: "User.Credentials.Invalid", message: "Invalid email or password.", type: 401 }
   ]
4. Error interceptor wraps into Result<null> { isSuccess: false, errors: [...] }
5. mapErrors() parses "User.Credentials.Invalid" → key "credentials" not a field name
   → falls through to serverErrors
6. LoginForm shows toast: "Invalid email or password."
```

### Concurrent Action Guard (Edge Case)

If the user double-clicks submit, `isLoading` is `true` on the second click —
the action returns early with no side effects. The form submit button is
disabled via `:disabled="isLoading"`.

### Dark Mode Persistence (Edge Case)

`useDarkMode()` from `shared/composables/` reads `localStorage` on init.
Both `AuthLayout` pages and `MainLayout` pages share the same dark mode state.
Toggling dark mode on a login page persists when the user reaches the
dashboard.

## 10. Validation Criteria

- **VAL-001**: `pnpm run lint` — 0 errors
- **VAL-002**: `pnpm run build` — clean build, no warnings
- **VAL-003**: `pnpm run test:unit` — all existing 60 tests pass, new tests
  pass for store, API, and components
- **VAL-004**: Manual smoke test — navigate to `/login`, enter valid
  credentials, verify redirect to dashboard
- **VAL-005**: Manual smoke test — navigate to any admin page unauthenticated,
  verify redirect to `/login?redirect=<path>`
- **VAL-006**: Manual smoke test — log in, navigate, verify session survives
  navigation (no re-login)
- **VAL-007**: Manual smoke test — toggle dark mode on login page, log in,
  verify dark mode persists on dashboard
- **VAL-008**: Manual smoke test — register new account, verify auto-login
  and redirect
- **VAL-009**: Manual smoke test — forgot password form submits without errors
- **VAL-010**: Manual smoke test — change password as authenticated user

## 11. Files to Create & Modify

### Create (18 files)

| File | Description |
|---|---|
| `features/auth/api/auth.api.ts` | Typed API wrappers |
| `features/auth/store/auth.store.ts` | Pinia auth store |
| `features/auth/composables/useAuth.ts` | Page component facade |
| `features/auth/types/index.ts` | Request/response DTOs + form shapes |
| `features/auth/models/index.ts` | Zod validation schemas |
| `features/auth/routes.ts` | Route definitions |
| `features/auth/index.ts` | Barrel export |
| `features/auth/pages/LoginPage.vue` | Login page (AuthLayout + LoginForm) |
| `features/auth/pages/RegisterPage.vue` | Register page (AuthLayout + RegisterForm) |
| `features/auth/pages/ForgotPasswordPage.vue` | Forgot password page |
| `features/auth/pages/ResetPasswordPage.vue` | Reset password page |
| `features/auth/pages/ChangePasswordPage.vue` | Change password page (MainLayout) |
| `features/auth/components/LoginForm.vue` | VeeValidate + Zod login form |
| `features/auth/components/RegisterForm.vue` | VeeValidate + Zod register form |
| `features/auth/components/SocialLogin.vue` | Placeholder stub |
| `features/auth/components/PasswordStrength.vue` | Real-time strength indicator |
| `shared/components/layout/AuthLayout.vue` | Centered card wrapper (Sakai pattern) |
| `shared/components/layout/FloatingConfigurator.vue` | Dark mode toggle (if not existing) |

### Modify (4 files)

| File | Change |
|---|---|
| `router/index.ts` | Add `authRoutes` at top level, `changePasswordRoute` to MainLayout children |
| `main.ts` | Call `useAuthStore().initialize()` after router + Pinia, before mount |
| `router/guards.ts` | Uncomment the redirect logic to activate auth guard |
| `assets/styles/tailwind.css` | Add `@import 'tailwindcss-primeui'` to enable Token-based Tailwind classes |
| `shared/localization/messages/en/auth.json` | Add i18n keys for validation messages |

### Remove

None.

## 12. Related Specifications / Further Reading

- [Admin SPA — Consistent List + Detail Page Pattern](../spec/design-admin-spa-list-detail-pattern.md)
- [Sakai Vue — Admin Template Reference](https://sakai.primevue.org/start/documentation)
  (local reference at `app/references/sakai-vue/src/`)
- [PrimeVue — Tailwind CSS Integration](https://primevue.dev/tailwind/)
  (token-based utility classes from `tailwindcss-primeui` plugin)
- [PrimeVue — Forms Guide](https://primevue.dev/forms/)
  (component usage with validation libraries)
- [PrimeVue — Auto Import](https://primevue.dev/autoimport/)
  (unplugin-vue-components with PrimeVueResolver)
- [VeeValidate — Zod Integration](https://vee-validate.logaretm.com/v4/integrations/zod-schema-validation/)
- [Pinia — Setup Stores](https://pinia.vuejs.org/core-concepts/#setup-stores)
- [Backend Identity Feature](service/Api/src/Module/Identity/Features/Identity.Feature.cs)
  (endpoint route constants and contracts)
- [Backend Identity Smoke Tests](service/Api/tests/Api.SmokeTests/Identity/Store/)
  (HTTP test files for manual endpoint testing)
