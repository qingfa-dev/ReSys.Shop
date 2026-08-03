import { recommendationsApiRepository } from '../repositories/recommendations.api'
import type { IRecommendationsService } from './recommendations.service.interface'
import type { Product } from '@/features/catalog/types'
import type { Result } from '@/core/models/result'

export class RecommendationsService implements IRecommendationsService {
  private readonly repository = recommendationsApiRepository

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
