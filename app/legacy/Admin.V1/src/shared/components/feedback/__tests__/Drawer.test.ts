import { describe, it, expect } from 'vitest'
import { mount } from '@vue/test-utils'
import Drawer from '../Drawer.vue'

const drawerStub = {
  template: '<div class="p-drawer"><div class="p-drawer-header"><div class="p-drawer-title">{{ header }}</div></div><div class="p-drawer-content"><slot /></div></div>',
  props: ['modelValue', 'header', 'position', 'style'],
}

describe('Drawer', () => {
  it('renders when visible', () => {
    const wrapper = mount(Drawer, {
      props: { modelValue: true },
      global: { stubs: { Drawer: drawerStub } },
    })
    expect(wrapper.find('.p-drawer').exists()).toBe(true)
  })

  it('renders header text', () => {
    const wrapper = mount(Drawer, {
      props: { modelValue: true, header: 'Test Drawer' },
      global: { stubs: { Drawer: drawerStub } },
    })
    expect(wrapper.text()).toContain('Test Drawer')
  })

  it('renders slot content', () => {
    const wrapper = mount(Drawer, {
      props: { modelValue: true },
      slots: { default: '<div class="slot-content">Content</div>' },
      global: { stubs: { Drawer: drawerStub } },
    })
    expect(wrapper.text()).toContain('Content')
  })
})
