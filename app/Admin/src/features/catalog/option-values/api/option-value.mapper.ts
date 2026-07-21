import type { OptionValueListItem } from '../models/option-value.response'

export function mapToListItem(dto: unknown): OptionValueListItem {
  const r = dto as Record<string, unknown> ?? {}
  return {
    id: String(r.id ?? ''),
    optionTypeId: String(r.optionTypeId ?? ''),
    name: String(r.name ?? ''),
    presentation: String(r.presentation ?? ''),
    position: Number(r.position ?? 0),
  }
}
