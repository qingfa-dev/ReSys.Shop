import { describe, it, expect } from 'vitest'
import { mount } from '@vue/test-utils'
import PasswordStrength from '../PasswordStrength.vue'

describe('PasswordStrength', () => {
  it('renders nothing when password is empty', () => {
    const wrapper = mount(PasswordStrength, { props: { password: '' } })
    expect(wrapper.find('ul').exists()).toBe(false)
  })

  it('shows all rules as unmet for weak password', () => {
    const wrapper = mount(PasswordStrength, { props: { password: 'a' } })
    const items = wrapper.findAll('li')
    expect(items).toHaveLength(5)
    expect(items[0]!.text()).toContain('At least 8 characters')
    expect(items[0]!.classes()).toContain('text-muted-color')
  })

  it('shows all rules met for strong password', () => {
    const wrapper = mount(PasswordStrength, { props: { password: 'Strong1@pass' } })
    const items = wrapper.findAll('li')
    items.forEach((item) => {
      expect(item.classes()).toContain('text-green-600')
    })
  })

  it('updates reactively when password changes', async () => {
    const wrapper = mount(PasswordStrength, { props: { password: 'weak' } })
    await wrapper.setProps({ password: 'Strong1@pass' })
    const items = wrapper.findAll('li')
    items.forEach((item) => {
      expect(item.classes()).toContain('text-green-600')
    })
  })
})
