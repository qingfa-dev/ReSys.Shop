import type { Result } from '@/core/models/result'
import type { Product } from '@/features/catalog/types'

export interface IRecommendationsService {
  getSimilarProducts(productId: string): Promise<Result<Product[]>>
  getPersonalizedRecommendations(): Promise<Result<Product[]>>
  searchByImage(file: File): Promise<Result<Product[]>>
}
