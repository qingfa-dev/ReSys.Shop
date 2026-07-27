import { describe, it, expect } from 'vitest'
import {
  parseFilterDsl,
  parseFilterJson,
  parseFilterQueryString,
  parseSortString,
  parseSortJson,
  parseSortQueryString,
  parseSearchText,
  parseSearchJson,
  parseSearchQueryString,
  parsePageValues,
  parsePageJson,
} from './parsers'
import { isSuccess, isFailure } from '../result'

describe('parseFilterDsl', () => {
  it('returns empty model for null/undefined/empty', () => {
    expect(isSuccess(parseFilterDsl(null))).toBe(true)
    expect(isSuccess(parseFilterDsl(undefined))).toBe(true)
    expect(isSuccess(parseFilterDsl(''))).toBe(true)
  })

  it('parses a single condition', () => {
    const result = parseFilterDsl('name=bolt')
    expect(isSuccess(result)).toBe(true)
    if (isSuccess(result)) {
      expect(result.value.root.conditions).toHaveLength(1)
      expect(result.value.root.conditions[0]!.field).toBe('name')
      expect(result.value.root.conditions[0]!.operator).toBe('Equal')
      expect(result.value.root.conditions[0]!.value).toBe('bolt')
    }
  })

  it('parses multiple conditions', () => {
    const result = parseFilterDsl('name=bolt,age>18')
    expect(isSuccess(result)).toBe(true)
    if (isSuccess(result)) {
      expect(result.value.root.conditions).toHaveLength(2)
    }
  })

  it('rejects invalid DSL syntax', () => {
    const result = parseFilterDsl('invalid-segment')
    expect(isFailure(result)).toBe(true)
  })

  it('rejects disallowed fields', () => {
    const result = parseFilterDsl('name=bolt', ['id'])
    expect(isFailure(result)).toBe(true)
    if (isFailure(result)) {
      expect(result.errors[0]!.code).toBe('Filter.Field.Disallowed')
    }
  })
})

describe('parseFilterJson', () => {
  it('returns empty model for null', () => {
    expect(isSuccess(parseFilterJson(null))).toBe(true)
  })

  it('parses valid JSON array', () => {
    const result = parseFilterJson('[{"field":"name","op":"eq","value":"bolt"}]')
    expect(isSuccess(result)).toBe(true)
    if (isSuccess(result)) {
      expect(result.value.root.conditions).toHaveLength(1)
      expect(result.value.root.conditions[0]!.field).toBe('name')
    }
  })

  it('rejects invalid JSON', () => {
    expect(isFailure(parseFilterJson('{bad}'))).toBe(true)
  })

  it('rejects non-array JSON', () => {
    const result = parseFilterJson('{"field":"name"}')
    expect(isFailure(result)).toBe(true)
    if (isFailure(result)) {
      expect(result.errors[0]!.code).toBe('Filter.Json.InvalidStructure')
    }
  })

  it('rejects missing field', () => {
    const result = parseFilterJson('[{"op":"eq","value":"x"}]')
    expect(isFailure(result)).toBe(true)
  })

  it('rejects missing operator', () => {
    const result = parseFilterJson('[{"field":"name","value":"x"}]')
    expect(isFailure(result)).toBe(true)
  })

  it('rejects disallowed fields', () => {
    const result = parseFilterJson('[{"field":"secret","op":"eq","value":"x"}]', ['name'])
    expect(isFailure(result)).toBe(true)
  })
})

describe('parseSortString', () => {
  it('returns empty model for null', () => {
    expect(isSuccess(parseSortString(null))).toBe(true)
  })

  it('parses ascending sort', () => {
    const result = parseSortString('name')
    expect(isSuccess(result)).toBe(true)
    if (isSuccess(result)) {
      expect(result.value.clauses[0]!.field).toBe('name')
      expect(result.value.clauses[0]!.direction).toBe('Ascending')
    }
  })

  it('parses descending sort with - prefix', () => {
    const result = parseSortString('-name')
    expect(isSuccess(result)).toBe(true)
    if (isSuccess(result)) {
      expect(result.value.clauses[0]!.field).toBe('name')
      expect(result.value.clauses[0]!.direction).toBe('Descending')
    }
  })

  it('parses + prefix as ascending', () => {
    const result = parseSortString('+name')
    expect(isSuccess(result)).toBe(true)
    if (isSuccess(result)) {
      expect(result.value.clauses[0]!.direction).toBe('Ascending')
    }
  })

  it('parses colon syntax for direction', () => {
    const result = parseSortString('name:desc')
    expect(isSuccess(result)).toBe(true)
    if (isSuccess(result)) {
      expect(result.value.clauses[0]!.field).toBe('name')
      expect(result.value.clauses[0]!.direction).toBe('Descending')
    }
  })

  it('parses multiple sort clauses', () => {
    const result = parseSortString('name,-age')
    expect(isSuccess(result)).toBe(true)
    if (isSuccess(result)) {
      expect(result.value.clauses).toHaveLength(2)
    }
  })

  it('rejects unknown colon direction', () => {
    const result = parseSortString('name:sideways')
    expect(isFailure(result)).toBe(true)
    if (isFailure(result)) {
      expect(result.errors[0]!.code).toBe('Sorting.Direction.Unknown')
    }
  })

  it('rejects missing field', () => {
    const result = parseSortString('-')
    expect(isFailure(result)).toBe(true)
  })

  it('rejects disallowed fields', () => {
    const result = parseSortString('secret', ['name'])
    expect(isFailure(result)).toBe(true)
  })
})

