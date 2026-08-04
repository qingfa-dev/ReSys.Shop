import { post } from '@/shared/api'
import { ENDPOINTS } from '@/shared/constants/api'
import type { PagedResult } from '@/shared/types/result'
import type { SearchByImageResponse } from '../types/searchByImage'

export function searchByImage(image: File, topK = 20, model?: string): Promise<PagedResult<SearchByImageResponse>> {
  const formData = new FormData()
  formData.append('image', image)
  if (topK) formData.append('TopK', String(topK))
  if (model) formData.append('Model', model)
  return post<PagedResult<SearchByImageResponse>>(ENDPOINTS.searchByImage, formData)
}
