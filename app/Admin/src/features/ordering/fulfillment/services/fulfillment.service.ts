import { orderingApi } from '../../services/ordering.api'
import type { ApiResult } from '@/shared/api/types/api.types'

export const fulfillmentService = {
  getQueue: orderingApi.fulfillments.getQueue,

  async markAsShipped(_id: string, _trackingNumber: string): Promise<ApiResult<void>> {
    return { success: false, data: null, error: { statusCode: 501, title: 'Not Implemented', message: 'Shipment creation not implemented in quick action', detail: null, isSuccess: false, errors: {}, error_code: undefined } }
  },
}
