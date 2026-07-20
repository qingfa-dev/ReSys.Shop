import { describe, it, expect } from 'vitest'
import { mount } from '@vue/test-utils'
import FormField from '../form/FormField.vue'

describe('FormField', () => {
  it('renders label and slot content', () => {
    const wrapper = mount(FormField, {
      props: { label: 'Name', forId: 'name' },
      slots: { default: '<input id="name" />' },
    })
    expect(wrapper.find('label').text()).toContain('Name')
  })

  it('renders label with for attribute when forId provided', () => {
    const wrapper = mount(FormField, {
      props: { label: 'Email', forId: 'email' },
      slots: { default: '<input id="email" />' },
    })
    expect(wrapper.find('label').attributes('for')).toBe('email')
  })

  it('shows required asterisk', () => {
    const wrapper = mount(FormField, {
      props: { label: 'Name', forId: 'name', required: true },
      slots: { default: '<input id="name" />' },
    })
    const label = wrapper.find('label')
    expect(label.text()).toContain('*')
  })

  it('does not show asterisk when not required', () => {
    const wrapper = mount(FormField, {
      props: { label: 'Name', forId: 'name' },
      slots: { default: '<input id="name" />' },
    })
    expect(wrapper.find('label').text()).not.toContain('*')
  })

  it('shows error message when provided', () => {
    const wrapper = mount(FormField, {
      props: { label: 'Name', forId: 'name', error: 'Required field' },
      slots: { default: '<input id="name" />' },
    })
    expect(wrapper.text()).toContain('Required field')
  })

  it('shows hint when no error', () => {
    const wrapper = mount(FormField, {
      props: { label: 'Name', forId: 'name', hint: 'Enter your full name' },
      slots: { default: '<input id="name" />' },
    })
    expect(wrapper.text()).toContain('Enter your full name')
  })

  it('does not show hint when error is present', () => {
    const wrapper = mount(FormField, {
      props: { label: 'Name', forId: 'name', error: 'Required', hint: 'Enter name' },
      slots: { default: '<input id="name" />' },
    })
    expect(wrapper.text()).toContain('Required')
    expect(wrapper.text()).not.toContain('Enter name')
  })
})
