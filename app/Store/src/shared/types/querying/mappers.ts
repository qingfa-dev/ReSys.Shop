import type { QueryingParameters, QueryingModel } from './querying'
import { emptyQueryingModel, parseAll } from './querying'
import type { PageBounds } from './page'
import { toDslString } from './behaviors'
import type { Result } from '../result'
import { ok } from '../result'

export function queryingModelToParams(model: QueryingModel): QueryingParameters {
  const params: QueryingParameters = {}

  if (!model.filter.isEmpty) {
    params.filter = toDslString(model.filter)
  }

  if (!model.search.isEmpty) {
    params.search = model.search.term.value
    params.searchFields = model.search.fields.length > 0 ? model.search.fields : undefined
    params.searchMode = model.search.mode
  }

  if (!model.sort.isEmpty) {
    params.sort = model.sort.clauses.map(c =>
      c.direction === 'Descending' ? `-${c.field}` : c.field,
    )
  }

  if (!model.page.isEmpty) {
    params.pageNumber = model.page.page
    params.pageSize = model.page.pageSize
  }

  return params
}

export function queryingParamsToModel(
  params: QueryingParameters,
  allowedFilterFields?: string[] | null,
  allowedSortFields?: string[] | null,
  allowedSearchFields?: string[] | null,
  pageBounds?: PageBounds,
): Result<QueryingModel> {
  if (!params.filter && !params.search && !params.sort && !params.pageNumber && !params.pageSize) {
    return ok(emptyQueryingModel)
  }
  return parseAll(params, allowedFilterFields, allowedSortFields, allowedSearchFields, pageBounds)
}
