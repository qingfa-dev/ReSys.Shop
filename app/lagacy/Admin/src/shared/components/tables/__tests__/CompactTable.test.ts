import { describe, it, expect } from 'vitest'
import { mount } from '@vue/test-utils'
import CompactTable from '../CompactTable.vue'

const columns = [
  { field: 'name', header: 'Name' },
  { field: 'status', header: 'Status' },
]
const rows = [{ id: '1', name: 'Item A', status: 'Active' }, { id: '2', name: 'Item B', status: 'Inactive' }]

describe('CompactTable', () => {
  it('renders rows from value prop', () => {
    const wrapper = mount(CompactTable, {
      global: { stubs: { DataTable: true, Column: true } },
      props: { value: rows, columns },
    })
    expect(wrapper.props('value')).toEqual(rows)
  })

  it('accepts loading prop', () => {
    const wrapper = mount(CompactTable, {
      global: { stubs: { DataTable: true, Column: true } },
      props: { value: [], columns, loading: true },
    })
    expect(wrapper.props('loading')).toBe(true)
  })

  it('defaults rows to 5', () => {
    const wrapper = mount(CompactTable, {
      global: { stubs: { DataTable: true, Column: true } },
      props: { value: [], columns },
    })
    expect(wrapper.props('rows')).toBe(5)
  })
})
