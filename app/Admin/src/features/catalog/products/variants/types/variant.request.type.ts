import type { VariantParameters } from '../schemas/variant.schema'
export type CreateVariantRequest = VariantParameters & { productId?: string }
export type UpdateVariantRequest = Partial<CreateVariantRequest>
