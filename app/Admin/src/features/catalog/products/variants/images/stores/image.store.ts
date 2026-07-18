import { defineStore } from "pinia";
import { ref } from "vue";
import { imageService } from "../services/image.service";
import type { VariantImage } from "../types/image.response.type";

export const useImageStore = defineStore("variantImage", () => {
  const items = ref<VariantImage[]>([]);

  async function fetchByVariant(variantId: string) {
    const result = await imageService.listByVariant(variantId);
    if (result.isSuccess) items.value = result.value;
    return result;
  }

  return { items, fetchByVariant };
});
