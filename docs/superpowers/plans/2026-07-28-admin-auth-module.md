# Admin Auth Module — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Build the complete auth module — types, validation schemas, token service, API service, Pinia store, three view pages (login, forgot-password, reset-password), fix the refresh URL bug, and wire store init into the router guard.

**Architecture:** Co-located feature module. Views dispatch store actions, store calls API service, API service calls backend. Token service is a pure localStorage wrapper shared between store and axios interceptor. Validation schemas use Zod with reusable field-level validators composed into form schemas.

**Tech Stack:** Vue 3, Pinia, Vue Router, Zod 4, vee-validate 4, Vitest, Axios (via shared/api/client), PrimeVue, TypeScript.

## Global Constraints

- All operations return `Result<T>` or `Promise<Result<T>>` — exceptions only for unrecoverable errors
- Tests use `.spec.ts` extension, co-located or in `__tests__/` subdirectories
- Tests follow TDD: write failing test first, then implementation
- Barrel files (`index.ts`) export all public symbols from each subfolder
- `TreatWarningsAsErrors=true` — any TypeScript warning fails the build
- Backend wraps responses in `Result<T>` envelope with data under `.value`; camelCase interceptor handles snake_case→camelCase
- Auth API base: `api/store/identity/auth`, passwords: `api/store/identity/passwords`
- All auth routes have `requiresAuth: false`
- `@` alias resolves to `src/`, `@ui` alias to `src/shared/components/ui/`

---

### Task 1: Token service

**Files:**
- Create: `app/Admin/src/features/auth/services/tokenService.ts`
- Test: `app/Admin/src/features/auth/services/__tests__/tokenService.spec.ts`

**Interfaces:**
- Consumes: `STORAGE_KEYS` from `@/shared/constants/storage`, `TokenPair` type (not yet created — use inline interface for Task 1, replaced in Task 2)
- Produces: `getAccessToken(): string | null`, `getRefreshToken(): string | null`, `setTokens(pair: TokenPair): void`, `clearTokens(): void`, `hasValidAccessToken(): boolean`

- [ ] **Step 1: Write failing tests**

Create `app/Admin/src/features/auth/services/__tests__/tokenService.spec.ts`:

```ts
import { describe, it, expect, beforeEach } from 'vitest'
import {
  getAccessToken,
  getRefreshToken,
  setTokens,
  clearTokens,
  hasValidAccessToken,
} from '../tokenService'

function makePair(overrides: Partial<{ accessTokenExpiresIn: number; refreshTokenExpiresIn: number }> = {}) {
  return {
    accessToken: 'access-token-123',
    accessTokenExpiresIn: overrides.accessTokenExpiresIn ?? Date.now() / 1000 + 3600,
    refreshToken: 'refresh-token-456',
    refreshTokenExpiresIn: overrides.refreshTokenExpiresIn ?? Date.now() / 1000 + 7200,
  }
}

beforeEach(() => {
  localStorage.clear()
})

describe('getAccessToken', () => {
  it('returns null when no token stored', () => {
    expect(getAccessToken()).toBeNull()
  })

  it('returns token after setTokens', () => {
    setTokens(makePair())
    expect(getAccessToken()).toBe('access-token-123')
  })
})

describe('getRefreshToken', () => {
  it('returns null when no token stored', () => {
    expect(getRefreshToken()).toBeNull()
  })

  it('returns token after setTokens', () => {
    setTokens(makePair())
    expect(getRefreshToken()).toBe('refresh-token-456')
  })
})

describe('setTokens', () => {
  it('stores access and refresh tokens in localStorage', () => {
    setTokens(makePair())
    expect(localStorage.getItem('accessToken')).toBe('access-token-123')
    expect(localStorage.getItem('refreshToken')).toBe('refresh-token-456')
  })

  it('stores expiry timestamps', () => {
    setTokens(makePair())
    expect(localStorage.getItem('accessToken_expires_at')).toBeTruthy()
    expect(localStorage.getItem('refreshToken_expires_at')).toBeTruthy()
  })
})

describe('clearTokens', () => {
  it('removes all four localStorage keys', () => {
    setTokens(makePair())
    clearTokens()
    expect(localStorage.getItem('accessToken')).toBeNull()
    expect(localStorage.getItem('refreshToken')).toBeNull()
    expect(localStorage.getItem('accessToken_expires_at')).toBeNull()
    expect(localStorage.getItem('refreshToken_expires_at')).toBeNull()
  })
})

describe('hasValidAccessToken', () => {
  it('returns false when no token', () => {
    expect(hasValidAccessToken()).toBe(false)
  })

  it('returns true for a future expiry', () => {
    setTokens(makePair({ accessTokenExpiresIn: Date.now() / 1000 + 600 }))
    expect(hasValidAccessToken()).toBe(true)
  })

  it('returns false for a past expiry', () => {
    setTokens(makePair({ accessTokenExpiresIn: Date.now() / 1000 - 600 }))
    expect(hasValidAccessToken()).toBe(false)
  })
})
```

- [ ] **Step 2: Run tests to verify they fail**

Run: `cd app/Admin && npx vitest run features/auth/services/__tests__/tokenService.spec.ts 2>&1`
Expected: All tests fail — service file not found or functions not exported.

- [ ] **Step 3: Implement tokenService.ts**

