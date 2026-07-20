import { ref, onMounted, onUnmounted, computed } from 'vue'

const BREAKPOINTS = {
  xs: 480,
  sm: 640,
  md: 768,
  lg: 1024,
  xl: 1280,
  '2xl': 1536,
}

export function useBreakpoint() {
  const width = ref(typeof window !== 'undefined' ? window.innerWidth : BREAKPOINTS.lg)

  const isXs = computed(() => width.value < BREAKPOINTS.sm)
  const isSm = computed(() => width.value >= BREAKPOINTS.sm && width.value < BREAKPOINTS.md)
  const isMd = computed(() => width.value >= BREAKPOINTS.md && width.value < BREAKPOINTS.lg)
  const isLg = computed(() => width.value >= BREAKPOINTS.lg && width.value < BREAKPOINTS.xl)
  const isXl = computed(() => width.value >= BREAKPOINTS.xl && width.value < BREAKPOINTS['2xl'])
  const is2xl = computed(() => width.value >= BREAKPOINTS['2xl'])

  const isMobile = computed(() => width.value < BREAKPOINTS.md)
  const isTablet = computed(() => width.value >= BREAKPOINTS.md && width.value < BREAKPOINTS.lg)
  const isDesktop = computed(() => width.value >= BREAKPOINTS.lg)

  const currentBreakpoint = computed(() => {
    if (width.value < BREAKPOINTS.xs) return 'xs'
    if (width.value < BREAKPOINTS.sm) return 'xs'
    if (width.value < BREAKPOINTS.md) return 'sm'
    if (width.value < BREAKPOINTS.lg) return 'md'
    if (width.value < BREAKPOINTS.xl) return 'lg'
    if (width.value < BREAKPOINTS['2xl']) return 'xl'
    return '2xl'
  })

  function onResize() {
    if (typeof window !== 'undefined') {
      width.value = window.innerWidth
    }
  }

  onMounted(() => {
    if (typeof window !== 'undefined') {
      window.addEventListener('resize', onResize, { passive: true })
    }
  })

  onUnmounted(() => {
    if (typeof window !== 'undefined') {
      window.removeEventListener('resize', onResize)
    }
  })

  return {
    width,
    breakpoint: currentBreakpoint,
    isXs,
    isSm,
    isMd,
    isLg,
    isXl,
    is2xl,
    isMobile,
    isTablet,
    isDesktop,
  }
}
