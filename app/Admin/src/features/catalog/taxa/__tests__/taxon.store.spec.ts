import { describe, it, expect, vi, beforeEach } from 'vitest';
import { setActivePinia, createPinia } from 'pinia';
import { useTaxonStore } from '../store/taxon.store';
import type { TaxonListItem } from '../models/taxon.response';

vi.mock('../api/taxon.api', () => ({
  taxonRepository: {
    getTaxons: vi.fn(),
    getById: vi.fn(),
    create: vi.fn(),
    update: vi.fn(),
    delete: vi.fn()
  }
}));

describe('TaxonStore', () => {
  beforeEach(() => {
    setActivePinia(createPinia());
    vi.clearAllMocks();
  });

  it('should build taxon tree correctly with unique keys', () => {
    const store = useTaxonStore();
    store.currentTaxons = [
      { id: '1', name: 'Root', presentation: 'Root', parentId: undefined, position: 0 } as unknown as TaxonListItem,
      { id: '2', name: 'Child', presentation: 'Child', parentId: '1', position: 0 } as unknown as TaxonListItem
    ];

    expect(store.taxonTree).toHaveLength(1);
    expect(store.taxonTree[0]!.key).toBe('1');
    expect(store.taxonTree[0]!.children).toHaveLength(1);
    expect(store.taxonTree[0]!.children[0]!.key).toBe('2');
  });
});
