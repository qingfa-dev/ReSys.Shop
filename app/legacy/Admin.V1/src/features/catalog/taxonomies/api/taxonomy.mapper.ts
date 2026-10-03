import type { TaxonomyListItem, TaxonomyDetail, TaxonNode } from '../models/taxonomy.response'

export const TaxonomyMapper = {
  toListItem(dto: Record<string, unknown>): TaxonomyListItem {
    return {
      id: String(dto.id ?? ''),
      name: String(dto.name ?? ''),
      presentation: dto.presentation as string | null ?? null,
      position: Number(dto.position ?? 0),
      taxonsCount: Number(dto.taxonsCount ?? 0),
      createdAtUtc: String(dto.createdAtUtc ?? ''),
      modifiedAtUtc: String(dto.modifiedAtUtc ?? ''),
    }
  },

  toDetail(dto: Record<string, unknown>): TaxonomyDetail {
    return {
      ...this.toListItem(dto),
      root: dto.root as TaxonNode | null ?? null,
    }
  },
}
