import { ref, computed, type Ref } from 'vue'
import { DEFAULT_PAGE_SIZE, DEFAULT_PAGE } from '../types/page'

export function usePagination(total: Ref<number>, pageSize = DEFAULT_PAGE_SIZE) {
  const page = ref(DEFAULT_PAGE)
  const totalPages = computed(() => Math.max(1, Math.ceil(total.value / pageSize)))
  const offset = computed(() => (page.value - 1) * pageSize)

  function next() {
    if (page.value < totalPages.value) page.value += 1
  }
  function prev() {
    if (page.value > 1) page.value -= 1
  }
  function reset() {
    page.value = DEFAULT_PAGE
  }

  return { page, pageSize, totalPages, offset, next, prev, reset }
}
