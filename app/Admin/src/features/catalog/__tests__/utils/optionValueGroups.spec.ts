import { describe, it, expect } from 'vitest'
import { buildOptionValueGroups, selectedIdsForGroup } from '../../utils/optionValueGroups'
import type { OptionValueAssignment } from '../../types/variant'

function ov(id: string, typeId: string, typeName: string): OptionValueAssignment {
  return {
    optionValueId: id,
    optionTypeId: typeId,
    optionTypeName: typeName,
    name: id,
    presentation: id,
    isAssigned: false,
  }
}

describe('buildOptionValueGroups', () => {
  const assignments = [
    ov('red', 'color', 'Color'),
    ov('blue', 'color', 'Color'),
    ov('small', 'size', 'Size'),
    ov('other', 'unassigned-type', 'Unassigned'),
  ]

  it('groups values by option type and keeps group order', () => {
    const groups = buildOptionValueGroups(assignments, new Set(['color', 'size']))
    expect(groups.map((g) => g.optionTypeName)).toEqual(['Color', 'Size'])
    expect(groups[0]!.values.map((v) => v.optionValueId)).toEqual(['red', 'blue'])
  })

  it('excludes option types not assigned to the product', () => {
    const groups = buildOptionValueGroups(assignments, new Set(['color']))
    expect(groups.map((g) => g.optionTypeId)).toEqual(['color'])
  })

  it('returns empty list when nothing is assigned', () => {
    expect(buildOptionValueGroups(assignments, new Set())).toEqual([])
  })
})

describe('selectedIdsForGroup', () => {
  const group = {
    optionTypeId: 'color',
    optionTypeName: 'Color',
    values: [
      ov('red', 'color', 'Color'),
      ov('blue', 'color', 'Color'),
    ],
  }

  it('returns only ids belonging to the group', () => {
    expect(selectedIdsForGroup(group, ['red', 'blue', 'small'])).toEqual(['red', 'blue'])
  })

  it('returns empty when none selected', () => {
    expect(selectedIdsForGroup(group, [])).toEqual([])
  })
})
