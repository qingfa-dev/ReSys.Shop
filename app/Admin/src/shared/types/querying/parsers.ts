import type { Result } from '../result'
import { ok, validation } from '../result'
import type { FilterModel, FilterGroup, FilterCondition } from './filter'
import { emptyFilterModel, emptyFilterGroup } from './filter'
import type { SearchModel } from './search'
import { emptySearchModel } from './search'
import type { SortModel, SortClause } from './sort'
import { emptySortModel } from './sort'
import type { PageModel, PageBounds } from './page'
import { emptyPageModel, defaultPageBounds } from './page'
import type { FilterOperator } from './enums'
import { FilterLogic, SearchMode, SortDirection } from './enums'
import { tryParseOperator } from './constants'
import { FilterErrors, SortErrors, SearchErrors, PageErrors } from './error-codes'
import type { ApiError } from '../error'

function isAllowed(field: string, allowed: string[] | null): boolean {
  return !allowed || allowed.includes(field)
}

export function parseFilterDsl(
  dsl: string | null | undefined,
  allowedFields: string[] | null = null,
): Result<FilterModel> {
  if (!dsl || dsl.trim() === '') return ok(emptyFilterModel)

  const errors: ApiError[] = []
  const conditions: FilterCondition[] = []

  const segments = dsl.split(',').map(s => s.trim()).filter(Boolean)
  for (const segment of segments) {
    const match = segment.match(/^(\w[\w.]*)\s*(!=|>=|<=|==|>|<|\*~|!\*|\^~|!\^|\$~|!\$|\*|\^|\$|=|!)\s*(.+)$/)
    if (!match) {
      errors.push(FilterErrors.invalidSyntax(segment))
      continue
    }

    const field = match[1]!
    const opToken = match[2]!
    const value = (match[3] ?? '').replace(/^"|"$/g, '')

    const operator = tryParseOperator(opToken)
    if (!operator) {
      errors.push(FilterErrors.unknownOperator(opToken))
      continue
    }

    if (!isAllowed(field, allowedFields!)) {
      errors.push(FilterErrors.disallowedField(field))
      continue
    }

    conditions.push({
      field,
      operator: operator as FilterOperator,
      value,
    })
  }

  if (errors.length > 0) return validation(errors)

  const filterGroup: FilterGroup = { logic: 'And', conditions, groups: [] }
  return ok({
    ...emptyFilterModel,
    root: filterGroup,
    isEmpty: conditions.length === 0,
    rawInput: dsl,
  })
}

export function parseFilterJson(
  json: string | null | undefined,
  allowedFields: string[] | null = null,
): Result<FilterModel> {
  if (!json || json.trim() === '') return ok(emptyFilterModel)

  let parsed: unknown
  try {
    parsed = JSON.parse(json)
  } catch {
    return validation([FilterErrors.invalidJson('invalid JSON')])
  }

  if (!Array.isArray(parsed)) {
    return validation([FilterErrors.invalidJson('expected an array')])
  }

  const errors: ApiError[] = []
  const conditions: FilterCondition[] = []

  for (const item of parsed) {
    if (!item || typeof item !== 'object') {
      errors.push(FilterErrors.invalidJson('each item must be an object'))
      continue
    }
    const entry = item as Record<string, unknown>
    const field = typeof entry.field === 'string' ? entry.field : ''
    const op = typeof entry.op === 'string' ? entry.op : ''
    const value = typeof entry.value === 'string' ? entry.value : ''

    if (!field) { errors.push(FilterErrors.missingField()); continue }
    if (!op) { errors.push(FilterErrors.missingOperator()); continue }

    const operator = tryParseOperator(op)
    if (!operator) { errors.push(FilterErrors.unknownOperator(op)); continue }
    if (!isAllowed(field, allowedFields)) { errors.push(FilterErrors.disallowedField(field)); continue }

    conditions.push({ field, operator: operator as FilterOperator, value })
  }

  if (errors.length > 0) return validation(errors)

  const filterGroup: FilterGroup = { logic: 'And', conditions, groups: [] }
  return ok({
    ...emptyFilterModel,
    root: filterGroup,
    isEmpty: conditions.length === 0,
    rawInput: json,
  })
}

