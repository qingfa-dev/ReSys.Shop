import type { Result } from '@/core/models/result'
import type { Product } from '@/features/catalog/types'
import type { IRecommendationsRepository } from './recommendations.repository.interface'

const mockProducts: Product[] = [
  { id: 'rec-1', name: 'Recommended Product 1', slug: 'rec-product-1', price: 29.99, currency: 'USD', description: 'A recommended product', images: [], thumbnail: '', inStock: true },
  { id: 'rec-2', name: 'Recommended Product 2', slug: 'rec-product-2', price: 39.99, currency: 'USD', description: 'Another recommendation', images: [], thumbnail: '', inStock: true },
  { id: 'rec-3', name: 'Recommended Product 3', slug: 'rec-product-3', price: 49.99, currency: 'USD', description: 'Yet another recommendation', images: [], thumbnail: '', inStock: true },
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
