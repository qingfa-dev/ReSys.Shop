/**
 * Product Store Unit Tests
 * Verified against the Golden Standard pattern.
 */
import { setActivePinia, createPinia } from 'pinia';
import { describe, it, expect, beforeEach, vi } from 'vitest';
import { useProductStore } from '../store/product.store';
import { productRepository } from '../api/product.api';

import { createMockPagedResult } from '@/common/test/mock-types'
import type { ProductSummaryModel } from '../models/product.model'

vi.mock('../api/product.api', () => ({
  productRepository: {
    list: vi.fn<any>(),
    getById: vi.fn<any>(),
    create: vi.fn<any>(),
    update: vi.fn<any>(),
    delete: vi.fn<any>(),
  }
}));

vi.mock('primevue/usetoast', () => ({
  useToast: () => ({
    add: vi.fn<any>()
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
    const mockData = [
      { id: '1', name: 'Product A' },
    ] as ProductSummaryModel[];
    
    vi.mocked(productRepository.list).mockResolvedValue(
      createMockPagedResult(mockData, { page: 1, pageSize: 10, totalCount: 1 })
    );

    await store.fetchProducts();

    expect(store.products).toEqual(mockData);
    expect(store.totalRecords).toBe(1);
    expect(store.loading).toBe(false);
  });

  it('handles errors gracefully during fetch', async () => {
    const store = useProductStore();
    
    vi.mocked(productRepository.list).mockResolvedValue(
      createMockPagedResult([], {
        isSuccess: false,
        statusCode: 500,
        errors: [{ code: 'Error', message: 'Network Error', type: 4, metadata: null }],
        message: 'Network Error',
        page: 1,
        pageSize: 10,
        totalCount: 0,
      })
    );

    await store.fetchProducts();

    expect(store.error).toBe('Network Error');
    expect(store.loading).toBe(false);
  });
});
