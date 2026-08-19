export interface Review {
    id: string
    productId: string
    userId: string
    userName: string
    rating: number        // 1-5
    title: string
    body: string
    images?: string[]
    verified: boolean     // Purchased product badge
    fit?: 'runs-small' | 'true-to-size' | 'runs-large'
    helpful: number
    unhelpful: number
    createdAt: string
    status: 'pending' | 'approved' | 'rejected'
}

export interface ReviewStats {
    totalReviews: number
    averageRating: number
    ratingDistribution: Record<number, number>  // 1-5: count
    verifiedCount: number
}

export interface ReviewFilter {
    rating?: number
    verified?: boolean
    sortBy?: 'helpful' | 'recent' | 'rating-high' | 'rating-low'
}
