import type { CreateProductParameters } from '../schemas/create-product.schema'
import type { UpdateProductParameters } from '../schemas/update-product.schema'
import type { ManageClassificationsParameters } from '../classifications/schemas/product-classification.schema'

export type CreateProductRequest = CreateProductParameters
export type UpdateProductRequest = UpdateProductParameters
export type ManageClassificationsRequest = ManageClassificationsParameters
