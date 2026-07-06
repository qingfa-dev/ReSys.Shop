import { ref, watch, type Ref } from 'vue'

export function useDebouncedRef<T>(source: Ref<T>, delay = 200): Ref<T> {
  const debounced = ref(source.value) as Ref<T>
  let timer: ReturnType<typeof setTimeout> | undefined
  watch(source, (next) => {
    clearTimeout(timer)
    timer = setTimeout(() => {
      debounced.value = next
    }, delay)
  })
  return debounced
}