```ts
import { STORAGE_KEYS } from '@/shared/constants/storage'

interface TokenPair {
  accessToken: string
  accessTokenExpiresIn: number
  refreshToken: string
  refreshTokenExpiresIn: number
}

export function getAccessToken(): string | null {
  try {
    return localStorage.getItem(STORAGE_KEYS.ACCESS_TOKEN)
  } catch {
    return null
  }
}

export function getRefreshToken(): string | null {
  try {
    return localStorage.getItem(STORAGE_KEYS.REFRESH_TOKEN)
  } catch {
    return null
  }
}

export function setTokens(pair: TokenPair): void {
  try {
    localStorage.setItem(STORAGE_KEYS.ACCESS_TOKEN, pair.accessToken)
    localStorage.setItem(`${STORAGE_KEYS.ACCESS_TOKEN}_expires_at`, String(pair.accessTokenExpiresIn))
    localStorage.setItem(STORAGE_KEYS.REFRESH_TOKEN, pair.refreshToken)
    localStorage.setItem(`${STORAGE_KEYS.REFRESH_TOKEN}_expires_at`, String(pair.refreshTokenExpiresIn))
  } catch {
    // Ignore — localStorage may be unavailable
  }
}

export function clearTokens(): void {
  try {
    localStorage.removeItem(STORAGE_KEYS.ACCESS_TOKEN)
    localStorage.removeItem(`${STORAGE_KEYS.ACCESS_TOKEN}_expires_at`)
    localStorage.removeItem(STORAGE_KEYS.REFRESH_TOKEN)
    localStorage.removeItem(`${STORAGE_KEYS.REFRESH_TOKEN}_expires_at`)
  } catch {
    // Ignore
  }
}

export function hasValidAccessToken(): boolean {
  try {
    const token = localStorage.getItem(STORAGE_KEYS.ACCESS_TOKEN)
    if (!token) return false
    const expiresAt = localStorage.getItem(`${STORAGE_KEYS.ACCESS_TOKEN}_expires_at`)
    if (!expiresAt) return true
    return Number(expiresAt) > Date.now() / 1000
  } catch {
    return false
  }
}
```

- [ ] **Step 4: Run tests to verify they pass**

Run: `cd app/Admin && npx vitest run features/auth/services/__tests__/tokenService.spec.ts 2>&1`
Expected: All 10 tests pass.

- [ ] **Step 5: Commit**

```bash
git add app/Admin/src/features/auth/services/
git commit -m "feat(admin): add token service with localStorage wrapper"
```

---

### Task 2: Auth types

**Files:**
- Create: `app/Admin/src/features/auth/types/auth.ts`
- Modify: `app/Admin/src/features/auth/types/index.ts`

**Interfaces:**
- Produces: `LoginRequest`, `ForgotPasswordRequest`, `ResetPasswordRequest`, `LogoutRequest`, `TokenPair`, `SessionInfo`, `AuthUser`

- [ ] **Step 1: Create auth.ts with all type definitions**

```ts
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

- [ ] **Step 2: Update types/index.ts barrel**

```ts
export * from './auth'
```

- [ ] **Step 3: Update tokenService.ts to use shared TokenPair type**

In `app/Admin/src/features/auth/services/tokenService.ts`, replace the inline `TokenPair` interface with:
```ts
import type { TokenPair } from '../types/auth'
```

- [ ] **Step 4: Verify build and tests pass**

Run: `cd app/Admin && pnpm run build 2>&1 && npx vitest run features/auth/services/__tests__/tokenService.spec.ts 2>&1 | tail -5`

- [ ] **Step 5: Commit**

```bash
git add app/Admin/src/features/auth/types/ app/Admin/src/features/auth/services/tokenService.ts
git commit -m "feat(admin): add auth types"
```

---

### Task 3: Validation schemas

**Files:**
- Create: `app/Admin/src/features/auth/validations/auth.ts`
- Test: `app/Admin/src/features/auth/validations/__tests__/auth.spec.ts`
- Modify: `app/Admin/src/features/auth/validations/index.ts`

**Interfaces:**
- Consumes: Zod 4
- Produces: `emailField`, `credentialField`, `passwordField`, `newPasswordField`, `userIdField`, `tokenField`, `loginSchema`, `forgotPasswordSchema`, `resetPasswordSchema`, `LoginFormValues`, `ForgotPasswordFormValues`, `ResetPasswordFormValues`

- [ ] **Step 1: Write failing tests**

Create `app/Admin/src/features/auth/validations/__tests__/auth.spec.ts`:

```ts
import { describe, it, expect } from 'vitest'
import {
  emailField,
  credentialField,
  passwordField,
  newPasswordField,
  loginSchema,
  forgotPasswordSchema,
  resetPasswordSchema,
} from '../auth'

describe('emailField', () => {
  it('accepts a valid email', () => {
    expect(emailField.safeParse('test@example.com').success).toBe(true)
  })

  it('rejects empty string', () => {
    expect(emailField.safeParse('').success).toBe(false)
  })

  it('rejects invalid email format', () => {
    expect(emailField.safeParse('not-an-email').success).toBe(false)
  })
})

describe('credentialField', () => {
  it('accepts a non-empty string', () => {
    expect(credentialField.safeParse('admin').success).toBe(true)
  })

  it('rejects empty string', () => {
    expect(credentialField.safeParse('').success).toBe(false)
  })
})

describe('passwordField', () => {
  it('accepts a non-empty string', () => {
    expect(passwordField.safeParse('secret').success).toBe(true)
  })

  it('rejects empty string', () => {
    expect(passwordField.safeParse('').success).toBe(false)
  })
})

describe('newPasswordField', () => {
  it('accepts password with 8+ characters', () => {
    expect(newPasswordField.safeParse('password123').success).toBe(true)
  })

  it('rejects password shorter than 8 characters', () => {
    expect(newPasswordField.safeParse('short').success).toBe(false)
  })
})

