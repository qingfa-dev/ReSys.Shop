import { describe, it, expect } from 'vitest'
import { mount } from '@vue/test-utils'
import ErrorState from '../ErrorState.vue'

describe('ErrorState', () => {
  it('renders default title and message', () => {
    const wrapper = mount(ErrorState)
    expect(wrapper.text()).toContain('Something went wrong')
    expect(wrapper.text()).toContain('An unexpected error occurred')
  })

  it('renders custom title and message', () => {
    const wrapper = mount(ErrorState, {
      props: { title: 'Custom Error', message: 'Custom message' },
    })
    expect(wrapper.text()).toContain('Custom Error')
    expect(wrapper.text()).toContain('Custom message')
  })

  it('emits retry when button clicked', async () => {
    const wrapper = mount(ErrorState, {
      props: { retryLabel: 'Retry' },
    })
    await wrapper.find('button').trigger('click')
    expect(wrapper.emitted('retry')).toBeTruthy()
  })

  it('hides retry button when no label provided', () => {
    const wrapper = mount(ErrorState)
    expect(wrapper.find('button').exists()).toBe(false)
  })
})
