import { defineStore } from 'pinia';
import { ref } from 'vue';
import { usePagedList } from '@/shared/composables/paged-list.use';
import { propertyTypeService } from '../services/property-type.service';
import type { PropertyTypeListItem, PropertyTypeDetail } from '../types/property-type.domain.types'
import type { CreatePropertyTypeRequest, UpdatePropertyTypeRequest, PropertyTypeQuery } from '../types/property-type.request.types'
import type { ServerResult } from '@/shared/api/types/result.types'

export const usePropertyTypeStore = defineStore('property-type', () => {
  const currentItem = ref<PropertyTypeDetail | null>(null);

  const { items, loading, totalRecords, params, fetch: fetchList } = usePagedList<PropertyTypeListItem, PropertyTypeQuery>(
    (p) => propertyTypeService.list(p),
    { sort: ['position'] },
  );

  async function fetchById(id: string) {
    loading.value = true;
    const result = await propertyTypeService.getById(id);
    if (result.isSuccess) {
      currentItem.value = result.value;
    }
    loading.value = false;
    return result;
  }

  async function create(request: CreatePropertyTypeRequest): Promise<ServerResult<PropertyTypeDetail>> {
    loading.value = true;
    const result = await propertyTypeService.create(request);
    loading.value = false;
    return result;
  }

  async function update(id: string, request: UpdatePropertyTypeRequest): Promise<ServerResult<PropertyTypeDetail>> {
    loading.value = true;
    const result = await propertyTypeService.update(id, request);
    if (result.isSuccess) {
        currentItem.value = result.value;
    }
    loading.value = false;
    return result;
  }

  async function remove(id: string): Promise<ServerResult<void>> {
    loading.value = true;
    const result = await propertyTypeService.delete(id);
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