describe('loginSchema', () => {
  it('accepts valid credential and password', () => {
    const result = loginSchema.safeParse({ credential: 'admin', password: 'pass' })
    expect(result.success).toBe(true)
  })

  it('rejects empty credential', () => {
    const result = loginSchema.safeParse({ credential: '', password: 'pass' })
    expect(result.success).toBe(false)
  })

  it('rejects empty password', () => {
    const result = loginSchema.safeParse({ credential: 'admin', password: '' })
    expect(result.success).toBe(false)
  })

  it('returns error messages on the correct fields', () => {
    const result = loginSchema.safeParse({ credential: '', password: '' })
    if (!result.success) {
      expect(result.error.issues.some(i => i.path[0] === 'credential')).toBe(true)
      expect(result.error.issues.some(i => i.path[0] === 'password')).toBe(true)
    }
  })
})

describe('forgotPasswordSchema', () => {
  it('accepts valid email', () => {
    const result = forgotPasswordSchema.safeParse({ email: 'user@example.com' })
    expect(result.success).toBe(true)
  })

  it('rejects invalid email', () => {
    const result = forgotPasswordSchema.safeParse({ email: 'bad' })
    expect(result.success).toBe(false)
  })
})

describe('resetPasswordSchema', () => {
  it('accepts valid reset data', () => {
    const result = resetPasswordSchema.safeParse({
      email: 'user@example.com',
      userId: 'abc-123',
      token: 'reset-token-xyz',
      newPassword: 'newpassword123',
    })
    expect(result.success).toBe(true)
  })

  it('rejects when token is empty', () => {
    const result = resetPasswordSchema.safeParse({
      email: 'user@example.com',
      userId: 'abc',
      token: '',
      newPassword: 'newpassword123',
    })
    expect(result.success).toBe(false)
  })

  it('rejects short new password', () => {
    const result = resetPasswordSchema.safeParse({
      email: 'user@example.com',
      userId: 'abc',
      token: 'tok',
      newPassword: 'short',
    })
    expect(result.success).toBe(false)
  })
})
```

- [ ] **Step 2: Run tests to verify they fail**

Run: `cd app/Admin && npx vitest run features/auth/validations/__tests__/auth.spec.ts 2>&1`
Expected: All tests fail — file not found.

- [ ] **Step 3: Implement validation schemas**

Create `app/Admin/src/features/auth/validations/auth.ts`:

```ts
import { z } from 'zod'

export const emailField = z.string().min(1, 'Email is required').email('Invalid email address')
export const credentialField = z.string().min(1, 'Email or username is required')
export const passwordField = z.string().min(1, 'Password is required')
export const newPasswordField = z.string().min(8, 'Password must be at least 8 characters')
export const userIdField = z.string().min(1, 'User ID is required')
export const tokenField = z.string().min(1, 'Reset token is required')

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

export type LoginFormValues = z.infer<typeof loginSchema>
export type ForgotPasswordFormValues = z.infer<typeof forgotPasswordSchema>
export type ResetPasswordFormValues = z.infer<typeof resetPasswordSchema>
```

- [ ] **Step 4: Run tests to verify they pass**

Run: `cd app/Admin && npx vitest run features/auth/validations/__tests__/auth.spec.ts 2>&1`
Expected: All 14 tests pass.

- [ ] **Step 5: Update validations/index.ts barrel**

```ts
export * from './auth'
```

- [ ] **Step 6: Commit**

```bash
git add app/Admin/src/features/auth/validations/
git commit -m "feat(admin): add auth validation schemas with Zod"
```

---

### Task 4: Auth API service

**Files:**
- Create: `app/Admin/src/features/auth/services/authApi.ts`
- Test: `app/Admin/src/features/auth/services/__tests__/authApi.spec.ts`

**Interfaces:**
- Consumes: `get`, `post` from `@/shared/api/client`, `Result`, `ok` from `@/shared/types/result`, auth types from `../types/auth`
- Produces: `login()`, `logout()`, `getSession()`, `forgotPassword()`, `resetPassword()`

- [ ] **Step 1: Write failing tests**

Create `app/Admin/src/features/auth/services/__tests__/authApi.spec.ts`:

```ts
import { describe, it, expect, vi, beforeEach } from 'vitest'

const { mockPost, mockGet } = vi.hoisted(() => ({
  mockPost: vi.fn(),
  mockGet: vi.fn(),
}))

vi.mock('@/shared/api/client', () => ({
  post: mockPost,
  get: mockGet,
}))

import { login, logout, getSession, forgotPassword, resetPassword } from '../authApi'

beforeEach(() => {
  vi.clearAllMocks()
})

describe('login', () => {
  it('calls POST login/password with correct body', async () => {
    mockPost.mockResolvedValue({
      value: { accessToken: 'at', accessTokenExpiresIn: 999, refreshToken: 'rt', refreshTokenExpiresIn: 888 },
      isSuccess: true,
      statusCode: 200,
      message: null,
      errors: [],
      metadata: null,
    })

    const result = await login({ credential: 'admin', password: 'pass' })

    expect(mockPost).toHaveBeenCalledWith('api/store/identity/auth/login/password', {
      credential: 'admin',
      password: 'pass',
    })
    expect(result.isSuccess).toBe(true)
    expect(result.value.accessToken).toBe('at')
  })

  it('returns failure when backend returns error', async () => {
    mockPost.mockResolvedValue({
      isSuccess: false,
      statusCode: 401,
      message: 'Invalid credentials',
      errors: [{ code: 'AuthFailed', message: 'Invalid credentials', type: 401 }],
      metadata: null,
      value: null,
    })

    const result = await login({ credential: 'wrong', password: 'wrong' })
    expect(result.isSuccess).toBe(false)
  })
})

