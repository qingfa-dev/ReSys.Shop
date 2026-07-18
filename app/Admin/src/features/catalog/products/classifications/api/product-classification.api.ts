import apiClient from '@/shared/api/http/api.client'
import { CATALOG } from '@/shared/api/constants'
import type { ServerResult } from '@/shared/api/types/result.types'
import type { ProductClassification } from '../../types/Product.Response.Type'
import type { SyncClassificationsRequest } from '../types/Classification.Request.Type'

export const productClassificationApi = {
  getClassifications: (productId: string): Promise<ServerResult<ProductClassification[]>> =>
    apiClient.get(`${CATALOG}/products/${productId}/classifications`).then(res => res.data as ServerResult<ProductClassification[]>),

  syncClassifications: (productId: string, data: SyncClassificationsRequest): Promise<ServerResult<void>> =>
    apiClient.put(`${CATALOG}/products/${productId}/classifications/sync`, data).then(res => res.data as ServerResult<void>),
}
