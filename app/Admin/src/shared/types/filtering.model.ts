export type FilterOperator =
  | '='
  | '=='
  | '!='
  | '>'
  | '>='
  | '<'
  | '<='
  | '*'
  | '*~'
  | '!*'
  | '^'
  | '^~'
  | '!^'
  | '$'
  | '$~'
  | '!$'

export const FilterOp = {
  eq: '=' as FilterOperator,
  eqCs: '==' as FilterOperator,
  neq: '!=' as FilterOperator,
  gt: '>' as FilterOperator,
  gte: '>=' as FilterOperator,
  lt: '<' as FilterOperator,
  lte: '<=' as FilterOperator,
  contains: '*' as FilterOperator,
  containsCs: '*~' as FilterOperator,
  notContains: '!*' as FilterOperator,
  starts: '^' as FilterOperator,
  startsCs: '^~' as FilterOperator,
  notStarts: '!^' as FilterOperator,
  ends: '$' as FilterOperator,
  endsCs: '$~' as FilterOperator,
  notEnds: '!$' as FilterOperator,
} as const

export type FilterLogic = 'and' | 'or'

export interface FilterCondition {
  field: string
  op: FilterOperator
  value: string
}

export interface FilterGroup {
  logic: FilterLogic
  conditions: FilterCondition[]
  groups: FilterGroup[]
}

export function createFilterGroup(
  logic: FilterLogic = 'and',
  conditions: FilterCondition[] = [],
  groups: FilterGroup[] = [],
): FilterGroup {
  return { logic, conditions, groups }
}

export interface FilterModel {
  root: FilterGroup
  conditions: FilterCondition[]
  allowedFields?: string[]
  isValid: boolean
  violations: string[]
  isEmpty: boolean
}

export const emptyFilterGroup: FilterGroup = Object.freeze({
  logic: 'and',
  conditions: [],
  groups: [],
})

export const emptyFilterModel: FilterModel = Object.freeze({
  root: emptyFilterGroup,
  conditions: [],
  isValid: true,
  violations: [],
  isEmpty: true,
})