describe('logout', () => {
  it('calls POST logout with body', async () => {
    mockPost.mockResolvedValue({
      isSuccess: true,
      statusCode: 200,
      message: null,
      errors: [],
      metadata: null,
      value: null,
    })

    await logout({ revokeAll: true })
    expect(mockPost).toHaveBeenCalledWith('api/store/identity/auth/logout', { revokeAll: true })
  })

  it('calls POST logout with empty body when no args', async () => {
    mockPost.mockResolvedValue({
      isSuccess: true,
      statusCode: 200,
      message: null,
      errors: [],
      metadata: null,
      value: null,
    })

    await logout()
    expect(mockPost).toHaveBeenCalledWith('api/store/identity/auth/logout', undefined)
  })
})

describe('getSession', () => {
  it('calls GET sessions', async () => {
    mockGet.mockResolvedValue({
      value: { id: 'uid-1', roles: ['Admin'], permissions: ['read'] },
      isSuccess: true,
      statusCode: 200,
      message: null,
      errors: [],
      metadata: null,
    })

    const result = await getSession()
    expect(mockGet).toHaveBeenCalledWith('api/store/identity/auth/sessions')
    expect(result.isSuccess).toBe(true)
    expect(result.value.roles).toEqual(['Admin'])
  })
})

describe('forgotPassword', () => {
  it('calls POST passwords/forgot with email', async () => {
    mockPost.mockResolvedValue(undefined)

    await forgotPassword({ email: 'user@example.com' })
    expect(mockPost).toHaveBeenCalledWith('api/store/identity/passwords/forgot', {
      email: 'user@example.com',
    })
  })
})

describe('resetPassword', () => {
  it('calls POST passwords/reset with full body', async () => {
    mockPost.mockResolvedValue({
      isSuccess: true,
      statusCode: 200,
      message: null,
      errors: [],
      metadata: null,
      value: null,
    })

    await resetPassword({
      email: 'u@e.com',
      userId: 'uid',
      token: 'tok',
      newPassword: 'newpass123',
    })

    expect(mockPost).toHaveBeenCalledWith('api/store/identity/passwords/reset', {
      email: 'u@e.com',
      userId: 'uid',
      token: 'tok',
      newPassword: 'newpass123',
    })
  })
})
```

- [ ] **Step 2: Run tests to verify they fail**

Run: `cd app/Admin && npx vitest run features/auth/services/__tests__/authApi.spec.ts 2>&1`
Expected: All tests fail — file not found.

- [ ] **Step 3: Implement authApi.ts**

Create `app/Admin/src/features/auth/services/authApi.ts`:

```ts
import { post, get } from '@/shared/api/client'
import type { Result } from '@/shared/types/result'
import type {
  LoginRequest,
  LogoutRequest,
  ResetPasswordRequest,
  ForgotPasswordRequest,
  TokenPair,
  SessionInfo,
} from '../types/auth'

const AUTH_BASE = 'api/store/identity/auth'
const PASSWORD_BASE = 'api/store/identity/passwords'

export function login(request: LoginRequest): Promise<Result<TokenPair>> {
  return post<Result<TokenPair>>(`${AUTH_BASE}/login/password`, request)
}

export function logout(request?: LogoutRequest): Promise<Result<void>> {
  return post<Result<void>>(`${AUTH_BASE}/logout`, request ?? undefined)
}

export function getSession(): Promise<Result<SessionInfo>> {
  return get<Result<SessionInfo>>(`${AUTH_BASE}/sessions`)
}

export function forgotPassword(request: ForgotPasswordRequest): Promise<void> {
  return post<void>(`${PASSWORD_BASE}/forgot`, request)
}

export function resetPassword(request: ResetPasswordRequest): Promise<Result<void>> {
  return post<Result<void>>(`${PASSWORD_BASE}/reset`, request)
}
```

- [ ] **Step 4: Run tests to verify they pass**

Run: `cd app/Admin && npx vitest run features/auth/services/__tests__/authApi.spec.ts 2>&1`
Expected: All 7 tests pass.

- [ ] **Step 5: Commit**

```bash
git add app/Admin/src/features/auth/services/authApi.ts app/Admin/src/features/auth/services/__tests__/authApi.spec.ts
git commit -m "feat(admin): add auth API service"
```

---

### Task 5: Fix refresh interceptor URL

**Files:**
- Modify: `app/Admin/src/shared/api/interceptors/refresh.ts:4`
- Modify: `app/Admin/src/shared/api/__tests__/refresh.spec.ts:17`

**Interfaces:**
- Produces: Corrected refresh URL (`/api/store/identity/auth/sessions/refresh`)

- [ ] **Step 1: Fix the refresh URL**

In `app/Admin/src/shared/api/interceptors/refresh.ts`, change line 4:
```ts
// Before:
let refreshUrl = '/api/identity/auth/sessions/refresh'
// After:
let refreshUrl = '/api/store/identity/auth/sessions/refresh'
```

No other changes — the interceptor keeps using localStorage directly. The token service is for the store's use only; the interceptor is an independent concern.

- [ ] **Step 2: Update refresh.spec.ts**

In the test file, update the refresh URL in `beforeEach`:
```ts
setRefreshUrl('/api/store/identity/auth/sessions/refresh')
```

- [ ] **Step 3: Run refresh tests**

Run: `cd app/Admin && npx vitest run shared/api/__tests__/refresh.spec.ts 2>&1`
Expected: All existing refresh tests pass.

- [ ] **Step 4: Verify full build and test suite**

Run: `cd app/Admin && pnpm run build 2>&1 && pnpm run test:unit -- run 2>&1 | tail -5`
Expected: Build passes, all tests pass.

- [ ] **Step 5: Commit**

```bash
git add app/Admin/src/shared/api/interceptors/refresh.ts app/Admin/src/shared/api/__tests__/refresh.spec.ts
git commit -m "fix(admin): correct refresh token URL to /api/store/identity/auth/sessions/refresh"
```

---

### Task 6: Use STORAGE_KEYS constant in client.ts

**Files:**
- Modify: `app/Admin/src/shared/api/client.ts:8-14`

**Interfaces:**
- Consumes: `STORAGE_KEYS` from `@/shared/constants/storage`
- Produces: `setAuthToken` uses the constant instead of hardcoded `'accessToken'`

- [ ] **Step 1: Replace hardcoded 'accessToken' with STORAGE_KEYS constant**

In `app/Admin/src/shared/api/client.ts`, add the import and replace the hardcoded string:

```ts
import { STORAGE_KEYS } from '@/shared/constants/storage'

