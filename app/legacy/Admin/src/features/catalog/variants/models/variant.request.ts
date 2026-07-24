import type { VariantParameters } from './variant.parameters'

export type CreateVariantRequest = VariantParameters & { productId?: string; optionValueIds?: string[] }
export type UpdateVariantRequest = Partial<CreateVariantRequest>
