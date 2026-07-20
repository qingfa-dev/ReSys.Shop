import { describe, it, expect } from 'vitest'
import { mount } from '@vue/test-utils'
import DetailGroup from '../data-display/DetailGroup.vue'

describe('DetailGroup', () => {
  it('renders title', () => {
    const wrapper = mount(DetailGroup, {
      props: { title: 'General Information' },
      slots: { default: '<div>content</div>' },
    })
    expect(wrapper.text()).toContain('General Information')
  })

  it('renders slot content', () => {
    const wrapper = mount(DetailGroup, {
      props: { title: 'Details' },
      slots: { default: '<p class="field">Name: John</p>' },
    })
    expect(wrapper.find('.field').exists()).toBe(true)
  })

  it('applies grid columns class for 2 columns (default)', () => {
    const wrapper = mount(DetailGroup, {
      props: { title: 'Test' },
      slots: { default: '<div>test</div>' },
    })
    expect(wrapper.find('.grid').exists()).toBe(true)
    expect(wrapper.find('.grid').classes()).toContain('md:grid-cols-2')
  })

  it('applies grid columns class for 3 columns', () => {
    const wrapper = mount(DetailGroup, {
      props: { title: 'Test', columns: 3 },
      slots: { default: '<div>test</div>' },
    })
    expect(wrapper.find('.grid').classes()).toContain('lg:grid-cols-3')
  })

  it('applies responsive columns for 4 columns', () => {
    const wrapper = mount(DetailGroup, {
      props: { title: 'Test', columns: 4 },
      slots: { default: '<div>test</div>' },
    })
    expect(wrapper.find('.grid').classes()).toContain('md:grid-cols-2')
    expect(wrapper.find('.grid').classes()).toContain('xl:grid-cols-4')
  })
})
