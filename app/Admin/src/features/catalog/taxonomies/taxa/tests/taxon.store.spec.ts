import { describe, it, expect, vi, beforeEach } from 'vitest';
import { setActivePinia, createPinia } from 'pinia';
import { useTaxonStore } from '../stores/taxon.store';
import { taxonService } from '../services/taxon.service';

vi.mock('../services/taxon.service', () => ({
  taxonService: {
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
      { id: '1', name: 'Root', presentation: 'Root', parent_id: undefined, position: 0 } as any,
      { id: '2', name: 'Child', presentation: 'Child', parent_id: '1', position: 0 } as any
    ];

    expect(store.taxonTree).toHaveLength(1);
    expect(store.taxonTree[0]!.key).toBe('1');
    expect(store.taxonTree[0]!.children).toHaveLength(1);
    expect(store.taxonTree[0]!.children[0]!.key).toBe('2');
  });
});
