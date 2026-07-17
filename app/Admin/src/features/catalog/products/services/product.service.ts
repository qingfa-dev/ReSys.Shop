import { productRepository } from '../repositories/product.repository'
import type { ServerResult, ServerPagedResult } from '@/shared/api/types/result.types'
import type { ProductImage } from '../types/Product.Response.Type'

export const productService = {
  list: productRepository.list,
  getById: productRepository.getById,
  create: productRepository.create,
  update: productRepository.update,
  delete: productRepository.delete,
  activate: productRepository.activate,
  discontinue: productRepository.discontinue,
  getOptionTypes: productRepository.getOptionTypes,
  syncOptionTypes: productRepository.syncOptionTypes,
  getClassifications: productRepository.getClassifications,
  syncClassifications: productRepository.syncClassifications,

  // Image methods — currently at variant level in API; stubs for future product-level endpoints
  async getImages(_productId: string): Promise<ServerPagedResult<ProductImage>> {
    return { isSuccess: true, statusCode: 200, errors: [], message: null, metadata: null, items: [], page: 1, pageSize: 0, totalCount: 0 }
  },
  async uploadImage(_productId: string, _file: File, _role?: number, _alt?: string): Promise<ServerResult<ProductImage>> {
    return { isSuccess: true, statusCode: 200, errors: [], message: null, metadata: null, value: null as unknown as ProductImage }
  },
  async deleteImage(_imageId: string): Promise<ServerResult<void>> {
    return { isSuccess: true, statusCode: 200, errors: [], message: null, metadata: null, value: undefined }
  },
  async updateImage(_imageId: string, _data: { alt?: string; role?: number }): Promise<ServerResult<void>> {
    return { isSuccess: true, statusCode: 200, errors: [], message: null, metadata: null, value: undefined }
  },

  // Option type methods
  async updateOptionTypes(_productId: string, _optionTypeIds: string[]): Promise<ServerResult<void>> {
    return { isSuccess: true, statusCode: 200, errors: [], message: null, metadata: null, value: undefined }
  },
}
