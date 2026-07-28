import { recommendationsApiRepository } from '../repositories/recommendations.api'
import { mockRecommendationsRepository } from '../repositories/recommendations.mock.repository'
import type { IRecommendationsService } from './recommendations.service.interface'
import type { Product } from '@/features/catalog/types'
import type { Result } from '@/core/models/result'

const USE_MOCK = true

export class RecommendationsService implements IRecommendationsService {
  private readonly repository = USE_MOCK ? mockRecommendationsRepository : recommendationsApiRepository

  async getSimilarProducts(productId: string): Promise<Result<Product[]>> {
    return this.repository.getSimilarProducts(productId)
  }

  async getPersonalizedRecommendations(): Promise<Result<Product[]>> {
    return this.repository.getPersonalizedRecommendations()
  }

  async searchByImage(file: File): Promise<Result<Product[]>> {
    return this.repository.searchByImage(file)
  }
}

export const recommendationsService = new RecommendationsService()
