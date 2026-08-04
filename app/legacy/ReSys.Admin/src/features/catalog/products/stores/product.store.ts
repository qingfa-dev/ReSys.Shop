import { defineStore } from 'pinia';
import { ref, watch } from 'vue';
import { productService } from '../services/product.service';
import { useToast } from '@/shared/composables/toast.use';
import type { 
  ProductSummary, 
  ProductDetail, 
  ProductSearchParams, 
  CreateProductRequest, 
  UpdateProductRequest,
  ProductClassification,
  ProductImage
} from '../types/product.types';
import type { ApiResult } from '@/shared/api/api.types';
import apiClient from '@/shared/api/api.client';

export const useProductStore = defineStore('product', () => {
  const { showToast } = useToast();

  // --- STATE ---
  const products = ref<ProductSummary[]>([]);
  const current_product = ref<ProductDetail | null>(null);
  const current_classifications = ref<ProductClassification[]>([]);
  const current_images = ref<ProductImage[]>([]);
  const loading = ref(false);
  const submitting = ref(false);
  const error = ref<string | null>(null);

  // Pagination & Search state
  const query = ref<ProductSearchParams>({
    page: 1,
    page_size: 10,
    search: '',
    sort_by: 'created_at',
    is_descending: true
  });

  const totalRecords = ref(0);

  // --- WATCHERS ---
  // Note: Auto-fetching is handled by the view via onFilter/onPage normally, 
  // but we keep the store state reactive.

  // --- ACTIONS ---
  async function fetchProducts(params: ProductSearchParams = {}) {
    loading.value = true;
    error.value = null;
    
    // Merge provided params with current query state
    query.value = { ...query.value, ...params };

    try {
      const result = await productService.list(query.value);
      if (result.success && result.data) {
        products.value = result.data;
        totalRecords.value = result.meta?.total_count || 0;
      } else {
        error.value = result.error?.title || 'Failed to fetch products';
      }
      return result;
    } catch (err) {
      error.value = 'An unexpected error occurred';
      throw err;
    } finally {
      loading.value = false;
    }
  }

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
        const result = (await apiClient.get(`/admin/catalog/products/${productId}/classifications`)) as unknown as ApiResult<any>;
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
        const result = (await apiClient.put(`/admin/catalog/products/${productId}/classifications`, data)) as unknown as ApiResult<any>;
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
    // State
    products,
    current_product,
    current_classifications,
    loading,
    submitting,
    error,
    query,
    totalRecords,
    
    // Actions
    fetchProducts,
    fetchProductById,
    createProduct,
    updateProduct,
    deleteProduct,
    fetchClassifications,
    updateClassifications
  };
});
