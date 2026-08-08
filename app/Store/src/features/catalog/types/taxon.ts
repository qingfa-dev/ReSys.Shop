export interface StoreTaxonomyListItem {
  id: string
  name: string
  presentation: string | null
  position: number
}

export interface TaxonBreadcrumbItem {
  id: string
  name: string
  permalink: string
}

export interface StoreTaxonListItemResponse {
  id: string
  name: string
  permalink: string
  depth: number
  slug: string
  presentation: string | null
  taxonomyId: string
  parentId: string | null
  position: number
  imageUrl: string | null
  taxonCount: number | null
  childrenCount: number | null
  prettyName: string
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
