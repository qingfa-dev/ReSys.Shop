export interface TaxonResponse {
  id: string
  name: string
  presentation: string | null
  description: string | null
  slug: string
  position: number
  depth: number
  lft: number
  rgt: number
  childrenCount: number
  hideFromNav: boolean
  automatic: boolean
  taxonomyId: string
  parentId: string | null
  parentName?: string
  taxonomyName?: string
  taxonRuleCount?: number
  productCount?: number
  permalink: string
  prettyName: string
  metaTitle?: string
  metaDescription?: string
  metaKeywords?: string
  imageUrl?: string
  squareImageUrl?: string
  rulesMatchPolicy: string
  sortOrder: string
  createdAt: string
  updatedAt: string
}

export interface TaxonTreeItem extends TaxonResponse {
  isExpanded: boolean
  isInActivePath: boolean
  children: TaxonTreeItem[]
}

export interface TaxonTreeResponse {
  tree: TaxonTreeItem[]
  breadcrumbs: TaxonTreeItem[]
  focusedNode?: TaxonTreeItem
  focusedSubtree?: TaxonTreeItem
}
