import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import type { SearchResult, SearchFacets, SearchFilters, SearchSuggestion } from '../types'

export const useSearchStore = defineStore('search', () => {
    const query = ref<string>('')
    const results = ref<SearchResult[]>([])
    const facets = ref<SearchFacets | null>(null)
    const isLoading = ref(false)
    const currentFilters = ref<SearchFilters>({})
    const suggestions = ref<SearchSuggestion[]>([])
    const total = ref(0)
    const currentPage = ref(1)

    const resultCount = computed(() => results.value.length)
    const hasFilters = computed(() => Object.keys(currentFilters.value).length > 0)

    function setQuery(q: string) {
        query.value = q
    }

    function setResults(items: SearchResult[], totalCount: number) {
        results.value = items
        total.value = totalCount
    }

    function setFacets(f: SearchFacets) {
        facets.value = f
    }

    function setSuggestions(s: SearchSuggestion[]) {
        suggestions.value = s
    }

    function updateFilter(key: keyof SearchFilters, value: any) {
        if (value === undefined || value === null) {
            delete currentFilters.value[key]
        } else {
            currentFilters.value[key] = value
        }
    }

    function clearFilters() {
        currentFilters.value = {}
    }

    function setPage(page: number) {
        currentPage.value = page
    }

    function reset() {
        query.value = ''
        results.value = []
        facets.value = null
        suggestions.value = []
        currentFilters.value = {}
        total.value = 0
        currentPage.value = 1
    }

    return {
        query,
        results,
        facets,
        isLoading,
        currentFilters,
        suggestions,
        total,
        currentPage,
        resultCount,
        hasFilters,
        setQuery,
        setResults,
        setFacets,
        setSuggestions,
        updateFilter,
        clearFilters,
        setPage,
        reset,
    }
})
