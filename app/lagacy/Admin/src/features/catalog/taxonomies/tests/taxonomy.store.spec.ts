/**
 * Taxonomy Store Unit Tests
 */
import { setActivePinia, createPinia } from 'pinia';
import { describe, it, expect, beforeEach, vi } from 'vitest';
import { useTaxonomyStore } from '../stores/taxonomy.store';
import { taxonomyService } from '../services/taxonomy.service';
import { createMockPagedResult } from '@/shared/test/mock-types';
import type { TaxonomyListItem } from '../types/taxonomy.response.type';

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
    const mockData: TaxonomyListItem[] = [{ id: '1', name: 'Categories', presentation: null, position: 1, taxonsCount: 0, createdAtUtc: '', modifiedAtUtc: '' }];
    
    vi.mocked(taxonomyService.list).mockResolvedValue(createMockPagedResult(mockData));

    await store.fetchTaxonomies();

    expect(store.taxonomies).toEqual(mockData);
    expect(store.totalRecords).toBe(1);
    expect(store.loading).toBe(false);
  });
});
