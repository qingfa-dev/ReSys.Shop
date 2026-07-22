# Admin SPA — Auth Feature Module Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Build a self-contained `features/auth/` module with login, register, forgot/reset password, change password, and session management, wrapping the existing shared auth infrastructure.

**Architecture:** Feature module imports from `shared/` only. Standalone auth routes at top level (self-wrap with `AuthLayout`). Change-password as child of `MainLayout`. Single-file Pinia Setup store. VeeValidate + Zod forms. Sakai Vue visual patterns (centered card, gradient border, Token-based Tailwind classes).

**Tech Stack:** Vue 3 + TypeScript, PrimeVue v5 (Aura Indigo preset), Tailwind CSS + tailwindcss-primeui, Pinia, Vue Router, vee-validate + @vee-validate/zod, vue-i18n, axios (apiClient)

## Global Constraints

- No changes to `shared/auth/`, `shared/api/interceptors/`, or `apiClient`
- No new npm dependencies
- Feature module SHALL NOT reference other feature modules
- `pnpm run lint` — 0 errors after each task
- `pnpm run build` — clean after each task group
- `pnpm run test:unit` — all 60 existing tests continue to pass

---

### Task 1: Types + Zod Schemas

**Files:**
- Create: `features/auth/types/index.ts`
- Create: `features/auth/models/index.ts`

**Interfaces:**
- Produces: `LoginRequest`, `RegisterRequest`, `ForgotPasswordRequest`, `ResetPasswordRequest`, `ChangePasswordRequest`, `TokenResponse`, `RegisterResponse`, `SessionResponse`
- Produces: `LoginForm`, `RegisterForm`, `ForgotPasswordForm`, `ResetPasswordForm`, `ChangePasswordForm`
- Produces: `createLoginSchema(t)`, `createRegisterSchema(t)`, `createForgotPasswordSchema(t)`, `createResetPasswordSchema(t)`, `createChangePasswordSchema(t)`

- [ ] **Step 1: Create directories and types file**

```bash
mkdir -p app/Admin/src/features/auth/{api,components,composables,models,pages,store,types}
```

```typescript
// features/auth/types/index.ts
export interface LoginRequest {
  credential: string
  password: string
}

export interface RegisterRequest {
  email: string
  userName: string
  password: string
  firstName: string
  lastName?: string
  phone?: string
  acceptTerm: boolean
}

export interface ForgotPasswordRequest {
  email: string
}

export interface ResetPasswordRequest {
  email: string
  userId: string
  token: string
  newPassword: string
}

export interface ChangePasswordRequest {
  email: string
  currentPassword: string
  newPassword: string
}

export interface TokenResponse {
  accessToken: string
  accessTokenExpiresIn: number
  refreshToken: string
  refreshTokenExpiresIn: number
}

export interface RegisterResponse {
  userId: string
  email: string
  message: string
}

export interface SessionResponse {
  id: string
  roles: string[]
  permissions: string[]
}

export interface LoginForm {
  credential: string
  password: string
}

export interface RegisterForm {
  email: string
  userName: string
  password: string
  confirmPassword: string
  firstName: string
  lastName: string
  phone: string
  acceptTerm: boolean
}

export interface ForgotPasswordForm {
  email: string
}

export interface ResetPasswordForm {
  password: string
  confirmPassword: string
}

export interface ChangePasswordForm {
  currentPassword: string
  newPassword: string
  confirmPassword: string
}
```

- [ ] **Step 2: Create Zod schemas file**

```typescript
// features/auth/models/index.ts
import { z } from 'zod'
import type { TFunction } from 'vue-i18n'

export function createLoginSchema(t: TFunction) {
  return z.object({
    credential: z.string().min(1, t('auth.validation.credential.required')),
    password: z.string().min(1, t('auth.validation.password.required')),
  })
}

export function createRegisterSchema(t: TFunction) {
  return z
    .object({
      email: z.string().email(t('auth.validation.email.invalid')),
      userName: z.string().min(3, t('auth.validation.userName.minLength')).max(50, t('auth.validation.userName.maxLength')),
      password: z
        .string()
        .min(8, t('auth.validation.password.minLength'))
        .regex(/[A-Z]/, t('auth.validation.password.uppercase'))
        .regex(/[a-z]/, t('auth.validation.password.lowercase'))
        .regex(/[0-9]/, t('auth.validation.password.digit'))
        .regex(/[^A-Za-z0-9]/, t('auth.validation.password.special')),
      confirmPassword: z.string(),
      firstName: z.string().min(1, t('auth.validation.firstName.required')),
      lastName: z.string().optional(),
      phone: z.string().optional(),
      acceptTerm: z.literal(true, {
        errorMap: () => ({ message: t('auth.validation.acceptTerms.required') }),
      }),
    })
    .refine((data) => data.password === data.confirmPassword, {
      message: t('auth.validation.password.mismatch'),
      path: ['confirmPassword'],
    })
}

export function createForgotPasswordSchema(t: TFunction) {
  return z.object({
    email: z.string().email(t('auth.validation.email.invalid')),
  })
}

export function createResetPasswordSchema(t: TFunction) {
  return z
    .object({
      password: z
        .string()
        .min(8, t('auth.validation.password.minLength'))
        .regex(/[A-Z]/, t('auth.validation.password.uppercase'))
        .regex(/[a-z]/, t('auth.validation.password.lowercase'))
        .regex(/[0-9]/, t('auth.validation.password.digit'))
        .regex(/[^A-Za-z0-9]/, t('auth.validation.password.special')),
      confirmPassword: z.string(),
    })
    .refine((data) => data.password === data.confirmPassword, {
      message: t('auth.validation.password.mismatch'),
      path: ['confirmPassword'],
    })
}

export function createChangePasswordSchema(t: TFunction) {
  return z
    .object({
      currentPassword: z.string().min(1, t('auth.validation.currentPassword.required')),
      newPassword: z
        .string()
        .min(8, t('auth.validation.password.minLength'))
        .regex(/[A-Z]/, t('auth.validation.password.uppercase'))
        .regex(/[a-z]/, t('auth.validation.password.lowercase'))
        .regex(/[0-9]/, t('auth.validation.password.digit'))
        .regex(/[^A-Za-z0-9]/, t('auth.validation.password.special')),
      confirmPassword: z.string(),
    })
    .refine((data) => data.newPassword === data.confirmPassword, {
      message: t('auth.validation.password.mismatch'),
      path: ['confirmPassword'],
    })
}

export type LoginSchema = ReturnType<typeof createLoginSchema>
export type RegisterSchema = ReturnType<typeof createRegisterSchema>
export type ForgotPasswordSchema = ReturnType<typeof createForgotPasswordSchema>
export type ResetPasswordSchema = ReturnType<typeof createResetPasswordSchema>
export type ChangePasswordSchema = ReturnType<typeof createChangePasswordSchema>
```

- [ ] **Step 3: Verify build**

