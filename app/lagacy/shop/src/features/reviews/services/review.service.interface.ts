import type { Result } from '@/core/models/result'
import type { Review } from '../types'

export interface IReviewService {
    getReviewsForProduct(productId: string): Promise<Result<Review[]>>
    addReview(productId: string, review: Omit<Review, 'id' | 'createdAt'>): Promise<Result<Review>>
    updateReview(reviewId: string, review: Partial<Review>): Promise<Result<Review>>
    deleteReview(reviewId: string): Promise<Result<void>>
    getAverageRating(productId: string): Promise<Result<number>>
}