describe('parseSortJson', () => {
  it('returns empty for null', () => {
    expect(isSuccess(parseSortJson(null))).toBe(true)
  })

  it('parses valid sort JSON', () => {
    const result = parseSortJson('[{"field":"name","direction":"Descending"}]')
    expect(isSuccess(result)).toBe(true)
    if (isSuccess(result)) {
      expect(result.value.clauses[0]!.direction).toBe('Descending')
    }
  })

  it('rejects invalid JSON', () => {
    expect(isFailure(parseSortJson('x'))).toBe(true)
  })

  it('rejects unknown direction', () => {
    const result = parseSortJson('[{"field":"name","direction":"sideways"}]')
    expect(isFailure(result)).toBe(true)
  })

  it('rejects unknown nulls', () => {
    const result = parseSortJson('[{"field":"name","nulls":"maybe"}]')
    expect(isFailure(result)).toBe(true)
  })

  it('rejects missing field', () => {
    const result = parseSortJson('[{"direction":"Ascending"}]')
    expect(isFailure(result)).toBe(true)
  })
})

describe('parseSearchText', () => {
  it('returns empty model for null', () => {
    const result = parseSearchText(null)
    expect(result.isEmpty).toBe(true)
  })

  it('parses plain text', () => {
    const result = parseSearchText('bolt')
    expect(result.term.value).toBe('bolt')
    expect(result.term.caseSensitive).toBe(false)
    expect(result.mode).toBe('Any')
  })

  it('detects case-sensitive suffix', () => {
    const result = parseSearchText('bolt~')
    expect(result.term.value).toBe('bolt')
    expect(result.term.caseSensitive).toBe(true)
  })
})

describe('parseSearchJson', () => {
  it('returns empty for null', () => {
    expect(isSuccess(parseSearchJson(null))).toBe(true)
  })

  it('parses valid search JSON', () => {
    const result = parseSearchJson('{"term":"bolt","fields":["name"],"mode":"All"}')
    expect(isSuccess(result)).toBe(true)
    if (isSuccess(result)) {
      expect(result.value.term.value).toBe('bolt')
      expect(result.value.mode).toBe('All')
    }
  })

  it('rejects invalid JSON', () => {
    expect(isFailure(parseSearchJson('bad'))).toBe(true)
  })

  it('rejects missing term', () => {
    const result = parseSearchJson('{"fields":["name"]}')
    expect(isFailure(result)).toBe(true)
  })

  it('falls back to Any mode for unknown mode', () => {
    const result = parseSearchJson('{"term":"bolt","mode":"unknown"}')
    expect(isSuccess(result)).toBe(true)
    if (isSuccess(result)) {
      expect(result.value.mode).toBe('Any')
    }
  })

  it('parses caseSensitive as boolean string "true"', () => {
    const result = parseSearchJson('{"term":"bolt","caseSensitive":"true"}')
    expect(isSuccess(result)).toBe(true)
    if (isSuccess(result)) {
      expect(result.value.term.caseSensitive).toBe(true)
    }
  })
})

describe('parsePageValues', () => {
  it('returns default page when null', () => {
    const result = parsePageValues(null, null)
    expect(isSuccess(result)).toBe(true)
    if (isSuccess(result)) {
      expect(result.value.page).toBe(1)
      expect(result.value.pageSize).toBe(20)
    }
  })

  it('parses valid values', () => {
    const result = parsePageValues(3, 50)
    expect(isSuccess(result)).toBe(true)
    if (isSuccess(result)) {
      expect(result.value.page).toBe(3)
      expect(result.value.pageSize).toBe(50)
    }
  })

  it('uses default page when only pageSize provided', () => {
    const result = parsePageValues(null, 10)
    expect(isSuccess(result)).toBe(true)
    if (isSuccess(result)) {
      expect(result.value.page).toBe(1)
      expect(result.value.pageSize).toBe(10)
    }
  })

  it('uses default pageSize when only page provided', () => {
    const result = parsePageValues(5, null)
    expect(isSuccess(result)).toBe(true)
    if (isSuccess(result)) {
      expect(result.value.page).toBe(5)
      expect(result.value.pageSize).toBe(20)
    }
  })

  it('clamps page size to max', () => {
    const result = parsePageValues(1, 999, { defaultPage: 1, defaultPageSize: 20, maxPageSize: 100 })
    expect(isSuccess(result)).toBe(true)
    if (isSuccess(result)) {
      expect(result.value.pageSize).toBe(100)
    }
  })

  it('rejects non-integer page', () => {
    expect(isFailure(parsePageValues(1.5, 20))).toBe(true)
  })

  it('rejects negative page', () => {
    expect(isFailure(parsePageValues(-1, 20))).toBe(true)
  })

  it('rejects non-integer pageSize', () => {
    expect(isFailure(parsePageValues(1, 1.5))).toBe(true)
  })
})

