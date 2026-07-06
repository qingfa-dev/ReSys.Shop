import { ref, watch, type Ref } from 'vue'

export function useQueryString(key: string, fallback: Ref<string>): Ref<string> {
  const url = new URL(window.location.href)
  const initial = url.searchParams.get(key) ?? fallback.value
  const value = ref(initial) as Ref<string>

  watch(value, (next) => {
    const u = new URL(window.location.href)
    if (next) u.searchParams.set(key, next)
    else u.searchParams.delete(key)
    window.history.replaceState({}, '', u.toString())
  })

  return value
}
