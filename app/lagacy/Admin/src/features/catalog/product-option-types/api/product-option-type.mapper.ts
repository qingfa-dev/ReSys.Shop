import type { ProductOptionTypeItem } from '../models/product-option-type.response'

export const ProductOptionTypeMapper = {
  toOptionTypeDetail(dto: Record<string, unknown>): ProductOptionTypeItem {
    return {
      id: String(dto.optionTypeId ?? ''),
      name: String(dto.name ?? ''),
      presentation: (dto.presentation as string) ?? null,
      position: Number(dto.position ?? 0),
      isAssigned: Boolean(dto.isAssigned),
      optionTypeId: String(dto.optionTypeId ?? ''),
    }
  },
}