export function setAuthToken(token: string | null): void {
  if (token) {
    localStorage.setItem(STORAGE_KEYS.ACCESS_TOKEN, token)
  } else {
    localStorage.removeItem(STORAGE_KEYS.ACCESS_TOKEN)
  }
}
```

The function stays in shared/ — it uses localStorage directly (same as the interceptor). The token service in features/auth is for store-level token management.

- [ ] **Step 2: Verify build passes**

Run: `cd app/Admin && pnpm run build 2>&1`

- [ ] **Step 3: Commit**

```bash
git add app/Admin/src/shared/api/client.ts
git commit -m "refactor(admin): use STORAGE_KEYS constant in client.ts setAuthToken"
```

---

### Task 7: Pinia auth store

**Files:**
- Create: `app/Admin/src/features/auth/stores/authStore.ts`
- Test: `app/Admin/src/features/auth/stores/__tests__/authStore.spec.ts`

**Interfaces:**
- Consumes: `tokenService`, `authApi`, auth types, `setTokenGetter` from `@/shared/api/interceptors/auth`
- Produces: `useAuthStore` — Pinia store with state, getters, actions

- [ ] **Step 1: Write failing tests**

Create `app/Admin/src/features/auth/stores/__tests__/authStore.spec.ts`:

```ts
import { describe, it, expect, vi, beforeEach } from 'vitest'
import { setActivePinia, createPinia } from 'pinia'

const { mockLogin, mockLogout, mockGetSession } = vi.hoisted(() => ({
  mockLogin: vi.fn(),
  mockLogout: vi.fn(),
  mockGetSession: vi.fn(),
}))

vi.mock('../../services/authApi', () => ({
  login: mockLogin,
  logout: mockLogout,
  getSession: mockGetSession,
}))

vi.mock('../../services/tokenService', () => ({
  getAccessToken: vi.fn(() => 'access-token'),
  getRefreshToken: vi.fn(() => 'refresh-token'),
  setTokens: vi.fn(),
  clearTokens: vi.fn(),
  hasValidAccessToken: vi.fn(() => true),
}))

import { useAuthStore } from '../authStore'

function makeSuccessResult<T>(value: T) {
  return { isSuccess: true, statusCode: 200, message: null, errors: [], metadata: null, value }
}

function makeFailureResult(message: string) {
  return {
    isSuccess: false,
    statusCode: 401,
    message,
    errors: [{ code: 'Error', message, type: 401 }],
    metadata: null,
    value: null,
  }
}

beforeEach(() => {
  setActivePinia(createPinia())
  vi.clearAllMocks()
})

describe('authStore', () => {
  describe('initial state', () => {
    it('has idle status and null user', () => {
      const store = useAuthStore()
      expect(store.status).toBe('idle')
      expect(store.user).toBeNull()
      expect(store.error).toBeNull()
    })

    it('isAuthenticated is false', () => {
      const store = useAuthStore()
      expect(store.isAuthenticated).toBe(false)
    })
  })

  describe('login', () => {
    it('sets status to authenticated on success', async () => {
      mockLogin.mockResolvedValue(makeSuccessResult({
        accessToken: 'at', accessTokenExpiresIn: 99, refreshToken: 'rt', refreshTokenExpiresIn: 88,
      }))
      mockGetSession.mockResolvedValue(makeSuccessResult({
        id: 'uid', roles: ['Admin'], permissions: ['read'],
      }))

      const store = useAuthStore()
      await store.login('admin', 'pass')

      expect(store.status).toBe('authenticated')
      expect(store.user?.userId).toBe('uid')
      expect(store.user?.roles).toEqual(['Admin'])
    })

    it('sets status to error on failure', async () => {
      mockLogin.mockResolvedValue(makeFailureResult('Invalid credentials'))

      const store = useAuthStore()
      await store.login('wrong', 'wrong')

      expect(store.status).toBe('error')
      expect(store.error).toBe('Invalid credentials')
    })
  })

  describe('logout', () => {
    it('resets state to idle', async () => {
      const store = useAuthStore()
      store.$patch({ user: { userId: 'x', roles: [], permissions: [], isAuthenticated: true }, status: 'authenticated' })

      mockLogout.mockResolvedValue({ isSuccess: true })

      await store.logout()

      expect(store.status).toBe('idle')
      expect(store.user).toBeNull()
    })
  })
})
```

- [ ] **Step 2: Run tests to verify they fail**

Run: `cd app/Admin && npx vitest run features/auth/stores/__tests__/authStore.spec.ts 2>&1`
Expected: All tests fail — store not defined.

- [ ] **Step 3: Implement authStore.ts**

Create `app/Admin/src/features/auth/stores/authStore.ts`:

```ts
import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import type { AuthUser } from '../types/auth'
import * as authApi from '../services/authApi'
import * as tokenService from '../services/tokenService'
import { setTokenGetter } from '@/shared/api/interceptors/auth'

