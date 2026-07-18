import type { VariantParameters } from '../../schemas/Variant.Schema'
export type CreateVariantRequest = VariantParameters & { productId?: string }
export type UpdateVariantRequest = Partial<CreateVariantRequest>
