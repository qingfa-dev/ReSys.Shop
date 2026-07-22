import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest'
import { setActivePinia, createPinia } from 'pinia'
import { useAuthStore } from '../auth.store'
import { useSessionStore } from '@/stores/useSessionStore'
import * as authApi from '../../api/auth.api'

const mockRouterPush = vi.hoisted(() => vi.fn<(...args: any[]) => any>())

vi.mock('../../api/auth.api', () => ({
  loginApi: vi.fn<(...args: any[]) => any>(),
  registerApi: vi.fn<(...args: any[]) => any>(),
  forgotPasswordApi: vi.fn<(...args: any[]) => any>(),
  resetPasswordApi: vi.fn<(...args: any[]) => any>(),
  changePasswordApi: vi.fn<(...args: any[]) => any>(),
  logoutApi: vi.fn<(...args: any[]) => any>(),
  getSessionApi: vi.fn<(...args: any[]) => any>(),
}))

vi.mock('vue-router', () => ({
  useRouter: () => ({ push: mockRouterPush, currentRoute: { value: { query: {} } } }),
}))

function createTestToken(exp: number, extra: Record<string, unknown> = {}): string {
  const header = btoa(JSON.stringify({ alg: 'none', typ: 'JWT' }))
  const payload = btoa(JSON.stringify({ sub: '1', email: 'a@b.com', name: 'Test', role: 'admin', permissions: [], exp, ...extra }))
  return `${header}.${payload}.`
}

function successResult(value: unknown): any {
  return { isSuccess: true, statusCode: 200, value, errors: [], message: null, metadata: null }
}

function errorResult(errors: Array<{ code: string; message: string; type: number; metadata: null }>): any {
  return { isSuccess: false, statusCode: 400, value: null, errors, message: null, metadata: null }
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
      let resolver!: (value: unknown) => void
      vi.mocked(authApi.loginApi).mockImplementation(
        () => new Promise(resolve => { resolver = resolve }) as any,
      )
      const store = useAuthStore()
      const promise = store.login('cred', 'pass')
      expect(store.isLoading).toBe(true)
      resolver(successResult({ accessToken: 'at', refreshToken: 'rt', accessTokenExpiresIn: 0, refreshTokenExpiresIn: 0 }))
      await promise
    })

    it('hydrates session on success', async () => {
      vi.mocked(authApi.loginApi).mockResolvedValue(successResult({ accessToken: createTestToken(9999999999), refreshToken: 'rt', accessTokenExpiresIn: 9999999999, refreshTokenExpiresIn: 9999999999 }))

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
      expect(store.serverErrors[0]?.code).toBe('User.Credentials.Invalid')
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
      localStorage.setItem('accessToken', createTestToken(9999999999))
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

  describe('forgotPassword', () => {
    it('cleans state on success', async () => {
      vi.mocked(authApi.forgotPasswordApi).mockResolvedValue(successResult(null))
      const store = useAuthStore()
      await store.forgotPassword('test@example.com')

      expect(store.isLoading).toBe(false)
      expect(store.serverErrors).toHaveLength(0)
      expect(store.fieldErrors).toEqual({})
    })

    it('populates serverErrors on failure', async () => {
      vi.mocked(authApi.forgotPasswordApi).mockResolvedValue(errorResult([
        { code: 'User.Email.NotFound', message: 'Email not found', type: 404, metadata: null },
      ]))
      const store = useAuthStore()
      await store.forgotPassword('test@example.com')

      expect(store.serverErrors).toHaveLength(1)
      expect(store.serverErrors[0]?.code).toBe('User.Email.NotFound')
      expect(store.isLoading).toBe(false)
    })
  })

  describe('resetPassword', () => {
    it('navigates to login on success', async () => {
      vi.mocked(authApi.resetPasswordApi).mockResolvedValue(successResult(null))
      const store = useAuthStore()
      await store.resetPassword({ email: 'test@example.com', userId: '1', token: 'tok', newPassword: 'NewPass123!' })

      expect(mockRouterPush).toHaveBeenCalledWith({ name: 'auth.login' })
      expect(store.isLoading).toBe(false)
      expect(store.serverErrors).toHaveLength(0)
    })

    it('populates serverErrors on failure', async () => {
      vi.mocked(authApi.resetPasswordApi).mockResolvedValue(errorResult([
        { code: 'User.Token.Expired', message: 'Token expired', type: 400, metadata: null },
      ]))
      const store = useAuthStore()
      await store.resetPassword({ email: 'test@example.com', userId: '1', token: 'tok', newPassword: 'NewPass123!' })

      expect(store.serverErrors).toHaveLength(1)
      expect(store.serverErrors[0]?.code).toBe('User.Token.Expired')
      expect(store.isLoading).toBe(false)
    })
  })

  describe('changePassword', () => {
    it('navigates to dashboard on success', async () => {
      vi.mocked(authApi.changePasswordApi).mockResolvedValue(successResult(null))
      const store = useAuthStore()
      await store.changePassword({ email: 'test@example.com', currentPassword: 'OldPass123!', newPassword: 'NewPass123!' })

      expect(mockRouterPush).toHaveBeenCalledWith({ name: 'reports.dashboard' })
      expect(store.isLoading).toBe(false)
      expect(store.serverErrors).toHaveLength(0)
    })

    it('populates serverErrors on failure', async () => {
      vi.mocked(authApi.changePasswordApi).mockResolvedValue(errorResult([
        { code: 'User.Password.Incorrect', message: 'Current password is incorrect', type: 400, metadata: null },
      ]))
      const store = useAuthStore()
      await store.changePassword({ email: 'test@example.com', currentPassword: 'WrongPass!', newPassword: 'NewPass123!' })

      expect(store.serverErrors).toHaveLength(1)
      expect(store.serverErrors[0]?.code).toBe('User.Password.Incorrect')
      expect(store.isLoading).toBe(false)
    })
  })
})
