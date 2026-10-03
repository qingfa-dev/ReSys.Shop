export interface TaxonTreeNode {
  id: string
  name: string
  presentation: string | null
  permalink: string
  depth: number
  hasChildren: boolean
  children: TaxonTreeNode[]
}

export interface TaxonomyGroup {
  taxonomy: { id: string; name: string; presentation: string | null }
  tree: TaxonTreeNode[]
}
