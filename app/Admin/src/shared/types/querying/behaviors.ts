import type { FilterGroup, FilterCondition, FilterModel } from './filter'
import type { SortClause, SortModel } from './sort'
import type { SearchModel } from './search'
import type { PageModel, PageBounds } from './page'
import { defaultPageBounds } from './page'

export function flatAnd(conditions: FilterCondition[]): FilterGroup {
  return { logic: 'And', conditions, groups: [] }
}

export function flatOr(conditions: FilterCondition[]): FilterGroup {
  return { logic: 'Or', conditions, groups: [] }
}

export function toStructuralKey(group: FilterGroup): string {
  const conds = group.conditions
    .map(c => `${c.field}:${c.operator}:${c.value}`)
    .sort()
    .join(',')
  const subs = group.groups.map(toStructuralKey).sort().join('|')
  return `[${group.logic}][${conds}][${subs}]`
}

export function toDslString(model: FilterModel): string {
  const render = (group: FilterGroup): string => {
    const parts: string[] = []
    for (const c of group.conditions) {
      const val = c.value.includes(',') || c.value.includes('"') ? `"${c.value}"` : c.value
      parts.push(`${c.field}=${val}`)
    }
    for (const sub of group.groups) {
      parts.push(`(${render(sub)})`)
    }
    return parts.join(',')
  }
  return render(model.root)
}

export function conditionsFor(model: FilterModel, field: string): FilterCondition[] {
  const result: FilterCondition[] = []
  const visit = (group: FilterGroup): void => {
    result.push(...group.conditions.filter(c => c.field === field))
    for (const sub of group.groups) visit(sub)
  }
  visit(model.root)
  return result
}

export function hasField(model: FilterModel, field: string): boolean {
  const visit = (group: FilterGroup): boolean =>
    group.conditions.some(c => c.field === field) || group.groups.some(visit)
  return visit(model.root)
}

export function resolveSortClauses(model: SortModel, defaults: SortClause[]): SortClause[] {
  return model.clauses.length > 0 ? model.clauses : defaults
}

export function hasSortField(model: SortModel, field: string): boolean {
  return model.clauses.some(c => c.field === field)
}

export function clauseFor(model: SortModel, field: string): SortClause | undefined {
  return model.clauses.find(c => c.field === field)
}

export function resolveSearchFields(model: SearchModel, defaults: string[]): string[] {
  return model.fields.length > 0 ? model.fields : defaults
}

export function hasSearchField(model: SearchModel, field: string): boolean {
  return model.fields.includes(field)
}

export function totalPages(model: PageModel, totalCount: number): number {
  if (model.pageSize <= 0) return 0
  return Math.ceil(totalCount / model.pageSize)
}

export function hasNextPage(model: PageModel, totalCount: number): boolean {
  return model.page < totalPages(model, totalCount)
}

export function hasPreviousPage(model: PageModel): boolean {
  return model.page > 1
}

export function normalizePage(page: number | null | undefined, bounds: PageBounds = defaultPageBounds): number {
  if (page === null || page === undefined || !Number.isInteger(page) || page < 1) return bounds.defaultPage
  return page
}

export function normalizePageSize(size: number | null | undefined, bounds: PageBounds = defaultPageBounds): number {
  if (size === null || size === undefined || !Number.isInteger(size) || size < 1) return bounds.defaultPageSize
  return Math.min(size, bounds.maxPageSize)
}
