import { describe, it, expect, vi, beforeEach } from 'vitest'
import { useNavigation, useBreadcrumbs, type NavLink } from '../useNavigation'
import { useRoute } from 'vue-router'
import { computed } from 'vue'

vi.mock('vue-router', () => ({
  useRoute: vi.fn(),
}))

describe('useNavigation', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  it('should return default nav links', () => {
    const mockRoute = {
      path: '/',
    }
    ;(useRoute as ReturnType<typeof vi.fn>).mockReturnValue(mockRoute as any)

    const { navLinks } = useNavigation()
    expect(navLinks.value).toHaveLength(4)
    expect(navLinks.value[0]).toEqual({ name: 'Home', path: '/' })
    expect(navLinks.value[1]).toEqual({ name: 'Shop', path: '/shop' })
    expect(navLinks.value[2]).toEqual({ name: 'Collections', path: '/collections' })
    expect(navLinks.value[3]).toEqual({ name: 'About', path: '/about' })
  })

  it('should return custom nav links when provided', () => {
    const customLinks: NavLink[] = [
      { name: 'Custom', path: '/custom' },
    ]
    const mockRoute = { path: '/' }
    ;(useRoute as ReturnType<typeof vi.fn>).mockReturnValue(mockRoute as any)

    const { navLinks } = useNavigation(customLinks)
    expect(navLinks.value).toHaveLength(1)
    expect(navLinks.value[0]).toEqual({ name: 'Custom', path: '/custom' })
  })

  it('should return the route object', () => {
    const mockRoute = { path: '/test' }
    ;(useRoute as ReturnType<typeof vi.fn>).mockReturnValue(mockRoute as any)

    const { route } = useNavigation()
    expect(route).toEqual(mockRoute)
  })

  describe('isActive', () => {
    it('should return true when route path matches', () => {
      const mockRoute = { path: '/shop' }
      ;(useRoute as ReturnType<typeof vi.fn>).mockReturnValue(mockRoute as any)

      const { isActive } = useNavigation()
      expect(isActive('/shop')).toBe(true)
    })

    it('should return false when route path does not match', () => {
      const mockRoute = { path: '/home' }
      ;(useRoute as ReturnType<typeof vi.fn>).mockReturnValue(mockRoute as any)

      const { isActive } = useNavigation()
      expect(isActive('/shop')).toBe(false)
    })
  })

  describe('isExactActive', () => {
    it('should return true when route path matches exactly', () => {
      const mockRoute = { path: '/shop' }
      ;(useRoute as ReturnType<typeof vi.fn>).mockReturnValue(mockRoute as any)

      const { isExactActive } = useNavigation()
      expect(isExactActive('/shop')).toBe(true)
    })

    it('should return false when route path does not match', () => {
      const mockRoute = { path: '/home' }
      ;(useRoute as ReturnType<typeof vi.fn>).mockReturnValue(mockRoute as any)

      const { isExactActive } = useNavigation()
      expect(isExactActive('/shop')).toBe(false)
    })
  })
})

describe('useBreadcrumbs', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  it('should return empty breadcrumbs when no matched routes have breadcrumb meta', () => {
    const mockRoute = {
      path: '/',
      matched: [
        { path: '/', meta: {} },
      ],
    }
    ;(useRoute as ReturnType<typeof vi.fn>).mockReturnValue(mockRoute as any)

    const { breadcrumbs } = useBreadcrumbs()
    expect(breadcrumbs.value).toHaveLength(0)
  })

  it('should return breadcrumbs for routes with breadcrumb meta', () => {
    const mockRoute = {
      path: '/products',
      matched: [
        { path: '/', meta: {} },
        { path: '/products', meta: { breadcrumb: 'Products' } },
      ],
    }
    ;(useRoute as ReturnType<typeof vi.fn>).mockReturnValue(mockRoute as any)

    const { breadcrumbs } = useBreadcrumbs()
    expect(breadcrumbs.value).toHaveLength(1)
    expect(breadcrumbs.value[0]).toEqual({
      label: 'Products',
      path: '/products',
    })
  })

  it('should return multiple breadcrumbs for nested routes', () => {
    const mockRoute = {
      path: '/products/1',
      matched: [
        { path: '/', meta: {} },
        { path: '/products', meta: { breadcrumb: 'Products' } },
        { path: '/products/1', meta: { breadcrumb: 'Product 1' } },
      ],
    }
    ;(useRoute as ReturnType<typeof vi.fn>).mockReturnValue(mockRoute as any)

    const { breadcrumbs } = useBreadcrumbs()
    expect(breadcrumbs.value).toHaveLength(2)
    expect(breadcrumbs.value[0]).toEqual({ label: 'Products', path: '/products' })
    expect(breadcrumbs.value[1]).toEqual({ label: 'Product 1', path: '/products/1' })
  })
})
