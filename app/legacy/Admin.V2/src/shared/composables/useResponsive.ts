import { computed } from 'vue'
import { useWindowSize } from './useWindowSize'

const BP = { sm: 640, md: 768, lg: 1024, xl: 1280, xxl: 1536 } as const

export function useResponsive() {
  const { width } = useWindowSize()

  const isMobile = computed(() => width.value < BP.md)
  const isTablet = computed(() => width.value >= BP.md && width.value < BP.lg)
  const isDesktop = computed(() => width.value >= BP.lg && width.value < BP.xl)
  const isWide = computed(() => width.value >= BP.xl)

  return { isMobile, isTablet, isDesktop, isWide }
}
