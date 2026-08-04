import { describe, it, expect, vi, beforeEach } from 'vitest';
import { setActivePinia, createPinia } from 'pinia';
import { usePropertyTypeStore } from '../stores/property-type.store';
import { propertyTypeService } from '../services/property-type.service';
import { PropertyKind } from '../types/property-kind';

vi.mock('../services/property-type.service', () => ({
  propertyTypeService: {
    getList: vi.fn(),
    getById: vi.fn(),
    create: vi.fn(),
    update: vi.fn(),
    delete: vi.fn()
  }
}));

describe('PropertyTypeStore', () => {
  beforeEach(() => {
    setActivePinia(createPinia());
    vi.clearAllMocks();
  });

  it('should fetch list successfully', async () => {
    const store = usePropertyTypeStore();
    const mockData = [{ id: '1', name: 'Material', presentation: 'Mat', kind: PropertyKind.String, position: 0, filterable: false }];
    vi.mocked(propertyTypeService.getList).mockResolvedValue({ success: true, data: mockData, meta: { total_count: 1 } } as any);

    await store.fetchList();

    expect(store.items).toEqual(mockData);
    expect(store.totalRecords).toBe(1);
  });

  it('should fetch by id successfully', async () => {
    const store = usePropertyTypeStore();
    const mockItem = { id: '1', name: 'Material' };
    vi.mocked(propertyTypeService.getById).mockResolvedValue({ success: true, data: mockItem } as any);

    await store.fetchById('1');

    expect(store.currentItem).toEqual(mockItem);
  });
});
