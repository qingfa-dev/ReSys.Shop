import { defineStore } from 'pinia';
import { ref } from 'vue';
import { optionValueService } from '../services/option-value.service';
import type { 
  OptionValueListItem,
  CreateOptionValueRequest,
  UpdateOptionValueRequest,
  OptionValueQuery,
  ApiResult
} from '../types/option-value.types';

export const useOptionValueStore = defineStore('option-value', () => {
  const values = ref<OptionValueListItem[]>([]);
  
  // State for Flat List View
  const items = ref<OptionValueListItem[]>([]);
  const totalRecords = ref(0);
  const query = ref<OptionValueQuery>({
    page: 1,
    page_size: 10,
    sort: 'position'
  });

  const loading = ref(false);

  async function fetchValues(option_type_id: string, queryParams?: Partial<OptionValueQuery>) {
    loading.value = true;
    const result = await optionValueService.list({ ...queryParams, option_type_id });
    if (result.success && result.data) {
      values.value = result.data;
      values.value.sort((a, b) => a.position - b.position);
    }
    loading.value = false;
    return result;
  }

  async function fetchList(params?: Partial<OptionValueQuery>) {
    loading.value = true;
    if (params) {
        query.value = { ...query.value, ...params };
    }
    
    const result = await optionValueService.list(query.value);
    if (result.success && result.data) {
        items.value = result.data;
        totalRecords.value = result.meta?.total_count || 0;
    }
    loading.value = false;
    return result;
  }

  async function create(option_type_id: string, payload: Omit<CreateOptionValueRequest, 'option_type_id'>): Promise<ApiResult<OptionValueListItem>> {
    loading.value = true;
    const request: CreateOptionValueRequest = { ...payload, option_type_id };
    const result = await optionValueService.create(request);
    
    if (result.success && result.data) {
        values.value.push(result.data);
        values.value.sort((a, b) => a.position - b.position);
    }
    loading.value = false;
    return result;
  }

  async function update(id: string, request: UpdateOptionValueRequest): Promise<ApiResult<OptionValueListItem>> {
    loading.value = true;
    const result = await optionValueService.update(id, request);
    if (result.success && result.data) {
      const idx = values.value.findIndex(v => v.id === id);
      if (idx !== -1) {
        values.value[idx] = result.data;
        values.value.sort((a, b) => a.position - b.position);
      }
    }
    loading.value = false;
    return result;
  }

  async function remove(id: string): Promise<ApiResult<void>> {
    loading.value = true;
    const result = await optionValueService.delete(id);
    if (result.success) {
      values.value = values.value.filter(v => v.id !== id);
    }
    loading.value = false;
    return result;
  }

  async function updatePositions(option_type_id: string, positions: { id: string; position: number }[]): Promise<ApiResult<void>> {
    loading.value = true;
    const result = await optionValueService.reorder({ option_type_id, positions });
    if (result.success) {
      positions.forEach(p => {
          const val = values.value.find(v => v.id === p.id);
          if (val) val.position = p.position;
      });
      values.value.sort((a, b) => a.position - b.position);
    }
    loading.value = false;
    return result;
  }

  function clearValues() {
    values.value = [];
  }

  return {
    values,
    items,
    totalRecords,
    query,
    loading,
    fetchValues,
    fetchList,
    create,
    update,
    remove,
    updatePositions,
    clearValues
  };
});