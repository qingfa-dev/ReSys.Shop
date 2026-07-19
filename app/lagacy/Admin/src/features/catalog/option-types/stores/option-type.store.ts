import { defineStore } from 'pinia';
import { ref } from 'vue';
import { usePagedList } from '@/shared/composables/paged-list.use';
import { optionTypeService } from '../services/option-type.service';
import type { OptionTypeListItem, OptionTypeDetail } from '../types/option-type.response.type'
import type { CreateOptionTypeRequest, UpdateOptionTypeRequest } from '../types/option-type.request.type'
import type { OptionTypeQuery } from '../types/option-type.query.type'
import type { ServerResult } from '@/shared/api/types/result.types'

export const useOptionTypeStore = defineStore('option-type', () => {
  const currentItem = ref<OptionTypeDetail | null>(null);

  const { items, loading, totalRecords, params, fetch: fetchList } = usePagedList<OptionTypeListItem, OptionTypeQuery>(
    (p) => optionTypeService.list(p),
    { sort: ['position'] },
  );

  async function fetchById(id: string) {
    loading.value = true;
    const result = await optionTypeService.getById(id);
    if (result.isSuccess) {
      currentItem.value = result.value;
    }
    loading.value = false;
    return result;
  }

  async function create(request: CreateOptionTypeRequest): Promise<ServerResult<OptionTypeDetail>> {
    loading.value = true;
    const result = await optionTypeService.create(request);
    loading.value = false;
    return result;
  }

  async function update(id: string, request: UpdateOptionTypeRequest): Promise<ServerResult<OptionTypeDetail>> {
    loading.value = true;
    const result = await optionTypeService.update(id, request);
    if (result.isSuccess) {
      currentItem.value = result.value;
    }
    loading.value = false;
    return result;
  }

  async function remove(id: string): Promise<ServerResult<void>> {
    loading.value = true;
    const result = await optionTypeService.delete(id);
    if (result.isSuccess) {
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
    params,
    fetchList,
    fetchById,
    create,
    update,
    remove,
    clearCurrent
  };
});
