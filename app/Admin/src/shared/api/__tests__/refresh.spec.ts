import { describe, it, expect, vi, beforeEach } from 'vitest'
import { handleTokenRefresh, setRefreshUrl } from '../interceptors/refresh'

const { mockPost } = vi.hoisted(() => ({ mockPost: vi.fn<(...args: unknown[]) => unknown>() }))

vi.mock('axios', () => ({
  default: {
    create: vi.fn<(...args: unknown[]) => unknown>(() => ({ post: mockPost })),
    isCancel: vi.fn<(...args: unknown[]) => unknown>(() => false),
    isAxiosError: vi.fn<(...args: unknown[]) => unknown>(() => false),
  },
}))

beforeEach(() => {
  vi.clearAllMocks()
  localStorage.clear()
  setRefreshUrl('/api/storefront/identity/auth/sessions/refresh')
})

describe('handleTokenRefresh', () => {
  it('posts refresh token and stores new tokens', async () => {
    localStorage.setItem('refreshToken', 'old-refresh')

    mockPost.mockResolvedValue({
      data: {
        value: {
          accessToken: 'new-access',
          refreshToken: 'new-refresh',
        },
      },
    })

    const token = await handleTokenRefresh()
    expect(token).toBe('new-access')
    expect(localStorage.getItem('accessToken')).toBe('new-access')
    expect(localStorage.getItem('refreshToken')).toBe('new-refresh')
    expect(mockPost).toHaveBeenCalledWith('/api/storefront/identity/auth/sessions/refresh', {
      refreshToken: 'old-refresh',
    })
  })

  it('handles response without nested value wrapper', async () => {
    localStorage.setItem('refreshToken', 'old-refresh')

    mockPost.mockResolvedValue({
      data: { accessToken: 'new-access', refreshToken: 'new-refresh' },
    })

    const token = await handleTokenRefresh()
    expect(token).toBe('new-access')
  })

  it('throws when no refresh token is available', async () => {
    localStorage.removeItem('refreshToken')

    await expect(handleTokenRefresh()).rejects.toThrow('No refresh token available')
    expect(mockPost).not.toHaveBeenCalled()
  })

  it('throws when response has no access token', async () => {
    localStorage.setItem('refreshToken', 'old-refresh')

    mockPost.mockResolvedValue({ data: { value: {} } })

    await expect(handleTokenRefresh()).rejects.toThrow('no access token')
  })

  it('clears tokens on failure', async () => {
    localStorage.setItem('accessToken', 'old-access')
    localStorage.setItem('refreshToken', 'old-refresh')

    mockPost.mockRejectedValue(new Error('Network error'))

    await expect(handleTokenRefresh()).rejects.toThrow('Network error')
    expect(localStorage.getItem('accessToken')).toBeNull()
    expect(localStorage.getItem('refreshToken')).toBeNull()
  })

  it('queues concurrent calls and resolves with same token', async () => {
    localStorage.setItem('refreshToken', 'old-refresh')

    let resolvePost: (v: unknown) => void
    mockPost.mockImplementation(() => new Promise(resolve => { resolvePost = resolve }))

    const promise1 = handleTokenRefresh()
    const promise2 = handleTokenRefresh()

    resolvePost!({
      data: { value: { accessToken: 'new-access', refreshToken: 'new-refresh' } },
    })

    const [token1, token2] = await Promise.all([promise1, promise2])
    expect(token1).toBe('new-access')
    expect(token2).toBe('new-access')
    expect(mockPost).toHaveBeenCalledTimes(1)
  })

  it('rejects all queued calls on failure', async () => {
    localStorage.setItem('refreshToken', 'old-refresh')

    mockPost.mockRejectedValue(new Error('Network error'))

    const promise1 = handleTokenRefresh()
    const promise2 = handleTokenRefresh()

    await expect(promise1).rejects.toThrow('Network error')
    await expect(promise2).rejects.toThrow('Network error')
    expect(mockPost).toHaveBeenCalledTimes(1)
  })

  it('uses configurable refresh URL', async () => {
    localStorage.setItem('refreshToken', 'old-refresh')
    setRefreshUrl('/custom/refresh')

    mockPost.mockResolvedValue({ data: { accessToken: 'new' } })

    await handleTokenRefresh()
    expect(mockPost).toHaveBeenCalledWith('/custom/refresh', { refreshToken: 'old-refresh' })
  })
})
