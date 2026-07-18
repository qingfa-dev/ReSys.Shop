import { describe, it, expect, vi, beforeEach } from 'vitest';
import { setActivePinia, createPinia } from 'pinia';
import { useOptionTypeStore } from '../stores/option-type.store';
import { optionTypeService } from '../services/option-type.service';
import { createMockPagedResult, createMockResult, createMockErrorResult } from '@/shared/test/mock-types';
import type { OptionTypeListItem } from '../types/OptionType.Response.Type'

const makeItem = (overrides?: Partial<OptionTypeListItem>): OptionTypeListItem => ({
  id: '', name: '', presentation: '', position: 0, filterable: false,
  optionValuesCount: 0, productsCount: 0, createdAtUtc: '', modifiedAtUtc: '',
  ...overrides,
})

// Mock service
vi.mock('../services/option-type.service', () => ({
  optionTypeService: {
    list: vi.fn(),
    getById: vi.fn(),
    create: vi.fn(),
    update: vi.fn(),
    delete: vi.fn()
  }
}));

describe('OptionTypeStore', () => {
  beforeEach(() => {
    setActivePinia(createPinia());
    vi.clearAllMocks();
  });

  describe('fetchList', () => {
    it('should fetch list and update state on success', async () => {
      const store = useOptionTypeStore();
      const mockData: OptionTypeListItem[] = [
        makeItem({ id: '1', name: 'Color', presentation: 'Color', position: 1, filterable: true })
      ];
      vi.mocked(optionTypeService.list).mockResolvedValue(createMockPagedResult(mockData));

      await store.fetchList();

      expect(optionTypeService.list).toHaveBeenCalled();
      expect(store.items).toEqual(mockData);
      expect(store.totalRecords).toBe(1);
      expect(store.loading).toBe(false);
    });

    it('should handle errors gracefully', async () => {
      const store = useOptionTypeStore();
      vi.mocked(optionTypeService.list).mockResolvedValue(
        createMockErrorResult<OptionTypeListItem[]>({ statusCode: 500, errors: [{ code: 'Error', message: 'Err', type: 4, metadata: null }], message: 'Err' }) as any
      );

      await store.fetchList();

      expect(store.items).toEqual([]);
      expect(store.loading).toBe(false);
    });
  });

  describe('fetchById', () => {
    it('should fetch detail and set currentItem', async () => {
      const store = useOptionTypeStore();
      const mockItem: OptionTypeListItem = makeItem({ id: '1', name: 'Size', presentation: 'Size' });
      vi.mocked(optionTypeService.getById).mockResolvedValue(createMockResult(mockItem));

      await store.fetchById('1');

      expect(optionTypeService.getById).toHaveBeenCalledWith('1');
      expect(store.currentItem).toEqual(mockItem);
    });
  });

  describe('CRUD Actions', () => {
    it('create should call service', async () => {
      const store = useOptionTypeStore();
      const newItem = { name: 'New', presentation: 'New', position: 0, filterable: false };
      vi.mocked(optionTypeService.create).mockResolvedValue(createMockResult(makeItem({ ...newItem, id: '123' })));

      const result = await store.create(newItem);

      expect(optionTypeService.create).toHaveBeenCalledWith(newItem);
      expect(result.isSuccess).toBe(true);
    });

    it('remove should update list state on success', async () => {
      const store = useOptionTypeStore();
      store.items = [
        { id: '1', name: 'A', presentation: 'A', position: 1, filterable: false, optionValuesCount: 0, productsCount: 0, createdAtUtc: '', modifiedAtUtc: '' },
        { id: '2', name: 'B', presentation: 'B', position: 2, filterable: false, optionValuesCount: 0, productsCount: 0, createdAtUtc: '', modifiedAtUtc: '' }
      ];
      store.totalRecords = 2;

      vi.mocked(optionTypeService.delete).mockResolvedValue(createMockResult<void>(undefined));

      await store.remove('1');

      expect(optionTypeService.delete).toHaveBeenCalledWith('1');
      expect(store.items).toHaveLength(1);
      expect(store.items[0]!.id).toBe('2');
      expect(store.totalRecords).toBe(1);
    });
  });
});
