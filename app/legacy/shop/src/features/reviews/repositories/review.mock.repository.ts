import type { Review } from '../types'
import type { Result } from '@/core/models/result'
import { succeed, fail } from '@/core/utils/result-helpers'
import { mockReviews, getReviewsForProduct, getAverageRating } from '../data/mock-reviews.data'

export class MockReviewRepository {
    private reviews: Review[] = [...mockReviews]
    private nextId = 100

    async getReviewsForProduct(productId: string): Promise<Result<Review[]>> {
        const reviews = this.reviews.filter((r) => r.productId === productId)
        return succeed(reviews)
    }

    async addReview(productId: string, review: Omit<Review, 'id' | 'createdAt'>): Promise<Result<Review>> {
        const newReview: Review = {
            ...review,
            id: `rev-${this.nextId++}`,
            createdAt: new Date().toISOString(),
        }
        this.reviews.push(newReview)
        return succeed(newReview)
    }

    async updateReview(reviewId: string, updates: Partial<Review>): Promise<Result<Review>> {
        const review = this.reviews.find((r) => r.id === reviewId)
        if (!review) {
            return fail('Review not found', 404)
        }
        Object.assign(review, updates)
        return succeed(review)
    }

    async deleteReview(reviewId: string): Promise<Result<void>> {
        const index = this.reviews.findIndex((r) => r.id === reviewId)
        if (index === -1) {
            return fail('Review not found', 404)
        }
        this.reviews.splice(index, 1)
        return succeed(undefined)
    }

    async getAverageRating(productId: string): Promise<Result<number>> {
        const reviews = this.reviews.filter((r) => r.productId === productId)
        if (reviews.length === 0) {
            return succeed(0)
        }
        const sum = reviews.reduce((acc, r) => acc + r.rating, 0)
        return succeed(Number((sum / reviews.length).toFixed(1)))
    }
}

export const mockReviewRepository = new MockReviewRepository()