export function parseFilterQueryString(
  values: (string | null | undefined)[] | null | undefined,
  allowedFields: string[] | null = null,
): Result<FilterModel> {
  if (!values || values.length === 0) return ok(emptyFilterModel)

  const errors: ApiError[] = []
  const conditions: FilterCondition[] = []

  const entries = values
    .map(v => (v ?? '').trim())
    .filter(Boolean)

  if (entries.length === 0) return ok(emptyFilterModel)

  for (const entry of entries) {
    const parts = entry.split(':')
    if (parts.length < 2) {
      errors.push(FilterErrors.invalidTriplet(entry))
      continue
    }

    const field = (parts[0] ?? '').trim()
    const opToken = (parts[1] ?? '').trim().toLowerCase()
    const value = parts.slice(2).join(':').trim()

    if (!field) { errors.push(FilterErrors.missingField()); continue }

    const operator = tryParseOperator(opToken)
    if (!operator) { errors.push(FilterErrors.unknownOperator(opToken)); continue }
    if (!isAllowed(field, allowedFields)) { errors.push(FilterErrors.disallowedField(field)); continue }

    conditions.push({ field, operator: operator as FilterOperator, value })
  }

  if (errors.length > 0) return validation(errors)

  const filterGroup: FilterGroup = { logic: 'And', conditions, groups: [] }
  return ok({
    ...emptyFilterModel,
    root: filterGroup,
    isEmpty: conditions.length === 0,
    rawInput: JSON.stringify(values),
  })
}

function parseSortDirection(dir: string): string | null {
  const lower = dir.toLowerCase()
  if (lower === 'asc' || lower === 'ascending') return 'Ascending'
  if (lower === 'desc' || lower === 'descending') return 'Descending'
  return null
}

export function parseSortString(
  sortString: string | null | undefined,
  allowedFields: string[] | null = null,
): Result<SortModel> {
  if (!sortString || sortString.trim() === '') return ok(emptySortModel)

  const errors: ApiError[] = []
  const clauses: SortClause[] = []

  const parts = sortString.split(',').map(s => s.trim()).filter(Boolean)
  for (const part of parts) {
    let field = part
    let direction: 'Ascending' | 'Descending' = 'Ascending'

    if (part.startsWith('-')) {
      field = part.slice(1)
      direction = 'Descending'
    } else if (part.startsWith('+')) {
      field = part.slice(1)
    } else if (part.includes(':')) {
      const colonParts = part.split(':')
      field = colonParts[0] ?? ''
      const dirStr = colonParts.slice(1).join(':').trim()
      const resolved = parseSortDirection(dirStr)
      if (!resolved) {
        errors.push(SortErrors.unknownDirection(dirStr))
        continue
      }
      direction = resolved as 'Ascending' | 'Descending'
    }

    if (!field) { errors.push(SortErrors.missingField()); continue }
    if (!isAllowed(field, allowedFields)) { errors.push(SortErrors.disallowedField(field)); continue }

    clauses.push({ field, direction: direction as typeof SortDirection.Ascending, nulls: null })
  }

  if (errors.length > 0) return validation(errors)

  return ok({
    ...emptySortModel,
    clauses,
    isEmpty: clauses.length === 0,
    rawInput: sortString,
  })
}

