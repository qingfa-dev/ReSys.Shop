import apiClient from '@/shared/api/api.client';
import type { ApiResult } from '@/shared/api/api.types';
import type { OrderListItem } from '../../types/order.types';

export const fulfillmentService = {
  async getQueue(params: any): Promise<ApiResult<OrderListItem[]>> {
    // Backend filters by State, not ShipmentState. 'Processing' is the likely state for unfulfilled orders.
    return apiClient.get('/api/admin/orders', { 
        params: { ...params, state: 'Processing' } 
    });
  },

  async markAsShipped(id: string, trackingNumber: string): Promise<ApiResult<void>> {
    // TODO: Implement complex shipment creation (requires selecting stock location and inventory units)
    // return apiClient.post(`/api/admin/orders/${id}/shipments`, { ... });
    console.warn('Shipment creation requires inventory unit selection. Not implemented in quick action.');
    return Promise.resolve({ success: false, error: 'Not implemented' } as any);
  }
};
