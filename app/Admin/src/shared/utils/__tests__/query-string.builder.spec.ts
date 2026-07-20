import { describe, it, expect } from 'vitest'
import {
  buildFilterParam,
  buildSearchParams,
  buildSortParams,
  buildPageParams,
} from '../query-string.builder'
import {
  createFilterGroup,
  emptyFilterModel,
  FilterOp,
  type FilterModel,
  type FilterCondition,
} from '../../types/filtering.model'
import {
  emptySearchModel,
  type SearchModel,
} from '../../types/searching.model'
import {
  emptySortModel,
  type SortModel,
} from '../../types/sorting.model'
import {
  createPageModel,
  type PageModel,
} from '../../types/pagination.model'

describe('buildFilterParam', () => {
  it('returns empty string for empty model', () => {
    expect(buildFilterParam(emptyFilterModel)).toBe('')
  })

  it('serializes a single condition with AND logic', () => {
    const root = createFilterGroup('and', [
      { field: 'name', op: '=', value: 'test' },
    ])
    const model: FilterModel = {
      root,
      conditions: root.conditions,
      isValid: true,
      violations: [],
      isEmpty: false,
    }
    expect(buildFilterParam(model)).toBe('name=test')
  })

  it('joins multiple conditions with pipe for OR logic', () => {
    const root = createFilterGroup('or', [
      { field: 'a', op: '=', value: '1' },
      { field: 'b', op: '=', value: '2' },
    ])
    const model: FilterModel = {
      root,
      conditions: root.conditions,
      isValid: true,
      violations: [],
      isEmpty: false,
    }
    expect(buildFilterParam(model)).toBe('a=1|b=2')
  })

  it('joins multiple conditions with comma for AND logic', () => {
    const root = createFilterGroup('and', [
      { field: 'a', op: '=', value: '1' },
      { field: 'b', op: '=', value: '2' },
    ])
    const model: FilterModel = {
      root,
      conditions: root.conditions,
      isValid: true,
      violations: [],
      isEmpty: false,
    }
    expect(buildFilterParam(model)).toBe('a=1,b=2')
  })

  it('serializes varied operators correctly', () => {
    const root = createFilterGroup('and', [
      { field: 'field', op: '!=', value: 'val' },
      { field: 'count', op: '>', value: '10' },
      { field: 'name', op: '*', value: 'test' },
    ])
    const model: FilterModel = {
      root,
      conditions: root.conditions,
      isValid: true,
      violations: [],
      isEmpty: false,
    }
    expect(buildFilterParam(model)).toBe('field!=val,count>10,name*test')
  })

  it('serializes a nested group with parenthesized output', () => {
    const root = createFilterGroup('and', [], [
      createFilterGroup('or', [
        { field: 'x', op: '=', value: '1' },
        { field: 'y', op: '=', value: '2' },
      ]),
    ])
    const model: FilterModel = {
      root,
      conditions: root.groups[0]!.conditions,
      isValid: true,
      violations: [],
      isEmpty: false,
    }
    expect(buildFilterParam(model)).toBe('(x=1,y=2)')
  })

  it('serializes deep nesting with parenthesized output', () => {
    const root = createFilterGroup('and', [], [
      createFilterGroup('or', [], [
        createFilterGroup('and', [
          { field: 'x', op: '=', value: '1' },
        ]),
      ]),
    ])
    const model: FilterModel = {
      root,
      conditions: root.groups[0]!.groups[0]!.conditions,
      isValid: true,
      violations: [],
      isEmpty: false,
    }
    expect(buildFilterParam(model)).toBe('((x=1))')
  })

  it('skips empty nested group without producing empty parens', () => {
    const root = createFilterGroup('and', [
      { field: 'c', op: '=', value: '1' },
    ], [
      createFilterGroup('and', [], []),
    ])
    const model: FilterModel = {
      root,
      conditions: root.conditions,
      isValid: true,
      violations: [],
      isEmpty: false,
    }
    expect(buildFilterParam(model)).toBe('c=1')
  })

  it('serializes all 16 filter operators via serializeCondition', () => {
    const conditions: FilterCondition[] = [
      { field: 'feq', op: FilterOp.eq, value: 'v1' },
      { field: 'feqcs', op: FilterOp.eqCs, value: 'v2' },
      { field: 'fneq', op: FilterOp.neq, value: 'v3' },
      { field: 'fgt', op: FilterOp.gt, value: 'v4' },
      { field: 'fgte', op: FilterOp.gte, value: 'v5' },
      { field: 'flt', op: FilterOp.lt, value: 'v6' },
      { field: 'flte', op: FilterOp.lte, value: 'v7' },
      { field: 'fcontains', op: FilterOp.contains, value: 'v8' },
      { field: 'fcontainsCs', op: FilterOp.containsCs, value: 'v9' },
      { field: 'fnotContains', op: FilterOp.notContains, value: 'v10' },
      { field: 'fstarts', op: FilterOp.starts, value: 'v11' },
      { field: 'fstartsCs', op: FilterOp.startsCs, value: 'v12' },
      { field: 'fnotStarts', op: FilterOp.notStarts, value: 'v13' },
      { field: 'fends', op: FilterOp.ends, value: 'v14' },
      { field: 'fendsCs', op: FilterOp.endsCs, value: 'v15' },
      { field: 'fnotEnds', op: FilterOp.notEnds, value: 'v16' },
    ]
    const root = createFilterGroup('and', conditions)
    const model: FilterModel = {
      root,
      conditions: root.conditions,
      isValid: true,
      violations: [],
      isEmpty: false,
    }
    expect(buildFilterParam(model)).toBe(
      'feq=v1,feqcs==v2,fneq!=v3,fgt>v4,fgte>=v5,flt<v6,flte<=v7,' +
      'fcontains*v8,fcontainsCs*~v9,fnotContains!*v10,fstarts^v11,' +
      'fstartsCs^~v12,fnotStarts!^v13,fends$v14,fendsCs$~v15,fnotEnds!$v16',
    )
  })
})

