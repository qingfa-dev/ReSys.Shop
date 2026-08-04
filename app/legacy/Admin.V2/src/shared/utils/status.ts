import type { StatusDef } from './types'

export const booleanStatusMap: Record<string, StatusDef> = {
  true: { label: 'Active', severity: 'success' },
  false: { label: 'Inactive', severity: 'secondary' },
}
