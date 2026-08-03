import { defineStore } from 'pinia'
import { computed, ref } from 'vue'
import type { TaxonListItem, TaxonQuery } from '../types/taxon'
import { TaxonApi } from '../services/taxonApi'

export const useTaxonStore = defineStore('taxons', () => {
  const activeTaxons = ref<TaxonListItem[]>([])
  const loaded = ref(false)

  const items = ref<TaxonListItem[]>([])
  const loading = ref(false)
  const error = ref<string | null>(null)
  const page = ref(1)
  const pageSize = ref(20)
  const totalCount = ref(0)
  const filter = ref('')
  const sort = ref<string[]>(['position'])
  const search = ref('')
  const searchFields = ref<string[]>(['name', 'slug'])
  const searchMode = ref('any')
  const selectedTaxonomyId = ref<string | null>(null)

  const totalPages = computed(() => {
    if (pageSize.value <= 0) return 0
    return Math.ceil(totalCount.value / pageSize.value)
  })

  function buildQuery(): TaxonQuery {
    const query: TaxonQuery = {
      filter: filter.value || undefined,
      search: search.value || undefined,
      searchFields: searchFields.value.length > 0 ? searchFields.value : undefined,
      searchMode: searchMode.value || undefined,
      page: page.value,
      pageSize: pageSize.value,
    }

    if (sort.value.length > 0) {
      const raw = sort.value[0]
      if (!raw) return query
      const descending = raw.startsWith('-')
      const field = descending ? raw.slice(1) : raw
      if (field) {
        query.sortBy = field as TaxonQuery['sortBy']
        query.sortDirection = descending ? 'desc' : 'asc'
      }
    }

    return query
  }

  async function fetchList(): Promise<void> {
    loading.value = true
    error.value = null

    const query = buildQuery()
    const result = selectedTaxonomyId.value
      ? await TaxonApi.getList(selectedTaxonomyId.value, query)
      : await TaxonApi.getTaxons(query)

    if (result.isSuccess) {
      items.value = result.items
      totalCount.value = result.totalCount
      page.value = result.page
      pageSize.value = result.pageSize
    } else {
      error.value = result.message ?? result.errors[0]?.message ?? 'Request failed.'
    }

    loading.value = false
  }

  function setPage(p: number) {
    page.value = Math.max(1, p)
    fetchList()
  }

  function setPageSize(s: number) {
    pageSize.value = Math.max(1, s)
    page.value = 1
    fetchList()
  }

  function setSort(s: string[]) {
    sort.value = s
    page.value = 1
    fetchList()
  }

  function setSearch(s: string) {
    search.value = s
    page.value = 1
    fetchList()
  }

  function setFilter(f: string) {
    filter.value = f
    page.value = 1
    fetchList()
  }

  function setSearchFields(sf: string[]) {
    searchFields.value = sf
    page.value = 1
    fetchList()
  }

  function setSearchMode(m: string) {
    searchMode.value = m
    page.value = 1
    fetchList()
  }

  function setSelectedTaxonomy(id: string | null) {
    selectedTaxonomyId.value = id
    page.value = 1
    fetchList()
  }

  function refresh(): Promise<void> {
    return fetchList()
  }

  function reset() {
    items.value = []
    loading.value = false
    error.value = null
    page.value = 1
    pageSize.value = 20
    totalCount.value = 0
    filter.value = ''
    sort.value = ['position']
    search.value = ''
    searchFields.value = ['name', 'slug']
    searchMode.value = 'any'
    selectedTaxonomyId.value = null
  }

  async function fetchActive(): Promise<void> {
    if (loaded.value) return

    const result = await TaxonApi.getTaxons({})

    if (result.isSuccess) {
      activeTaxons.value = result.items
      loaded.value = true
    }
  }

  return {
    activeTaxons,
    loaded,
    items,
    loading,
    error,
    page,
    pageSize,
    totalCount,
    totalPages,
    filter,
    sort,
    search,
    searchFields,
    searchMode,
    selectedTaxonomyId,
    fetchList,
    setPage,
    setPageSize,
    setSort,
    setSearch,
    setFilter,
    setSearchFields,
    setSearchMode,
    setSelectedTaxonomy,
    refresh,
    reset,
    fetchActive,
  }
})
