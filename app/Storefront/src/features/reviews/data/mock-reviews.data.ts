import type { Review } from '../types'

export const mockReviews: Review[] = [
    {
        id: 'rev-1',
        productId: 'prod-1',
        userId: 'user-1',
        userName: 'Sarah M.',
        rating: 5,
        title: 'Amazing quality!',
        body: 'This t-shirt is incredibly comfortable and the material quality is excellent. Highly recommend!',
        verified: true,
        helpful: 24,
        unhelpful: 2,
        createdAt: '2026-03-20T10:00:00Z',
        status: 'approved',
    },
    {
        id: 'rev-2',
        productId: 'prod-1',
        userId: 'user-2',
        userName: 'John D.',
        rating: 4,
        title: 'Good fit, but',
        body: 'The shirt fits well and feels great. Only issue is it shrinks a bit after washing.',
        verified: true,
        helpful: 12,
        unhelpful: 5,
        createdAt: '2026-03-15T10:00:00Z',
        status: 'approved',
    },
    {
        id: 'rev-3',
        productId: 'prod-1',
        userId: 'user-3',
        userName: 'Emily T.',
        rating: 5,
        title: 'Perfect for everyday wear',
        body: 'I bought this in multiple colors. Perfect for everyday wear, wash well, and maintain quality.',
        verified: true,
        helpful: 18,
        unhelpful: 1,
        createdAt: '2026-03-10T10:00:00Z',
        status: 'approved',
    },
    {
        id: 'rev-4',
        productId: 'prod-2',
        userId: 'user-4',
        userName: 'Mike L.',
        rating: 5,
        title: 'Perfect fit!',
        body: 'These jeans fit perfectly. The denim is high quality and very comfortable.',
        verified: true,
        helpful: 15,
        unhelpful: 3,
        createdAt: '2026-03-18T10:00:00Z',
        status: 'approved',
    },
    {
        id: 'rev-5',
        productId: 'prod-2',
        userId: 'user-5',
        userName: 'Rachel G.',
        rating: 4,
        title: 'Good quality, runs small',
        body: 'Great jeans but they run a bit small. I would recommend sizing up.',
        verified: true,
        helpful: 22,
        unhelpful: 4,
        createdAt: '2026-03-12T10:00:00Z',
        status: 'approved',
    },
]

export function getReviewsForProduct(productId: string): Review[] {
    return mockReviews.filter((r) => r.productId === productId)
}

export function getAverageRating(productId: string): number {
    const reviews = getReviewsForProduct(productId)
    if (reviews.length === 0) return 0
    const sum = reviews.reduce((acc, r) => acc + r.rating, 0)
    return Number((sum / reviews.length).toFixed(1))
}
