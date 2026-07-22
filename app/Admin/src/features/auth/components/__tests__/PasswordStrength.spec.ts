import { describe, it, expect, vi } from 'vitest'
import { mount } from '@vue/test-utils'
import PasswordStrength from '../PasswordStrength.vue'

vi.mock('vue-i18n', () => ({
  useI18n: () => ({
    t: (key: string) => {
      const map: Record<string, string> = {
        'auth.validation.password.rules.min_length': 'At least 8 characters',
        'auth.validation.password.rules.uppercase': 'At least one uppercase letter',
        'auth.validation.password.rules.lowercase': 'At least one lowercase letter',
        'auth.validation.password.rules.digit': 'At least one digit',
        'auth.validation.password.rules.special': 'At least one special character',
      }
      return map[key] ?? key
    },
  }),
}))

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
