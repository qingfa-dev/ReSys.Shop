import { describe, it, expect } from 'vitest'
import { mount } from '@vue/test-utils'
import FormField from '../FormField.vue'

describe('FormField', () => {
  it('renders label and slot content', () => {
    const wrapper = mount(FormField, {
      props: { label: 'Name', name: 'name' },
      slots: { default: '<input id="name" />' },
      global: { stubs: { InputText: true } },
    })
    expect(wrapper.find('label').text()).toBe('Name')
  })

  it('shows required asterisk', () => {
    const wrapper = mount(FormField, {
      props: { label: 'Name', name: 'name', required: true },
      slots: { default: '<input id="name" />' },
      global: { stubs: { InputText: true } },
    })
    expect(wrapper.find('label span').text()).toBe('*')
  })

  it('shows error message when provided', () => {
    const wrapper = mount(FormField, {
      props: { label: 'Name', name: 'name', error: 'Required field' },
      slots: { default: '<input id="name" />' },
      global: { stubs: { InputText: true } },
    })
    expect(wrapper.find('.p-error').text()).toBe('Required field')
  })

  it('shows hint when no error', () => {
    const wrapper = mount(FormField, {
      props: { label: 'Name', name: 'name', hint: 'Enter your full name' },
      slots: { default: '<input id="name" />' },
      global: { stubs: { InputText: true } },
    })
    expect(wrapper.find('.text-surface-400').text()).toBe('Enter your full name')
  })
})
