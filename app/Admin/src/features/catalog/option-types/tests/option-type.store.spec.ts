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
        success: true,
        data: mockData,
        meta: { totalCount: 1, page_count: 1 }
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
      vi.mocked(optionTypeService.list).mockResolvedValue({ success: false, error: { title: 'Err' } } as any);

      await store.fetchList();

      expect(store.items).toEqual([]);
      expect(store.loading).toBe(false);
    });
  });

  describe('fetchById', () => {
    it('should fetch detail and set currentItem', async () => {
      const store = useOptionTypeStore();
      const mockItem = { id: '1', name: 'Size', presentation: 'Size', optionValues: [] };
      vi.mocked(optionTypeService.getById).mockResolvedValue({ success: true, data: mockItem } as any);

      await store.fetchById('1');

      expect(optionTypeService.getById).toHaveBeenCalledWith('1');
      expect(store.currentItem).toEqual(mockItem);
    });
  });

  describe('CRUD Actions', () => {
    it('create should call service', async () => {
      const store = useOptionTypeStore();
      const newItem = { name: 'New', presentation: 'New', position: 0, filterable: false };
      vi.mocked(optionTypeService.create).mockResolvedValue({ success: true, data: { ...newItem, id: '123' } } as any);

      const result = await store.create(newItem);

      expect(optionTypeService.create).toHaveBeenCalledWith(newItem);
      expect(result.success).toBe(true);
    });

    it('remove should update list state on success', async () => {
      const store = useOptionTypeStore();
      store.items = [{ id: '1', name: 'A' } as any, { id: '2', name: 'B' } as any];
      store.totalRecords = 2;

      vi.mocked(optionTypeService.delete).mockResolvedValue({ success: true } as any);

      await store.remove('1');

      expect(optionTypeService.delete).toHaveBeenCalledWith('1');
      expect(store.items).toHaveLength(1);
      expect(store.items[0]!.id).toBe('2');
      expect(store.totalRecords).toBe(1);
    });
  });
});
