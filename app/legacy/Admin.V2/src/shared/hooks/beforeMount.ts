import { onBeforeMount } from 'vue'

export function useBeforeMount(fn: () => void): void {
  onBeforeMount(fn)
}
