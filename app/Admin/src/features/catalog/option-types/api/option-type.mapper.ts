import type { OptionTypeListItem, OptionTypeDetail } from '../models/option-type.response'

export function mapToListItem(dto: unknown): OptionTypeListItem {
  const r = dto as Record<string, unknown> ?? {}
  return {
    id: String(r.id ?? ''),
    name: String(r.name ?? ''),
    presentation: String(r.presentation ?? ''),
    position: Number(r.position ?? 0),
    filterable: Boolean(r.filterable),
    optionValuesCount: Number(r.optionValuesCount ?? 0),
    productsCount: Number(r.productsCount ?? 0),
    createdAtUtc: String(r.createdAtUtc ?? ''),
    modifiedAtUtc: String(r.modifiedAtUtc ?? ''),
  }
}

export function mapToDetail(dto: unknown): OptionTypeDetail {
  return mapToListItem(dto)
}
