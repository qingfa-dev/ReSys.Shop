import apiClient from '@/shared/api/api.client'
import type { ApiResult, PagedList } from '@/shared/api/api.types'
import type {
  ProductSummary,
  ProductDetail,
  CreateProductRequest,
  UpdateProductRequest,
  ProductSearchParams,
  ProductProperty,
} from '../types/product.types'
import type { ProductImage } from '../types/product.types'

export const productService = {
  async list(params: ProductSearchParams): Promise<ApiResult<ProductSummary[]>> {
    return apiClient.get('/admin/catalog/products', { params })
  },

  async getById(id: string): Promise<ApiResult<ProductDetail>> {
    return apiClient.get(`/admin/catalog/products/${id}`)
  },

  async create(data: CreateProductRequest): Promise<ApiResult<ProductDetail>> {
    // Backend requires 'presentation' (Display Name) as well.
    // If not provided in the form (which currently only has 'name'), map it from name.
    const payload = { ...data, presentation: (data as any).presentation || data.name }
    return apiClient.post('/admin/catalog/products', payload)
  },

  async update(id: string, data: UpdateProductRequest): Promise<ApiResult<ProductDetail>> {
    return apiClient.put(`/admin/catalog/products/${id}`, data)
  },

  async delete(id: string): Promise<ApiResult<void>> {
    return apiClient.delete(`/admin/catalog/products/${id}`)
  },

  // --- Option Types ---
  async getOptionTypes(productId: string): Promise<ApiResult<any[]>> {
    return apiClient.get(`/admin/catalog/products/option-types`, {
      params: { product_id: productId },
    })
  },

  async updateOptionTypes(productId: string, optionTypeIds: string[]): Promise<ApiResult<void>> {
    return apiClient.put(`/admin/catalog/products/option-types`, {
      product_id: productId,
      option_type_ids: optionTypeIds,
    })
  },

  // --- Properties ---
  async getProperties(productId: string): Promise<ApiResult<ProductProperty[]>> {
    return apiClient.get(`/admin/catalog/products/properties`, {
      params: { product_id: productId },
    })
  },

  async updateProperties(productId: string, properties: any[]): Promise<ApiResult<void>> {
    return apiClient.put(`/admin/catalog/products/properties`, {
      product_id: productId,
      properties: properties,
    })
  },

  // --- Images ---
  async getImages(productId: string): Promise<ApiResult<ProductImage[]>> {
    return apiClient.get(`/admin/catalog/products/images`, {
      params: { product_id: productId },
    })
  },

  async uploadImage(
    productId: string,
    file: File,
    role: number,
    alt?: string,
  ): Promise<ApiResult<ProductImage>> {
    const formData = new FormData()
    formData.append('file', file)
    // Append params to query string to ensure binding
    let url = `/admin/catalog/products/images?product_id=${productId}&role=${role}`
    if (alt) {
      url += `&alt=${encodeURIComponent(alt)}`
    }

    return apiClient.post(url, formData, {
      headers: { 'Content-Type': 'multipart/form-data' },
    })
  },

  async updateImage(
    productId: string,
    imageId: string,
    role: number,
    alt: string,
  ): Promise<ApiResult<void>> {
    return apiClient.put(`/admin/catalog/products/images/${imageId}?product_id=${productId}`, {
      role,
      alt,
      position: 0, // Backend might require position, default to 0 for now
    })
  },

  async deleteImage(productId: string, imageId: string): Promise<ApiResult<void>> {
    return apiClient.delete(`/admin/catalog/products/images/${imageId}`, {
      params: { product_id: productId },
    })
  },
}
