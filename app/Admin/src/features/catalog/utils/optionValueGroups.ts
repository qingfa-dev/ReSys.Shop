import type { OptionValueAssignment } from '../types/variant'

export interface OptionValueGroup {
  optionTypeId: string
  optionTypeName: string
  values: OptionValueAssignment[]
}

export function buildOptionValueGroups(
  assignments: OptionValueAssignment[],
  assignedOptionTypeIds: Set<string>,
): OptionValueGroup[] {
  const groups = new Map<string, OptionValueGroup>()

  for (const ov of assignments) {
    if (!assignedOptionTypeIds.has(ov.optionTypeId)) continue
    const key = ov.optionTypeId
    if (!groups.has(key)) {
      groups.set(key, {
        optionTypeId: ov.optionTypeId,
        optionTypeName: ov.optionTypeName,
        values: [],
      })
    }
    groups.get(key)!.values.push(ov)
  }

  return [...groups.values()]
}

export function selectedIdsForGroup(
  group: OptionValueGroup,
  selectedOptionValueIds: string[],
): string[] {
  const groupIds = new Set(group.values.map((v) => v.optionValueId))
  return selectedOptionValueIds.filter((id) => groupIds.has(id))
}
