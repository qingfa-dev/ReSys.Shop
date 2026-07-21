import type { OptionTypeListItem, OptionTypeDetail } from '../models/option-type.response'

export function mapToListItem(dto: Record<string, unknown>): OptionTypeListItem {
  return {
    id: String(dto.id ?? ''),
    name: String(dto.name ?? ''),
    presentation: String(dto.presentation ?? ''),
    position: Number(dto.position ?? 0),
    filterable: Boolean(dto.filterable),
    optionValuesCount: Number(dto.optionValuesCount ?? 0),
    productsCount: Number(dto.productsCount ?? 0),
    createdAtUtc: String(dto.createdAtUtc ?? ''),
    modifiedAtUtc: String(dto.modifiedAtUtc ?? ''),
  }
}

export function mapToDetail(dto: Record<string, unknown>): OptionTypeDetail {
  return mapToListItem(dto)
}
