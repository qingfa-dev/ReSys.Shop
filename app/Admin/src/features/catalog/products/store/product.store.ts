import { defineStore } from "pinia";
import { ref } from "vue";
import { useI18n } from 'vue-i18n';
import { useToast } from "@/common/composables/toast.use";
import { usePagedList } from "@/common/composables/paged-list.use";
import { productRepository } from "../api/product.api";
import type { ProductSummaryModel, ProductDetailModel } from '../models/product.model';
import type { ProductClassification } from "../classifications/types/classification.response";
import type { ProductImage } from "../types/product-image.response";
import type { CreateProductRequest, UpdateProductRequest } from "../types/product.request";
import type { ProductQuery } from "../types/product.query";

export const useProductStore = defineStore("product", () => {
  const { showToast } = useToast();
  const { t } = useI18n();

  // --- STATE ---
  const current_product = ref<ProductDetailModel | null>(null);
  const current_classifications = ref<ProductClassification[]>([]);
  const _current_images = ref<ProductImage[]>([]);
  const submitting = ref(false);

  const {
    items: products,
    totalRecords,
    params: query,
    fetch: fetchProducts,
    loading,
    error,
  } = usePagedList<ProductSummaryModel, ProductQuery>((p) => productRepository.list(p), {
    page: 1,
    pageSize: 10,
    search: "",
    sort: ["-createdAtUtc"],
  });

  async function fetchProductById(id: string) {
    loading.value = true;
    error.value = null;
    try {
      const result = await productRepository.getById(id);
      if (result.isSuccess && result.value) {
        current_product.value = result.value;
      }
      return result;
    } finally {
      loading.value = false;
    }
  }

  async function createProduct(data: CreateProductRequest) {
    submitting.value = true;
    try {
      const result = await productRepository.create(data);
      if (result.isSuccess) {
        showToast("success", t('common.created'), t('catalog.products.messages.create_success'));
        await fetchProducts();
      }
      return result;
    } finally {
      submitting.value = false;
    }
  }

  async function updateProduct(id: string, data: UpdateProductRequest) {
    submitting.value = true;
    try {
      const result = await productRepository.update(id, data);
      if (result.isSuccess) {
        showToast("success", t('common.updated'), t('catalog.products.messages.update_success'));
        await fetchProducts();
      }
      return result;
    } finally {
      submitting.value = false;
    }
  }

  async function deleteProduct(id: string) {
    loading.value = true;
    try {
      const result = await productRepository.delete(id);
      if (result.isSuccess) {
        showToast("success", t('common.deleted'), t('catalog.products.messages.delete_success'));
        await fetchProducts();
      }
      return result;
    } finally {
      loading.value = false;
    }
  }

  async function fetchClassifications(productId: string) {
    loading.value = true;
    try {
      const result = await productRepository.getClassifications(productId);
      if (result.isSuccess && result.value) {
        current_classifications.value = result.value;
      }
      return result;
    } finally {
      loading.value = false;
    }
  }

  async function updateClassifications(
    productId: string,
    data: { taxonIds: string[]; mainTaxonId?: string },
  ) {
    submitting.value = true;
    try {
      const result = await productRepository.syncClassifications(productId, data);
      if (result.isSuccess) {
        showToast("success", t('common.updated'), t('catalog.products.messages.classifications_saved'));
        await fetchClassifications(productId);
      }
      return result;
    } finally {
      loading.value = false;
    }
  }

  return {
    products,
    current_product,
    current_classifications,
    loading,
    submitting,
    error,
    query,
    totalRecords,
    fetchProducts,
    fetchProductById,
    createProduct,
    updateProduct,
    deleteProduct,
    fetchClassifications,
    updateClassifications,
  };
});
