import type { TaxonRuleListItem } from './taxon-rule.response'

export interface TaxonListItem {
  id: string; taxonomyId: string; parentId?: string; name: string; presentation: string
  description?: string; slug: string; permalink: string; prettyName: string
  position: number; hideFromNav: boolean; depth: number; productCount: number
  childrenCount: number; lft: number; rgt: number; hasChildren: boolean
  automatic: boolean; createdAtUtc: string; modifiedAtUtc: string
}

export interface TaxonTreeItem extends TaxonListItem {
  key: string; isExpanded?: boolean; children: TaxonTreeItem[]
}

export interface TaxonDetail extends TaxonListItem {
  rulesMatchPolicy: string; sortOrder: string; metaTitle?: string
  metaDescription?: string; metaKeywords?: string; taxonRuleCount: number
  rules?: TaxonRuleListItem[]
}
