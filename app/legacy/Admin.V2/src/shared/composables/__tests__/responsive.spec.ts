import { describe, it, expect, vi } from 'vitest'
import { useResponsive } from '../useResponsive'

vi.mock('../useWindowSize', () => ({
  useWindowSize: () => ({ width: { value: 1024 } }),
}))

describe('useResponsive', () => {
  it('returns breakpoint booleans', () => {
    const r = useResponsive()
    expect(typeof r.isMobile.value).toBe('boolean')
    expect(typeof r.isTablet.value).toBe('boolean')
    expect(typeof r.isDesktop.value).toBe('boolean')
    expect(typeof r.isWide.value).toBe('boolean')
  })
})
