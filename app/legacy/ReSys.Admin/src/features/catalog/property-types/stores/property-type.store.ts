import { defineStore } from 'pinia';
import { ref } from 'vue';
import { propertyTypeService } from '../services/property-type.service';
import type { 
  PropertyTypeListItem, 
  PropertyTypeDetail, 
  CreatePropertyTypeRequest, 
  UpdatePropertyTypeRequest, 
  PropertyTypeQuery,
  ApiResult
} from '../types/property-type.types';

export const usePropertyTypeStore = defineStore('property-type', () => {
  const items = ref<PropertyTypeListItem[]>([]);
  const currentItem = ref<PropertyTypeDetail | null>(null);
  const loading = ref(false);
  const totalRecords = ref(0);
  const query = ref<PropertyTypeQuery>({
    page: 1,
    page_size: 10,
    sort: 'position',
  });

  async function fetchList(params?: Partial<PropertyTypeQuery>) {
    loading.value = true;
    if (params) {
      query.value = { ...query.value, ...params };
    }

    const result = await propertyTypeService.getList(query.value);
    if (result.success && result.data) {
      items.value = result.data;
      totalRecords.value = result.meta?.total_count || 0;
    }
    loading.value = false;
    return result;
  }

  async function fetchById(id: string) {
    loading.value = true;
    const result = await propertyTypeService.getById(id);
    if (result.success && result.data) {
      currentItem.value = result.data;
    }
    loading.value = false;
    return result;
  }

  async function create(request: CreatePropertyTypeRequest): Promise<ApiResult<PropertyTypeDetail>> {
    loading.value = true;
    const result = await propertyTypeService.create(request);
    loading.value = false;
    return result;
  }

  async function update(id: string, request: UpdatePropertyTypeRequest): Promise<ApiResult<PropertyTypeDetail>> {
    loading.value = true;
    const result = await propertyTypeService.update(id, request);
    if (result.success && result.data) {
        currentItem.value = result.data;
    }
    loading.value = false;
    return result;
  }

  async function remove(id: string): Promise<ApiResult<void>> {
    loading.value = true;
    const result = await propertyTypeService.delete(id);
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
