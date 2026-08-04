import { defineStore } from 'pinia';
import { ref } from 'vue';
import { optionTypeService } from '../services/option-type.service';
import type { 
  OptionTypeListItem, 
  OptionTypeDetail, 
  CreateOptionTypeRequest, 
  UpdateOptionTypeRequest, 
  OptionTypeQuery,
  ApiResult
} from '../types/option-type.types';

export const useOptionTypeStore = defineStore('option-type', () => {
  const items = ref<OptionTypeListItem[]>([]);
  const currentItem = ref<OptionTypeDetail | null>(null);
  const loading = ref(false);
  const totalRecords = ref(0);
  const query = ref<OptionTypeQuery>({
    page: 1,
    page_size: 10,
    sort: 'position',
  });

  // Option Type actions
  async function fetchList(params?: Partial<OptionTypeQuery>) {
    loading.value = true;
    if (params) {
      query.value = { ...query.value, ...params };
    }

    const result = await optionTypeService.getList(query.value);
    if (result.success && result.data) {
      items.value = result.data;
      totalRecords.value = result.meta?.total_count || 0;
    }
    loading.value = false;
    return result;
  }

  async function fetchById(id: string) {
    loading.value = true;
    const result = await optionTypeService.getById(id);
    if (result.success && result.data) {
      currentItem.value = result.data;
    }
    loading.value = false;
    return result;
  }

  async function create(request: CreateOptionTypeRequest): Promise<ApiResult<OptionTypeDetail>> {
    loading.value = true;
    const result = await optionTypeService.create(request);
    loading.value = false;
    return result;
  }

  async function update(id: string, request: UpdateOptionTypeRequest): Promise<ApiResult<OptionTypeDetail>> {
    loading.value = true;
    const result = await optionTypeService.update(id, request);
    if (result.success && result.data) {
      currentItem.value = result.data;
    }
    loading.value = false;
    return result;
  }

  async function remove(id: string): Promise<ApiResult<void>> {
    loading.value = true;
    const result = await optionTypeService.delete(id);
    if (result.success) {
      items.value = items.value.filter(i => i.id !== id);
      totalRecords.value--;
    }
    loading.value = false;
    return result;
  }

  function clearCurrent() {
    currentItem.value = null;
  }

  return {
    items,
    currentItem,
    loading,
    totalRecords,
    query,
    fetchList,
    fetchById,
    create,
    update,
    remove,
    clearCurrent
  };
});