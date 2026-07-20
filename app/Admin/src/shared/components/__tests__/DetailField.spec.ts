import { describe, it, expect } from 'vitest'
import { mount } from '@vue/test-utils'
import DetailField from '../data-display/DetailField.vue'

describe('DetailField', () => {
  it('renders label and value', () => {
    const wrapper = mount(DetailField, {
      props: { label: 'Name', value: 'John' },
    })
    expect(wrapper.text()).toContain('Name')
    expect(wrapper.text()).toContain('John')
  })

  it('shows em-dash fallback when value is null', () => {
    const wrapper = mount(DetailField, {
      props: { label: 'Name', value: null },
    })
    expect(wrapper.text()).toContain('\u2014')
  })

  it('shows em-dash fallback when value is undefined', () => {
    const wrapper = mount(DetailField, {
      props: { label: 'Name' },
    })
    expect(wrapper.text()).toContain('\u2014')
  })

  it('shows em-dash fallback when value is empty string', () => {
    const wrapper = mount(DetailField, {
      props: { label: 'Name', value: '' },
    })
    expect(wrapper.text()).toContain('\u2014')
  })

  it('uses custom emptyText when provided', () => {
    const wrapper = mount(DetailField, {
      props: { label: 'Name', value: null, emptyText: 'N/A' },
    })
    expect(wrapper.text()).toContain('N/A')
    expect(wrapper.text()).not.toContain('\u2014')
  })

  it('renders number zero as value not fallback', () => {
    const wrapper = mount(DetailField, {
      props: { label: 'Count', value: 0 },
    })
    expect(wrapper.text()).toContain('0')
    expect(wrapper.text()).not.toContain('\u2014')
  })

  it('renders custom value via default slot', () => {
    const wrapper = mount(DetailField, {
      props: { label: 'Status' },
      slots: { default: '<span class="custom">Active</span>' },
    })
    expect(wrapper.find('.custom').exists()).toBe(true)
    expect(wrapper.find('.custom').text()).toBe('Active')
  })
})
