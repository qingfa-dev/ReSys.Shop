import { z } from 'zod'
import { get, post } from '@/shared/api/client'
import { getPaged } from '@/shared/api'
import { CATALOG } from '@/shared/constants/api'
import { SearchByImageResponseSchema, VisualSearchModelSchema } from '../validations/searchByImage'
import { PagedResultSchema } from '@/shared/validations/result'
import type { PagedResult } from '@/shared/types'
import type { SearchByImageResponse, VisualSearchModel } from '../types'

export class SearchByImageApi {
  static async getVisualSearchModels(): Promise<Result<VisualSearchModel[]>> {
    const result = await get<unknown>(`${CATALOG}/products/visual-search/models`)
    if (!result.isSuccess) return result as Result<VisualSearchModel[]>
    result.value = VisualSearchModelSchema.array().parse(result.value)
    return result as Result<VisualSearchModel[]>
  }

  static async searchByImage(
    file: File,
    topK?: number,
    model?: string
  ): Promise<PagedResult<SearchByImageResponse>> {
    const form = new FormData()
    form.append('image', file)
    if (topK) form.append('topK', String(topK))
    if (model) form.append('model', model)
    const result = await post<unknown>(`${CATALOG}/products/images/search`, form)
    if (!result.isSuccess) return result as PagedResult<SearchByImageResponse>
    return result as PagedResult<SearchByImageResponse>
  }
}
