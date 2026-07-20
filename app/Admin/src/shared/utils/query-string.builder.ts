import type { FilterModel, FilterGroup, FilterCondition, FilterLogic } from '../types/filtering.model'
import type { SortModel, SortClause } from '../types/sorting.model'
import type { SearchModel } from '../types/searching.model'
import type { PageModel } from '../types/pagination.model'

function serializeCondition(c: FilterCondition): string {
  return `${c.field}${c.op}${c.value}`
}

function serializeGroup(group: FilterGroup, separator: string): string {
  const parts: string[] = []

  for (const cond of group.conditions) {
    parts.push(serializeCondition(cond))
  }
  for (const sub of group.groups) {
    const inner = serializeGroup(sub, ',')
    if (inner) {
      parts.push(`(${inner})`)
    }
  }

  return parts.join(separator)
}

export function buildFilterParam(model: FilterModel): string {
  if (model.isEmpty) return ''
  const logic: FilterLogic = model.root.logic
  return serializeGroup(
    model.root,
    logic === 'or' ? '|' : ',',
  )
}

export function buildSearchParams(model: SearchModel): Record<string, string> {
  if (model.isEmpty) return {}
  const params: Record<string, string> = {}
  if (model.term.value) {
    params.search = model.term.value
  }
  if (model.fields.length > 0) {
    params.searchFields = model.fields.join(',')
  }
  if (model.mode) {
    params.searchMode = model.mode
  }
  if (model.term.caseSensitive) {
    params.caseSensitive = 'true'
  }
  return params
}

export function buildSortParams(model: SortModel): Record<string, string> {
  if (model.isEmpty) return {}
  const segments = model.clauses.map(serializeSortClause)
  return { sort: segments.join(',') }
}

function serializeSortClause(clause: SortClause): string {
  const prefix = clause.direction === 'desc' ? '-' : '+'
  return `${prefix}${clause.field}`
}

export function buildPageParams(model: PageModel): Record<string, string> {
  return {
    page: String(model.page),
    pageSize: String(model.pageSize),
  }
}
