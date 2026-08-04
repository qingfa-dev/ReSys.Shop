import type { QueryingParameters } from '@/shared/types/querying'

export interface TaxonRequest {
  taxonomyId: string
  parentId: string | null
  name: string
  presentation: string
  description: string | null
  slug: string
  position: number
  metaTitle: string | null
  metaDescription: string | null
  metaKeywords: string | null
  imageUrl: string | null
  squareImageUrl: string | null
  automatic: boolean
  rulesMatchPolicy: 'All' | 'Any'
  sortOrder: string
  hideFromNav: boolean
}

export interface TaxonListItem extends TaxonRequest {
  id: string
  parentName: string | null
  taxonomyName: string | null
  lft: number
  rgt: number
  depth: number
  childrenCount: number
  taxonRuleCount: number
  productCount: number
  permalink: string
  prettyName: string
  createdAtUtc: string
  modifiedAtUtc: string | null
}

export type TaxonDetail = TaxonListItem

export interface TaxonQuery {
  taxonomyId?: string
  name?: string
  filter?: string
  search?: string
  searchFields?: string[]
  searchMode?: string
  sortBy?: 'name' | 'slug' | 'position' | 'depth' | 'createdAtUtc' | 'modifiedAtUtc'
  sortDirection?: 'asc' | 'desc'
  page?: number
  pageSize?: number
}

export const TAXON_FILTER_FIELDS = [
  'taxonomyId',
  'name',
  'slug',
  'depth',
  'createdAtUtc',
  'modifiedAtUtc',
]

export const TAXON_SORT_FIELDS = [
  'name',
  'slug',
  'position',
  'depth',
  'createdAtUtc',
  'modifiedAtUtc',
]

export const TAXON_SORT_ORDERS = [
  'Manual',
  'BestSelling',
  'AlphabeticallyAZ',
  'AlphabeticallyZA',
  'PriceHigh2Low',
  'PriceLow2High',
  'Newest',
  'Oldest',
]

export const TAXON_MATCH_POLICIES = ['All', 'Any']

export function toTaxonQueryParams(query: TaxonQuery): QueryingParameters {
  const filters: string[] = []

  if (query.filter !== undefined && query.filter !== '') {
    filters.push(query.filter)
  }
  if (query.taxonomyId !== undefined && query.taxonomyId !== '') {
    filters.push(`taxonomyId=${query.taxonomyId}`)
  }
  if (query.name !== undefined && query.name !== '') {
    filters.push(`name*=${query.name}`)
  }

  let sort: string[] | null = null
  if (query.sortBy) {
    const dir = query.sortDirection === 'desc' ? '-' : ''
    sort = [`${dir}${query.sortBy}`]
  }

  return {
    filter: filters.length > 0 ? filters.join(',') : null,
    search: query.search ?? null,
    searchFields: query.searchFields && query.searchFields.length > 0 ? query.searchFields : null,
    searchMode: query.searchMode ?? null,
    sort,
    pageNumber: query.page ?? null,
    pageSize: query.pageSize ?? null,
  }
}
