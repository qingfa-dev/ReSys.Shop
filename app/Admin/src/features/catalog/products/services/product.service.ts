import { catalogApi } from '../../services/catalog.api'
import type { ProductDetail, CreateProductRequest, UpdateProductRequest, ProductSearchParams } from '../types/product.types'

export const productService = {
  list: catalogApi.products.list,
  getById: catalogApi.products.getById,
  create: catalogApi.products.create,
  update: catalogApi.products.update,
  delete: catalogApi.products.delete,
  getOptionTypes: catalogApi.products.getOptionTypes,
  updateOptionTypes: catalogApi.products.updateOptionTypes,
  getProperties: catalogApi.products.getProperties,
  updateProperties: catalogApi.products.updateProperties,
  getImages: catalogApi.products.getImages,
  uploadImage: catalogApi.products.uploadImage,
  updateImage: catalogApi.products.updateImage,
  deleteImage: catalogApi.products.deleteImage,
}
