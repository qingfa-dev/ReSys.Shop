import { defineStore } from 'pinia';
import { ref } from 'vue';
import { useToast } from '@/shared/composables/toast.use';
import { usePagedList } from '@/shared/composables/paged-list.use';
import { productService } from '../services/product.service';
import type { 
  ProductSummary, 
  ProductDetail, 
  ProductSearchParams, 
  CreateProductRequest, 
  UpdateProductRequest,
  ProductClassification,
  ProductImage
} from '../types/product.types';

export const useProductStore = defineStore('product', () => {
  const { showToast } = useToast();

  // --- STATE ---
  const current_product = ref<ProductDetail | null>(null);
  const current_classifications = ref<ProductClassification[]>([]);
  const current_images = ref<ProductImage[]>([]);
  const submitting = ref(false);

  const { items: products, totalRecords, params: query, fetch: fetchProducts, loading, error } = usePagedList<ProductSummary, ProductSearchParams>(
    (p) => productService.list(p),
    { page: 1, pageSize: 10, search: '', sort: ['-created_at'] },
  );

  async function fetchProductById(id: string) {
    loading.value = true;
    error.value = null;
    try {
      const result = await productService.getById(id);
      if (result.success && result.data) {
        current_product.value = result.data;
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
      if (result.success) {
        showToast('success', 'Created', 'Product created successfully');
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
      if (result.success) {
        showToast('success', 'Updated', 'Product updated successfully');
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
      if (result.success) {
        showToast('success', 'Deleted', 'Product removed successfully');
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
        const result = await productService.getClassifications(productId)
        if (result.success && result.data) {
            current_classifications.value = result.data;
        }
        return result;
    } finally {
        loading.value = false;
    }
  }

  async function updateClassifications(productId: string, data: any) {
    submitting.value = true;
    try {
        const result = await productService.syncClassifications(productId, data)
        if (result.success) {
            showToast('success', 'Updated', 'Classifications saved');
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
    updateClassifications
  };
});
