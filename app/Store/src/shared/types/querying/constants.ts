import { FilterOperator } from './enums'

export const DSL_DELIMITERS = {
  OR: '|',
  AND: ',',
  GROUP_OPEN: '(',
  GROUP_CLOSE: ')',
  QUOTE: '"',
  WILDCARD: '*',
} as const

export const JSON_KEYS = {
  LOGIC: 'logic',
  CONDITIONS: 'conditions',
  FIELD: 'field',
  OP: 'op',
  VALUE: 'value',
  OR_VALUE: 'or',
} as const

export const TOKEN_ALIASES: Record<string, string> = {
  eq: '=',
  neq: '!=',
  gt: '>',
  gte: '>=',
  lt: '<',
  lte: '<=',
  contains: '*',
  ncontains: '!*',
  starts: '^',
  nstarts: '!^',
  ends: '$',
  nends: '!$',
}

export type DslToken = string

export const OPERATOR_TO_TOKEN: Record<string, string> = {
  [FilterOperator.Equal]: '=',
  [FilterOperator.EqualCaseSensitive]: '==',
  [FilterOperator.NotEqual]: '!=',
  [FilterOperator.GreaterThan]: '>',
  [FilterOperator.GreaterThanOrEqual]: '>=',
  [FilterOperator.LessThan]: '<',
  [FilterOperator.LessThanOrEqual]: '<=',
  [FilterOperator.Contains]: '*',
  [FilterOperator.ContainsCaseSensitive]: '*~',
  [FilterOperator.NotContains]: '!*',
  [FilterOperator.StartsWith]: '^',
  [FilterOperator.StartsWithCaseSensitive]: '^~',
  [FilterOperator.NotStartsWith]: '!^',
  [FilterOperator.EndsWith]: '$',
  [FilterOperator.EndsWithCaseSensitive]: '$~',
  [FilterOperator.NotEndsWith]: '!$',
}

const TOKEN_TO_OPERATOR: Record<string, string> = {
  '=': FilterOperator.Equal,
  '==': FilterOperator.EqualCaseSensitive,
  '!=': FilterOperator.NotEqual,
  '>': FilterOperator.GreaterThan,
  '>=': FilterOperator.GreaterThanOrEqual,
  '<': FilterOperator.LessThan,
  '<=': FilterOperator.LessThanOrEqual,
  '*': FilterOperator.Contains,
  '*~': FilterOperator.ContainsCaseSensitive,
  '!*': FilterOperator.NotContains,
  '^': FilterOperator.StartsWith,
  '^~': FilterOperator.StartsWithCaseSensitive,
  '!^': FilterOperator.NotStartsWith,
  '$': FilterOperator.EndsWith,
  '$~': FilterOperator.EndsWithCaseSensitive,
  '!$': FilterOperator.NotEndsWith,
}

const CASE_SENSITIVE_OPERATORS = new Set<string>([
  FilterOperator.EqualCaseSensitive,
  FilterOperator.ContainsCaseSensitive,
  FilterOperator.StartsWithCaseSensitive,
  FilterOperator.EndsWithCaseSensitive,
])

const NEGATION_OPERATORS = new Set<string>([
  FilterOperator.NotEqual,
  FilterOperator.NotContains,
  FilterOperator.NotStartsWith,
  FilterOperator.NotEndsWith,
])

const STRING_ONLY_OPERATORS = new Set<string>([
  FilterOperator.Contains,
  FilterOperator.ContainsCaseSensitive,
  FilterOperator.NotContains,
  FilterOperator.StartsWith,
  FilterOperator.StartsWithCaseSensitive,
  FilterOperator.NotStartsWith,
  FilterOperator.EndsWith,
  FilterOperator.EndsWithCaseSensitive,
  FilterOperator.NotEndsWith,
])

export const NULL_SENTINEL = 'null'

export const BOOLEAN_ALIASES: Record<string, boolean> = {
  '1': true,
  yes: true,
  y: true,
  '0': false,
  no: false,
  n: false,
}

export const NAVIGATION_SEPARATOR = '.'

export const CASE_INSENSITIVE_SUFFIX = '~'

export function tryParseOperator(token: string): string | null {
  const exact = TOKEN_TO_OPERATOR[token]
  if (exact) return exact
  const aliased = TOKEN_ALIASES[token]
  if (aliased) return TOKEN_TO_OPERATOR[aliased] ?? null
  return null
}

export function toDslToken(operator: string): string {
  return OPERATOR_TO_TOKEN[operator] ?? '='
}

export function isCaseSensitiveOperator(operator: string): boolean {
  return CASE_SENSITIVE_OPERATORS.has(operator)
}

export function isNegationOperator(operator: string): boolean {
  return NEGATION_OPERATORS.has(operator)
}

export function isStringOnlyOperator(operator: string): boolean {
  return STRING_ONLY_OPERATORS.has(operator)
}