export function parseSortQueryString(
  values: (string | null | undefined)[] | null | undefined,
  allowedFields: string[] | null = null,
): Result<SortModel> {
  if (!values || values.length === 0) return ok(emptySortModel)

  const errors: ApiError[] = []
  const clauses: SortClause[] = []

  const entries = values
    .map(v => (v ?? '').trim())
    .filter(Boolean)

  if (entries.length === 0) return ok(emptySortModel)

  for (const entry of entries) {
    let field = entry
    let direction: 'Ascending' | 'Descending' = 'Ascending'

    if (entry.startsWith('-')) {
      field = entry.slice(1).trim()
      direction = 'Descending'
    } else if (entry.startsWith('+')) {
      field = entry.slice(1).trim()
    } else if (entry.includes(':')) {
      const parts = entry.split(':')
      field = (parts[0] ?? '').trim()
      const dirStr = parts.slice(1).join(':').trim()
      const resolved = parseSortDirection(dirStr)
      if (!resolved) {
        errors.push(SortErrors.unknownDirection(dirStr))
        continue
      }
      direction = resolved as 'Ascending' | 'Descending'
    }

    if (!field) { errors.push(SortErrors.missingField()); continue }
    if (!isAllowed(field, allowedFields)) { errors.push(SortErrors.disallowedField(field)); continue }

    clauses.push({ field, direction: direction as typeof SortDirection.Ascending, nulls: null })
  }

  if (errors.length > 0) return validation(errors)

  return ok({
    ...emptySortModel,
    clauses,
    isEmpty: clauses.length === 0,
    rawInput: JSON.stringify(values),
  })
}

export function parseSortJson(
  json: string | null | undefined,
  allowedFields: string[] | null = null,
): Result<SortModel> {
  if (!json || json.trim() === '') return ok(emptySortModel)

  let parsed: unknown
  try {
    parsed = JSON.parse(json)
  } catch {
    return validation([SortErrors.invalidJson('invalid JSON')])
  }

  if (!Array.isArray(parsed)) {
    return validation([SortErrors.invalidJson('expected an array')])
  }

  const errors: ApiError[] = []
  const clauses: SortClause[] = []

  for (const item of parsed) {
    if (!item || typeof item !== 'object') {
      errors.push(SortErrors.invalidJson('each item must be an object'))
      continue
    }
    const entry = item as Record<string, unknown>
    const field = typeof entry.field === 'string' ? entry.field : ''
    const dir = typeof entry.direction === 'string' ? entry.direction : 'Ascending'
    const nulls = typeof entry.nulls === 'string' ? entry.nulls : null

    if (!field) { errors.push(SortErrors.missingField()); continue }
    if (dir !== 'Ascending' && dir !== 'Descending') { errors.push(SortErrors.unknownDirection(dir)); continue }
    if (!isAllowed(field, allowedFields)) { errors.push(SortErrors.disallowedField(field)); continue }
    if (nulls !== null && nulls !== 'First' && nulls !== 'Last') { errors.push(SortErrors.unknownNulls(nulls)); continue }

    clauses.push({
      field,
      direction: dir as 'Ascending' | 'Descending',
      nulls: nulls as 'First' | 'Last' | null,
    })
  }

  if (errors.length > 0) return validation(errors)

  return ok({
    ...emptySortModel,
    clauses,
    isEmpty: clauses.length === 0,
    rawInput: json,
  })
}

export function parseSearchText(text: string | null | undefined): SearchModel {
  if (!text || text.trim() === '') return emptySearchModel

  const caseSensitive = text.endsWith('~')
  const value = caseSensitive ? text.slice(0, -1) : text

  return {
    term: { value, caseSensitive },
    fields: [],
    mode: 'Any',
    allowedFields: null,
    isValid: true,
    violations: [],
    rawInput: text,
    isEmpty: false,
  }
}

