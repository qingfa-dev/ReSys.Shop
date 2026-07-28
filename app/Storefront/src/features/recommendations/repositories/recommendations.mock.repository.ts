import type { Result } from '@/core/models/result'
import type { Product } from '@/features/catalog/types'
import type { IRecommendationsRepository } from './recommendations.repository.interface'

const mockProducts: Product[] = [
  {
    id: 'rec-1', name: 'Recommended Product 1', slug: 'rec-product-1', description: 'A recommended product',
    price: 29.99, compareAtPrice: 39.99, images: [], tags: [],
    category: { id: 'cat-1', name: 'Category', slug: 'category' },
    inventory: { quantity: 10, trackQuantity: true, allowBackorder: false },
    createdAt: new Date().toISOString(), updatedAt: new Date().toISOString(),
  },
  {
    id: 'rec-2', name: 'Recommended Product 2', slug: 'rec-product-2', description: 'Another recommendation',
    price: 39.99, images: [], tags: [],
    category: { id: 'cat-1', name: 'Category', slug: 'category' },
    inventory: { quantity: 5, trackQuantity: true, allowBackorder: false },
    createdAt: new Date().toISOString(), updatedAt: new Date().toISOString(),
  },
  {
    id: 'rec-3', name: 'Recommended Product 3', slug: 'rec-product-3', description: 'Yet another recommendation',
    price: 49.99, images: [], tags: [],
    category: { id: 'cat-2', name: 'Other', slug: 'other' },
    inventory: { quantity: 0, trackQuantity: true, allowBackorder: true },
    createdAt: new Date().toISOString(), updatedAt: new Date().toISOString(),
  },
]

export class MockRecommendationsRepository implements IRecommendationsRepository {
  async getSimilarProducts(_productId: string): Promise<Result<Product[]>> {
    return { isSuccess: true, isFailure: false, statusCode: 200, data: mockProducts }
  }

  async getPersonalizedRecommendations(): Promise<Result<Product[]>> {
    return { isSuccess: true, isFailure: false, statusCode: 200, data: mockProducts }
  }

  async searchByImage(_file: File): Promise<Result<Product[]>> {
    return { isSuccess: true, isFailure: false, statusCode: 200, data: mockProducts.slice(0, 1) }
  }
}

export const mockRecommendationsRepository = new MockRecommendationsRepository()
