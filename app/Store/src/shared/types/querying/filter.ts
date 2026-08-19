import type { FilterOperator, FilterLogic } from './enums'

export interface FilterCondition {
  field: string
  operator: FilterOperator
  value: string
}

export interface FilterGroup {
  logic: FilterLogic
  conditions: FilterCondition[]
  groups: FilterGroup[]
}

export const emptyFilterGroup: FilterGroup = {
  logic: 'And',
  conditions: [],
  groups: [],
}

export interface FilterModel {
  root: FilterGroup
  allowedFields: string[] | null
  isValid: boolean
  violations: string[]
  rawInput: string | null
  isEmpty: boolean
}

export const emptyFilterModel: FilterModel = {
  root: emptyFilterGroup,
  allowedFields: null,
  isValid: true,
  violations: [],
  rawInput: null,
  isEmpty: true,
}

export function flattenConditions(group: FilterGroup): FilterCondition[] {
  const result: FilterCondition[] = []
  const visit = (g: FilterGroup): void => {
    result.push(...g.conditions)
    for (const sub of g.groups) visit(sub)
  }
  visit(group)
  return result
}
