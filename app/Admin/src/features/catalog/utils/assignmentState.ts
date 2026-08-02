export interface AssignmentLists {
  unassigned: unknown[]
  assigned: unknown[]
}

export function makeEmptyAssignments(): { unassigned: []; assigned: [] } {
  return { unassigned: [], assigned: [] }
}
