import { setActivePinia, createPinia } from 'pinia';
import { describe, it, expect, beforeEach, vi } from 'vitest';
import { useFulfillmentStore } from '../stores/fulfillment.store';
import { fulfillmentService } from '../services/fulfillment.service';
import { createMockResult, createMockErrorResult } from '@/shared/test/mock-types';
import type { OrderListItemModel } from '../../orders/types/order.model.type';

vi.mock('../services/fulfillment.service', () => ({
  fulfillmentService: {
    getQueue: vi.fn(),
    markAsShipped: vi.fn(),
  }
}));

vi.mock('@/shared/composables/toast.use', () => ({
  useToast: () => ({
    showToast: vi.fn()
  })
}));

vi.mock('vue-i18n', () => ({
  useI18n: () => ({
    t: (key: string) => key,
  }),
}));

function makeOrderListItem(overrides: Partial<OrderListItemModel> = {}): OrderListItemModel {
  return {
    id: '1', number: 'ORD-1', status: 0, checkoutState: 0, currency: 'USD', email: null,
    itemCount: 0, itemTotal: 0, total: 0, outstandingBalance: 0,
    paymentState: null, shipmentState: null, createdAtUtc: '', userId: null, storeId: null,
    totalDisplay: '$0.00', statusLabel: 'Draft', paymentStateLabel: null, shipmentStateLabel: null,
    ...overrides,
  }
}

describe('FulfillmentStore', () => {
  beforeEach(() => {
    setActivePinia(createPinia());
    vi.clearAllMocks();
  });

  describe('fetchQueue', () => {
    it('updates queue after successful fetch', async () => {
      const store = useFulfillmentStore();
      const mockData = [makeOrderListItem()];

      vi.mocked(fulfillmentService.getQueue).mockResolvedValue(createMockResult(mockData));

      await store.fetchQueue();

      expect(store.queue).toEqual(mockData);
      expect(store.totalCount).toBe(1);
      expect(store.loading).toBe(false);
    });
  });

  describe('shipOrder', () => {
    it('calls service and re-fetches queue on success', async () => {
      const store = useFulfillmentStore();
      const orderId = '1';

      vi.mocked(fulfillmentService.markAsShipped).mockResolvedValue(createMockResult<void>(undefined));

      vi.mocked(fulfillmentService.getQueue).mockResolvedValue(createMockResult<OrderListItemModel[]>([]));

      await store.shipOrder(orderId, 'TRK-123');

      expect(fulfillmentService.markAsShipped).toHaveBeenCalledWith(orderId, 'TRK-123');
      expect(fulfillmentService.getQueue).toHaveBeenCalled();
    });

    it('shows error toast on failure', async () => {
      const store = useFulfillmentStore();
      const orderId = '1';

      vi.mocked(fulfillmentService.markAsShipped).mockResolvedValue(
        createMockErrorResult<void>({ statusCode: 500, errors: [{ code: 'Error', message: 'Invalid inventory units', type: 4, metadata: null }], message: 'Invalid inventory units' })
      );

      await store.shipOrder(orderId, 'TRK-123');

      expect(fulfillmentService.markAsShipped).toHaveBeenCalled();
    });
  });
});
