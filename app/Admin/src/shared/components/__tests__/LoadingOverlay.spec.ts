import { describe, it, expect } from 'vitest'
import { mount } from '@vue/test-utils'
import LoadingOverlay from '../feedback/LoadingOverlay.vue'

describe('LoadingOverlay', () => {
  it('renders slot content', () => {
    const wrapper = mount(LoadingOverlay, {
      props: { loading: false },
      slots: { default: '<p class="content">Data loaded</p>' },
    })
    expect(wrapper.find('.content').exists()).toBe(true)
  })

  it('shows overlay when loading is true', () => {
    const wrapper = mount(LoadingOverlay, {
      props: { loading: true },
      slots: { default: '<p>content</p>' },
    })
    expect(wrapper.find('.loading-overlay').exists()).toBe(true)
  })

  it('does not show overlay when loading is false', () => {
    const wrapper = mount(LoadingOverlay, {
      props: { loading: false },
      slots: { default: '<p>content</p>' },
    })
    expect(wrapper.find('.loading-overlay').exists()).toBe(false)
  })

  it('shows default spinner', () => {
    const wrapper = mount(LoadingOverlay, {
      props: { loading: true },
      slots: { default: '<p>content</p>' },
    })
    expect(wrapper.find('i.pi-spin').exists()).toBe(true)
  })

  it('shows message when provided', () => {
    const wrapper = mount(LoadingOverlay, {
      props: { loading: true, message: 'Saving changes...' },
      slots: { default: '<p>content</p>' },
    })
    expect(wrapper.text()).toContain('Saving changes...')
  })
})
