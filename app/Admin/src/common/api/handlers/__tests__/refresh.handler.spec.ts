import { describe, it, expect, vi, beforeEach } from 'vitest'
import axios from 'axios'

vi.mock('axios')
vi.mock('@/common/auth/token.service', () => ({
  tokenService: {
    getRefreshToken: vi.fn(),
    getAccessToken: vi.fn(),
    setTokens: vi.fn(),
    clearTokens: vi.fn(),
    hasTokens: vi.fn(),
  },
}))

const mockLocation = { href: '' }
Object.defineProperty(window, 'location', {
  value: mockLocation,
  writable: true,
})

describe('refreshTokens', () => {
  let tokenService: {
    getRefreshToken: ReturnType<typeof vi.fn>
    getAccessToken: ReturnType<typeof vi.fn>
    setTokens: ReturnType<typeof vi.fn>
    clearTokens: ReturnType<typeof vi.fn>
    hasTokens: ReturnType<typeof vi.fn>
  }

  beforeEach(async () => {
    vi.clearAllMocks()
    mockLocation.href = ''

    const mod = await import('@/common/auth/token.service')
    tokenService = mod.tokenService as unknown as typeof tokenService
  })

  it('returns false and redirects to /login when no refresh token', async () => {
    tokenService.getRefreshToken.mockReturnValue(null)

    const { refreshTokens } = await import('../refresh.handler')
    const result = await refreshTokens()

    expect(result).toBe(false)
    expect(mockLocation.href).toBe('/login')
  })

  it('sets tokens and returns true on successful refresh', async () => {
    tokenService.getRefreshToken.mockReturnValue('rt123')
    vi.mocked(axios.post).mockResolvedValue({
      data: { value: { accessToken: 'at', refreshToken: 'rt2' } },
    })

    const { refreshTokens } = await import('../refresh.handler')
    const result = await refreshTokens()

    expect(result).toBe(true)
    expect(tokenService.setTokens).toHaveBeenCalledWith('at', 'rt2')
  })

  it('returns false when response has no value key', async () => {
    tokenService.getRefreshToken.mockReturnValue('rt123')
    vi.mocked(axios.post).mockResolvedValue({ data: {} })

    const { refreshTokens } = await import('../refresh.handler')
    const result = await refreshTokens()

    expect(result).toBe(false)
    expect(tokenService.setTokens).not.toHaveBeenCalled()
  })

  it('returns false when response value is null', async () => {
    tokenService.getRefreshToken.mockReturnValue('rt123')
    vi.mocked(axios.post).mockResolvedValue({ data: { value: null } })

    const { refreshTokens } = await import('../refresh.handler')
    const result = await refreshTokens()

    expect(result).toBe(false)
  })

  it('clears tokens and redirects on network error', async () => {
    tokenService.getRefreshToken.mockReturnValue('rt123')
    vi.mocked(axios.post).mockRejectedValue(new Error('Network Error'))

    const { refreshTokens } = await import('../refresh.handler')
    const result = await refreshTokens()

    expect(result).toBe(false)
    expect(tokenService.clearTokens).toHaveBeenCalled()
    expect(mockLocation.href).toBe('/login')
  })

  it('clears tokens and redirects on server 500 error', async () => {
    tokenService.getRefreshToken.mockReturnValue('rt123')
    vi.mocked(axios.post).mockRejectedValue(new Error('Internal Server Error'))

    const { refreshTokens } = await import('../refresh.handler')
    const result = await refreshTokens()

    expect(result).toBe(false)
    expect(tokenService.clearTokens).toHaveBeenCalled()
    expect(mockLocation.href).toBe('/login')
  })

  it('calls setTokens with exact argument from response', async () => {
    tokenService.getRefreshToken.mockReturnValue('rt123')
    vi.mocked(axios.post).mockResolvedValue({
      data: { value: { accessToken: 'at-secret-xyz', refreshToken: 'rt-secret-abc' } },
    })

    const { refreshTokens } = await import('../refresh.handler')
    await refreshTokens()

    expect(tokenService.setTokens).toHaveBeenCalledWith('at-secret-xyz', 'rt-secret-abc')
  })

  it('verifies redirect on failure when no token', async () => {
    tokenService.getRefreshToken.mockReturnValue(null)

    const { refreshTokens } = await import('../refresh.handler')
    const result = await refreshTokens()

    expect(mockLocation.href).toBe('/login')
    expect(result).toBe(false)
  })
})
