import type { TaxonomyListItem, TaxonomyDetail, TaxonNode } from '../models/taxonomy.response'

export function mapToListItem(dto: Record<string, unknown>): TaxonomyListItem {
  return {
    id: String(dto.id ?? ''),
    name: String(dto.name ?? ''),
    presentation: dto.presentation as string | null ?? null,
    position: Number(dto.position ?? 0),
    taxonsCount: Number(dto.taxonsCount ?? 0),
    createdAtUtc: String(dto.createdAtUtc ?? ''),
    modifiedAtUtc: String(dto.modifiedAtUtc ?? ''),
  }
}

export function mapToDetail(dto: Record<string, unknown>): TaxonomyDetail {
  return {
    ...mapToListItem(dto),
    root: dto.root as TaxonNode | null ?? null,
  }
}
