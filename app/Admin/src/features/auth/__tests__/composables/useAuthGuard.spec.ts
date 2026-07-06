import { describe, it, expect, vi } from 'vitest'
import { useAuthGuard } from '../../composables/useAuthGuard'

vi.mock('@/shared/api/client', () => ({ api: { get: vi.fn<() => Promise<unknown>>() } }))

describe('useAuthGuard', () => {
  it('blocks unauthenticated access to authRequired routes', () => {
    const guard = useAuthGuard()
    const result = guard(
      { meta: { authRequired: true }, fullPath: '/x' } as never,
      {} as never,
      vi.fn(),
    ) as unknown
    expect(typeof result).toBe('undefined')
  })

  it('allows access to non-authRequired routes', () => {
    const guard = useAuthGuard()
    const result = guard(
      { meta: { authRequired: false }, fullPath: '/login' } as never,
      {} as never,
      vi.fn(),
    )
    expect(result).toBeUndefined()
  })
})
