import { ref, onMounted, onUnmounted, type Ref } from 'vue'

const SCROLL_THRESHOLD = 20
const SCROLL_TOP_THRESHOLD = 500

export function useScroll() {
  const scrollY = ref(0)
  const isScrolled = ref(false)
  const showScrollTop = ref(false)

  function handleScroll() {
    scrollY.value = window.scrollY
    isScrolled.value = scrollY.value > SCROLL_THRESHOLD
    showScrollTop.value = scrollY.value > SCROLL_TOP_THRESHOLD
  }

  function scrollToTop() {
    window.scrollTo({ top: 0, behavior: 'smooth' })
  }

  onMounted(() => {
    window.addEventListener('scroll', handleScroll, { passive: true })
    handleScroll()
  })

  onUnmounted(() => {
    window.removeEventListener('scroll', handleScroll)
  })

  return {
    scrollY,
    isScrolled,
    showScrollTop,
    scrollToTop,
  }
}

export function useScrollObserver(targetRef: Ref<Element | null>) {
  const isIntersecting = ref(false)
  let observer: IntersectionObserver | null = null

  onMounted(() => {
    if (!targetRef.value) return

    observer = new IntersectionObserver(
      (entries) => {
        const entry = entries[0]
        if (entry) {
          isIntersecting.value = entry.isIntersecting
        }
      },
      { threshold: 0.1 }
    )
    observer.observe(targetRef.value)
  })

  onUnmounted(() => {
    observer?.disconnect()
  })

  return { isIntersecting }
}
