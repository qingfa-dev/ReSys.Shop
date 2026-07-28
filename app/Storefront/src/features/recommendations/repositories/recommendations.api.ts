import { BaseRepository } from '@/core/repositories'
import type { Result } from '@/core/models/result'
import type { Product } from '@/features/catalog/types'
import type { IRecommendationsRepository } from './recommendations.repository.interface'

export class RecommendationsApiRepository extends BaseRepository implements IRecommendationsRepository {
  async getSimilarProducts(productId: string): Promise<Result<Product[]>> {
    return this.get<Product[]>(`/api/storefront/products/${productId}/similar`)
  }

  async getPersonalizedRecommendations(): Promise<Result<Product[]>> {
    return this.get<Product[]>('/api/storefront/recommendations/personalized')
  }

  async searchByImage(file: File): Promise<Result<Product[]>> {
    const formData = new FormData()
    formData.append('image', file)
    try {
      const response = await this.client.post<Result<Product[]>>('/api/storefront/search-by-image', formData, {
        headers: { 'Content-Type': 'multipart/form-data' },
      })
      return response.data
    } catch (error) {
      return this.handleError(error)
    }
  }
}

export const recommendationsApiRepository = new RecommendationsApiRepository()
