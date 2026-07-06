import { describe, it, expect, vi } from 'vitest'
import { useLogin } from '../../api/login'

vi.mock('@/shared/api/client', () => ({
  api: { post: vi.fn<() => Promise<unknown>>().mockResolvedValue({ accessToken: 'a', refreshToken: 'b', expiresAt: '2026-07-06T00:00:00Z' }) },
}))

describe('useLogin', () => {
  it('calls api.post with login endpoint', async () => {
    const { mutateAsync } = useLogin()
    await mutateAsync({ email: 'a@b.co', password: 'secret123' })
    const { api } = await import('@/shared/api/client')
    expect(api.post).toHaveBeenCalledWith('/api/auth/login', { email: 'a@b.co', password: 'secret123' })
  })
})
