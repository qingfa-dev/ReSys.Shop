import apiClient from '@/common/api/http/api.client'
import { CATALOG } from '@/common/api/constants'
import type { ServerResult } from '@/common/api/types/result.types'
import type { ProductClassification } from '../types/classification.response'
import type { SyncClassificationsRequest } from '../types/classification.request'
export const productClassificationApi = {
  getClassifications: async (productId: string): Promise<ServerResult<ProductClassification[]>> => {
    return apiClient.get(`${CATALOG}/products/${productId}/classifications`).then(res => res.data as ServerResult<ProductClassification[]>);
  },

  syncClassifications: (productId: string, data: SyncClassificationsRequest): Promise<ServerResult<void>> =>
    apiClient.put(`${CATALOG}/products/${productId}/classifications/sync`, data).then(res => res.data as ServerResult<void>),

  assignClassifications: (productId: string, taxonIds: string[]): Promise<ServerResult<void>> =>
    apiClient.post(`${CATALOG}/products/${productId}/classifications/assign`, { taxonIds }).then(res => res.data as ServerResult<void>),

  revokeClassifications: (productId: string, taxonIds: string[]): Promise<ServerResult<void>> =>
    apiClient.delete(`${CATALOG}/products/${productId}/classifications/revoke`, { data: { taxonIds } }).then(res => res.data as ServerResult<void>),
}
