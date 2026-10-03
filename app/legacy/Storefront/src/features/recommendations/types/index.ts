export type RecommendationType =
    | 'collaborative-filtering'
    | 'content-based'
    | 'visual-similarity'
    | 'trending'
    | 'new-arrivals'
    | 'personalized'

export interface RecommendedProduct {
    id: string
    name: string
    brand: string
    price: number
    image: string
    score: number          // 0-1 confidence score
    reason: string         // Why recommended
    badge?: 'trending' | 'new' | 'sale'
}

export interface RecommendationSet {
    id: string
    type: RecommendationType
    title: string
    description?: string
    products: RecommendedProduct[]
    contextProductId?: string
}

export interface RecommendationRequest {
    userId: string
    contextProductId?: string
    limit?: number
    type?: RecommendationType
}
