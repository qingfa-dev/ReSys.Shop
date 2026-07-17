import { catalogApi } from '../../services/catalog.api'

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
}
