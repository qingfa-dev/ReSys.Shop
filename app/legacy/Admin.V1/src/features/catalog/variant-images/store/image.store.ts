import { defineStore } from "pinia";
import { ref } from "vue";
import { imageApi } from "../api/image.api";
import type { VariantImage } from "../models/image.response";

export const useImageStore = defineStore("variantImage", () => {
  const items = ref<VariantImage[]>([]);

  async function fetchByVariant(variantId: string) {
    const result = await imageApi.listByVariant(variantId);
    if (result.isSuccess) items.value = result.value;
    return result;
  }

  return { items, fetchByVariant };
});
