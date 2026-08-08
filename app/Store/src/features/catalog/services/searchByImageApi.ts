import { z } from 'zod'
import { get, post } from '@/shared/api/client'
import { getPaged } from '@/shared/api'
import { CATALOG } from '@/shared/constants/api'
import { SearchByImageResponseSchema, VisualSearchModelSchema } from '../validations/searchByImage'
import { PagedResultSchema } from '@/shared/validations/result'
import type { PagedResult, Result } from '@/shared/types'
import type { SearchByImageResponse, VisualSearchModel } from '../types'

export class SearchByImageApi {
  static async getVisualSearchModels(): Promise<Result<VisualSearchModel[]>> {
    // Call: Catalog API to fetch available visual search ML models
    const result = await get<Result<VisualSearchModel[]>>(`${CATALOG}/products/visual-search/models`)
    if (!result.isSuccess) return result
    // Validate: Ensure API response conforms to VisualSearchModel schema
    result.value = VisualSearchModelSchema.array().parse(result.value)
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
    if (model) form.append('model', model)
    const result = await post<PagedResult<SearchByImageResponse>>(`${CATALOG}/products/images/search`, form)
    return result
  }
}
