import { defineStore } from 'pinia';
import { ref } from 'vue';
import { useToast } from '@/shared/composables/toast.use';
import { fulfillmentService } from '@/features/ordering/fulfillment/services/fulfillment.service';
import type { OrderListItem } from '../../types/order.types';

export const useFulfillmentStore = defineStore('fulfillment', () => {
  const { showToast } = useToast();
  const queue = ref<OrderListItem[]>([]);
  const loading = ref(false);
  const total_count = ref(0);

  async function fetchQueue() {
    loading.value = true;
    try {
      const result = await fulfillmentService.getQueue({ page: 1, page_size: 50 });
      if (result.success && result.data) {
        queue.value = result.data;
        total_count.value = result.meta?.total_count || 0;
      }
    } finally {
      loading.value = false;
    }
  }

  async function shipOrder(id: string, trackingNumber: string) {
    loading.value = true;
    try {
      const result = await fulfillmentService.markAsShipped(id, trackingNumber);
      if (result.success) {
        showToast('success', 'Shipped', 'Order marked as shipped');
        await fetchQueue();
      } else {
        showToast('error', 'Error', result.error.title || 'Failed to ship');
      }
      return result;
    } finally {
      loading.value = false;
    }
  }

  return {
    queue,
    loading,
    total_count,
    fetchQueue,
    shipOrder
  };
});
