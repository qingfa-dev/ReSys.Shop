import { useSearchStore } from '../store/search'

export function useSearch() {
    const store = useSearchStore()

    return {
        query: store.query,
        results: store.results,
        facets: store.facets,
        isLoading: store.isLoading,
        currentFilters: store.currentFilters,
        suggestions: store.suggestions,
        total: store.total,
        currentPage: store.currentPage,
        resultCount: store.resultCount,
        hasFilters: store.hasFilters,
        setQuery: store.setQuery,
        setResults: store.setResults,
        setSuggestions: store.setSuggestions,
        updateFilter: store.updateFilter,
        clearFilters: store.clearFilters,
        reset: store.reset,
    }
}