describe('buildSearchParams', () => {
  it('returns empty object for empty model', () => {
    expect(buildSearchParams(emptySearchModel)).toEqual({})
  })

  it('includes search term only', () => {
    const model: SearchModel = {
      term: { value: 'shirt', caseSensitive: false },
      fields: [],
      mode: 'any',
      isValid: true,
      violations: [],
      isEmpty: false,
    }
    expect(buildSearchParams(model)).toEqual({ search: 'shirt', searchMode: 'any' })
  })

  it('includes term and fields', () => {
    const model: SearchModel = {
      term: { value: 'shirt', caseSensitive: false },
      fields: ['name', 'description'],
      mode: 'any',
      isValid: true,
      violations: [],
      isEmpty: false,
    }
    expect(buildSearchParams(model)).toEqual({
      search: 'shirt',
      searchFields: 'name,description',
      searchMode: 'any',
    })
  })

  it('includes term, fields, and search mode', () => {
    const model: SearchModel = {
      term: { value: 'shirt', caseSensitive: false },
      fields: ['name'],
      mode: 'all',
      isValid: true,
      violations: [],
      isEmpty: false,
    }
    expect(buildSearchParams(model)).toEqual({
      search: 'shirt',
      searchFields: 'name',
      searchMode: 'all',
    })
  })

  it('includes caseSensitive flag when enabled', () => {
    const model: SearchModel = {
      term: { value: 'Shirt', caseSensitive: true },
      fields: [],
      mode: 'any',
      isValid: true,
      violations: [],
      isEmpty: false,
    }
    expect(buildSearchParams(model)).toEqual({
      search: 'Shirt',
      searchMode: 'any',
      caseSensitive: 'true',
    })
  })

  it('omits search key when term value is empty', () => {
    const model: SearchModel = {
      term: { value: '', caseSensitive: false },
      fields: ['name'],
      mode: 'any',
      isValid: true,
      violations: [],
      isEmpty: false,
    }
    const result = buildSearchParams(model)
    expect(result).toEqual({ searchFields: 'name', searchMode: 'any' })
    expect('search' in result).toBe(false)
  })
})

describe('buildSortParams', () => {
  it('returns empty object for empty model', () => {
    expect(buildSortParams(emptySortModel)).toEqual({})
  })

  it('serializes a single ascending clause', () => {
    const model: SortModel = {
      clauses: [{ field: 'name', direction: 'asc' }],
      isValid: true,
      violations: [],
      isEmpty: false,
    }
    expect(buildSortParams(model)).toEqual({ sort: '+name' })
  })

  it('serializes a single descending clause', () => {
    const model: SortModel = {
      clauses: [{ field: 'price', direction: 'desc' }],
      isValid: true,
      violations: [],
      isEmpty: false,
    }
    expect(buildSortParams(model)).toEqual({ sort: '-price' })
  })

  it('serializes multiple clauses comma-separated', () => {
    const model: SortModel = {
      clauses: [
        { field: 'name', direction: 'asc' },
        { field: 'date', direction: 'desc' },
      ],
      isValid: true,
      violations: [],
      isEmpty: false,
    }
    expect(buildSortParams(model)).toEqual({ sort: '+name,-date' })
  })
})

describe('buildPageParams', () => {
  it('serializes page 3, size 20', () => {
    const model: PageModel = createPageModel(3, 20)
    expect(buildPageParams(model)).toEqual({ page: '3', pageSize: '20' })
  })

  it('serializes page 1, size 10', () => {
    const model: PageModel = createPageModel(1, 10)
    expect(buildPageParams(model)).toEqual({ page: '1', pageSize: '10' })
  })
})
