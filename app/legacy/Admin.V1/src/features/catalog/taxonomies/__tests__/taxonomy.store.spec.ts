/**
 * Taxonomy Store Unit Tests
 */
import { setActivePinia, createPinia } from 'pinia';
import { describe, it, expect, beforeEach, vi } from 'vitest';
import { useTaxonomyStore } from '../store/taxonomy.store';
import { taxonomyRepository } from '../api/taxonomy.api';
import { createMockPagedResult } from '@/common/test/mock-types';
import type { TaxonomyListItem } from '../models/taxonomy.response';

vi.mock('../api/taxonomy.api', () => ({
  taxonomyRepository: {
    list: vi.fn<() => void>(),
    getById: vi.fn<() => void>(),
    create: vi.fn<() => void>(),
    update: vi.fn<() => void>(),
    delete: vi.fn<() => void>(),
  }
}));

vi.mock('primevue/usetoast', () => ({
  useToast: () => ({
    add: vi.fn<() => void>()
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
    
    vi.mocked(taxonomyRepository.list).mockResolvedValue(createMockPagedResult(mockData));

    await store.fetchTaxonomies();

    expect(store.taxonomies).toEqual(mockData);
    expect(store.totalRecords).toBe(1);
    expect(store.loading).toBe(false);
  });
});
