import { describe, it, expect } from 'vitest'
import { queryingModelToParams, queryingParamsToModel } from '../mappers'
import { emptyQueryingModel } from '../querying'
import { isSuccess } from '../../result'

describe('queryingModelToParams', () => {
  it('returns empty object for empty model', () => {
    const params = queryingModelToParams(emptyQueryingModel)
    expect(params).toEqual({})
  })

  it('serializes filter DSL', () => {
    const model = {
      ...emptyQueryingModel,
      filter: {
        ...emptyQueryingModel.filter,
        isEmpty: false,
        root: {
          logic: 'And' as const,
          conditions: [{ field: 'name', operator: 'Equal' as const, value: 'bolt' }],
          groups: [],
        },
      },
    }
    const params = queryingModelToParams(model)
    expect(params.filter).toBe('name=bolt')
  })

  it('serializes search', () => {
    const model = {
      ...emptyQueryingModel,
      search: {
        ...emptyQueryingModel.search,
        isEmpty: false,
        term: { value: 'bolt', caseSensitive: false },
        fields: ['name'],
        mode: 'Any' as const,
      },
    }
    const params = queryingModelToParams(model)
    expect(params.search).toBe('bolt')
    expect(params.searchFields).toEqual(['name'])
    expect(params.searchMode).toBe('Any')
  })

  it('serializes sort (descending)', () => {
    const model = {
      ...emptyQueryingModel,
      sort: {
        ...emptyQueryingModel.sort,
        isEmpty: false,
        clauses: [{ field: 'name', direction: 'Descending' as const, nulls: null }],
      },
    }
    const params = queryingModelToParams(model)
    expect(params.sort).toEqual(['-name'])
  })

  it('serializes page', () => {
    const model = {
      ...emptyQueryingModel,
      page: {
        ...emptyQueryingModel.page,
        isEmpty: false,
        page: 3,
        pageSize: 50,
      },
    }
    const params = queryingModelToParams(model)
    expect(params.pageNumber).toBe(3)
    expect(params.pageSize).toBe(50)
  })
})

describe('queryingParamsToModel', () => {
  it('returns empty model for empty params', () => {
    const result = queryingParamsToModel({})
    expect(isSuccess(result)).toBe(true)
  })

  it('parses page values', () => {
    const result = queryingParamsToModel({ pageNumber: 3, pageSize: 50 })
    expect(isSuccess(result)).toBe(true)
  })
})
