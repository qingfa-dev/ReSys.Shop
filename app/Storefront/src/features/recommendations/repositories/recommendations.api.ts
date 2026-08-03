import { BaseRepository } from '@/core/repositories'
import type { Result } from '@/core/models/result'
import type { Product } from '@/features/catalog/types'
import type { IRecommendationsRepository } from './recommendations.repository.interface'

// Backend shape: PagedResult<SearchByImage.Response> — NOT Result<Product[]>.
// Each item is a slim variant+product projection used by the visual (CBIR) search.
interface SearchByImageResponseItem {
  variantId: string
  productId: string
  productName: string
  sku: string
  price: number
  imageUrl?: string
  similarityScore?: number
}

interface SearchByImageResponseBody {
  isSuccess: boolean
  statusCode: number
  items?: SearchByImageResponseItem[]
  message?: string
  errors?: Result<never>['errors']
}

export class RecommendationsApiRepository extends BaseRepository implements IRecommendationsRepository {
  async getSimilarProducts(productId: string): Promise<Result<Product[]>> {
    return this.get<Product[]>('/api/storefront/products/similar', { filter: `productId:${productId}` })
  }

  async getPersonalizedRecommendations(): Promise<Result<Product[]>> {
    // TODO: Backend endpoint /api/storefront/recommendations/personalized does not exist yet.
    // Return a not-implemented result until the backend adds this endpoint.
    return { isSuccess: false, isFailure: true, statusCode: 501, message: 'Personalized recommendations endpoint not yet implemented' }
  }

  async searchByImage(file: File): Promise<Result<Product[]>> {
    const formData = new FormData()
    formData.append('image', file)
    try {
      const response = await this.client.post<SearchByImageResponseBody>('/api/storefront/search-by-image', formData, {
        headers: { 'Content-Type': 'multipart/form-data' },
      })

      const body = response.data

      // The backend returns PagedResult<SearchByImage.Response> — map each item to a
      // minimal Product-compatible object the CBIR view understands. An empty but
      // successful page is surfaced as success with [] so the view can show its
      // "No similar products found" state.
      if (body.isSuccess) {
        const products: Array<Product & { similarityScore?: number }> = (body.items ?? []).map((item) => ({
          id: item.productId,
          name: item.productName,
          slug: '',
          description: '',
          price: item.price,
          images: item.imageUrl ? [item.imageUrl] : [],
          tags: [],
          category: { id: '', name: '', slug: '' },
          inventory: { quantity: 0, trackQuantity: false, allowBackorder: false },
          createdAt: new Date().toISOString(),
          updatedAt: new Date().toISOString(),
          ...(item.similarityScore !== undefined ? { similarityScore: item.similarityScore } : {}),
        }))

        return { isSuccess: true, isFailure: false, statusCode: 200, data: products }
      }

      return {
        isSuccess: false,
        isFailure: true,
        statusCode: body.statusCode ?? 400,
        message: body.message ?? 'Search failed. Please try again.',
        errors: body.errors,
      }
    } catch (error) {
      return this.handleError(error)
    }
  }
}

export const recommendationsApiRepository = new RecommendationsApiRepository()
