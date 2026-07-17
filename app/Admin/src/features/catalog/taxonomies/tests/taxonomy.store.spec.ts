/**
 * Taxonomy Store Unit Tests
 */
import { setActivePinia, createPinia } from 'pinia';
import { describe, it, expect, beforeEach, vi } from 'vitest';
import { useTaxonomyStore } from '../stores/taxonomy.store';
import { taxonomyService } from '../services/taxonomy.service';

vi.mock('../services/taxonomy.service', () => ({
  taxonomyService: {
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

describe('TaxonomyStore', () => {
  beforeEach(() => {
    setActivePinia(createPinia());
    vi.clearAllMocks();
  });

  it('initializes with correct default state', () => {
    const store = useTaxonomyStore();
    expect(store.taxonomies).toEqual([]);
    expect(store.loading).toBe(false);
    expect(store.totalRecords).toBe(0);
  });

  it('updates state after successful fetch', async () => {
    const store = useTaxonomyStore();
    const mockData = [{ id: '1', name: 'Categories' }] as any;
    
    vi.mocked(taxonomyService.list).mockResolvedValue({
      success: true,
      data: mockData,
      meta: { totalCount: 1 } as any
    });

    await store.fetchTaxonomies();

    expect(store.taxonomies).toEqual(mockData);
    expect(store.totalRecords).toBe(1);
    expect(store.loading).toBe(false);
  });
});