export const useAuthStore = defineStore('auth', () => {
  const user = ref<AuthUser | null>(null)
  const status = ref<'idle' | 'loading' | 'authenticated' | 'error'>('idle')
  const error = ref<string | null>(null)

  const isAuthenticated = computed(() => status.value === 'authenticated' && user.value !== null)

  const currentUser = computed(() => user.value)

  function hasRole(role: string): boolean {
    return user.value?.roles.includes(role) ?? false
  }

  function hasPermission(perm: string): boolean {
    return user.value?.permissions.includes(perm) ?? false
  }

  async function login(credential: string, password: string): Promise<void> {
    status.value = 'loading'
    error.value = null

    const result = await authApi.login({ credential, password })

    if (result.isSuccess) {
      tokenService.setTokens(result.value)
      setTokenGetter(tokenService.getAccessToken)

      const sessionResult = await authApi.getSession()
      if (sessionResult.isSuccess) {
        user.value = {
          userId: sessionResult.value.id,
          roles: sessionResult.value.roles,
          permissions: sessionResult.value.permissions,
          isAuthenticated: true,
        }
        status.value = 'authenticated'
      } else {
        status.value = 'error'
        error.value = 'Failed to fetch session'
      }
    } else {
      status.value = 'error'
      error.value = result.message ?? result.errors[0]?.message ?? 'Login failed'
    }
  }

  async function logout(revokeAll?: boolean): Promise<void> {
    try {
      await authApi.logout({ revokeAll })
    } catch {
      // Fire-and-forget — always clear local state
    }

    tokenService.clearTokens()
    user.value = null
    status.value = 'idle'
    error.value = null
  }

  async function init(): Promise<void> {
    if (!tokenService.hasValidAccessToken()) {
      status.value = 'idle'
      return
    }

    try {
      const sessionResult = await authApi.getSession()
      if (sessionResult.isSuccess) {
        user.value = {
          userId: sessionResult.value.id,
          roles: sessionResult.value.roles,
          permissions: sessionResult.value.permissions,
          isAuthenticated: true,
        }
        status.value = 'authenticated'
      } else {
        tokenService.clearTokens()
        status.value = 'idle'
      }
    } catch {
      tokenService.clearTokens()
      status.value = 'idle'
    }
  }

  async function fetchSession(): Promise<void> {
    const result = await authApi.getSession()
    if (result.isSuccess) {
      user.value = {
        userId: result.value.id,
        roles: result.value.roles,
        permissions: result.value.permissions,
        isAuthenticated: true,
      }
      status.value = 'authenticated'
    }
  }

  return {
    user,
    status,
    error,
    isAuthenticated,
    currentUser,
    hasRole,
    hasPermission,
    login,
    logout,
    init,
    fetchSession,
  }
})
```

- [ ] **Step 4: Run tests to verify they pass**

Run: `cd app/Admin && npx vitest run features/auth/stores/__tests__/authStore.spec.ts 2>&1`
Expected: All 5 tests pass.

- [ ] **Step 5: Commit**

```bash
git add app/Admin/src/features/auth/stores/
git commit -m "feat(admin): add auth Pinia store with login/logout/init"
```

---

### Task 8: Auth routes + views

**Files:**
- Modify: `app/Admin/src/features/auth/routes/index.ts`
- Modify: `app/Admin/src/features/auth/views/LoginPage.vue`
- Create: `app/Admin/src/features/auth/views/ForgotPasswordPage.vue`
- Create: `app/Admin/src/features/auth/views/ResetPasswordPage.vue`
- Modify: `app/Admin/src/features/auth/views/index.ts`

**Interfaces:**
- Consumes: `useAuthStore`, validation schemas, `PageShell` from `@ui/PageShell.vue`, PrimeVue components, vee-validate
- Produces: Three working auth pages, three routes

- [ ] **Step 1: Update routes/index.ts**

Replace `app/Admin/src/features/auth/routes/index.ts`:

```ts
import type { RouteRecordRaw } from 'vue-router'

const LoginPage = () => import('../views/LoginPage.vue')
const ForgotPasswordPage = () => import('../views/ForgotPasswordPage.vue')
const ResetPasswordPage = () => import('../views/ResetPasswordPage.vue')

export const authRoutes: RouteRecordRaw[] = [
  {
    path: 'login',
    name: 'login',
    component: LoginPage,
    meta: { title: 'Sign In', requiresAuth: false },
  },
  {
    path: 'forgot-password',
    name: 'forgot-password',
    component: ForgotPasswordPage,
    meta: { title: 'Forgot Password', requiresAuth: false },
  },
  {
    path: 'reset-password',
    name: 'reset-password',
    component: ResetPasswordPage,
    meta: { title: 'Reset Password', requiresAuth: false },
  },
]

export const authMenuItems: Array<Record<string, unknown>> = []
```

- [ ] **Step 2: Create LoginPage.vue** (replaces placeholder)

```vue
<script setup lang="ts">
import { computed } from 'vue'
import { useRouter } from 'vue-router'
import { useForm } from 'vee-validate'
import { toTypedSchema } from '@vee-validate/zod'
import { loginSchema } from '../validations/auth'
import { useAuthStore } from '../stores/authStore'
import PageShell from '@ui/PageShell.vue'
import Button from 'primevue/button'
import InputText from 'primevue/inputtext'
import Password from 'primevue/password'
import Message from 'primevue/message'

