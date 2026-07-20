import type { VariantParameters } from '../types/variant.field'
export type CreateVariantRequest = VariantParameters & { productId?: string }
export type UpdateVariantRequest = Partial<CreateVariantRequest>
