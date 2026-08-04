import { ref, watch, onUnmounted } from 'vue'

export function useScrollLock() {
  const locked = ref(false)
  const originalStyle = ref('')
  const originalOverflow = ref('')

  watch(locked, (isLocked) => {
    if (typeof document === 'undefined') return

    if (isLocked) {
      originalOverflow.value = document.body.style.overflow
      originalStyle.value = document.body.style.cssText
      document.body.style.overflow = 'hidden'
      document.body.style.cssText += ';height:100vh;overflow:hidden;'
    } else {
      document.body.style.overflow = originalOverflow.value
      document.body.style.cssText = originalStyle.value
    }
  })

  onUnmounted(() => {
    if (locked.value) {
      document.body.style.overflow = originalOverflow.value
      document.body.style.cssText = originalStyle.value
    }
  })

  function lock() {
    locked.value = true
  }

  function unlock() {
    locked.value = false
  }

  function toggle() {
    locked.value = !locked.value
  }

  return {
    locked,
    lock,
    unlock,
    toggle,
  }
}
