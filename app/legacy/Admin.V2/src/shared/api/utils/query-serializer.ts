import apiClient from '@/shared/api/client'
import type { ListQuery } from '@/shared/models'
import type { PagedResult } from '@/shared/models'
import type { FilterGroup } from '@/shared/models/querying'

function flattenFilter(group: FilterGroup, prefix: string): Record<string, string> {
  const params: Record<string, string> = {}
  params[`${prefix}.logic`] = group.logic
  group.conditions.forEach((c, i) => {
    params[`${prefix}.conditions[${i}].field`] = c.field
    params[`${prefix}.conditions[${i}].operator`] = c.operator
    params[`${prefix}.conditions[${i}].value`] = c.value
  })
  group.groups?.forEach((g, i) => {
    Object.assign(params, flattenFilter(g, `${prefix}.groups[${i}]`))
  })
  return params
}

export function toQueryParams(query: ListQuery): Record<string, string | number | boolean | undefined> {
  const raw: Record<string, string | number | boolean | undefined> = {
    'page.page': query.page,
    'page.pageSize': query.pageSize,
    'search.term.value': query.search?.value,
    'search.term.caseSensitive': query.search?.caseSensitive,
    'search.fields': query.search?.fields?.join(','),
    'search.mode': query.search?.mode,
    ...query.sort?.reduce((acc, s, i) => ({
      ...acc,
      [`sort.clauses[${i}].field`]: s.field,
      [`sort.clauses[${i}].direction`]: s.direction,
      ...(s.nulls && { [`sort.clauses[${i}].nulls`]: s.nulls }),
    }), {}),
    ...(query.filters ? flattenFilter(query.filters, 'filter.root') : {}),
  }
  return Object.fromEntries(
    Object.entries(raw).filter(([_, v]) => v !== undefined),
  ) as Record<string, string | number | boolean | undefined>
}

export async function getPagedList<T>(url: string, query: ListQuery): Promise<PagedResult<T>> {
  const res = await apiClient.get<PagedResult<T>>(url, { params: toQueryParams(query) })
  return res.data
}
