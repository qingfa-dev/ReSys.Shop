import { ref, watch } from 'vue'

// Throttle: Debounced ref wrapper — delays value propagation by delay ms
export function useDebounce<T>(value: { value: T }, delay = 300) {
  const debounced = ref<T>(value.value) as { value: T }
  let timeout: ReturnType<typeof setTimeout>

  watch(
    () => value.value,
    (newVal) => {
      // Throttle: Clear previous timer on each change — only last value propagates
      clearTimeout(timeout)
      timeout = setTimeout(() => {
        debounced.value = newVal
      }, delay)
    },
  )

  return { value: debounced }
}