const router = useRouter()
const store = useAuthStore()

const { defineField, errors, handleSubmit } = useForm({
  validationSchema: toTypedSchema(loginSchema),
})

const [credential, credentialAttrs] = defineField('credential', { validateOnModelUpdate: false })
const [password, passwordAttrs] = defineField('password', { validateOnModelUpdate: false })

const isLoading = computed(() => store.status === 'loading')
const authError = computed(() => store.error)

const onSubmit = handleSubmit(async (values) => {
  await store.login(values.credential, values.password)
  if (store.isAuthenticated) {
    router.replace('/')
  }
})
</script>

<template>
  <PageShell title="Sign In">
    <form @submit="onSubmit" class="flex flex-col gap-4 max-w-md mx-auto">
      <div class="flex flex-col gap-2">
        <label for="credential" class="text-sm font-medium">Email or Username</label>
        <InputText id="credential" v-model="credential" v-bind="credentialAttrs" autocomplete="username" class="w-full" :invalid="!!errors.credential" />
        <small v-if="errors.credential" class="text-red-500">{{ errors.credential }}</small>
      </div>

      <div class="flex flex-col gap-2">
        <label for="password" class="text-sm font-medium">Password</label>
        <Password id="password" v-model="password" v-bind="passwordAttrs" autocomplete="current-password" class="w-full" :feedback="false" :invalid="!!errors.password" toggleMask />
        <small v-if="errors.password" class="text-red-500">{{ errors.password }}</small>
      </div>

      <Message v-if="authError" severity="error" :closable="false">{{ authError }}</Message>

      <Button type="submit" label="Sign In" severity="primary" :loading="isLoading" />

      <router-link to="/auth/forgot-password" class="text-sm text-primary hover:underline text-center">
        Forgot password?
      </router-link>
    </form>
  </PageShell>
</template>
```

- [ ] **Step 3: Create ForgotPasswordPage.vue**

```vue
<script setup lang="ts">
import { ref } from 'vue'
import { useForm } from 'vee-validate'
import { toTypedSchema } from '@vee-validate/zod'
import { forgotPasswordSchema } from '../validations/auth'
import { forgotPassword } from '../services/authApi'
import PageShell from '@ui/PageShell.vue'
import Button from 'primevue/button'
import InputText from 'primevue/inputtext'
import Message from 'primevue/message'

const { defineField, errors, handleSubmit } = useForm({
  validationSchema: toTypedSchema(forgotPasswordSchema),
})

const [email, emailAttrs] = defineField('email', { validateOnModelUpdate: false })

const isSubmitting = ref(false)
const isSuccess = ref(false)
const submitError = ref<string | null>(null)

const onSubmit = handleSubmit(async (values) => {
  isSubmitting.value = true
  submitError.value = null
  try {
    await forgotPassword({ email: values.email })
    isSuccess.value = true
  } catch {
    submitError.value = 'Something went wrong. Please try again.'
  } finally {
    isSubmitting.value = false
  }
})
</script>

<template>
  <PageShell title="Forgot Password">
    <router-link to="/auth/login" class="text-sm text-primary hover:underline">&larr; Back to login</router-link>

    <p v-if="isSuccess" class="mt-4 text-green-600">
      If an account exists with that email, a reset link has been sent.
    </p>

    <form v-else @submit="onSubmit" class="flex flex-col gap-4 max-w-md mx-auto mt-4">
      <div class="flex flex-col gap-2">
        <label for="email" class="text-sm font-medium">Email</label>
        <InputText id="email" v-model="email" v-bind="emailAttrs" autocomplete="email" class="w-full" :invalid="!!errors.email" />
        <small v-if="errors.email" class="text-red-500">{{ errors.email }}</small>
      </div>

      <Message v-if="submitError" severity="error" :closable="false">{{ submitError }}</Message>

      <Button type="submit" label="Send Reset Link" severity="primary" :loading="isSubmitting" />
    </form>
  </PageShell>
</template>
```

- [ ] **Step 4: Create ResetPasswordPage.vue**

```vue
<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useForm } from 'vee-validate'
import { toTypedSchema } from '@vee-validate/zod'
import { useToast } from 'primevue/usetoast'
import { resetPasswordSchema } from '../validations/auth'
import { resetPassword } from '../services/authApi'
import PageShell from '@ui/PageShell.vue'
import Button from 'primevue/button'
import InputText from 'primevue/inputtext'
import Password from 'primevue/password'
import Message from 'primevue/message'

const route = useRoute()
const router = useRouter()
const toast = useToast()

const { defineField, errors, handleSubmit, setFieldValue } = useForm({
  validationSchema: toTypedSchema(resetPasswordSchema),
})

const [email] = defineField('email', { validateOnModelUpdate: false })
const [userId] = defineField('userId', { validateOnModelUpdate: false })
const [token, tokenAttrs] = defineField('token', { validateOnModelUpdate: false })
const [newPassword, newPasswordAttrs] = defineField('newPassword', { validateOnModelUpdate: false })

const isSubmitting = ref(false)
const formError = ref<string | null>(null)

onMounted(() => {
  const q = route.query as Record<string, string>
  setFieldValue('email', q.email ?? '')
  setFieldValue('userId', q.userId ?? '')
  setFieldValue('token', q.token ?? '')
})

const onSubmit = handleSubmit(async (values) => {
  isSubmitting.value = true
  formError.value = null
  try {
    const result = await resetPassword({
      email: values.email,
      userId: values.userId,
      token: values.token,
      newPassword: values.newPassword,
    })
    if (result.isSuccess) {
      toast.add({ severity: 'success', summary: 'Password reset successful', life: 5000 })
      router.push('/auth/login')
    } else {
      formError.value = result.message ?? 'Invalid or expired reset link'
    }
  } catch {
    formError.value = 'Invalid or expired reset link'
  } finally {
    isSubmitting.value = false
  }
})
</script>

