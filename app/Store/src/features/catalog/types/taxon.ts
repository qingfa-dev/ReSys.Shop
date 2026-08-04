export interface TaxonTreeNode {
  id: string
  name: string
  presentation: string | null
  permalink: string
  depth: number
  hasChildren: boolean
  children: TaxonTreeNode[]
}

export interface StoreTaxonomyTreeResponse {
  id: string
  name: string
  presentation: string | null
  position: number
  nodes: TaxonTreeNode[]
}

export interface StoreTaxonListItemResponse {
  id: string
  name: string
  permalink: string
  depth: number
  taxonCount: number
  parentId: string | null
  taxonomyId: string
  position: number
  slug: string
  imageUrl: string | null
}
