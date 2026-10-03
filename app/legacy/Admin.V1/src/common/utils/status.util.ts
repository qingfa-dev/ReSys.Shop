export type StatusDef = { label: string; severity: string }

export const booleanStatusMap: Record<string, StatusDef> = {
  true: { label: 'Active', severity: 'success' },
  false: { label: 'Inactive', severity: 'secondary' },
}
