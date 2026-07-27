export interface TableColumn {
  field: string
  header: string
  sortable?: boolean
  filterable?: boolean
  width?: string
}

export interface TableAction {
  label: string
  icon?: string
  severity?: 'success' | 'info' | 'warn' | 'danger' | 'secondary' | 'contrast'
  visible?: (row: unknown) => boolean
  action: (row: unknown) => void
}

export interface BreadcrumbItem {
  label: string
  to?: string
  icon?: string
}

export interface DropdownOption {
  label: string
  value: string | number | boolean
  disabled?: boolean
}

export interface ToastMessage {
  severity: 'success' | 'info' | 'warn' | 'error'
  summary: string
  detail: string
  life?: number
}
