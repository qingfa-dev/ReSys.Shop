import { describe, it, expect } from 'vitest'
import { toQueryParams } from '../query-serializer'
import type { ListQuery } from '@/shared/models'
import { defaultListQuery } from '@/shared/models'

describe('defaultListQuery', () => {
  it('returns default values', () => {
    const q = defaultListQuery()
    expect(q.page).toBe(1)
    expect(q.pageSize).toBe(20)
    expect(q.sort).toEqual([{ field: 'createdAt', direction: 'Descending' }])
  })

  it('accepts custom pageSize', () => {
    const q = defaultListQuery(50)
    expect(q.page).toBe(1)
    expect(q.pageSize).toBe(50)
  })

  it('returns a new object each call', () => {
    expect(defaultListQuery()).not.toBe(defaultListQuery())
  })
})

describe('toQueryParams', () => {
  it('serializes page and pageSize', () => {
    const params = toQueryParams({ page: 2, pageSize: 50 })
    expect(params).toMatchObject({ 'page.page': 2, 'page.pageSize': 50 })
  })

  it('serializes default sort', () => {
    const params = toQueryParams({ page: 1, pageSize: 20, sort: [{ field: 'name', direction: 'Ascending' }] })
    expect(params).toMatchObject({ 'sort.clauses[0].field': 'name', 'sort.clauses[0].direction': 'Ascending' })
  })

  it('serializes search term', () => {
    const params = toQueryParams({
      page: 1, pageSize: 20,
      search: { value: 'red', fields: ['name', 'slug'], mode: 'Any' },
    })
    expect(params).toMatchObject({
      'search.term.value': 'red',
      'search.fields': 'name,slug',
      'search.mode': 'Any',
    })
  })

  it('serializes filter group with conditions', () => {
    const query: ListQuery = {
      page: 1, pageSize: 20,
      filters: {
        logic: 'And',
        conditions: [{ field: 'status', operator: 'Equal', value: 'Active' }],
        groups: [],
      },
    }
    const params = toQueryParams(query)
    expect(params).toMatchObject({
      'filter.root.logic': 'And',
      'filter.root.conditions[0].field': 'status',
      'filter.root.conditions[0].operator': 'Equal',
      'filter.root.conditions[0].value': 'Active',
    })
  })

  it('serializes nested filter groups', () => {
    const query: ListQuery = {
      page: 1, pageSize: 20,
      filters: {
        logic: 'Or',
        conditions: [{ field: 'status', operator: 'Equal', value: 'Active' }],
        groups: [{
          logic: 'And',
          conditions: [{ field: 'price', operator: 'GreaterThan', value: '100' }],
          groups: [],
        }],
      },
    }
    const params = toQueryParams(query)
    expect(params).toMatchObject({
      'filter.root.logic': 'Or',
      'filter.root.conditions[0].field': 'status',
      'filter.root.groups[0].logic': 'And',
      'filter.root.groups[0].conditions[0].field': 'price',
    })
  })

  it('omits undefined fields', () => {
    const params = toQueryParams({ page: 1, pageSize: 20 })
    expect(params).not.toHaveProperty('search.term.value')
    expect(params).not.toHaveProperty('sort.clauses')
    expect(params).not.toHaveProperty('filter.root')
  })

  it('serializes sort with nulls', () => {
    const params = toQueryParams({
      page: 1, pageSize: 20,
      sort: [{ field: 'name', direction: 'Ascending', nulls: 'Last' }],
    })
    expect(params).toMatchObject({ 'sort.clauses[0].nulls': 'Last' })
  })

  it('serializes search with caseSensitive', () => {
    const params = toQueryParams({
      page: 1, pageSize: 20,
      search: { value: 'Red', caseSensitive: true },
    })
    expect(params).toMatchObject({ 'search.term.caseSensitive': true })
  })
})
