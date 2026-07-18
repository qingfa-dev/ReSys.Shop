import { describe, it, expect, beforeAll } from 'vitest'
import { mount } from '@vue/test-utils'
import DataTableShell from '../DataTableShell.Component.vue'

beforeAll(() => {
  if (!window.matchMedia) {
    window.matchMedia = (() => ({ matches: false, addListener: () => {}, removeListener: () => {}, addEventListener: () => {}, removeEventListener: () => {}, dispatchEvent: () => false })) as typeof window.matchMedia
  }
})

const columns = [
  { field: 'name', header: 'Name', sortable: true },
  { field: 'status', header: 'Status' },
]

const items = [
  { id: '1', name: 'Item 1', status: 'Active' },
  { id: '2', name: 'Item 2', status: 'Draft' },
]

describe('DataTableShell', () => {
  function mountOptions(props: Record<string, any> = {}) {
    return {
      props,
      global: {
        mocks: {
          $primevue: { config: { locale: {}, aria: {} } },
        },
        stubs: {
          DataTable: false,
          Column: false,
          Paginator: true,
          Select: true,
          Button: true,
          InputText: true,
          IconField: true,
          InputIcon: true,
          Skeleton: true,
        },
      },
    }
  }

  it('renders columns and data', () => {
    const wrapper = mount(DataTableShell, mountOptions({ columns, value: items, totalRecords: 2 }))
    expect(wrapper.html()).toContain('Item 1')
  })

  it('shows empty state when no data', () => {
    const wrapper = mount(DataTableShell, mountOptions({ columns, value: [], totalRecords: 0, emptyTitle: 'Nothing here' }))
    expect(wrapper.text()).toContain('Nothing here')
  })

  it('shows skeleton when loading', () => {
    const wrapper = mount(DataTableShell, mountOptions({ columns, value: [], loading: true, totalRecords: 0 }))
    expect(wrapper.html()).toContain('skeleton')
  })
})
