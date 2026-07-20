import { describe, it, expect } from 'vitest'
import { mount } from '@vue/test-utils'
import ModalDialog from '../ModalDialog.vue'

const DialogStub = { template: '<div><slot /><slot name="footer" /></div>', props: ['visible', 'header', 'closable', 'dismissableMask', 'modal', 'class'] }

describe('ModalDialog', () => {
  it('renders header and content', () => {
    const wrapper = mount(ModalDialog, {
      global: { stubs: { Dialog: DialogStub } },
      props: { modelValue: true, header: 'My Title' },
      slots: { default: '<p>Body content</p>' },
    })
    expect(wrapper.html()).toContain('Body content')
  })

  it('toggles visibility via v-model', async () => {
    const wrapper = mount(ModalDialog, {
      global: { stubs: { Dialog: DialogStub } },
      props: { modelValue: false, header: 'Test' },
    })
    await wrapper.setProps({ modelValue: true })
    expect(wrapper.vm.visible).toBe(true)
  })

  it('renders footer slot', () => {
    const wrapper = mount(ModalDialog, {
      global: { stubs: { Dialog: DialogStub } },
      props: { modelValue: true, header: 'Title' },
      slots: { footer: '<button>Save</button>' },
    })
    expect(wrapper.html()).toContain('Save')
  })
})
