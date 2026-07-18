import type { CreateProductParameters } from '../schemas/CreateProduct.Schema'
import type { UpdateProductParameters } from '../schemas/UpdateProduct.Schema'
import type { ManageClassificationsParameters } from '../classifications/schemas/ProductClassification.Schema'

export type CreateProductRequest = CreateProductParameters
export type UpdateProductRequest = UpdateProductParameters
export type ManageClassificationsRequest = ManageClassificationsParameters
