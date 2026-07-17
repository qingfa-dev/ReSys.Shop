import { describe, it, expect, vi, beforeEach } from 'vitest';
import { setActivePinia, createPinia } from 'pinia';
import { useOptionTypeStore } from '../stores/option-type.store';
import { optionTypeService } from '../services/option-type.service';

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
      const mockData = [
        { id: '1', name: 'Color', presentation: 'Color', position: 1, filterable: true }
      ];
      const mockResponse = {
        isSuccess: true,
        statusCode: 200,
        errors: [],
        message: null,
        metadata: null,
        items: mockData,
        page: 1,
        pageSize: 10,
        totalCount: 1,
      };

      vi.mocked(optionTypeService.list).mockResolvedValue(mockResponse as any);

      await store.fetchList();

      expect(optionTypeService.list).toHaveBeenCalled();
      expect(store.items).toEqual(mockData);
      expect(store.totalRecords).toBe(1);
      expect(store.loading).toBe(false);
    });

    it('should handle errors gracefully', async () => {
      const store = useOptionTypeStore();
      vi.mocked(optionTypeService.list).mockResolvedValue({
        isSuccess: false,
        statusCode: 500,
        errors: [{ code: 'Error', message: 'Err', type: 4, metadata: null }],
        message: 'Err',
        metadata: null,
        items: [],
        page: 1,
        pageSize: 10,
        totalCount: 0,
      } as any);

      await store.fetchList();

      expect(store.items).toEqual([]);
      expect(store.loading).toBe(false);
    });
  });

  describe('fetchById', () => {
    it('should fetch detail and set currentItem', async () => {
      const store = useOptionTypeStore();
      const mockItem = { id: '1', name: 'Size', presentation: 'Size', optionValues: [] };
      vi.mocked(optionTypeService.getById).mockResolvedValue({ isSuccess: true, statusCode: 200, errors: [], message: null, metadata: null, value: mockItem } as any);

      await store.fetchById('1');

      expect(optionTypeService.getById).toHaveBeenCalledWith('1');
      expect(store.currentItem).toEqual(mockItem);
    });
  });

  describe('CRUD Actions', () => {
    it('create should call service', async () => {
      const store = useOptionTypeStore();
      const newItem = { name: 'New', presentation: 'New', position: 0, filterable: false };
      vi.mocked(optionTypeService.create).mockResolvedValue({ isSuccess: true, statusCode: 200, errors: [], message: null, metadata: null, value: { ...newItem, id: '123' } } as any);

      const result = await store.create(newItem);

      expect(optionTypeService.create).toHaveBeenCalledWith(newItem);
      expect(result.isSuccess).toBe(true);
    });

    it('remove should update list state on success', async () => {
      const store = useOptionTypeStore();
      store.items = [{ id: '1', name: 'A' } as any, { id: '2', name: 'B' } as any];
      store.totalRecords = 2;

      vi.mocked(optionTypeService.delete).mockResolvedValue({ isSuccess: true, statusCode: 200, errors: [], message: null, metadata: null, value: undefined } as any);

      await store.remove('1');

      expect(optionTypeService.delete).toHaveBeenCalledWith('1');
      expect(store.items).toHaveLength(1);
      expect(store.items[0]!.id).toBe('2');
      expect(store.totalRecords).toBe(1);
    });
  });
});
