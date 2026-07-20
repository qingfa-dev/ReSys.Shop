export interface TaxonomyListItem {
  id: string
  name: string
  presentation: string | null
  position: number
  taxonsCount: number
  createdAtUtc: string
  modifiedAtUtc: string
}

export interface TaxonNode {
  id: string
  name: string
  slug: string
  position: number
  child: TaxonNode[]
}

export interface TaxonomyDetail extends TaxonomyListItem {
  root: TaxonNode | null
}
