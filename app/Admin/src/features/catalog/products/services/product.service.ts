import { catalogApi } from '../../services/catalog.api'
import type { ApiResult } from '@/shared/api/types/api.types'

export const productService = {
  list: catalogApi.products.list,
  getById: catalogApi.products.getById,
  create: catalogApi.products.create,
  update: catalogApi.products.update,
  delete: catalogApi.products.delete,
  activate: catalogApi.products.activate,
  discontinue: catalogApi.products.discontinue,
  getOptionTypes: catalogApi.products.getOptionTypes,
  syncOptionTypes: catalogApi.products.syncOptionTypes,
  getClassifications: catalogApi.products.getClassifications,
  syncClassifications: catalogApi.products.syncClassifications,

  // Image methods — currently at variant level in API; stubs for future product-level endpoints
  async getImages(_productId: string): Promise<ApiResult<any[]>> {
    return { success: true, data: [] }
  },
  async uploadImage(_productId: string, _file: File, _role?: number, _alt?: string): Promise<ApiResult<any>> {
    return { success: true, data: null }
  },
  async deleteImage(_imageId: string): Promise<ApiResult<void>> {
    return { success: true, data: undefined }
  },
  async updateImage(_imageId: string, _data: { alt?: string; role?: number }): Promise<ApiResult<void>> {
    return { success: true, data: undefined }
  },

  // Option type methods
  async updateOptionTypes(_productId: string, _optionTypeIds: string[]): Promise<ApiResult<void>> {
    return { success: true, data: undefined }
  },

  // Property methods — no backend endpoint
  async getProperties(_productId: string): Promise<ApiResult<any[]>> {
    return { success: true, data: [] }
  },
  async updateProperties(_productId: string, _properties: Record<string, unknown>[]): Promise<ApiResult<void>> {
    return { success: true, data: undefined }
  },
}
