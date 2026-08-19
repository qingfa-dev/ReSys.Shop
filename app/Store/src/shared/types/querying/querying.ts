import type { FilterModel } from './filter'
import { emptyFilterModel } from './filter'
import type { SearchModel } from './search'
import { emptySearchModel } from './search'
import type { SortModel } from './sort'
import { emptySortModel } from './sort'
import type { PageModel, PageBounds } from './page'
import { emptyPageModel, defaultPageBounds } from './page'
import type { Result } from '../result'
import { ok, validation } from '../result'
import { parseFilterDsl } from './parsers'
import { parseSortString } from './parsers'
import { parseSearchQueryString } from './parsers'
import { parsePageValues } from './parsers'
import type { ApiError } from '../error'

// Boundary: QueryingParameters — raw API input; QueryingModel — parsed internal model
export interface QueryingParameters {
  filter?: string | null
  search?: string | null
  searchFields?: string[] | null
  searchMode?: string | null
  sort?: string[] | null
  pageNumber?: number | null
  pageSize?: number | null
  // Dedicated: Storefront filter params — mirror GetStorefrontProducts.Parameters
  taxonId?: string[] | null
  optionValueId?: string[] | null
  minPrice?: number | null
  maxPrice?: number | null
  // Dedicated: Product context params — used by related/similar product endpoints
  productId?: string | null
  topK?: number | null
}

export interface QueryingModel {
  filter: FilterModel
  search: SearchModel
  sort: SortModel
  page: PageModel
}

export const emptyQueryingModel: QueryingModel = {
  filter: emptyFilterModel,
  search: emptySearchModel,
  sort: emptySortModel,
  page: emptyPageModel,
}

// Validate: Parse all query parameters — collects errors across filter/sort/search/page
export function parseAll(
  params: QueryingParameters,
  allowedFilterFields?: string[] | null,
  allowedSortFields?: string[] | null,
  allowedSearchFields?: string[] | null,
  pageBounds: PageBounds = defaultPageBounds,
): Result<QueryingModel> {
  const errors: ApiError[] = []

  const filterResult = parseFilterDsl(params.filter, allowedFilterFields ?? null)
  if (!filterResult.isSuccess) errors.push(...filterResult.errors)

  const sortResult = parseSortString(
    params.sort?.join(',') ?? null,
    allowedSortFields ?? null,
  )
  if (!sortResult.isSuccess) errors.push(...sortResult.errors)

  const pageResult = parsePageValues(params.pageNumber, params.pageSize, pageBounds)
  if (!pageResult.isSuccess) errors.push(...pageResult.errors)

  const searchResult = parseSearchQueryString(
    params.search,
    params.searchFields?.join(',') ?? null,
    params.searchMode ?? null,
    null,
    allowedSearchFields ?? null,
  )
  if (!searchResult.isSuccess) errors.push(...searchResult.errors)

  // Guard: Return all validation errors at once — caller decides retry strategy
  if (errors.length > 0) return validation(errors)

  return ok({
    filter: filterResult.value,
    search: searchResult.value,
    sort: sortResult.value,
    page: pageResult.value,
  })
}
