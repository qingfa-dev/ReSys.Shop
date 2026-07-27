import type { DataTableFilterMeta } from 'primevue/datatable'

interface FilterMetaData {
  value: unknown
  matchMode?: string
}

function isFilterMetaData(obj: unknown): obj is FilterMetaData {
  return obj !== null && typeof obj === 'object' && 'value' in obj
}

export function getFilterValue(filters: DataTableFilterMeta, key: string): unknown {
  const meta = filters[key]
  if (isFilterMetaData(meta)) {
    return meta.value
  }
  return null
}
