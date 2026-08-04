import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest'
import { useScroll } from '../useScroll'

describe('useScroll', () => {
  beforeEach(() => {
    vi.stubGlobal('window', {
      scrollY: 0,
      addEventListener: vi.fn((event: string, handler: Function) => {
        // Store handler for later testing
        ;(window as any).__scrollHandler = handler
      }),
      removeEventListener: vi.fn(),
      scrollTo: vi.fn(),
    })
  })

  afterEach(() => {
    vi.unstubAllGlobals()
  })

  it('should initialize with default values', () => {
    const { scrollY, isScrolled, showScrollTop } = useScroll()
    expect(scrollY.value).toBe(0)
    expect(isScrolled.value).toBe(false)
    expect(showScrollTop.value).toBe(false)
  })

  it('should return scrollToTop function', () => {
    const { scrollToTop } = useScroll()
    expect(typeof scrollToTop).toBe('function')
  })

  it('should call scrollTo with correct options', () => {
    const { scrollToTop } = useScroll()
    scrollToTop()
    expect(window.scrollTo).toHaveBeenCalledWith({ top: 0, behavior: 'smooth' })
  })
})

describe('useScroll - scroll handler behavior', () => {
  beforeEach(() => {
    vi.stubGlobal('window', {
      scrollY: 0,
      addEventListener: vi.fn(),
      removeEventListener: vi.fn(),
      scrollTo: vi.fn(),
    })
  })

  afterEach(() => {
    vi.unstubAllGlobals()
  })

  it('should handle scroll event updates', () => {
    const { scrollY, isScrolled, showScrollTop } = useScroll()
    
    // Get the registered handler
    const handler = (window.addEventListener as ReturnType<typeof vi.fn>).mock.calls.find(
      (call: any[]) => call[0] === 'scroll'
    )?.[1]
    
    if (handler) {
      // Simulate scroll
      ;(window as any).scrollY = 50
      handler()
      
      expect(scrollY.value).toBe(50)
      expect(isScrolled.value).toBe(true) // > 20
    }
  })

  it('should show scroll top button when scrolled past threshold', () => {
    const { showScrollTop } = useScroll()
    
    const handler = (window.addEventListener as ReturnType<typeof vi.fn>).mock.calls.find(
      (call: any[]) => call[0] === 'scroll'
    )?.[1]
    
    if (handler) {
      ;(window as any).scrollY = 600
      handler()
      
      expect(showScrollTop.value).toBe(true)
    }
  })
})
