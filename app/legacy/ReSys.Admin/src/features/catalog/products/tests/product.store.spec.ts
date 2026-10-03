/**
 * Product Store Unit Tests
 * Verified against the Golden Standard pattern.
 */
import { setActivePinia, createPinia } from 'pinia';
import { describe, it, expect, beforeEach, vi } from 'vitest';
import { useProductStore } from '../stores/product.store';
import { productService } from '../services/product.service';

vi.mock('../services/product.service', () => ({
  productService: {
    list: vi.fn(),
    getById: vi.fn(),
    create: vi.fn(),
    update: vi.fn(),
    delete: vi.fn(),
  }
}));

vi.mock('primevue/usetoast', () => ({
  useToast: () => ({
    add: vi.fn()
  })
}));

describe('ProductStore', () => {
  beforeEach(() => {
    setActivePinia(createPinia());
    vi.clearAllMocks();
  });

  it('initializes with correct default state', () => {
    const store = useProductStore();
    expect(store.products).toEqual([]);
    expect(store.loading).toBe(false);
    expect(store.totalRecords).toBe(0);
    expect(store.query.page).toBe(1);
  });

  it('updates state after successful fetch', async () => {
    const store = useProductStore();
    const mockData = [{ id: '1', name: 'Product A' }] as any;
    
    vi.mocked(productService.list).mockResolvedValue({
      success: true,
      data: mockData,
      meta: { total_count: 1 } as any
    });

    await store.fetchProducts();

    expect(store.products).toEqual(mockData);
    expect(store.totalRecords).toBe(1);
    expect(store.loading).toBe(false);
  });

  it('handles errors gracefully during fetch', async () => {
    const store = useProductStore();
    
    vi.mocked(productService.list).mockResolvedValue({
      success: false,
      error: { title: 'Network Error' },
      data: null as any
    });

    await store.fetchProducts();

    expect(store.error).toBe('Network Error');
    expect(store.loading).toBe(false);
  });
});
