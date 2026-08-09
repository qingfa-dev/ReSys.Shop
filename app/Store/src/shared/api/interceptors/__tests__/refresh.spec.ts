import { describe, it, expect, vi, beforeEach } from 'vitest'

const { postMock } = vi.hoisted(() => ({ postMock: vi.fn<(...args: unknown[]) => unknown>() }))

vi.mock('axios', () => ({
  default: {
    create: vi.fn<(...args: unknown[]) => unknown>(() => ({ post: postMock })),
  },
}))

import { handleTokenRefresh } from '../refresh'
import { STORAGE_KEYS } from '@/shared/constants/storage'

const REFRESH_URL = 'api/storefront/identity/auth/sessions/refresh'

describe('handleTokenRefresh', () => {
  beforeEach(() => {
    localStorage.clear()
    postMock.mockReset()
  })

  it('rotates both tokens and returns the new access token', async () => {
    localStorage.setItem(STORAGE_KEYS.REFRESH_TOKEN, 'old-refresh')
    postMock.mockResolvedValue({
      data: { accessToken: 'acc-1', refreshToken: 'refresh-2' },
    })

    const token = await handleTokenRefresh()

    expect(token).toBe('acc-1')
    expect(localStorage.getItem(STORAGE_KEYS.ACCESS_TOKEN)).toBe('acc-1')
    expect(localStorage.getItem(STORAGE_KEYS.REFRESH_TOKEN)).toBe('refresh-2')
    expect(postMock).toHaveBeenCalledTimes(1)
    expect(postMock).toHaveBeenCalledWith(REFRESH_URL, { refreshToken: 'old-refresh' })
  })

  it('single-flights concurrent refresh requests', async () => {
    localStorage.setItem(STORAGE_KEYS.REFRESH_TOKEN, 'old-refresh')
    let resolvePost: (value: unknown) => void = () => {}
    postMock.mockImplementation(
      () => new Promise((resolve) => { resolvePost = resolve }),
    )

    const p1 = handleTokenRefresh()
    const p2 = handleTokenRefresh()

    // Only one network call issued for the two queued requests.
    expect(postMock).toHaveBeenCalledTimes(1)

    resolvePost({ data: { accessToken: 'acc-1', refreshToken: 'refresh-2' } })

    await expect(p1).resolves.toBe('acc-1')
    await expect(p2).resolves.toBe('acc-1')
  })

  it('clears tokens and rejects when the refresh request fails', async () => {
    localStorage.setItem(STORAGE_KEYS.ACCESS_TOKEN, 'stale-access')
    localStorage.setItem(STORAGE_KEYS.REFRESH_TOKEN, 'stale-refresh')
    postMock.mockRejectedValue(new Error('network'))

    await expect(handleTokenRefresh()).rejects.toThrow('network')

    expect(localStorage.getItem(STORAGE_KEYS.ACCESS_TOKEN)).toBeNull()
    expect(localStorage.getItem(STORAGE_KEYS.REFRESH_TOKEN)).toBeNull()
  })

  it('rejects when no refresh token is available and never calls the API', async () => {
    await expect(handleTokenRefresh()).rejects.toThrow('No refresh token available')
    expect(postMock).not.toHaveBeenCalled()
  })
})
