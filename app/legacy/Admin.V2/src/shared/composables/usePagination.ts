import { ref, computed } from 'vue'

export function usePagination(defaultPageSize = 10) {
  const page = ref(1)
  const pageSize = ref(defaultPageSize)
  const totalCount = ref(0)

  const totalPages = computed(() => Math.max(1, Math.ceil(totalCount.value / pageSize.value)))

  const isFirstPage = computed(() => page.value <= 1)
  const isLastPage = computed(() => page.value >= totalPages.value)

  function goToPage(newPage: number) {
    page.value = Math.max(1, Math.min(newPage, totalPages.value))
  }

  function nextPage() {
    if (!isLastPage.value) page.value++
  }

  function prevPage() {
    if (!isFirstPage.value) page.value--
  }

  function reset() {
    page.value = 1
    pageSize.value = defaultPageSize
    totalCount.value = 0
  }

  return {
    page,
    pageSize,
    totalCount,
    totalPages,
    isFirstPage,
    isLastPage,
    goToPage,
    nextPage,
    prevPage,
    reset,
  }
}
