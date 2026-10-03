import type { ProductParameters } from './product.parameters'

export type CreateProductRequest = ProductParameters
export type UpdateProductRequest = Partial<ProductParameters> & { status?: number }
