import { describe, it, expect, beforeAll } from 'vitest'
import { mount } from '@vue/test-utils'
import DataTableShell from '../DataTableShell.Component.vue'
import type { ColumnDef } from '../DataTableShell.Component.vue'

beforeAll(() => {
  if (!window.matchMedia) {
    window.matchMedia = (() => ({ matches: false, addListener: () => {}, removeListener: () => {}, addEventListener: () => {}, removeEventListener: () => {}, dispatchEvent: () => false })) as typeof window.matchMedia
  }
})

const columns: ColumnDef[] = [
  { field: 'name', header: 'Name', sortable: true },
  { field: 'status', header: 'Status' },
]

const items = [
  { id: '1', name: 'Item 1', status: 'Active' },
  { id: '2', name: 'Item 2', status: 'Draft' },
]

const stubs = {
  DataTable: false as const,
  Column: false as const,
  Paginator: true as const,
  Select: true as const,
  Button: true as const,
  InputText: true as const,
  IconField: true as const,
  InputIcon: true as const,
  Skeleton: true as const,
}

describe('DataTableShell', () => {
  it('renders columns and data', () => {
    const wrapper = mount(DataTableShell, {
      props: { columns, value: items as any, totalRecords: 2 },
      global: { stubs },
    })
    expect(wrapper.html()).toContain('Item 1')
  })

  it('shows empty state when no data', () => {
    const wrapper = mount(DataTableShell, {
      props: { columns, value: [] as any[], totalRecords: 0, emptyTitle: 'Nothing here' },
      global: { stubs },
    })
    expect(wrapper.text()).toContain('Nothing here')
  })

  it('shows skeleton when loading', () => {
    const wrapper = mount(DataTableShell, {
      props: { columns, value: [] as any[], loading: true, totalRecords: 0 },
      global: { stubs },
    })
    expect(wrapper.html()).toContain('skeleton')
  })
})