```bash
cd app/Admin && pnpm run build 2>&1 | tail -3
```
Expected: clean build (types-only file won't break the build)

- [ ] **Step 4: Commit**

```bash
git add app/Admin/src/features/auth/types/index.ts app/Admin/src/features/auth/models/index.ts
git commit -m "feat: add auth types and Zod validation schemas"
```

---

### Task 2: API Layer

**Files:**
- Create: `features/auth/api/auth.api.ts`
- Create: `features/auth/api/__tests__/auth.api.spec.ts`

**Interfaces:**
- Consumes: `LoginRequest`, `RegisterRequest`, etc. from Task 1 types
- Consumes: `apiClient` from `@/shared/api/client`
- Consumes: `AuthService.login`, `AuthService.logout`, `AuthService.getCurrentUser` from shared
- Produces: `loginApi(credential, password)`, `registerApi(fields)`, `forgotPasswordApi(email)`, `resetPasswordApi(params)`, `changePasswordApi(params)`, `logoutApi()`, `getSessionApi()`

- [ ] **Step 1: Write API tests**

```bash
mkdir -p app/Admin/src/features/auth/api/__tests__
```

```typescript
// features/auth/api/__tests__/auth.api.spec.ts
import { describe, it, expect, vi, beforeEach } from 'vitest'
import apiClient from '@/shared/api/client'
import { AuthService } from '@/shared/auth/auth.service'
import {
  loginApi,
  registerApi,
  forgotPasswordApi,
  resetPasswordApi,
  changePasswordApi,
  logoutApi,
  getSessionApi,
} from '../auth.api'

vi.mock('@/shared/api/client', () => ({
  default: {
    post: vi.fn(),
    get: vi.fn(),
  },
}))

vi.mock('@/shared/auth/auth.service', () => ({
  AuthService: {
    login: vi.fn(),
    logout: vi.fn(),
    getCurrentUser: vi.fn(),
    isAuthenticated: vi.fn(),
  },
}))

function mockResult<T>(value: T) {
  return { data: { isSuccess: true, statusCode: 200, value, errors: [], message: null } }
}

describe('auth.api', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  describe('loginApi', () => {
    it('calls AuthService.login with credential and password', async () => {
      const mockResponse = { isSuccess: true, statusCode: 200, value: { accessToken: 'at', refreshToken: 'rt' }, errors: [], message: null }
      vi.mocked(AuthService.login).mockResolvedValue(mockResponse)

      const result = await loginApi('user@test.com', 'secret')

      expect(AuthService.login).toHaveBeenCalledWith({ email: 'user@test.com', password: 'secret' })
      expect(result).toBe(mockResponse)
    })
  })

  describe('registerApi', () => {
    it('posts to register endpoint', async () => {
      const fields = { email: 'a@b.com', userName: 'test', password: 'Pass1234!', firstName: 'Test', acceptTerm: true }
      vi.mocked(apiClient.post).mockResolvedValue(mockResult({ userId: '1', email: 'a@b.com', message: 'ok' }))

      const result = await registerApi(fields)

      expect(apiClient.post).toHaveBeenCalledWith('/store/identity/auth/register', fields)
      expect(result.isSuccess).toBe(true)
    })
  })

  describe('forgotPasswordApi', () => {
    it('posts to forgot-password endpoint', async () => {
      vi.mocked(apiClient.post).mockResolvedValue(mockResult(null))

      await forgotPasswordApi('a@b.com')

      expect(apiClient.post).toHaveBeenCalledWith('/store/identity/passwords/forgot', { email: 'a@b.com' })
    })
  })

  describe('resetPasswordApi', () => {
    it('posts to reset-password endpoint', async () => {
      const params = { email: 'a@b.com', userId: '1', token: 'tok', newPassword: 'Pass1234!' }
      vi.mocked(apiClient.post).mockResolvedValue(mockResult(null))

      await resetPasswordApi(params)

      expect(apiClient.post).toHaveBeenCalledWith('/store/identity/passwords/reset', params)
    })
  })

  describe('changePasswordApi', () => {
    it('posts to change-password endpoint', async () => {
      const params = { email: 'a@b.com', currentPassword: 'old', newPassword: 'newPass1!' }
      vi.mocked(apiClient.post).mockResolvedValue(mockResult(null))

      await changePasswordApi(params)

      expect(apiClient.post).toHaveBeenCalledWith('/store/identity/passwords/change', params)
    })
  })

  describe('logoutApi', () => {
    it('calls AuthService.logout', async () => {
      vi.mocked(AuthService.logout).mockResolvedValue(undefined)

      await logoutApi()

      expect(AuthService.logout).toHaveBeenCalled()
    })
  })

  describe('getSessionApi', () => {
    it('calls AuthService.getCurrentUser', async () => {
      const mockResponse = { isSuccess: true, statusCode: 200, value: { id: '1', email: 'a@b.com', name: 'A', role: 'admin', permissions: [] }, errors: [], message: null }
      vi.mocked(AuthService.getCurrentUser).mockResolvedValue(mockResponse)

      const result = await getSessionApi()

      expect(AuthService.getCurrentUser).toHaveBeenCalled()
      expect(result).toBe(mockResponse)
    })
  })
})
```

- [ ] **Step 2: Run tests to verify they fail**

```bash
cd app/Admin && pnpm run test:unit -- --reporter=verbose src/features/auth/api/__tests__/auth.api.spec.ts 2>&1 | tail -10
```
Expected: FAIL — module not found

- [ ] **Step 3: Implement API layer**

```typescript
// features/auth/api/auth.api.ts
import apiClient from '@/shared/api/client'
import { AuthService } from '@/shared/auth/auth.service'
import type { Result } from '@/shared/models'
import type {
  LoginRequest,
  RegisterRequest,
  ForgotPasswordRequest,
  ResetPasswordRequest,
  ChangePasswordRequest,
  TokenResponse,
  RegisterResponse,
  SessionResponse,
} from '../types'

export async function loginApi(credential: string, password: string): Promise<Result<TokenResponse>> {
  return AuthService.login({ email: credential, password }) as Promise<Result<TokenResponse>>
}

export async function registerApi(fields: RegisterRequest): Promise<Result<RegisterResponse>> {
  const response = await apiClient.post<Result<RegisterResponse>>('/store/identity/auth/register', fields)
  return response.data
}

export async function forgotPasswordApi(email: string): Promise<Result<null>> {
  const response = await apiClient.post<Result<null>>('/store/identity/passwords/forgot', { email })
  return response.data
}

export async function resetPasswordApi(params: ResetPasswordRequest): Promise<Result<null>> {
  const response = await apiClient.post<Result<null>>('/store/identity/passwords/reset', params)
  return response.data
}

export async function changePasswordApi(params: ChangePasswordRequest): Promise<Result<null>> {
  const response = await apiClient.post<Result<null>>('/store/identity/passwords/change', params)
  return response.data
}

export async function logoutApi(): Promise<void> {
  await AuthService.logout()
}

export async function getSessionApi(): Promise<Result<SessionResponse>> {
  return AuthService.getCurrentUser() as Promise<Result<SessionResponse>>
}
```

- [ ] **Step 4: Run tests to verify they pass**

```bash
cd app/Admin && pnpm run test:unit 2>&1 | tail -5
```
Expected: 66 tests passed (60 existing + 6 new)

- [ ] **Step 5: Verify lint**

```bash
cd app/Admin && pnpm run lint 2>&1 | tail -3
```
Expected: 0 errors

- [ ] **Step 6: Commit**

```bash
git add app/Admin/src/features/auth/api/
git commit -m "feat: add auth API layer with typed wrappers"
```

---

### Task 3: Auth Store

**Files:**
- Create: `features/auth/store/auth.store.ts`
- Create: `features/auth/store/__tests__/auth.store.spec.ts`

**Interfaces:**
- Consumes: Types from Task 1, API from Task 2
- Consumes: `useSessionStore` from `@/stores/useSessionStore`
- Consumes: `TokenService` from `@/shared/auth/token.service`
- Produces: `useAuthStore` with actions: `login`, `register`, `forgotPassword`, `resetPassword`, `changePassword`, `logout`, `initialize`
- Produces: `mapErrors` helper: maps `ApiProblemDetail[]` to `fieldErrors` + `serverErrors`

- [ ] **Step 1: Write store tests**

```bash
mkdir -p app/Admin/src/features/auth/store/__tests__
```

```typescript
// features/auth/store/__tests__/auth.store.spec.ts
import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest'
import { setActivePinia, createPinia } from 'pinia'
import { useAuthStore } from '../auth.store'
import { useSessionStore } from '@/stores/useSessionStore'
import * as authApi from '../../api/auth.api'

vi.mock('../../api/auth.api', () => ({
  loginApi: vi.fn(),
  registerApi: vi.fn(),
  forgotPasswordApi: vi.fn(),
  resetPasswordApi: vi.fn(),
  changePasswordApi: vi.fn(),
  logoutApi: vi.fn(),
  getSessionApi: vi.fn(),
}))

vi.mock('vue-router', () => ({
  useRouter: () => ({ push: vi.fn(), currentRoute: { value: { query: {} } } }),
}))

function successResult<T>(value: T) {
  return { isSuccess: true, statusCode: 200, value, errors: [], message: null }
}

function errorResult(errors: Array<{ code: string; message: string; type: number; metadata: null }>) {
  return { isSuccess: false, statusCode: 400, value: null, errors, message: null }
}

describe('useAuthStore', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
    vi.clearAllMocks()
  })

  afterEach(() => {
    localStorage.clear()
  })

  describe('login', () => {
    it('sets isLoading during request', async () => {
      vi.mocked(authApi.loginApi).mockImplementation(() => new Promise(() => {}))
      const store = useAuthStore()
      const promise = store.login('cred', 'pass')
      expect(store.isLoading).toBe(true)
      // cleanup
      vi.mocked(authApi.loginApi).mockResolvedValue(successResult({ accessToken: 'at', refreshToken: 'rt', accessTokenExpiresIn: 0, refreshTokenExpiresIn: 0 }))
      await promise
    })

    it('hydrates session on success', async () => {
      vi.mocked(authApi.loginApi).mockResolvedValue(successResult({ accessToken: 'at', refreshToken: 'rt', accessTokenExpiresIn: 9999999999, refreshTokenExpiresIn: 9999999999 }))

      const store = useAuthStore()
      await store.login('cred', 'pass')

      const session = useSessionStore()
      expect(session.isAuthenticated).toBeTruthy()
      expect(authApi.loginApi).toHaveBeenCalledWith('cred', 'pass')
    })

    it('populates serverErrors on failure', async () => {
      vi.mocked(authApi.loginApi).mockResolvedValue(errorResult([
        { code: 'User.Credentials.Invalid', message: 'Invalid credentials', type: 401, metadata: null },
      ]))

      const store = useAuthStore()
      await store.login('cred', 'pass')

      expect(store.serverErrors).toHaveLength(1)
      expect(store.serverErrors[0].code).toBe('User.Credentials.Invalid')
      expect(store.isLoading).toBe(false)
    })

    it('populates fieldErrors for field-specific codes', async () => {
      vi.mocked(authApi.loginApi).mockResolvedValue(errorResult([
        { code: 'User.Email.Duplicate', message: 'Email taken', type: 409, metadata: null },
      ]))

      const store = useAuthStore()
      await store.login('cred', 'pass')

      expect(store.fieldErrors.email).toContain('Email taken')
      expect(store.serverErrors).toHaveLength(1)
    })
  })

  describe('initialize', () => {
    it('fetches session when valid token exists', async () => {
      localStorage.setItem('accessToken', 'fake-valid-token')
      vi.mocked(authApi.getSessionApi).mockResolvedValue(successResult({ id: '1', roles: ['Admin'], permissions: ['*'] }))

      const store = useAuthStore()
      await store.initialize()

      expect(authApi.getSessionApi).toHaveBeenCalled()
      const session = useSessionStore()
      expect(session.isAuthenticated).toBe(true)
    })

    it('clears tokens when no valid token', async () => {
      const store = useAuthStore()
      await store.initialize()

      expect(authApi.getSessionApi).not.toHaveBeenCalled()
      const session = useSessionStore()
      expect(session.isAuthenticated).toBe(false)
    })
  })

  describe('logout', () => {
    it('clears session and tokens', async () => {
      const session = useSessionStore()
      session.setUser({ id: '1', email: 'a@b.com', name: 'A', role: 'admin', permissions: [] })
      localStorage.setItem('accessToken', 'at')
      localStorage.setItem('refreshToken', 'rt')

      const store = useAuthStore()
      await store.logout()

      expect(authApi.logoutApi).toHaveBeenCalled()
      expect(session.isAuthenticated).toBe(false)
      expect(localStorage.getItem('accessToken')).toBeNull()
    })
  })

  describe('register', () => {
    it('auto-logins on success', async () => {
      vi.mocked(authApi.registerApi).mockResolvedValue(successResult({ userId: '1', email: 'a@b.com', message: 'ok' }))
      vi.mocked(authApi.loginApi).mockResolvedValue(successResult({ accessToken: 'at', refreshToken: 'rt', accessTokenExpiresIn: 9999999999, refreshTokenExpiresIn: 9999999999 }))

      const store = useAuthStore()
      await store.register({ email: 'a@b.com', userName: 'test', password: 'Pass1234!', firstName: 'A', acceptTerm: true })

      expect(authApi.registerApi).toHaveBeenCalled()
      expect(authApi.loginApi).toHaveBeenCalled()
    })
  })
})
```

- [ ] **Step 2: Run tests to verify they fail**

```bash
cd app/Admin && pnpm run test:unit -- --reporter=verbose src/features/auth/store/__tests__/auth.store.spec.ts 2>&1 | tail -10
```
Expected: FAIL — module not found

- [ ] **Step 3: Implement the store**

```typescript
// features/auth/store/auth.store.ts
import { ref, readonly } from 'vue'
import { defineStore } from 'pinia'
import { useRouter } from 'vue-router'
import { useSessionStore } from '@/stores/useSessionStore'
import { TokenService } from '@/shared/auth/token.service'
import type { ApiProblemDetail } from '@/shared/models'
import {
  loginApi,
  registerApi,
  forgotPasswordApi,
  resetPasswordApi,
  changePasswordApi,
  logoutApi,
  getSessionApi,
} from '../api/auth.api'
import type { RegisterRequest, ResetPasswordRequest, ChangePasswordRequest } from '../types'

function mapErrors(
  errors: ApiProblemDetail[],
  fieldErrors: Ref<Record<string, string[]>>,
  serverErrors: Ref<ApiProblemDetail[]>,
) {
  const fields: Record<string, string[]> = {}
  const server: ApiProblemDetail[] = []

  for (const error of errors) {
    const segments = error.code.split('.')
    const mapped = segments.length >= 2
      ? segments[1].charAt(0).toLowerCase() + segments[1].slice(1)
      : null

    if (mapped) {
      if (!fields[mapped]) fields[mapped] = []
      fields[mapped].push(error.message)
    } else {
      server.push(error)
    }
  }

  fieldErrors.value = fields
  serverErrors.value = server
}

function fromJwtToUser(payload: Record<string, unknown>) {
  return {
    id: (payload.sub as string) ?? '',
    email: (payload.email as string) ?? '',
    name: (payload.name as string) ?? '',
    role: (payload.role as string) ?? '',
    permissions: (payload.permissions as string[]) ?? [],
  }
}

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
      const payload = TokenService.getAccessTokenPayload()
      if (payload) {
        session.setUser(fromJwtToUser(payload as unknown as Record<string, unknown>))
      }
      const redirect = (router.currentRoute.value.query.redirect as string) ?? '/'
      router.push(redirect)
    } else {
      mapErrors(result.errors, fieldErrors as any, serverErrors as any)
    }
    isLoading.value = false
  }

  async function register(fields: RegisterRequest) {
    resetFormState()
    isLoading.value = true
    const result = await registerApi(fields)
    if (result.isSuccess) {
      await login(fields.email, fields.password)
    } else {
      mapErrors(result.errors, fieldErrors as any, serverErrors as any)
    }
    isLoading.value = false
  }

  async function forgotPassword(email: string) {
    resetFormState()
    isLoading.value = true
    const result = await forgotPasswordApi(email)
    if (!result.isSuccess) {
      mapErrors(result.errors, fieldErrors as any, serverErrors as any)
    }
    isLoading.value = false
  }

  async function resetPassword(params: ResetPasswordRequest) {
    resetFormState()
    isLoading.value = true
    const result = await resetPasswordApi(params)
    if (result.isSuccess) {
      router.push({ name: 'auth.login' })
    } else {
      mapErrors(result.errors, fieldErrors as any, serverErrors as any)
    }
    isLoading.value = false
  }

  async function changePassword(params: ChangePasswordRequest) {
    resetFormState()
    isLoading.value = true
    const result = await changePasswordApi(params)
    if (result.isSuccess) {
      router.push({ name: 'reports.dashboard' })
    } else {
      mapErrors(result.errors, fieldErrors as any, serverErrors as any)
    }
    isLoading.value = false
  }

  async function logout() {
    resetFormState()
    await logoutApi()
    session.clear()
    router.push({ name: 'auth.login' })
  }

  async function initialize() {
    isLoading.value = true
    if (TokenService.hasValidAccessToken()) {
      const result = await getSessionApi()
      if (result.isSuccess && result.value) {
        session.setUser({
          id: result.value.id,
          email: '',
          name: '',
          role: result.value.roles[0] ?? '',
          permissions: Array.isArray(result.value.permissions) ? result.value.permissions : [],
        })
      } else {
        TokenService.clearTokens()
        session.clear()
      }
    } else {
      TokenService.clearTokens()
      session.clear()
    }
    isLoading.value = false
  }

  return {
    isLoading: readonly(isLoading),
    serverErrors: readonly(serverErrors),
    fieldErrors: readonly(fieldErrors),
    login,
    register,
    forgotPassword,
    resetPassword,
    changePassword,
    logout,
    initialize,
    isAuthenticated: session.isAuthenticated,
    currentUser: session.user,
  }
})
```

- [ ] **Step 4: Run tests to verify they pass**

```bash
cd app/Admin && pnpm run test:unit 2>&1 | tail -5
```
Expected: ~72 tests passed

- [ ] **Step 5: Verify lint**

```bash
cd app/Admin && pnpm run lint 2>&1 | tail -3
```
Expected: 0 errors

- [ ] **Step 6: Commit**

```bash
git add app/Admin/src/features/auth/store/
git commit -m "feat: add auth Pinia store with login, register, session management"
```

---

### Task 4: Auth Composable

**Files:**
- Create: `features/auth/composables/useAuth.ts`

**Interfaces:**
- Consumes: `useAuthStore` from Task 3
- Consumes: Schema factories from Task 1
- Consumes: `useI18n` from `vue-i18n`
- Produces: `useAuth()` — single facade with store state, actions, and i18n-wired schemas

- [ ] **Step 1: Create composable**

```typescript
// features/auth/composables/useAuth.ts
import { computed } from 'vue'
import { useI18n } from 'vue-i18n'
import { useAuthStore } from '../store/auth.store'
import {
  createLoginSchema,
  createRegisterSchema,
  createForgotPasswordSchema,
  createResetPasswordSchema,
  createChangePasswordSchema,
} from '../models'
import type { RegisterRequest, ResetPasswordRequest, ChangePasswordRequest } from '../types'

export function useAuth() {
  const store = useAuthStore()
  const { t } = useI18n()

  return {
    isLoading: computed(() => store.isLoading),
    isAuthenticated: computed(() => store.isAuthenticated),
    serverErrors: computed(() => store.serverErrors),
    fieldErrors: computed(() => store.fieldErrors),
    currentUser: computed(() => store.currentUser),

    login: (credential: string, password: string) => store.login(credential, password),
    register: (fields: RegisterRequest) => store.register(fields),
    forgotPassword: (email: string) => store.forgotPassword(email),
    resetPassword: (params: ResetPasswordRequest) => store.resetPassword(params),
    changePassword: (params: ChangePasswordRequest) => store.changePassword(params),
    logout: () => store.logout(),

    loginSchema: createLoginSchema(t),
    registerSchema: createRegisterSchema(t),
    forgotPasswordSchema: createForgotPasswordSchema(t),
    resetPasswordSchema: createResetPasswordSchema(t),
    changePasswordSchema: createChangePasswordSchema(t),

    initialize: () => store.initialize(),
  }
}
```

- [ ] **Step 2: Verify build**

```bash
cd app/Admin && pnpm run build 2>&1 | tail -3
```
Expected: clean build

- [ ] **Step 3: Verify lint**

```bash
cd app/Admin && pnpm run lint 2>&1 | tail -3
```
Expected: 0 errors

- [ ] **Step 4: Commit**

```bash
git add app/Admin/src/features/auth/composables/useAuth.ts
git commit -m "feat: add useAuth composable facade"
```

---

### Task 5: Shared UI Components

**Files:**
- Create: `shared/components/layout/AuthLayout.vue`
- Create: `features/auth/components/PasswordStrength.vue`
- Create: `features/auth/components/SocialLogin.vue`
- Create: `features/auth/components/__tests__/PasswordStrength.spec.ts`

**Interfaces:**
- Consumes: `FloatingConfigurator` from `@/app/layout/components/FloatingConfigurator.vue`
- Produces: `AuthLayout` — centered card wrapper with gradient border (Sakai pattern), `<slot />` for form content
- Produces: `PasswordStrength` — accepts `password: string` prop, shows real-time strength bar + rule checklist
- Produces: `SocialLogin` — placeholder stub

- [ ] **Step 1: Create AuthLayout**

```vue
<!-- shared/components/layout/AuthLayout.vue -->
<script setup lang="ts">
import FloatingConfigurator from '@/app/layout/components/FloatingConfigurator.vue'
</script>

<template>
  <div class="bg-surface-50 dark:bg-surface-950 flex items-center justify-center min-h-screen min-w-[100vw] overflow-hidden">
    <FloatingConfigurator />

    <div
      style="border-radius: 56px; padding: 0.3rem;
             background: linear-gradient(180deg, var(--p-primary-color) 10%, rgba(33, 150, 243, 0) 30%)"
    >
      <div class="w-full bg-surface-0 dark:bg-surface-900 py-20 px-8 sm:px-20" style="border-radius: 53px">
        <slot />
      </div>
    </div>
  </div>
</template>
```

- [ ] **Step 2: Create PasswordStrength**

```vue
<!-- features/auth/components/PasswordStrength.vue -->
<script setup lang="ts">
import { computed } from 'vue'

const props = defineProps<{ password: string }>()

const rules = computed(() => [
  { key: 'minLength', label: 'At least 8 characters', met: (props.password?.length ?? 0) >= 8 },
  { key: 'uppercase', label: 'At least one uppercase letter', met: /[A-Z]/.test(props.password ?? '') },
  { key: 'lowercase', label: 'At least one lowercase letter', met: /[a-z]/.test(props.password ?? '') },
  { key: 'digit', label: 'At least one digit', met: /[0-9]/.test(props.password ?? '') },
  { key: 'special', label: 'At least one special character', met: /[^A-Za-z0-9]/.test(props.password ?? '') },
])

const metCount = computed(() => rules.value.filter((r) => r.met).length)
const strengthPercent = computed(() => (metCount.value / rules.value.length) * 100)
const strengthColor = computed(() => {
  if (metCount.value <= 2) return 'var(--p-red-500)'
  if (metCount.value <= 4) return 'var(--p-amber-500)'
  return 'var(--p-green-500)'
})
</script>

<template>
  <div v-if="password" class="mt-2">
    <div class="flex gap-1 mb-2">
      <div
        v-for="i in rules.length"
        :key="i"
        class="h-1 flex-1 rounded-full transition-colors duration-200"
        :style="{ backgroundColor: i <= metCount ? strengthColor : 'var(--p-surface-200)' }"
      />
    </div>

    <ul class="space-y-1 text-sm">
      <li
        v-for="rule in rules"
        :key="rule.key"
        class="flex items-center gap-2"
        :class="rule.met ? 'text-green-600 dark:text-green-400' : 'text-muted-color'"
      >
        <i :class="rule.met ? 'pi pi-check-circle' : 'pi pi-circle'" class="text-xs" />
        {{ rule.label }}
      </li>
    </ul>
  </div>
</template>
```

- [ ] **Step 3: Create SocialLogin placeholder**

```vue
<!-- features/auth/components/SocialLogin.vue -->
<script setup lang="ts">
</script>

<template>
  <div class="text-center text-muted-color text-sm">
    <!-- Social login providers — not yet implemented -->
  </div>
</template>
```

- [ ] **Step 4: Create PasswordStrength test**

```bash
mkdir -p app/Admin/src/features/auth/components/__tests__
```

```typescript
// features/auth/components/__tests__/PasswordStrength.spec.ts
import { describe, it, expect } from 'vitest'
import { mount } from '@vue/test-utils'
import PasswordStrength from '../PasswordStrength.vue'

describe('PasswordStrength', () => {
  it('renders nothing when password is empty', () => {
    const wrapper = mount(PasswordStrength, { props: { password: '' } })
    expect(wrapper.find('ul').exists()).toBe(false)
  })

  it('shows all rules as unmet for weak password', () => {
    const wrapper = mount(PasswordStrength, { props: { password: 'a' } })
    const items = wrapper.findAll('li')
    expect(items).toHaveLength(5)
    expect(items[0].text()).toContain('At least 8 characters')
    expect(items[0].classes()).toContain('text-muted-color')
  })

  it('shows all rules met for strong password', () => {
    const wrapper = mount(PasswordStrength, { props: { password: 'Strong1@pass' } })
    const items = wrapper.findAll('li')
    items.forEach((item) => {
      expect(item.classes()).toContain('text-green-600')
    })
  })

  it('updates reactively when password changes', async () => {
    const wrapper = mount(PasswordStrength, { props: { password: 'weak' } })
    await wrapper.setProps({ password: 'Strong1@pass' })
    const items = wrapper.findAll('li')
    items.forEach((item) => {
      expect(item.classes()).toContain('text-green-600')
    })
  })
})
```

- [ ] **Step 5: Run tests**

```bash
cd app/Admin && pnpm run test:unit 2>&1 | tail -5
```
Expected: 76 tests passed

- [ ] **Step 6: Verify lint**

```bash
cd app/Admin && pnpm run lint 2>&1 | tail -3
```
Expected: 0 errors

- [ ] **Step 7: Commit**

```bash
git add app/Admin/src/shared/components/layout/AuthLayout.vue app/Admin/src/features/auth/components/PasswordStrength.vue app/Admin/src/features/auth/components/SocialLogin.vue app/Admin/src/features/auth/components/__tests__/
git commit -m "feat: add AuthLayout, PasswordStrength, and SocialLogin components"
```

---

### Task 6: LoginForm + LoginPage

**Files:**
- Create: `features/auth/components/LoginForm.vue`
- Create: `features/auth/pages/LoginPage.vue`

**Interfaces:**
- Consumes: `useAuth` from Task 4
- Consumes: `AuthLayout` from Task 5
- Consumes: `useI18n` from vue-i18n
- Consumes: `useForm` from vee-validate, `toTypedSchema` from @vee-validate/zod
- Produces: `LoginForm` — VeeValidate form with credential + password fields
- Produces: `LoginPage` — composes AuthLayout + LoginForm

- [ ] **Step 1: Create LoginForm**

```vue
<!-- features/auth/components/LoginForm.vue -->
<script setup lang="ts">
import { useForm } from 'vee-validate'
import { toTypedSchema } from '@vee-validate/zod'
import { useAuth } from '../composables/useAuth'
import { useI18n } from 'vue-i18n'

const { t } = useI18n()
const { login, loginSchema, isLoading, serverErrors, fieldErrors } = useAuth()

const { handleSubmit, defineField, errors } = useForm({
  validationSchema: toTypedSchema(loginSchema),
})

const [credential, credentialAttrs] = defineField('credential')
const [password, passwordAttrs] = defineField('password')

const onSubmit = handleSubmit((values) => {
  login(values.credential, values.password)
})
</script>

<template>
  <div>
    <div class="text-center mb-8">
      <div class="text-surface-900 dark:text-surface-0 text-3xl font-medium mb-4">
        {{ t('auth.titles.welcome') }}
      </div>
      <span class="text-muted-color font-medium">{{ t('auth.titles.login') }}</span>
    </div>

    <form @submit="onSubmit" class="flex flex-col" novalidate>
      <label for="credential" class="block text-surface-900 dark:text-surface-0 font-medium text-xl mb-2">
        {{ t('auth.labels.credential') }}
      </label>
      <InputText
        id="credential"
        v-model="credential"
        v-bind="credentialAttrs"
        :placeholder="t('auth.placeholders.credential')"
        class="w-full md:w-[30rem] mb-4"
        :invalid="!!errors.credential"
      />
      <small v-if="errors.credential" class="text-red-500 -mt-3 mb-2">{{ errors.credential }}</small>
      <small v-if="fieldErrors.credential?.length" class="text-red-500 -mt-3 mb-2">{{ fieldErrors.credential[0] }}</small>

      <label for="password1" class="block text-surface-900 dark:text-surface-0 font-medium text-xl mb-2">
        {{ t('auth.labels.password') }}
      </label>
      <Password
        id="password1"
        v-model="password"
        v-bind="passwordAttrs"
        :placeholder="t('auth.placeholders.password')"
        :toggleMask="true"
        :feedback="false"
        class="mb-4"
        fluid
        :invalid="!!errors.password"
      />
      <small v-if="errors.password" class="text-red-500 -mt-3 mb-2">{{ errors.password }}</small>

      <div class="flex items-center justify-between mt-2 mb-8 gap-8">
        <div />
        <router-link to="/forgot-password" class="font-medium no-underline ml-2 text-right cursor-pointer text-primary">
          {{ t('auth.labels.forgot_password') }}
        </router-link>
      </div>

      <Button type="submit" :label="t('auth.actions.sign_in')" class="w-full" :loading="isLoading" :disabled="isLoading" />

      <div v-if="serverErrors.length" class="mt-4">
        <small v-for="err in serverErrors" :key="err.code" class="text-red-500 block">{{ err.message }}</small>
      </div>
    </form>
  </div>
</template>
```

- [ ] **Step 2: Create LoginPage**

```vue
<!-- features/auth/pages/LoginPage.vue -->
<script setup lang="ts">
import AuthLayout from '@/shared/components/layout/AuthLayout.vue'
import LoginForm from '../components/LoginForm.vue'
</script>

<template>
  <AuthLayout>
    <LoginForm />
  </AuthLayout>
</template>
```

- [ ] **Step 3: Verify lint**

```bash
cd app/Admin && pnpm run lint 2>&1 | tail -3
```
Expected: 0 errors

- [ ] **Step 4: Commit**

```bash
git add app/Admin/src/features/auth/components/LoginForm.vue app/Admin/src/features/auth/pages/LoginPage.vue
git commit -m "feat: add LoginForm and LoginPage"
```

---

### Task 7: RegisterForm + RegisterPage

**Files:**
- Create: `features/auth/components/RegisterForm.vue`
- Create: `features/auth/pages/RegisterPage.vue`

**Interfaces:**
- Consumes: `useAuth` from Task 4
- Consumes: `AuthLayout` from Task 5
- Consumes: `PasswordStrength` from Task 5

- [ ] **Step 1: Create RegisterForm**

```vue
<!-- features/auth/components/RegisterForm.vue -->
<script setup lang="ts">
import { useForm } from 'vee-validate'
import { toTypedSchema } from '@vee-validate/zod'
import { useI18n } from 'vue-i18n'
import { useAuth } from '../composables/useAuth'
import PasswordStrength from './PasswordStrength.vue'

const { t } = useI18n()
const { register, registerSchema, isLoading, serverErrors, fieldErrors } = useAuth()

const { handleSubmit, defineField, errors } = useForm({
  validationSchema: toTypedSchema(registerSchema),
  initialValues: {
    email: '',
    userName: '',
    password: '',
    confirmPassword: '',
    firstName: '',
    lastName: '',
    phone: '',
    acceptTerm: false as unknown as true,
  },
})

const [email, emailAttrs] = defineField('email')
const [userName, userNameAttrs] = defineField('userName')
const [password, passwordAttrs] = defineField('password')
const [confirmPassword, confirmPasswordAttrs] = defineField('confirmPassword')
const [firstName, firstNameAttrs] = defineField('firstName')
const [lastName, lastNameAttrs] = defineField('lastName')
const [phone, phoneAttrs] = defineField('phone')
const [acceptTerm, acceptTermAttrs] = defineField('acceptTerm')

const onSubmit = handleSubmit((vals) => {
  register({
    email: vals.email,
    userName: vals.userName,
    password: vals.password,
    firstName: vals.firstName,
    lastName: vals.lastName || undefined,
    phone: vals.phone || undefined,
    acceptTerm: vals.acceptTerm,
  })
})
</script>

<template>
  <div>
    <div class="text-center mb-8">
      <div class="text-surface-900 dark:text-surface-0 text-3xl font-medium mb-4">
        {{ t('auth.titles.register') }}
      </div>
      <span class="text-muted-color font-medium">{{ t('auth.titles.createAccount') }}</span>
    </div>

    <form @submit="onSubmit" class="flex flex-col" novalidate>
      <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
        <!-- First Name -->
        <div>
          <label for="firstName" class="block text-surface-900 dark:text-surface-0 font-medium mb-2">
            {{ t('auth.labels.firstName') }}
          </label>
          <InputText id="firstName" v-model="firstName" v-bind="firstNameAttrs" class="w-full" :invalid="!!errors.firstName" />
          <small v-if="errors.firstName" class="text-red-500">{{ errors.firstName }}</small>
        </div>

        <!-- Last Name -->
        <div>
          <label for="lastName" class="block text-surface-900 dark:text-surface-0 font-medium mb-2">
            {{ t('auth.labels.lastName') }}
          </label>
          <InputText id="lastName" v-model="lastName" v-bind="lastNameAttrs" class="w-full" />
        </div>
      </div>

      <!-- Email -->
      <label for="email" class="block text-surface-900 dark:text-surface-0 font-medium mb-2 mt-4">
        {{ t('auth.labels.email') }}
      </label>
      <InputText id="email" v-model="email" v-bind="emailAttrs" type="email" class="w-full" :invalid="!!errors.email" />
      <small v-if="errors.email" class="text-red-500">{{ errors.email }}</small>
      <small v-if="fieldErrors.email?.length" class="text-red-500">{{ fieldErrors.email[0] }}</small>

      <!-- Username -->
      <label for="userName" class="block text-surface-900 dark:text-surface-0 font-medium mb-2 mt-4">
        {{ t('auth.labels.userName') }}
      </label>
      <InputText id="userName" v-model="userName" v-bind="userNameAttrs" class="w-full" :invalid="!!errors.userName" />
      <small v-if="errors.userName" class="text-red-500">{{ errors.userName }}</small>

      <!-- Password -->
      <label for="password" class="block text-surface-900 dark:text-surface-0 font-medium mb-2 mt-4">
        {{ t('auth.labels.password') }}
      </label>
      <Password id="password" v-model="password" v-bind="passwordAttrs" :toggleMask="true" :feedback="false" class="w-full" fluid :invalid="!!errors.password" />
      <small v-if="errors.password" class="text-red-500">{{ errors.password }}</small>
      <PasswordStrength :password="password" />

      <!-- Confirm Password -->
      <label for="confirmPassword" class="block text-surface-900 dark:text-surface-0 font-medium mb-2 mt-4">
        {{ t('auth.labels.confirmPassword') }}
      </label>
      <Password id="confirmPassword" v-model="confirmPassword" v-bind="confirmPasswordAttrs" :toggleMask="true" :feedback="false" class="w-full" fluid :invalid="!!errors.confirmPassword" />
      <small v-if="errors.confirmPassword" class="text-red-500">{{ errors.confirmPassword }}</small>

      <!-- Phone -->
      <label for="phone" class="block text-surface-900 dark:text-surface-0 font-medium mb-2 mt-4">
        {{ t('auth.labels.phone') }}
      </label>
      <InputText id="phone" v-model="phone" v-bind="phoneAttrs" class="w-full" />

      <!-- Accept Terms -->
      <div class="flex items-center mt-4 gap-2">
        <Checkbox id="acceptTerm" v-model="acceptTerm" v-bind="acceptTermAttrs" binary />
        <label for="acceptTerm" class="text-surface-900 dark:text-surface-0">
          {{ t('auth.labels.acceptTerms') }}
        </label>
      </div>
      <small v-if="errors.acceptTerm" class="text-red-500">{{ errors.acceptTerm }}</small>

      <Button type="submit" :label="t('auth.actions.register')" class="w-full mt-6" :loading="isLoading" :disabled="isLoading" />

      <div v-if="serverErrors.length" class="mt-4">
        <small v-for="err in serverErrors" :key="err.code" class="text-red-500 block">{{ err.message }}</small>
      </div>

      <p class="text-center text-muted-color mt-4 text-sm">
        {{ t('auth.messages.alreadyHaveAccount') }}
        <router-link to="/login" class="text-primary font-medium">{{ t('auth.actions.sign_in') }}</router-link>
      </p>
    </form>
  </div>
</template>
```

- [ ] **Step 2: Create RegisterPage**

```vue
<!-- features/auth/pages/RegisterPage.vue -->
<script setup lang="ts">
import AuthLayout from '@/shared/components/layout/AuthLayout.vue'
import RegisterForm from '../components/RegisterForm.vue'
</script>

<template>
  <AuthLayout>
    <RegisterForm />
  </AuthLayout>
</template>
```

- [ ] **Step 3: Verify lint**

```bash
cd app/Admin && pnpm run lint 2>&1 | tail -3
```
Expected: 0 errors

- [ ] **Step 4: Commit**

```bash
git add app/Admin/src/features/auth/components/RegisterForm.vue app/Admin/src/features/auth/pages/RegisterPage.vue
git commit -m "feat: add RegisterForm and RegisterPage"
```

---

### Task 8: Password Pages (Forgot, Reset, Change)

**Files:**
- Create: `features/auth/pages/ForgotPasswordPage.vue`
- Create: `features/auth/pages/ResetPasswordPage.vue`
- Create: `features/auth/pages/ChangePasswordPage.vue`

**Interfaces:**
- Consumes: `useAuth` from Task 4
- Consumes: `AuthLayout` from Task 5 (for Forgot and Reset pages)
- Consumes: `PasswordStrength` from Task 5 (for Reset page)

- [ ] **Step 1: Create ForgotPasswordPage**

```vue
<!-- features/auth/pages/ForgotPasswordPage.vue -->
<script setup lang="ts">
import { ref } from 'vue'
import { useForm } from 'vee-validate'
import { toTypedSchema } from '@vee-validate/zod'
import { useI18n } from 'vue-i18n'
import AuthLayout from '@/shared/components/layout/AuthLayout.vue'
import { useAuth } from '../composables/useAuth'

defineOptions({ name: 'ForgotPasswordPage' })

const { t } = useI18n()
const { forgotPassword, forgotPasswordSchema, isLoading, serverErrors, fieldErrors } = useAuth()

const { handleSubmit, defineField, errors } = useForm({
  validationSchema: toTypedSchema(forgotPasswordSchema),
})

const [email, emailAttrs] = defineField('email')
const submitted = ref(false)

const onSubmit = handleSubmit((values) => {
  forgotPassword(values.email)
  submitted.value = true
})
</script>

<template>
  <AuthLayout>
    <div class="text-center mb-8">
      <div class="text-surface-900 dark:text-surface-0 text-3xl font-medium mb-4">
        {{ t('auth.titles.forgotPassword') }}
      </div>
      <span class="text-muted-color font-medium">{{ t('auth.titles.forgotPasswordSubtitle') }}</span>
    </div>

    <div v-if="submitted && !serverErrors.length" class="text-center">
      <i class="pi pi-check-circle text-green-500 text-4xl mb-4" />
      <p class="text-surface-900 dark:text-surface-0 font-medium">{{ t('auth.messages.forgotPasswordSent') }}</p>
      <router-link to="/login" class="text-primary font-medium mt-4 inline-block">
        {{ t('auth.actions.backToLogin') }}
      </router-link>
    </div>

    <form v-else @submit="onSubmit" class="flex flex-col" novalidate>
      <label for="fpemail" class="block text-surface-900 dark:text-surface-0 font-medium text-xl mb-2">
        {{ t('auth.labels.email') }}
      </label>
      <InputText id="fpemail" v-model="email" v-bind="emailAttrs" type="email" class="w-full md:w-[30rem] mb-4" :invalid="!!errors.email" />
      <small v-if="errors.email" class="text-red-500 -mt-3 mb-2">{{ errors.email }}</small>

      <Button type="submit" :label="t('auth.actions.sendResetLink')" class="w-full" :loading="isLoading" :disabled="isLoading" />

      <div v-if="serverErrors.length" class="mt-4">
        <small v-for="err in serverErrors" :key="err.code" class="text-red-500 block">{{ err.message }}</small>
      </div>

      <p class="text-center text-muted-color mt-4 text-sm">
        <router-link to="/login" class="text-primary font-medium">{{ t('auth.actions.backToLogin') }}</router-link>
      </p>
    </form>
  </AuthLayout>
</template>
```

- [ ] **Step 2: Create ResetPasswordPage**

```vue
<!-- features/auth/pages/ResetPasswordPage.vue -->
<script setup lang="ts">
import { useForm } from 'vee-validate'
import { toTypedSchema } from '@vee-validate/zod'
import { useI18n } from 'vue-i18n'
import { useRoute } from 'vue-router'
import AuthLayout from '@/shared/components/layout/AuthLayout.vue'
import PasswordStrength from '../components/PasswordStrength.vue'
import { useAuth } from '../composables/useAuth'

const { t } = useI18n()
const route = useRoute()
const { resetPassword, resetPasswordSchema, isLoading, serverErrors } = useAuth()

const { handleSubmit, defineField, errors, values } = useForm({
  validationSchema: toTypedSchema(resetPasswordSchema),
})

const [password, passwordAttrs] = defineField('password')
const [confirmPassword, confirmPasswordAttrs] = defineField('confirmPassword')

const onSubmit = handleSubmit((vals) => {
  resetPassword({
    email: (route.query.email as string) ?? '',
    userId: (route.query.userId as string) ?? '',
    token: (route.query.token as string) ?? '',
    newPassword: vals.password,
  })
})
</script>

<template>
  <AuthLayout>
    <div class="text-center mb-8">
      <div class="text-surface-900 dark:text-surface-0 text-3xl font-medium mb-4">
        {{ t('auth.titles.resetPassword') }}
      </div>
      <span class="text-muted-color font-medium">{{ t('auth.titles.resetPasswordSubtitle') }}</span>
    </div>

    <form @submit="onSubmit" class="flex flex-col" novalidate>
      <label for="rspassword" class="block text-surface-900 dark:text-surface-0 font-medium text-xl mb-2">
        {{ t('auth.labels.newPassword') }}
      </label>
      <Password id="rspassword" v-model="password" v-bind="passwordAttrs" :toggleMask="true" :feedback="false" class="w-full md:w-[30rem]" fluid :invalid="!!errors.password" />
      <small v-if="errors.password" class="text-red-500 mt-1">{{ errors.password }}</small>
      <PasswordStrength :password="password" />

      <label for="rsconfirmPassword" class="block text-surface-900 dark:text-surface-0 font-medium text-xl mb-2 mt-4">
        {{ t('auth.labels.confirmPassword') }}
      </label>
      <Password id="rsconfirmPassword" v-model="confirmPassword" v-bind="confirmPasswordAttrs" :toggleMask="true" :feedback="false" class="w-full md:w-[30rem]" fluid :invalid="!!errors.confirmPassword" />
      <small v-if="errors.confirmPassword" class="text-red-500 mt-1">{{ errors.confirmPassword }}</small>

      <Button type="submit" :label="t('auth.actions.resetPassword')" class="w-full mt-6" :loading="isLoading" :disabled="isLoading" />

      <div v-if="serverErrors.length" class="mt-4">
        <small v-for="err in serverErrors" :key="err.code" class="text-red-500 block">{{ err.message }}</small>
      </div>
    </form>
  </AuthLayout>
</template>
```

- [ ] **Step 3: Create ChangePasswordPage**

```vue
<!-- features/auth/pages/ChangePasswordPage.vue -->
<script setup lang="ts">
import { useForm } from 'vee-validate'
import { toTypedSchema } from '@vee-validate/zod'
import { useI18n } from 'vue-i18n'
import { useAuth } from '../composables/useAuth'

const { t } = useI18n()
const { changePassword, changePasswordSchema, isLoading, serverErrors, fieldErrors, currentUser } = useAuth()

const { handleSubmit, defineField, errors } = useForm({
  validationSchema: toTypedSchema(changePasswordSchema),
})

const [currentPassword, currentPasswordAttrs] = defineField('currentPassword')
const [newPassword, newPasswordAttrs] = defineField('newPassword')
const [confirmPassword, confirmPasswordAttrs] = defineField('confirmPassword')

const onSubmit = handleSubmit((vals) => {
  changePassword({
    email: currentUser.value?.email ?? '',
    currentPassword: vals.currentPassword,
    newPassword: vals.newPassword,
  })
})
</script>

<template>
  <div class="max-w-lg mx-auto mt-8">
    <div class="card p-6">
      <h2 class="text-2xl font-medium text-surface-900 dark:text-surface-0 mb-6">
        {{ t('auth.titles.changePassword') }}
      </h2>

      <form @submit="onSubmit" class="flex flex-col gap-4" novalidate>
        <div>
          <label for="cpcurrent" class="block text-surface-900 dark:text-surface-0 font-medium mb-2">
            {{ t('auth.labels.currentPassword') }}
          </label>
          <Password id="cpcurrent" v-model="currentPassword" v-bind="currentPasswordAttrs" :toggleMask="true" :feedback="false" fluid :invalid="!!errors.currentPassword" />
          <small v-if="errors.currentPassword" class="text-red-500">{{ errors.currentPassword }}</small>
          <small v-if="fieldErrors.currentPassword?.length" class="text-red-500">{{ fieldErrors.currentPassword[0] }}</small>
        </div>

        <div>
          <label for="cpnew" class="block text-surface-900 dark:text-surface-0 font-medium mb-2">
            {{ t('auth.labels.newPassword') }}
          </label>
          <Password id="cpnew" v-model="newPassword" v-bind="newPasswordAttrs" :toggleMask="true" :feedback="false" fluid :invalid="!!errors.newPassword" />
          <small v-if="errors.newPassword" class="text-red-500">{{ errors.newPassword }}</small>
        </div>

        <div>
          <label for="cpconfirm" class="block text-surface-900 dark:text-surface-0 font-medium mb-2">
            {{ t('auth.labels.confirmPassword') }}
          </label>
          <Password id="cpconfirm" v-model="confirmPassword" v-bind="confirmPasswordAttrs" :toggleMask="true" :feedback="false" fluid :invalid="!!errors.confirmPassword" />
          <small v-if="errors.confirmPassword" class="text-red-500">{{ errors.confirmPassword }}</small>
        </div>

        <Button type="submit" :label="t('auth.actions.updatePassword')" class="w-full" :loading="isLoading" :disabled="isLoading" />

        <div v-if="serverErrors.length" class="mt-2">
          <small v-for="err in serverErrors" :key="err.code" class="text-red-500 block">{{ err.message }}</small>
        </div>
      </form>
    </div>
  </div>
</template>
```

- [ ] **Step 4: Verify lint**

```bash
cd app/Admin && pnpm run lint 2>&1 | tail -3
```
Expected: 0 errors

- [ ] **Step 5: Commit**

```bash
git add app/Admin/src/features/auth/pages/ForgotPasswordPage.vue app/Admin/src/features/auth/pages/ResetPasswordPage.vue app/Admin/src/features/auth/pages/ChangePasswordPage.vue
git commit -m "feat: add password pages (forgot, reset, change)"
```

---

### Task 9: Routes + Barrel + Integration

**Files:**
- Create: `features/auth/routes.ts`
- Create: `features/auth/index.ts`
- Modify: `router/index.ts:1-38`
- Modify: `router/guards.ts:1-17`
- Modify: `assets/styles/tailwind.css:1-3`
- Modify: `shared/localization/messages/en/auth.json:1-59`
- Modify: `main.ts:1-21`

**Interfaces:**
- Consumes: All page components from Tasks 6-8
- Consumes: Store + composable from Tasks 3-4
- Produces: `authRoutes`, `changePasswordRoute` imported by router
- Produces: Barrel `features/auth/index.ts` exporting routes, store, composable

- [ ] **Step 1: Create routes.ts**

```typescript
// features/auth/routes.ts
import type { RouteRecordRaw } from 'vue-router'

export const authRoutes: RouteRecordRaw[] = [
  {
    path: '/login',
    name: 'auth.login',
    component: () => import('./pages/LoginPage.vue'),
    meta: { layout: 'auth' },
  },
  {
    path: '/register',
    name: 'auth.register',
    component: () => import('./pages/RegisterPage.vue'),
    meta: { layout: 'auth' },
  },
  {
    path: '/forgot-password',
    name: 'auth.forgotPassword',
    component: () => import('./pages/ForgotPasswordPage.vue'),
    meta: { layout: 'auth' },
  },
  {
    path: '/reset-password',
    name: 'auth.resetPassword',
    component: () => import('./pages/ResetPasswordPage.vue'),
    meta: { layout: 'auth' },
  },
]

export const changePasswordRoute: RouteRecordRaw = {
  path: '/account/change-password',
  name: 'auth.changePassword',
  component: () => import('./pages/ChangePasswordPage.vue'),
}
```

- [ ] **Step 2: Create barrel index.ts**

```typescript
// features/auth/index.ts
export { authRoutes, changePasswordRoute } from './routes'
export { useAuthStore } from './store/auth.store'
export { useAuth } from './composables/useAuth'
export type * from './types'
```

- [ ] **Step 3: Wire routes into router/index.ts**

Replace the entire file:

```typescript
// router/index.ts
import { createRouter, createWebHistory } from 'vue-router'
import MainLayout from '@/app/layout/MainLayout.vue'
import { reportsRoutes } from '@/app/routes/reports.routes'
import { catalogRoutes } from '@/app/routes/catalog.routes'
import { inventoryRoutes } from '@/app/routes/inventory.routes'
import { orderingRoutes } from '@/app/routes/ordering.routes'
import { paymentRoutes } from '@/app/routes/payment.routes'
import { shippingRoutes } from '@/app/routes/shipping.routes'
import { locationRoutes } from '@/app/routes/location.routes'
import { usersRoutes } from '@/app/routes/users.routes'
import { profileRoutes } from '@/app/routes/profile.routes'
import { authRoutes, changePasswordRoute } from '@/features/auth'
import { registerAuthGuard } from './guards'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    ...authRoutes,
    {
      path: '/',
      component: MainLayout,
      children: [
        { path: '', redirect: { name: 'reports.dashboard' } },
        changePasswordRoute,
        profileRoutes,
        reportsRoutes,
        catalogRoutes,
        inventoryRoutes,
        orderingRoutes,
        paymentRoutes,
        shippingRoutes,
        locationRoutes,
        usersRoutes,
      ],
    },
  ],
})

registerAuthGuard(router)

export default router
```

- [ ] **Step 4: Activate the auth guard**

Replace `router/guards.ts`:

```typescript
// router/guards.ts
import type { Router } from 'vue-router'
import { TokenService } from '@/shared/auth/token.service'

const PUBLIC_ROUTES = ['auth.login', 'auth.register', 'auth.forgotPassword', 'auth.resetPassword']

export function registerAuthGuard(router: Router) {
  router.beforeEach((to, _from, next) => {
    const isAuthenticated = TokenService.hasValidAccessToken()

    if (!isAuthenticated && !PUBLIC_ROUTES.includes(to.name as string)) {
      next({ name: 'auth.login', query: { redirect: to.fullPath } })
      return
    }

    if (isAuthenticated && to.name === 'auth.login') {
      next({ name: 'reports.dashboard' })
      return
    }

    next()
  })
}
```

- [ ] **Step 5: Add tailwindcss-primeui plugin activation**

Edit `assets/styles/tailwind.css` — append the plugin line after existing directives:

```css
@tailwind base;
@tailwind components;
@tailwind utilities;
@plugin 'tailwindcss-primeui';
```

> **Note:** If `@plugin` does not work alongside `@tailwind` directives (build fails with unknown directive), switch to the Tailwind v4 import style: replace the file content with `@import 'tailwindcss'; @plugin 'tailwindcss-primeui';`

- [ ] **Step 6: Add i18n keys to auth.json**

Replace `shared/localization/messages/en/auth.json`:

```json
{
  "titles": {
    "login": "Sign In",
    "register": "Create Account",
    "createAccount": "Create your account",
    "welcome": "Welcome Back",
    "forgotPassword": "Forgot Password",
    "forgotPasswordSubtitle": "Enter your email to receive a reset link",
    "resetPassword": "Reset Password",
    "resetPasswordSubtitle": "Enter your new password",
    "changePassword": "Change Password",
    "app_name": "ReSys.Shop",
    "app_subtitle": "Admin Control Panel"
  },
  "labels": {
    "credential": "Email or Username",
    "email": "Email",
    "userName": "Username",
    "password": "Password",
    "currentPassword": "Current Password",
    "newPassword": "New Password",
    "confirmPassword": "Confirm New Password",
    "firstName": "First Name",
    "lastName": "Last Name",
    "phone": "Phone",
    "acceptTerms": "I accept the Terms of Service and Privacy Policy",
    "remember_me": "Remember me",
    "forgot_password": "Forgot password?",
    "sign_in": "Sign In",
    "account_details": "Account Details"
  },
  "placeholders": {
    "credential": "admin\\@resys.shop",
    "password": "••••••••"
  },
  "messages": {
    "login_success": "You have successfully logged in.",
    "login_failed": "Invalid credentials or server error.",
    "validation_failed": "Please check your input.",
    "loading": "Signing in...",
    "copyright": "© {year} ReSys.Shop. All rights reserved.",
    "password_mismatch": "New passwords do not match",
    "forgotPasswordSent": "If an account exists with this email, you will receive a password reset link shortly.",
    "alreadyHaveAccount": "Already have an account?",
    "dontHaveAccount": "Don't have an account?"
  },
  "actions": {
    "sign_in": "Sign In",
    "register": "Create Account",
    "updatePassword": "Update Password",
    "sendResetLink": "Send Reset Link",
    "resetPassword": "Reset Password",
    "backToLogin": "Back to Sign In"
  },
  "validation": {
    "credential": {
      "required": "Email or Username is required",
      "max_length": "Credential must not exceed 255 characters"
    },
    "email": {
      "invalid": "Please enter a valid email address"
    },
    "userName": {
      "minLength": "Username must be at least 3 characters",
      "maxLength": "Username must not exceed 50 characters"
    },
    "firstName": {
      "required": "First name is required"
    },
    "acceptTerms": {
      "required": "You must accept the Terms of Service"
    },
    "password": {
      "required": "Password is required",
      "minLength": "Password must be at least 8 characters",
      "uppercase": "Password must contain at least one uppercase letter",
      "lowercase": "Password must contain at least one lowercase letter",
      "digit": "Password must contain at least one digit",
      "special": "Password must contain at least one special character",
      "mismatch": "Passwords do not match",
      "max_length": "Password must not exceed 128 characters"
    },
    "currentPassword": {
      "required": "Current password is required",
      "max_length": "Password must not exceed 128 characters"
    },
    "new_password": {
      "min_length": "New password must be at least 6 characters",
      "max_length": "New password must not exceed 128 characters"
    },
    "confirmPassword": {
      "required": "Please confirm your new password"
    }
  }
}
```

- [ ] **Step 7: Call initialize in main.ts**

Replace `main.ts`:

```typescript
// main.ts
import { createApp } from 'vue'
import { createPinia } from 'pinia'

import App from './App.vue'
import router from './router'
import { setupPrimeVue } from '@/app/plugins/primevue'
import { createI18nPlugin } from '@/shared/localization'
import { createDirectivesPlugin } from '@/shared/directives'
import { useAuthStore } from '@/features/auth'

import './assets/styles/tailwind.css'
import './assets/styles/main.scss'

const app = createApp(App)

app.use(createPinia())
app.use(router)
setupPrimeVue(app)
app.use(createI18nPlugin())
app.use(createDirectivesPlugin())

const authStore = useAuthStore()
authStore.initialize()

app.mount('#app')
```

- [ ] **Step 8: Verify build**

```bash
cd app/Admin && pnpm run build 2>&1 | tail -3
```
Expected: clean build

- [ ] **Step 9: Verify lint**

```bash
cd app/Admin && pnpm run lint 2>&1 | tail -3
```
Expected: 0 errors

- [ ] **Step 10: Run all tests**

```bash
cd app/Admin && pnpm run test:unit 2>&1 | tail -5
```
Expected: all tests pass

- [ ] **Step 11: Commit**

```bash
git add app/Admin/src/features/auth/routes.ts app/Admin/src/features/auth/index.ts app/Admin/src/router/ app/Admin/src/main.ts app/Admin/src/assets/styles/tailwind.css app/Admin/src/shared/localization/messages/en/auth.json
git commit -m "feat: wire auth routes, activate guard, add i18n keys, bootstrap session"
```

---

### Task 10: Final Verification

**Files:** None (verification only)

- [ ] **Step 1: Clean build**

```bash
cd app/Admin && pnpm run build 2>&1
```
Expected: 0 errors, 0 warnings

- [ ] **Step 2: Lint check**

```bash
cd app/Admin && pnpm run lint 2>&1
```
Expected: 0 errors

- [ ] **Step 3: Full test suite**

```bash
cd app/Admin && pnpm run test:unit 2>&1
```
Expected: all tests pass (60 existing + ~18 new)

- [ ] **Step 4: Manual smoke test checklist**

Start the dev server (`pnpm run dev` in Admin, ensure backend is running):
- [ ] Navigate to `http://localhost:{port}/login` — See login form with gradient border card, dark mode toggle
- [ ] Toggle dark mode — Colors switch
- [ ] Submit empty form — See validation errors
- [ ] Navigate to `http://localhost:{port}/register` — See register form with PasswordStrength
- [ ] Navigate to `http://localhost:{port}/forgot-password` — See forgot password form
- [ ] Navigate to `http://localhost:{port}/catalog/products` unauthenticated — Redirected to `/login?redirect=/catalog/products`
- [ ] Enter valid credentials (seed admin: `admin@resys.shop` / `Admin@1234!`) — Logged in, redirected to dashboard
- [ ] Navigate to `http://localhost:{port}/account/change-password` — See change password form under MainLayout
- [ ] Refresh the page — Session survives, stay on same page
- [ ] Log out — Redirected to `/login`, session cleared

- [ ] **Step 5: Final commit (if any fixes needed)**

```bash
git status
```

---

## Self-Review Checklist

After completing all tasks, verify:

1. **Spec coverage:** Every REQ/PAT/CON from the spec maps to a task output — verify by re-reading `spec/design-admin-spa-auth-module.md`
2. **No shared mutation:** `shared/auth/`, `shared/api/interceptors/`, and `apiClient` are untouched
3. **Test isolation:** Existing 60 tests still pass
4. **Sakai pattern compliance:** Auth pages use `AuthLayout` with gradient border, `FloatingConfigurator`, Token-based Tailwind classes, `dark:` variants
5. **Guard active:** Unauthenticated → `/login?redirect=`, authenticated on `/login` → `/dashboard`
