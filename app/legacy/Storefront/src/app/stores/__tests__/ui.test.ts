import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest'
import { setActivePinia, createPinia } from 'pinia'
import { useUIStore } from '../ui'

describe('useUIStore', () => {
  let pinia: ReturnType<typeof createPinia>

  beforeEach(() => {
    pinia = createPinia()
    setActivePinia(pinia)
    
    vi.stubGlobal('localStorage', {
      getItem: vi.fn(() => null),
      setItem: vi.fn(),
      removeItem: vi.fn(),
    })
    
    vi.stubGlobal('document', {
      body: {
        style: {
          overflow: '',
        },
      },
    })
  })

  afterEach(() => {
    setActivePinia(undefined)
    vi.unstubAllGlobals()
  })

  describe('initial state', () => {
    it('should have default values', () => {
      const store = useUIStore()
      
      expect(store.mobileMenuOpen).toBe(false)
      expect(store.searchOpen).toBe(false)
      expect(store.cartOpen).toBe(false)
      expect(store.cookieBannerDismissed).toBe(false)
      expect(store.newsletterDismissed).toBe(false)
    })
  })

  describe('mobileMenuOpen', () => {
    it('should open mobile menu', () => {
      const store = useUIStore()
      
      store.openMobileMenu()
      
      expect(store.mobileMenuOpen).toBe(true)
    })

    it('should close mobile menu', () => {
      const store = useUIStore()
      store.mobileMenuOpen = true
      
      store.closeMobileMenu()
      
      expect(store.mobileMenuOpen).toBe(false)
    })

    it('should toggle mobile menu', () => {
      const store = useUIStore()
      
      store.toggleMobileMenu()
      expect(store.mobileMenuOpen).toBe(true)
      
      store.toggleMobileMenu()
      expect(store.mobileMenuOpen).toBe(false)
    })
  })

  describe('searchOpen', () => {
    it('should open search', () => {
      const store = useUIStore()
      
      store.openSearch()
      
      expect(store.searchOpen).toBe(true)
    })

    it('should close search', () => {
      const store = useUIStore()
      store.searchOpen = true
      
      store.closeSearch()
      
      expect(store.searchOpen).toBe(false)
    })
  })

  describe('cartOpen', () => {
    it('should toggle cart', () => {
      const store = useUIStore()
      
      store.toggleCart()
      expect(store.cartOpen).toBe(true)
      
      store.toggleCart()
      expect(store.cartOpen).toBe(false)
    })
  })

  describe('dismissCookieBanner', () => {
    it('should set cookieBannerDismissed to true', () => {
      const store = useUIStore()
      
      store.dismissCookieBanner()
      
      expect(store.cookieBannerDismissed).toBe(true)
    })

    it('should set localStorage item', () => {
      const store = useUIStore()
      
      store.dismissCookieBanner()
      
      expect(localStorage.setItem).toHaveBeenCalledWith('cookies-accepted', 'true')
    })
  })

  describe('dismissNewsletter', () => {
    it('should set newsletterDismissed to true', () => {
      const store = useUIStore()
      
      store.dismissNewsletter()
      
      expect(store.newsletterDismissed).toBe(true)
    })

    it('should set localStorage item', () => {
      const store = useUIStore()
      
      store.dismissNewsletter()
      
      expect(localStorage.setItem).toHaveBeenCalledWith('newsletter-dismissed', 'true')
    })
  })

  describe('hydrate', () => {
    it('should read cookies-accepted from localStorage', () => {
      ;(localStorage.getItem as ReturnType<typeof vi.fn>).mockImplementation((key: string) => {
        if (key === 'cookies-accepted') return 'true'
        return null
      })
      
      const store = useUIStore()
      store.hydrate()
      
      expect(store.cookieBannerDismissed).toBe(true)
    })

    it('should read newsletter-dismissed from localStorage', () => {
      ;(localStorage.getItem as ReturnType<typeof vi.fn>).mockImplementation((key: string) => {
        if (key === 'newsletter-dismissed') return 'true'
        return null
      })
      
      const store = useUIStore()
      store.hydrate()
      
      expect(store.newsletterDismissed).toBe(true)
    })
  })
})
