import { defineStore } from 'pinia';
import { useI18n } from 'vue-i18n';
import { useToast } from '@/shared/composables/toast.use';
import { usePagedList } from '@/shared/composables/paged-list.use';
import { fulfillmentService } from '@/features/ordering/fulfillment/services/fulfillment.service';
import type { OrderListItem } from '../../orders/types/Order.Response.Type';
import type { ServerQueryingParameters } from '@/shared/api/types/query.types';

export const useFulfillmentStore = defineStore('fulfillment', () => {
  const { showToast } = useToast();
  const { t } = useI18n();

  const { items: queue, loading, totalRecords: totalCount, fetch: fetchQueue } = usePagedList<OrderListItem, ServerQueryingParameters>(
    (p) => fulfillmentService.getQueue(p),
    { page: 1, pageSize: 50 },
  );

  async function shipOrder(id: string, trackingNumber: string) {
    loading.value = true;
    try {
      const result = await fulfillmentService.markAsShipped(id, trackingNumber);
      if (result.isSuccess) {
        showToast('success', t('common.success'), t('ordering.messages.shipped'));
        await fetchQueue();
      } else {
        showToast('error', t('common.error'), result.errors?.[0]?.message || t('ordering.messages.ship_failed'));
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
