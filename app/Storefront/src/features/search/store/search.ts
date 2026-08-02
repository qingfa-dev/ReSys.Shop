import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import type { SearchResult, SearchFacets, SearchFilters, SearchSuggestion } from '../types'
import { searchService } from '../services/search.service'

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

    async function search(q: string, filters?: Record<string, unknown>) {
        isLoading.value = true
        query.value = q
        try {
            const result = await searchService.search(q, filters)
            if (result.isSuccess && result.items) {
                results.value = result.items as unknown as SearchResult[]
                total.value = result.totalCount
                currentPage.value = result.page
            }
        } finally {
            isLoading.value = false
        }
    }

    async function fetchSuggestions(q: string) {
        try {
            const result = await searchService.getSuggestions(q)
            if (result.isSuccess && result.data) {
                suggestions.value = result.data as unknown as SearchSuggestion[]
            }
        } catch {
            // Suggestions are non-critical — silently fail
        }
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
        search,
        fetchSuggestions,
    }
})
