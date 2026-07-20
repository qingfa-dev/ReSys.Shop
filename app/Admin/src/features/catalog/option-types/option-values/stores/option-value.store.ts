import { defineStore } from 'pinia';
import { ref } from 'vue';
import { optionValueService } from '../services/option-value.service';
import type { OptionValueListItem } from '../types/option-value.response.type'
import type { OptionValueQuery } from '../types/option-value.query.type'
import type { CreateOptionValueRequest, UpdateOptionValueRequest } from '../types/option-value.request.type'
import type { ServerResult, ServerPagedResult } from '@/common/api/types/result.types'

export const useOptionValueStore = defineStore('option-value', () => {
  const values = ref<OptionValueListItem[]>([]);
  
  // State for Flat List View
  const items = ref<OptionValueListItem[]>([]);
  const totalRecords = ref(0);
  const query = ref<OptionValueQuery>({
    page: 1,
    pageSize: 10,
    sort: ['position']
  });

  const loading = ref(false);

  async function fetchValues(optionTypeId: string, queryParams?: Partial<OptionValueQuery>) {
    loading.value = true;
    const result = await optionValueService.list({ ...queryParams, optionTypeId } as OptionValueQuery);
    if (result.isSuccess) {
      values.value = result.items;
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
    if (result.isSuccess) {
        items.value = result.items;
        totalRecords.value = result.totalCount || 0;
    }
    loading.value = false;
    return result;
  }

  async function create(optionTypeId: string, payload: Omit<CreateOptionValueRequest, 'optionTypeId'>): Promise<ServerResult<OptionValueListItem>> {
    loading.value = true;
    const result = await optionValueService.create(optionTypeId, payload);
    
    if (result.isSuccess) {
        values.value.push(result.value);
        values.value.sort((a, b) => a.position - b.position);
    }
    loading.value = false;
    return result;
  }

  async function update(id: string, request: UpdateOptionValueRequest): Promise<ServerResult<OptionValueListItem>> {
    loading.value = true;
    const optionTypeId = request.optionTypeId || values.value.find(v => v.id === id)?.optionTypeId || '';
    const result = await optionValueService.update(optionTypeId, id, request);
    if (result.isSuccess) {
      const idx = values.value.findIndex(v => v.id === id);
      if (idx !== -1) {
        values.value[idx] = result.value;
        values.value.sort((a, b) => a.position - b.position);
      }
    }
    loading.value = false;
    return result;
  }

  async function remove(id: string): Promise<ServerResult<void>> {
    loading.value = true;
    const optionTypeId = values.value.find(v => v.id === id)?.optionTypeId || '';
    const result = await optionValueService.delete(optionTypeId, id);
    if (result.isSuccess) {
      values.value = values.value.filter(v => v.id !== id);
    }
    loading.value = false;
    return result;
  }

  async function updatePositions(optionTypeId: string, positions: { id: string; position: number }[]): Promise<ServerResult<void>> {
    loading.value = true;
    const result = await optionValueService.reorder({ optionTypeId, positions });
    if (result.isSuccess) {
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
