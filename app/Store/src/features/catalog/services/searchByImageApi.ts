import { get, post } from '@/shared/api/client'
import { VisualSearchModelSchema } from '../validations/searchByImage'
import type { PagedResult } from '@/shared/types'
import type { SearchByImageResponse, VisualSearchModel } from '../types'

export class CatalogImageApi {
  static async getVisualSearchModels(): Promise<PagedResult<VisualSearchModel>> {
    // Call: Catalog API to fetch available visual search ML models
    const result = await get<PagedResult<VisualSearchModel>>('/api/storefront/catalog/products/images/inferences')
    if (!result.isSuccess) return result
    // Validate: Ensure API response conforms to VisualSearchModel schema
    result.items = VisualSearchModelSchema.array().parse(result.items)
    return result
  }

  static async searchByImage(
    file: File,
    topK?: number,
    model?: string
  ): Promise<PagedResult<SearchByImageResponse>> {
    // Call: Catalog API image search endpoint — sends multipart form with image
    const form = new FormData()
    form.append('image', file)
    if (topK) form.append('topK', String(topK))
    if (model) form.append('modelName', model)
    const result = await post<PagedResult<SearchByImageResponse>>('/api/storefront/catalog/products/images/search', form)
    return result
  }
}
