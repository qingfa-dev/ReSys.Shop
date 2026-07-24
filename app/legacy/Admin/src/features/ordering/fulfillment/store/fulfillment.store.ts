import { defineStore } from 'pinia';
import { useI18n } from 'vue-i18n';
import { useToast } from '@/common/composables/toast.use';
import { usePagedList } from '@/common/composables/paged-list.use';
import { fulfillmentRepository } from '@/features/ordering/fulfillment/api/fulfillment.api';
import type { OrderListItemModel } from '../../orders/types/order.model';
import type { ServerQueryingParameters } from '@/common/api/types/query.types';

export const useFulfillmentStore = defineStore('fulfillment', () => {
  const { showToast } = useToast();
  const { t } = useI18n();

  const { items: queue, loading, totalRecords: totalCount, fetch: fetchQueue } = usePagedList<OrderListItemModel, ServerQueryingParameters>(
    (p) => fulfillmentRepository.getQueue(p),
    { page: 1, pageSize: 50 },
  );

  async function shipOrder(id: string, trackingNumber: string) {
    loading.value = true;
    try {
      const result = await fulfillmentRepository.markAsShipped(id, trackingNumber);
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
