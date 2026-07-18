/**
 * Order Store Unit Tests
 */
import { setActivePinia, createPinia } from 'pinia';
import { describe, it, expect, beforeEach, vi } from 'vitest';
import { useOrderStore } from '../stores/order.store';
import { orderService } from '../services/order.service';
import { createMockResult } from '@/shared/test/mock-types';
import type { OrderListItem, OrderDetail } from '../types/Order.Response.Type';

vi.mock('../services/order.service', () => ({
  orderService: {
    list: vi.fn(),
    getById: vi.fn(),
    updateStatus: vi.fn(),
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

describe('OrderStore', () => {
  beforeEach(() => {
    setActivePinia(createPinia());
    vi.clearAllMocks();
  });

  describe('fetchOrders', () => {
    it('updates state after successful fetch', async () => {
      const store = useOrderStore();
      const mockData: OrderListItem[] = [{ id: '1', number: 'ORD-1', state: '', currency: '', totalCents: 0, totalDisplay: '', createdAtUtc: '' }];
      
      vi.mocked(orderService.list).mockResolvedValue(createMockResult(mockData));

      await store.fetchOrders();

      expect(store.orders).toEqual(mockData);
      expect(store.totalRecords).toBe(1);
      expect(store.loading).toBe(false);
    });
  });

  describe('fetchOrderById', () => {
    it('sets current_order after successful fetch', async () => {
      const store = useOrderStore();
      const mockOrder: OrderDetail = { id: '1', number: 'ORD-1', state: '', currency: '', totalCents: 0, totalDisplay: '', createdAtUtc: '', itemTotalCents: 0, itemTotalDisplay: '', shipmentTotalCents: 0, shipmentTotalDisplay: '', lineItems: [], payments: [], shipments: [], history: [] };
      
      vi.mocked(orderService.getById).mockResolvedValue(createMockResult(mockOrder));

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
      
      vi.mocked(orderService.updateStatus).mockResolvedValue(createMockResult<void>(undefined));

      // Mock fetchOrderById call that happens after update
      const updatedOrder: OrderDetail = { id: orderId, state: 'Advanced', number: '', currency: '', totalCents: 0, totalDisplay: '', createdAtUtc: '', itemTotalCents: 0, itemTotalDisplay: '', shipmentTotalCents: 0, shipmentTotalDisplay: '', lineItems: [], payments: [], shipments: [], history: [] };
      vi.mocked(orderService.getById).mockResolvedValue(createMockResult(updatedOrder));

      await store.advanceOrderState(orderId, status);

      expect(orderService.updateStatus).toHaveBeenCalledWith(orderId, status);
      expect(orderService.getById).toHaveBeenCalledWith(orderId);
      expect(store.submitting).toBe(false);
    });
  });
});