describe('parsePageJson', () => {
  it('returns default for null', () => {
    const result = parsePageJson(null)
    expect(isSuccess(result)).toBe(true)
  })

  it('parses valid page JSON', () => {
    const result = parsePageJson('{"page":3,"pageSize":50}')
    expect(isSuccess(result)).toBe(true)
    if (isSuccess(result)) {
      expect(result.value.page).toBe(3)
    }
  })

  it('rejects invalid JSON', () => {
    expect(isFailure(parsePageJson('bad'))).toBe(true)
  })
})

describe('parseFilterQueryString', () => {
  it('returns empty model for null/undefined/empty', () => {
    expect(isSuccess(parseFilterQueryString(null))).toBe(true)
    expect(isSuccess(parseFilterQueryString(undefined))).toBe(true)
    expect(isSuccess(parseFilterQueryString([]))).toBe(true)
    expect(isSuccess(parseFilterQueryString(['']))).toBe(true)
  })

  it('parses triplet format name:eq:bolt', () => {
    const result = parseFilterQueryString(['name:eq:bolt'])
    expect(isSuccess(result)).toBe(true)
    if (isSuccess(result)) {
      expect(result.value.root.conditions).toHaveLength(1)
      expect(result.value.root.conditions[0]!.field).toBe('name')
      expect(result.value.root.conditions[0]!.value).toBe('bolt')
    }
  })

  it('rejects triplet with unknown operator', () => {
    const result = parseFilterQueryString(['name:xyz:bolt'])
    expect(isFailure(result)).toBe(true)
  })

  it('rejects missing field in triplet', () => {
    const result = parseFilterQueryString([':eq:bolt'])
    expect(isFailure(result)).toBe(true)
  })

  it('applies whitelist rejection', () => {
    const result = parseFilterQueryString(['name:eq:bolt'], ['id'])
    expect(isFailure(result)).toBe(true)
  })
})

describe('parseSortQueryString', () => {
  it('returns empty model for null/undefined/empty array', () => {
    expect(isSuccess(parseSortQueryString(null))).toBe(true)
    expect(isSuccess(parseSortQueryString(undefined))).toBe(true)
    expect(isSuccess(parseSortQueryString([]))).toBe(true)
    expect(isSuccess(parseSortQueryString(['']))).toBe(true)
  })

  it('parses multiple sort entries', () => {
    const result = parseSortQueryString(['name', '-age'])
    expect(isSuccess(result)).toBe(true)
    if (isSuccess(result)) {
      expect(result.value.clauses).toHaveLength(2)
      expect(result.value.clauses[0]!.field).toBe('name')
      expect(result.value.clauses[1]!.field).toBe('age')
      expect(result.value.clauses[1]!.direction).toBe('Descending')
    }
  })

  it('rejects empty field after prefix', () => {
    const result = parseSortQueryString(['-'])
    expect(isFailure(result)).toBe(true)
  })

  it('applies whitelist rejection', () => {
    const result = parseSortQueryString(['name', 'secret'], ['name'])
    expect(isFailure(result)).toBe(true)
  })
})

describe('parseSearchQueryString', () => {
  it('returns empty model for null/undefined/empty', () => {
    expect(isSuccess(parseSearchQueryString(null))).toBe(true)
    expect(isSuccess(parseSearchQueryString(undefined))).toBe(true)
    expect(isSuccess(parseSearchQueryString(''))).toBe(true)
  })

  it('parses plain text search', () => {
    const result = parseSearchQueryString('bolt')
    expect(isSuccess(result)).toBe(true)
    if (isSuccess(result)) {
      expect(result.value.term.value).toBe('bolt')
      expect(result.value.mode).toBe('Any')
    }
  })

  it('parses with fields and mode', () => {
    const result = parseSearchQueryString('bolt', 'name,title', 'All', 'true')
    expect(isSuccess(result)).toBe(true)
    if (isSuccess(result)) {
      expect(result.value.fields).toEqual(['name', 'title'])
      expect(result.value.mode).toBe('All')
      expect(result.value.term.caseSensitive).toBe(true)
    }
  })
})
