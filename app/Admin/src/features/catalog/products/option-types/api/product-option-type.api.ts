import apiClient from '@/shared/api/http/api.client'
import { CATALOG } from '@/shared/api/constants'
import type { ServerResult } from '@/shared/api/types/result.types'
import type { OptionTypeDetail } from '../../../option-types/types/option-type.response.type'
export const productOptionTypeApi = {
  getOptionTypes: async (productId: string): Promise<ServerResult<OptionTypeDetail[]>> => {
    return apiClient.get(`${CATALOG}/products/${productId}/option-types`).then(res => res.data as ServerResult<OptionTypeDetail[]>);
  },

  syncOptionTypes: (productId: string, optionTypeIds: string[]): Promise<ServerResult<void>> =>
    apiClient.put(`${CATALOG}/products/${productId}/option-types/sync`, { optionTypeIds }).then(res => res.data as ServerResult<void>),
}
