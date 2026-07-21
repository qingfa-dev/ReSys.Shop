import { describe, it, expect, vi, beforeEach } from 'vitest';
import { setActivePinia, createPinia } from 'pinia';
import { useOptionValueStore } from '../store/option-value.store';
import { optionValueRepository } from '../api/option-value.api';
import { createMockResult, createMockPagedResult } from '@/common/test/mock-types';
import type { OptionValueListItem } from '../models/option-value.response';

// Mock service
vi.mock('../api/option-value.api', () => ({
  optionValueRepository: {
    list: vi.fn(),
    listFlat: vi.fn(),
    create: vi.fn(),
    update: vi.fn(),
    delete: vi.fn(),
    reorder: vi.fn()
  }
}));

describe('OptionValueStore', () => {
  beforeEach(() => {
    setActivePinia(createPinia());
    vi.clearAllMocks();
  });

  describe('fetchValues (Nested)', () => {
    it('should fetch values for an option type and sort them', async () => {
      const store = useOptionValueStore();
      const mockData: OptionValueListItem[] = [
        { id: '2', name: 'Medium', presentation: 'M', position: 2, optionTypeId: 'type-123' },
        { id: '1', name: 'Small', presentation: 'S', position: 1, optionTypeId: 'type-123' }
      ];
      
      vi.mocked(optionValueRepository.list).mockResolvedValue(createMockPagedResult(mockData, { pageSize: 50 }));

      await store.fetchValues('type-123');

      expect(optionValueRepository.list).toHaveBeenCalledWith({ optionTypeId: 'type-123' });
      expect(store.values).toHaveLength(2);
      expect(store.values[0]!.id).toBe('1'); // Sorted by position
    });
  });

  describe('fetchList (Flat)', () => {
    it('should fetch all values and update state', async () => {
      const store = useOptionValueStore();
      const mockData: OptionValueListItem[] = [{ id: '1', name: 'Blue', presentation: 'Blue', position: 0, optionTypeId: 'type-1' }];
      
      vi.mocked(optionValueRepository.list).mockResolvedValue(createMockPagedResult(mockData));

      await store.fetchList({ search: 'Blue' });

      expect(optionValueRepository.list).toHaveBeenCalled();
      expect(store.items).toEqual(mockData);
      expect(store.totalRecords).toBe(1);
      expect(store.query.search).toBe('Blue');
    });
  });

  describe('CRUD Actions', () => {
    it('create should call service and add to list', async () => {
      const store = useOptionValueStore();
      const newData = { name: 'Large', presentation: 'L', position: 3 };
      vi.mocked(optionValueRepository.create).mockResolvedValue(createMockResult({ ...newData, id: '3', optionTypeId: 'type-123' }));

      const result = await store.create('type-123', newData);

      expect(optionValueRepository.create).toHaveBeenCalledWith('type-123', newData);
      expect(result.isSuccess).toBe(true);
      expect(store.values).toContainEqual(expect.objectContaining({ id: '3' }));
    });

    it('update should update local state', async () => {
      const store = useOptionValueStore();
      store.values = [{ id: '1', optionTypeId: 'type-123', name: 'Small', presentation: 'S', position: 1 }];
      
      const updatedData = { name: 'Small Updated', presentation: 'S!', position: 1 };
      vi.mocked(optionValueRepository.update).mockResolvedValue(createMockResult({ id: '1', optionTypeId: 'type-123', ...updatedData }));

      await store.update('1', updatedData);

      expect(store.values[0]!.presentation).toBe('S!');
    });

    it('remove should filter local state', async () => {
      const store = useOptionValueStore();
      store.values = [
        { id: '1', optionTypeId: 'type-1', name: 'A', presentation: 'A', position: 1 },
        { id: '2', optionTypeId: 'type-1', name: 'B', presentation: 'B', position: 2 }
      ];

      vi.mocked(optionValueRepository.delete).mockResolvedValue(createMockResult<void>(undefined));

      await store.remove('1');

      expect(store.values).toHaveLength(1);
      expect(store.values[0]!.id).toBe('2');
    });
  });
});
