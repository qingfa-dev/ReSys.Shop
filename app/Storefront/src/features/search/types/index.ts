export interface SearchQuery {
    q: string
    page: number
    limit: number
    filters?: SearchFilters
    sort?: 'relevance' | 'price-low' | 'price-high' | 'newest' | 'trending'
}

export interface SearchFilters {
    brand?: string[]
    priceRange?: [number, number]
    sizes?: string[]
    colors?: string[]
    material?: string[]
    category?: string
    rating?: number
    inStock?: boolean
}

export interface SearchResult {
    id: string
    name: string
    brand: string
    price: number
    image: string
    rating: number
    reviews: number
    inStock: boolean
}

export interface SearchResponse {
    results: SearchResult[]
    total: number
    facets: SearchFacets
}

export interface SearchFacets {
    brands: { name: string; count: number }[]
    colors: { name: string; count: number }[]
    sizes: { name: string; count: number }[]
    priceRanges: { min: number; max: number; count: number }[]
    materials: { name: string; count: number }[]
}

export interface SearchSuggestion {
    text: string
    type: 'query' | 'product' | 'brand'
    product?: { name: string; image: string }
}
