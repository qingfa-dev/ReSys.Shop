import { describe, it, expect, vi, beforeEach } from 'vitest';
import { setActivePinia, createPinia } from 'pinia';
import { useOptionValueStore } from '../stores/option-value.store';
import { optionValueService } from '../services/option-value.service';

// Mock service
vi.mock('../services/option-value.service', () => ({
  optionValueService: {
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
      const mockData = [
        { id: '2', name: 'Medium', presentation: 'M', position: 2 },
        { id: '1', name: 'Small', presentation: 'S', position: 1 }
      ];
      
      vi.mocked(optionValueService.list).mockResolvedValue({
        success: true,
        data: mockData
      } as any);

      await store.fetchValues('type-123');

      expect(optionValueService.list).toHaveBeenCalledWith({ option_type_id: 'type-123' });
      expect(store.values).toHaveLength(2);
      expect(store.values[0]!.id).toBe('1'); // Sorted by position
    });
  });

  describe('fetchList (Flat)', () => {
    it('should fetch all values and update state', async () => {
      const store = useOptionValueStore();
      const mockData = [{ id: '1', name: 'Blue', presentation: 'Blue', position: 0 }];
      
      vi.mocked(optionValueService.list).mockResolvedValue({
        success: true,
        data: mockData,
        meta: { total_count: 1 } as any
      } as any);

      await store.fetchList({ search: 'Blue' });

      expect(optionValueService.list).toHaveBeenCalled();
      expect(store.items).toEqual(mockData);
      expect(store.totalRecords).toBe(1);
      expect(store.query.search).toBe('Blue');
    });
  });

  describe('CRUD Actions', () => {
    it('create should call service and add to list', async () => {
      const store = useOptionValueStore();
      const newData = { name: 'Large', presentation: 'L', position: 3 };
      vi.mocked(optionValueService.create).mockResolvedValue({
        success: true,
        data: { ...newData, id: '3', option_type_id: 'type-123' }
      } as any);

      const result = await store.create('type-123', newData);

      expect(optionValueService.create).toHaveBeenCalledWith({ ...newData, option_type_id: 'type-123' });
      expect(result.success).toBe(true);
      expect(store.values).toContainEqual(expect.objectContaining({ id: '3' }));
    });

    it('update should update local state', async () => {
      const store = useOptionValueStore();
      store.values = [{ id: '1', option_type_id: 'type-123', name: 'Small', presentation: 'S', position: 1 }];
      
      const updatedData = { name: 'Small Updated', presentation: 'S!', position: 1 };
      vi.mocked(optionValueService.update).mockResolvedValue({
        success: true,
        data: { id: '1', option_type_id: 'type-123', ...updatedData }
      } as any);

      await store.update('1', updatedData);

      expect(store.values[0]!.presentation).toBe('S!');
    });

    it('remove should filter local state', async () => {
      const store = useOptionValueStore();
      store.values = [{ id: '1', name: 'A' } as any, { id: '2', name: 'B' } as any];

      vi.mocked(optionValueService.delete).mockResolvedValue({ success: true } as any);

      await store.remove('1');

      expect(store.values).toHaveLength(1);
      expect(store.values[0]!.id).toBe('2');
    });
  });
});
