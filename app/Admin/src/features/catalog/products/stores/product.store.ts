import { defineStore } from "pinia";
import { ref } from "vue";
import { useI18n } from 'vue-i18n';
import { useToast } from "@/shared/composables/toast.use";
import { usePagedList } from "@/shared/composables/paged-list.use";
import { productService } from "../services/product.service";
import type {
  ProductSummary,
  ProductDetail,
  ProductClassification,
  ProductImage,
} from "../types/Product.Response.Type";
import type { CreateProductRequest, UpdateProductRequest } from "../types/Product.Request.Type";
import type { ProductQuery } from "../types/Product.Query.Type";

export const useProductStore = defineStore("product", () => {
  const { showToast } = useToast();
  const { t } = useI18n();

  // --- STATE ---
  const current_product = ref<ProductDetail | null>(null);
  const current_classifications = ref<ProductClassification[]>([]);
  const current_images = ref<ProductImage[]>([]);
  const submitting = ref(false);

  const {
    items: products,
    totalRecords,
    params: query,
    fetch: fetchProducts,
    loading,
    error,
  } = usePagedList<ProductSummary, ProductQuery>((p) => productService.list(p), {
    page: 1,
    pageSize: 10,
    search: "",
    sort: ["-createdAtUtc"],
  });

  async function fetchProductById(id: string) {
    loading.value = true;
    error.value = null;
    try {
      const result = await productService.getById(id);
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
      const result = await productService.create(data);
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
      const result = await productService.update(id, data);
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
      const result = await productService.delete(id);
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
      const result = await productService.getClassifications(productId);
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
      const result = await productService.syncClassifications(productId, data);
      if (result.isSuccess) {
        showToast("success", t('common.updated'), t('catalog.products.messages.classifications_saved'));
        await fetchClassifications(productId);
      }
      return result;
    } finally {
      submitting.value = false;
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