export function parseSearchJson(
  json: string | null | undefined,
  allowedFields: string[] | null = null,
): Result<SearchModel> {
  if (!json || json.trim() === '') return ok(emptySearchModel)

  let parsed: unknown
  try {
    parsed = JSON.parse(json)
  } catch {
    return validation([SearchErrors.invalidJson('invalid JSON')])
  }

  if (!parsed || typeof parsed !== 'object') {
    return validation([SearchErrors.invalidJson('expected an object')])
  }

  const entry = parsed as Record<string, unknown>
  const termValue = typeof entry.term === 'string' ? entry.term : ''
  const fields = Array.isArray(entry.fields) ? entry.fields.filter((f): f is string => typeof f === 'string') : []

  const modeInput = entry.mode
  const mode: 'Any' | 'All' = modeInput === 'All' ? 'All' : 'Any'

  const csInput = entry.caseSensitive
  const caseSensitive = csInput === true || csInput === 'true'

  if (!termValue) return validation([SearchErrors.termRequired()])

  if (allowedFields) {
    const disallowed = fields.filter(f => !allowedFields.includes(f))
    if (disallowed.length > 0) {
      return validation(disallowed.map(f => SearchErrors.invalidJson(`field "${f}" not allowed`)))
    }
  }

  return ok({
    term: { value: termValue, caseSensitive },
    fields,
    mode,
    allowedFields,
    isValid: true,
    violations: [],
    rawInput: json,
    isEmpty: false,
  })
}

export function parseSearchQueryString(
  search: string | null | undefined,
  searchFields: string | null | undefined = null,
  searchMode: string | null | undefined = null,
  caseSensitive: string | null | undefined = null,
  allowedFields: string[] | null = null,
): Result<SearchModel> {
  if (!search || search.trim() === '') return ok(emptySearchModel)

  const fields = searchFields
    ? searchFields.split(',').map(s => s.trim()).filter(Boolean)
    : []

  const mode = searchMode?.toLowerCase() === 'all' ? 'All' as const : 'Any' as const
  const cs = caseSensitive?.toLowerCase() === 'true'

  if (allowedFields) {
    const disallowed = fields.filter(f => !allowedFields.includes(f))
    if (disallowed.length > 0) {
      return validation(disallowed.map(f => SearchErrors.invalidJson(`field "${f}" not allowed`)))
    }
  }

  return ok({
    term: { value: search, caseSensitive: cs },
    fields,
    mode,
    allowedFields,
    isValid: true,
    violations: [],
    rawInput: search,
    isEmpty: false,
  })
}

export function parsePageValues(
  page: number | null | undefined,
  pageSize: number | null | undefined,
  bounds: PageBounds = defaultPageBounds,
): Result<PageModel> {
  const errors: ApiError[] = []

  let normalizedPage = bounds.defaultPage
  let normalizedSize = bounds.defaultPageSize

  if (page !== null && page !== undefined) {
    if (!Number.isInteger(page) || page < 1) {
      errors.push(PageErrors.invalidNumber('page', String(page)))
    } else {
      normalizedPage = page
    }
  }

  if (pageSize !== null && pageSize !== undefined) {
    if (!Number.isInteger(pageSize) || pageSize < 1) {
      errors.push(PageErrors.invalidNumber('pageSize', String(pageSize)))
    } else {
      normalizedSize = Math.min(pageSize, bounds.maxPageSize)
    }
  }

  if (errors.length > 0) return validation(errors)

  return ok({
    page: normalizedPage,
    pageSize: normalizedSize,
    bounds,
    rawInput: null,
    isEmpty: false,
  })
}

export function parsePageJson(
  json: string | null | undefined,
  bounds: PageBounds = defaultPageBounds,
): Result<PageModel> {
  if (!json || json.trim() === '') {
    return ok({ ...emptyPageModel, bounds, isEmpty: false })
  }

  let parsed: unknown
  try {
    parsed = JSON.parse(json)
  } catch {
    return validation([PageErrors.invalidJson('invalid JSON')])
  }

  if (!parsed || typeof parsed !== 'object') {
    return validation([PageErrors.invalidJson('expected an object')])
  }

  const entry = parsed as Record<string, unknown>
  const page = typeof entry.page === 'number' ? entry.page : null
  const pageSize = typeof entry.pageSize === 'number' ? entry.pageSize : null

  return parsePageValues(page, pageSize, bounds)
}
