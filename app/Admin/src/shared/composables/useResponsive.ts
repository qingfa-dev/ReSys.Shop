import { ref, computed, onMounted, onUnmounted } from 'vue'

const BP = { sm: 640, md: 768, lg: 1024, xl: 1280, xxl: 1536 } as const

export function useResponsive() {
  const width = ref(typeof window !== 'undefined' ? window.innerWidth : 0)

  function onResize() {
    width.value = window.innerWidth
  }

  onMounted(() => window.addEventListener('resize', onResize))
  onUnmounted(() => window.removeEventListener('resize', onResize))

  const isMobile = computed(() => width.value < BP.md)
  const isTablet = computed(() => width.value >= BP.md && width.value < BP.lg)
  const isDesktop = computed(() => width.value >= BP.lg && width.value < BP.xl)
  const isWide = computed(() => width.value >= BP.xl)

  return { isMobile, isTablet, isDesktop, isWide }
}
