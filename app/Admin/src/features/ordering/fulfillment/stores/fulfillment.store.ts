import { defineStore } from 'pinia';
import { useToast } from '@/shared/composables/toast.use';
import { usePagedList } from '@/shared/composables/paged-list.use';
import { fulfillmentService } from '@/features/ordering/fulfillment/services/fulfillment.service';
import type { OrderListItem } from '../../types/order.domain.types';
import type { ServerQueryingParameters } from '@/shared/api/types/query.types';

export const useFulfillmentStore = defineStore('fulfillment', () => {
  const { showToast } = useToast();

  const { items: queue, loading, totalRecords: totalCount, fetch: fetchQueue } = usePagedList<OrderListItem, ServerQueryingParameters>(
    (p) => fulfillmentService.getQueue(p),
    { page: 1, pageSize: 50 },
  );

  async function shipOrder(id: string, trackingNumber: string) {
    loading.value = true;
    try {
      const result = await fulfillmentService.markAsShipped(id, trackingNumber);
      if (result.isSuccess) {
        showToast('success', 'Shipped', 'Order marked as shipped');
        await fetchQueue();
      } else {
        showToast('error', 'Error', result.errors?.[0]?.message || 'Failed to ship');
      }
      return result;
    } finally {
      loading.value = false;
    }
  }

  return {
    queue,
    loading,
    totalCount,
    fetchQueue,
    shipOrder
  };
});
