import { describe, it, expect } from 'vitest'
import { QueryBuilder } from '../query-builder'

describe('QueryBuilder', () => {
  it('builds with filter, sort, search, and page', () => {
    const { params } = new QueryBuilder()
      .filterBy((f) => f.where('name').contains('John'))
      .sortBy((s) => s.orderBy('name').thenByDesc('createdAt'))
      .search('term', ['name', 'email'], 'any')
      .page(1, 20)
      .build()

    expect(params.filter).toBe('name = *John*')
    expect(params.sort).toEqual(['name:asc', 'createdAt:desc'])
    expect(params.search).toBe('term')
    expect(params.searchFields).toEqual(['name', 'email'])
    expect(params.searchMode).toBe('any')
    expect(params.pageNumber).toBe(1)
    expect(params.pageSize).toBe(20)
  })

  it('builds with only page', () => {
    const { params } = new QueryBuilder()
      .page(2, 50)
      .build()

    expect(params.pageNumber).toBe(2)
    expect(params.pageSize).toBe(50)
    expect(params.filter).toBeUndefined()
    expect(params.sort).toBeUndefined()
    expect(params.search).toBeUndefined()
  })

  it('builds with search without fields', () => {
    const { params } = new QueryBuilder()
      .search('hello')
      .page(1, 10)
      .build()

    expect(params.search).toBe('hello')
    expect(params.searchFields).toBeUndefined()
    expect(params.searchMode).toBeUndefined()
  })

  it('builds with sort only', () => {
    const { params } = new QueryBuilder()
      .sortBy((s) => s.orderByDesc('createdAt'))
      .build()

    expect(params.sort).toEqual(['createdAt:desc'])
    expect(params.filter).toBeUndefined()
  })

  it('builds empty when nothing configured', () => {
    const { params } = new QueryBuilder().build()
    expect(params).toEqual({})
  })

  it('toUrl builds correct URL with all params', () => {
    const url = new QueryBuilder()
      .filterBy((f) => f.where('name').eq('John'))
      .sortBy((s) => s.orderBy('name'))
      .search('test')
      .page(1, 20)
      .build()
      .toUrl('/api/users')

    expect(url).toContain('/api/users?')
    expect(url).toContain('filter=name+%3D+John')
    expect(url).toContain('sort=name%3Aasc')
    expect(url).toContain('search=test')
    expect(url).toContain('pageNumber=1')
    expect(url).toContain('pageSize=20')
  })

  it('toUrl builds correct URL without params', () => {
    const url = new QueryBuilder().build().toUrl('/api/users')
    expect(url).toBe('/api/users')
  })
})
