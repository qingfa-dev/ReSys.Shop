import { describe, it, expect, vi, beforeEach } from 'vitest';
import { setActivePinia, createPinia } from 'pinia';
import { useOptionTypeStore } from '../store/option-type.store';
import { optionTypeRepository as OptionTypeRepository } from '../api/option-type.api';
import { createMockPagedResult, createMockResult, createMockErrorResult } from '@/common/test/mock-types';
import type { OptionTypeListItem } from '../models/option-type.response'
import type { ServerPagedResult } from '@/common/api/types'

const makeItem = (overrides?: Partial<OptionTypeListItem>): OptionTypeListItem => ({
  id: '', name: '', presentation: '', position: 0, filterable: false,
  optionValuesCount: 0, productsCount: 0, createdAtUtc: '', modifiedAtUtc: '',
  ...overrides,
})

// Mock service
vi.mock('../api/option-type.api', () => ({
  optionTypeRepository: {
    list: vi.fn<() => void>(),
    getById: vi.fn<() => void>(),
    create: vi.fn<() => void>(),
    update: vi.fn<() => void>(),
    delete: vi.fn<() => void>()
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
      vi.mocked(OptionTypeRepository.list).mockResolvedValue(createMockPagedResult(mockData));

      await store.fetchList();

      expect(OptionTypeRepository.list).toHaveBeenCalled();
      expect(store.items).toEqual(mockData);
      expect(store.totalRecords).toBe(1);
      expect(store.loading).toBe(false);
    });

    it('should handle errors gracefully', async () => {
      const store = useOptionTypeStore();
      vi.mocked(OptionTypeRepository.list).mockResolvedValue(
        createMockErrorResult<OptionTypeListItem[]>({ statusCode: 500, errors: [{ code: 'Error', message: 'Err', type: 4, metadata: null }], message: 'Err' }) as unknown as ServerPagedResult<OptionTypeListItem>
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
      vi.mocked(OptionTypeRepository.getById).mockResolvedValue(createMockResult(mockItem));

      await store.fetchById('1');

      expect(OptionTypeRepository.getById).toHaveBeenCalledWith('1');
      expect(store.currentItem).toEqual(mockItem);
    });
  });

  describe('CRUD Actions', () => {
    it('create should call service', async () => {
      const store = useOptionTypeStore();
      const newItem = { name: 'New', presentation: 'New', position: 0, filterable: false };
      vi.mocked(OptionTypeRepository.create).mockResolvedValue(createMockResult(makeItem({ ...newItem, id: '123' })));

      const result = await store.create(newItem);

      expect(OptionTypeRepository.create).toHaveBeenCalledWith(newItem);
      expect(result.isSuccess).toBe(true);
    });

    it('remove should delete and refetch list on success', async () => {
      const store = useOptionTypeStore();
      const remaining = [
        { id: '2', name: 'B', presentation: 'B', position: 2, filterable: false, optionValuesCount: 0, productsCount: 0, createdAtUtc: '', modifiedAtUtc: '' }
      ];

      vi.mocked(OptionTypeRepository.delete).mockResolvedValue(createMockResult<void>(undefined));
      vi.mocked(OptionTypeRepository.list).mockResolvedValue(createMockPagedResult(remaining));

      await store.remove('1');

      expect(OptionTypeRepository.delete).toHaveBeenCalledWith('1');
      expect(OptionTypeRepository.list).toHaveBeenCalled();
      expect(store.items).toHaveLength(1);
      expect(store.items[0]!.id).toBe('2');
    });
  });
});
