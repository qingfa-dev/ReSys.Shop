/**
 * Order Store Unit Tests
 */
import { setActivePinia, createPinia } from 'pinia';
import { describe, it, expect, beforeEach, vi } from 'vitest';
import { useOrderStore } from '../stores/order.store';
import { orderService } from '../services/order.service';

vi.mock('../services/order.service', () => ({
  orderService: {
    list: vi.fn(),
    getById: vi.fn(),
    updateState: vi.fn(),
  }
}));

vi.mock('@/shared/composables/toast.use', () => ({
  useToast: () => ({
    showToast: vi.fn()
  })
}));

describe('OrderStore', () => {
  beforeEach(() => {
    setActivePinia(createPinia());
    vi.clearAllMocks();
  });

  describe('fetchOrders', () => {
    it('updates state after successful fetch', async () => {
      const store = useOrderStore();
      const mockData = [{ id: '1', number: 'ORD-1' }] as any;
      
      vi.mocked(orderService.list).mockResolvedValue({
        success: true,
        data: mockData,
        meta: { totalCount: 1 } as any
      });

      await store.fetchOrders();

      expect(store.orders).toEqual(mockData);
      expect(store.totalRecords).toBe(1);
      expect(store.loading).toBe(false);
    });
  });

  describe('fetchOrderById', () => {
    it('sets current_order after successful fetch', async () => {
      const store = useOrderStore();
      const mockOrder = { id: '1', number: 'ORD-1', line_items: [] } as any;
      
      vi.mocked(orderService.getById).mockResolvedValue({
        success: true,
        data: mockOrder
      });

      await store.fetchOrderById('1');

      expect(store.current_order).toEqual(mockOrder);
      expect(store.loading).toBe(false);
    });
  });

  describe('advanceOrderState', () => {
    it('calls service and re-fetches order on success', async () => {
      const store = useOrderStore();
      const orderId = '1';
      
      vi.mocked(orderService.updateState).mockResolvedValue({
        success: true,
        data: null as any
      });

      // Mock fetchOrderById call that happens after update
      vi.mocked(orderService.getById).mockResolvedValue({
        success: true,
        data: { id: orderId, state: 'Advanced' } as any
      });

      await store.advanceOrderState(orderId);

      expect(orderService.updateState).toHaveBeenCalledWith(orderId);
      expect(orderService.getById).toHaveBeenCalledWith(orderId);
      expect(store.submitting).toBe(false);
    });
  });
});