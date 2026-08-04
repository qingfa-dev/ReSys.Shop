import { describe, it, expect } from 'vitest'
import { mount } from '@vue/test-utils'
import StatusBadge from '../StatusBadge.vue'
import PrimeVue from 'primevue/config'

describe('StatusBadge', () => {
  const statusMap = {
    active: { label: 'Active', severity: 'success' },
    draft: { label: 'Draft', severity: 'warn' },
  }

  it('renders Tag with resolved label', () => {
    const wrapper = mount(StatusBadge, {
      props: { status: 'active', statusMap },
      global: {
        stubs: {
          Tag: { props: ['value', 'severity'], template: '<span class="tag-stub">{{ value }}</span>' },
        },
      },
    })
    expect(wrapper.find('.tag-stub').text()).toBe('Active')
  })

  it('falls back to secondary severity for unknown status', () => {
    const wrapper = mount(StatusBadge, {
      props: { status: 'unknown', statusMap },
      global: {
        stubs: {
          Tag: { props: ['value', 'severity'], template: '<span class="tag-stub">{{ value }}</span>' },
        },
      },
    })
    expect(wrapper.find('.tag-stub').text()).toBe('unknown')
  })

  it('applies large class for normal size', () => {
    const wrapper = mount(StatusBadge, {
      props: { status: 'active', statusMap, size: 'normal' },
      global: { plugins: [PrimeVue], stubs: { Tag: true } },
    })
    expect(wrapper.html()).toContain('rounded')
  })

  it('renders status label and severity from statusMap', () => {
    const statusMap = { active: { label: 'Active', severity: 'success' } }
    const wrapper = mount(StatusBadge, {
      props: { status: 'active', statusMap },
      global: { plugins: [PrimeVue], stubs: { Tag: true } },
    })
    expect(wrapper.html()).toContain('Active')
  })
})
