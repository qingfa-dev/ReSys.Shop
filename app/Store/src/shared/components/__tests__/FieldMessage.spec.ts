import { describe, it, expect } from 'vitest'
import { mount } from '@vue/test-utils'
import PrimeVue from 'primevue/config'
import FieldMessage from '../FieldMessage.vue'

describe('FieldMessage', () => {
  it('renders the message text when an error is provided', () => {
    const wrapper = mount(FieldMessage, {
      props: { error: 'Email is required' },
      global: { plugins: [PrimeVue] },
    })

    expect(wrapper.text()).toContain('Email is required')
  })

  it.each([null, undefined, ''])('renders nothing when error is %s', (error) => {
    const wrapper = mount(FieldMessage, {
      props: { error },
      global: { plugins: [PrimeVue] },
    })

    expect(wrapper.text()).toBe('')
  })
})
