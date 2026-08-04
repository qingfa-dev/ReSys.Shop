import { setActivePinia, createPinia } from 'pinia';
import { describe, it, expect, beforeEach, vi } from 'vitest';
import { useOrderStore } from '../store/order.store';
import { orderRepository } from '../api/order.api';
import { createMockPagedResult, createMockResult } from '@/common/test/mock-types';
import type { OrderListItemModel, OrderDetailModel } from '../types/order.model';

vi.mock('../api/order.api', () => ({
  orderRepository: {
    list: vi.fn<() => void>(),
    getById: vi.fn<() => void>(),
    updateStatus: vi.fn<() => void>(),
  }
}));

vi.mock('@/common/composables/toast.use', () => ({
  useToast: () => ({
    showToast: vi.fn<() => void>()
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

function makeOrderDetail(overrides: Partial<OrderDetailModel> = {}): OrderDetailModel {
  return {
    id: '1', number: 'ORD-1', status: 0, checkoutState: 0, currency: 'USD', email: null,
    specialInstructions: null, billAddressId: null, shipAddressId: null, shippingMethodId: null,
    itemTotal: 0, adjustmentTotal: 0, shipmentTotal: 0, total: 0, paymentTotal: 0, outstandingBalance: 0,
    paymentState: null, shipmentState: null, userId: null, storeId: null, itemCount: 0,
    approvedById: null, approvedAtUtc: null, completedAtUtc: null, canceledAtUtc: null,
    createdAtUtc: '', modifiedAtUtc: null,
    totalDisplay: '$0.00', itemTotalDisplay: '$0.00', shipmentTotalDisplay: '$0.00',
    adjustmentTotalDisplay: '$0.00', outstandingBalanceDisplay: '$0.00',
    statusLabel: 'Draft', paymentStateLabel: null, shipmentStateLabel: null,
    ...overrides,
  }
}

describe('OrderStore', () => {
  beforeEach(() => {
    setActivePinia(createPinia());
    vi.clearAllMocks();
  });

  describe('fetchOrders', () => {
    it('updates state after successful fetch', async () => {
      const store = useOrderStore();
      const mockData = [makeOrderListItem()];

      vi.mocked(orderRepository.list).mockResolvedValue(createMockPagedResult(mockData));

      await store.fetchOrders();

      expect(store.orders).toEqual(mockData);
      expect(store.totalRecords).toBe(1);
      expect(store.loading).toBe(false);
    });
  });

  describe('fetchOrderById', () => {
    it('sets current_order after successful fetch', async () => {
      const store = useOrderStore();
      const mockOrder = makeOrderDetail();

      vi.mocked(orderRepository.getById).mockResolvedValue(createMockResult(mockOrder));

      await store.fetchOrderById('1');

      expect(store.current_order).toEqual(mockOrder);
      expect(store.loading).toBe(false);
    });
  });

  describe('advanceOrderState', () => {
    it('calls service with status and re-fetches order on success', async () => {
      const store = useOrderStore();
      const orderId = '1';
      const status = 'Processing';

      vi.mocked(orderRepository.updateStatus).mockResolvedValue(createMockResult<void>(undefined));

      const updatedOrder = makeOrderDetail({ id: orderId });
      vi.mocked(orderRepository.getById).mockResolvedValue(createMockResult(updatedOrder));

      await store.advanceOrderState(orderId, status);

      expect(orderRepository.updateStatus).toHaveBeenCalledWith(orderId, status);
      expect(orderRepository.getById).toHaveBeenCalledWith(orderId);
      expect(store.submitting).toBe(false);
    });
  });
});
