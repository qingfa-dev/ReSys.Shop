import { ref, watch } from 'vue'

export function useDebounce<T>(value: { value: T }, delay = 300) {
  const debounced = ref<T>(value.value) as { value: T }
  let timeout: ReturnType<typeof setTimeout>

  watch(
    () => value.value,
    (newVal) => {
      clearTimeout(timeout)
      timeout = setTimeout(() => {
        debounced.value = newVal
      }, delay)
    },
  )

  return { value: debounced }
}
