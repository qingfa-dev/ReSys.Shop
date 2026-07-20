import { describe, it, expect } from 'vitest'
import { mount } from '@vue/test-utils'
import Section from '../layout/Section.vue'

describe('Section', () => {
  it('renders title', () => {
    const wrapper = mount(Section, {
      props: { title: 'Basic Information' },
      slots: { default: '<p>content</p>' },
    })
    expect(wrapper.text()).toContain('Basic Information')
  })

  it('renders description', () => {
    const wrapper = mount(Section, {
      props: { title: 'Info', description: 'Core product details' },
      slots: { default: '<p>content</p>' },
    })
    expect(wrapper.text()).toContain('Core product details')
  })

  it('renders slot content', () => {
    const wrapper = mount(Section, {
      props: { title: 'Section' },
      slots: { default: '<p class="content">Hello World</p>' },
    })
    expect(wrapper.find('.content').exists()).toBe(true)
  })

  it('renders actions slot', () => {
    const wrapper = mount(Section, {
      props: { title: 'Section' },
      slots: { default: '<p>content</p>', actions: '<button class="edit-btn">Edit</button>' },
    })
    expect(wrapper.find('.edit-btn').exists()).toBe(true)
  })

  it('does not render header when no title or actions', () => {
    const wrapper = mount(Section, {
      props: {},
      slots: { default: '<p>content</p>' },
    })
    expect(wrapper.find('.section-header').exists()).toBe(false)
  })
})
