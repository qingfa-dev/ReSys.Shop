export interface StoreTaxonomyListItem {
  id: string
  name: string
  presentation: string | null
  position: number
  taxonsCount: number
}

export interface TaxonBreadcrumbItem {
  id: string
  name: string
  permalink: string
}

export interface StoreTaxonListItemResponse {
  id: string
  name: string
  presentation: string | null
  description: string | null
  position: number
  parentId: string | null
  taxonomyId: string
  parentName: string | null
  taxonomyName: string | null
  depth: number
  taxonRuleCount: number | null
  productCount: number | null
  childrenCount: number | null
  permalink: string
  prettyName: string
  slug: string
  imageUrl: string | null
  breadcrumb: TaxonBreadcrumbItem[]
}

export interface TaxonTreeNode {
  id: string
  name: string
  presentation: string | null
  permalink: string
  depth: number
  hasChildren: boolean
  children: TaxonTreeNode[]
}
