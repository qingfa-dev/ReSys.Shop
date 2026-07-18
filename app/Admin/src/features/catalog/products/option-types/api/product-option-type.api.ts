import apiClient from '@/shared/api/http/api.client'
import { CATALOG } from '@/shared/api/constants'
import type { ServerResult } from '@/shared/api/types/result.types'
import type { OptionTypeDetail } from '../../../option-types/types/OptionType.Response.Type'

export const productOptionTypeApi = {
  getOptionTypes: (productId: string): Promise<ServerResult<OptionTypeDetail[]>> =>
    apiClient.get(`${CATALOG}/products/${productId}/option-types`).then(res => res.data as ServerResult<OptionTypeDetail[]>),

  syncOptionTypes: (productId: string, optionTypeIds: string[]): Promise<ServerResult<void>> =>
    apiClient.put(`${CATALOG}/products/${productId}/option-types/sync`, { optionTypeIds }).then(res => res.data as ServerResult<void>),
}
