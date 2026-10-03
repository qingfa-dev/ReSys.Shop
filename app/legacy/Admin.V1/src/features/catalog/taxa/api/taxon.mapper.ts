import type { TaxonListItem, TaxonTreeItem, TaxonDetail } from '../models/taxon.response'
import type { TaxonRuleListItem } from '../models/taxon-rule.response'

export const TaxonMapper = {
  toListItem(dto: Record<string, unknown>): TaxonListItem {
    return {
      id: String(dto.id ?? ''),
      taxonomyId: String(dto.taxonomyId ?? ''),
      parentId: dto.parentId as string | undefined,
      name: String(dto.name ?? ''),
      presentation: String(dto.presentation ?? ''),
      description: dto.description as string | undefined,
      slug: String(dto.slug ?? ''),
      permalink: String(dto.permalink ?? ''),
      prettyName: String(dto.prettyName ?? ''),
      position: Number(dto.position ?? 0),
      hideFromNav: Boolean(dto.hideFromNav),
      depth: Number(dto.depth ?? 0),
      productCount: Number(dto.productCount ?? 0),
      childrenCount: Number(dto.childrenCount ?? 0),
      lft: Number(dto.lft ?? 0),
      rgt: Number(dto.rgt ?? 0),
      hasChildren: Boolean(dto.hasChildren),
      automatic: Boolean(dto.automatic),
      createdAtUtc: String(dto.createdAtUtc ?? ''),
      modifiedAtUtc: String(dto.modifiedAtUtc ?? ''),
    }
  },

  toTreeItem(dto: Record<string, unknown>): TaxonTreeItem {
    return {
      ...this.toListItem(dto),
      key: String(dto.key ?? dto.id ?? ''),
      isExpanded: dto.isExpanded as boolean | undefined,
      children: ((dto.children as Record<string, unknown>[]) ?? []).map((c) => this.toTreeItem(c)),
    }
  },

  toDetail(dto: Record<string, unknown>): TaxonDetail {
    return {
      ...this.toListItem(dto),
      rulesMatchPolicy: String(dto.rulesMatchPolicy ?? ''),
      sortOrder: String(dto.sortOrder ?? ''),
      metaTitle: dto.metaTitle as string | undefined,
      metaDescription: dto.metaDescription as string | undefined,
      metaKeywords: dto.metaKeywords as string | undefined,
      taxonRuleCount: Number(dto.taxonRuleCount ?? 0),
      rules: (dto.rules as Record<string, unknown>[])?.map((r) => this.toRuleListItem(r)),
    }
  },

  toRuleListItem(dto: Record<string, unknown>): TaxonRuleListItem {
    return {
      id: String(dto.id ?? ''),
      taxonId: String(dto.taxonId ?? ''),
      type: String(dto.type ?? ''),
      value: String(dto.value ?? ''),
      matchPolicy: String(dto.matchPolicy ?? ''),
    }
  },
}