<template>
  <PageShell title="Set New Password">
    <form @submit="onSubmit" class="flex flex-col gap-4 max-w-md mx-auto">
      <div class="flex flex-col gap-2">
        <label for="email" class="text-sm font-medium">Email</label>
        <InputText id="email" :modelValue="email" disabled class="w-full" />
      </div>

      <div class="flex flex-col gap-2">
        <label for="userId" class="text-sm font-medium">User ID</label>
        <InputText id="userId" :modelValue="userId" disabled class="w-full" />
      </div>

      <div class="flex flex-col gap-2">
        <label for="token" class="text-sm font-medium">Reset Token</label>
        <InputText id="token" v-model="token" v-bind="tokenAttrs" class="w-full" :invalid="!!errors.token" />
        <small v-if="errors.token" class="text-red-500">{{ errors.token }}</small>
      </div>

      <div class="flex flex-col gap-2">
        <label for="newPassword" class="text-sm font-medium">New Password</label>
        <Password id="newPassword" v-model="newPassword" v-bind="newPasswordAttrs" class="w-full" :feedback="false" :invalid="!!errors.newPassword" toggleMask />
        <small v-if="errors.newPassword" class="text-red-500">{{ errors.newPassword }}</small>
      </div>

      <Message v-if="formError" severity="error" :closable="false">{{ formError }}</Message>

      <Button type="submit" label="Reset Password" severity="primary" :loading="isSubmitting" />
    </form>
  </PageShell>
</template>
```

- [ ] **Step 5: Update views/index.ts barrel**

```ts
export { default as LoginPage } from './LoginPage.vue'
export { default as ForgotPasswordPage } from './ForgotPasswordPage.vue'
export { default as ResetPasswordPage } from './ResetPasswordPage.vue'
```

- [ ] **Step 6: Verify build passes**

Run: `cd app/Admin && pnpm run build 2>&1`
Expected: Build succeeds with zero errors.

- [ ] **Step 7: Commit**

```bash
git add app/Admin/src/features/auth/routes/ app/Admin/src/features/auth/views/
git commit -m "feat(admin): add auth route pages — login, forgot-password, reset-password"
```

---

### Task 9: Wire store init into router guard

**Files:**
- Modify: `app/Admin/src/app/router/guards.ts`

**Interfaces:**
- Consumes: `useAuthStore`
- Produces: Updated guard that calls `authStore.init()` before route resolution

- [ ] **Step 1: Update guards.ts**

Replace `app/Admin/src/app/router/guards.ts`:

```ts
import type { Router } from 'vue-router'
import { useAuthStore } from '@/features/auth/stores/authStore'

let isInitialized = false

export function setupGuards(router: Router): void {
  router.beforeEach(async (to, _from, next) => {
    const store = useAuthStore()

    if (!isInitialized) {
      await store.init()
      isInitialized = true
    }

    // TODO: re-enable auth guard after route scaffold review
    // if (to.meta.requiresAuth && !store.isAuthenticated) {
    //   return next({ name: 'login', query: { redirect: to.fullPath } })
    // }

    next()
  })

  router.afterEach((to) => {
    if (to.meta.title) {
      document.title = `${to.meta.title} | ReSys.Shop`
    }
  })
}
```

- [ ] **Step 2: Verify build passes**

Run: `cd app/Admin && pnpm run build 2>&1`

- [ ] **Step 3: Commit**

```bash
git add app/Admin/src/app/router/guards.ts
git commit -m "feat(admin): wire auth store init into router guard"
```

---

### Task 10: Barrel exports for auth module

**Files:**
- Modify: `app/Admin/src/features/auth/services/index.ts`
- Modify: `app/Admin/src/features/auth/stores/index.ts`
- Modify: `app/Admin/src/features/auth/index.ts`

- [ ] **Step 1: Update services/index.ts**

```ts
export * from './authApi'
export * from './tokenService'
```

- [ ] **Step 2: Update stores/index.ts**

```ts
export { useAuthStore } from './authStore'
```

- [ ] **Step 3: Update auth root index.ts** (already has barrel from scaffold, verify it re-exports all subfolders)

Read current `features/auth/index.ts` and ensure it re-exports all 8 subfolders.

- [ ] **Step 4: Verify build and full test suite**

Run: `cd app/Admin && pnpm run build 2>&1 && pnpm run test:unit -- run 2>&1 | tail -5`
Expected: Build zero errors, all tests pass (307+ new tests).

- [ ] **Step 5: Commit**

```bash
git add app/Admin/src/features/auth/services/index.ts app/Admin/src/features/auth/stores/index.ts
git commit -m "chore(admin): add auth module barrel exports"
```

---

### Task 11: Final verification

**Files:** None (verification only)

- [ ] **Step 1: Full build**

```bash
cd app/Admin && pnpm run build 2>&1
```
Expected: zero errors.

- [ ] **Step 2: Full test suite**

```bash
cd app/Admin && pnpm run test:unit -- run 2>&1
```
Expected: all tests passing.

- [ ] **Step 3: Verify route paths**

Confirm these routes load without errors:
- `/auth/login` — login form
- `/auth/forgot-password` — forgot password form
- `/auth/reset-password?email=x&userId=x&token=x` — reset form

- [ ] **Step 4: Commit**

```bash
git commit -m "chore(admin): final verification of auth module" --allow-empty
```
