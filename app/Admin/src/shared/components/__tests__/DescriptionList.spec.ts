import { describe, it, expect } from 'vitest'
import { mount } from '@vue/test-utils'
import DescriptionList from '../data-display/DescriptionList.vue'

describe('DescriptionList', () => {
  it('renders items with labels and values', () => {
    const wrapper = mount(DescriptionList, {
      props: { items: [{ label: 'Order ID', value: '#1234' }, { label: 'Total', value: '$99.00' }] },
    })
    expect(wrapper.text()).toContain('Order ID')
    expect(wrapper.text()).toContain('#1234')
    expect(wrapper.text()).toContain('Total')
    expect(wrapper.text()).toContain('$99.00')
  })

  it('shows emptyText when value is empty string', () => {
    const wrapper = mount(DescriptionList, {
      props: { items: [{ label: 'Notes', value: '' }] },
    })
    expect(wrapper.text()).toContain('\u2014')
  })

  it('uses custom emptyText', () => {
    const wrapper = mount(DescriptionList, {
      props: { items: [{ label: 'Notes', value: '', emptyText: 'None' }] },
    })
    expect(wrapper.text()).toContain('None')
    expect(wrapper.text()).not.toContain('\u2014')
  })

  it('renders number zero as value', () => {
    const wrapper = mount(DescriptionList, {
      props: { items: [{ label: 'Count', value: 0 }] },
    })
    expect(wrapper.text()).toContain('0')
  })

  it('applies columns class', () => {
    const wrapper = mount(DescriptionList, {
      props: { items: [{ label: 'A', value: '1' }], columns: 3 },
    })
    expect(wrapper.find('dl').classes()).toContain('lg:grid-cols-3')
  })

  it('defaults to 2 columns', () => {
    const wrapper = mount(DescriptionList, {
      props: { items: [{ label: 'A', value: '1' }] },
    })
    expect(wrapper.find('dl').classes()).toContain('md:grid-cols-2')
  })

  it('renders empty when items array is empty', () => {
    const wrapper = mount(DescriptionList, {
      props: { items: [] },
    })
    expect(wrapper.find('dl').exists()).toBe(true)
    expect(wrapper.findAll('div').length).toBe(0)
  })
})
