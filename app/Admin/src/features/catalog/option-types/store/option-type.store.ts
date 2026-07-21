import { defineStore } from "pinia";
import { ref } from "vue";
import { usePagedList } from "@/common/composables/paged-list.use";
import { optionTypeRepository } from "../api/option-type.api";
import { mapToListItem, mapToDetail } from "../api/option-type.mapper";
import type { OptionTypeQuery } from "../types/option-type.query";
import type { ServerResult } from "@/common/api/types/result.types";
import type {
  CreateOptionTypeRequest,
  OptionTypeDetail,
  OptionTypeListItem,
  UpdateOptionTypeRequest,
} from "../models";

export const useOptionTypeStore = defineStore("option-type", () => {
  const currentItem = ref<OptionTypeDetail | null>(null);

  const {
    items,
    loading,
    totalRecords,
    params,
    fetch: fetchList,
  } = usePagedList<OptionTypeListItem, OptionTypeQuery>(
    async (p) => {
      const result = await optionTypeRepository.list(p);
      return { ...result, items: result.items?.map(mapToListItem) ?? [] };
    },
    { sort: ["position"] },
  );

  async function fetchById(id: string) {
    loading.value = true;
    const result = await optionTypeRepository.getById(id);
    if (result.isSuccess && result.value) {
      currentItem.value = mapToDetail(result.value);
    }
    loading.value = false;
    return result;
  }

  async function create(request: CreateOptionTypeRequest): Promise<ServerResult<OptionTypeDetail>> {
    loading.value = true;
    const result = await optionTypeRepository.create(request);
    if (result.isSuccess && result.value) {
      currentItem.value = mapToDetail(result.value);
    }
    loading.value = false;
    return result.isSuccess ? { ...result, value: currentItem.value } : result;
  }

  async function update(
    id: string,
    request: UpdateOptionTypeRequest,
  ): Promise<ServerResult<OptionTypeDetail>> {
    loading.value = true;
    const result = await optionTypeRepository.update(id, request);
    if (result.isSuccess && result.value) {
      currentItem.value = mapToDetail(result.value);
    }
    loading.value = false;
    return result.isSuccess ? { ...result, value: currentItem.value } : result;
  }

  async function remove(id: string): Promise<ServerResult<void>> {
    loading.value = true;
    const result = await optionTypeRepository.delete(id);
    if (result.isSuccess) {
      items.value = items.value.filter((i) => i.id !== id);
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
    clearCurrent,
  };
});
