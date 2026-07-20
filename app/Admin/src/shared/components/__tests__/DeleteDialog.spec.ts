import { describe, it, expect } from 'vitest'
import { mount } from '@vue/test-utils'
import DeleteDialog from '../overlays/DeleteDialog.vue'

describe('DeleteDialog', () => {
  it('renders entity name in warning', () => {
    const wrapper = mount(DeleteDialog, {
      props: { entityName: 'Order #1234', visible: true },
      global: { stubs: { Dialog: { template: '<div v-if="visible"><slot /><slot name="footer" /></div>', props: ['visible'] }, Button: true } },
    })
    expect(wrapper.text()).toContain('Order #1234')
  })

  it('shows default warning text', () => {
    const wrapper = mount(DeleteDialog, {
      props: { entityName: 'this item', visible: true },
      global: { stubs: { Dialog: { template: '<div v-if="visible"><slot /><slot name="footer" /></div>', props: ['visible'] }, Button: true } },
    })
    expect(wrapper.text()).toContain('cannot be undone')
  })

  it('shows custom warning text', () => {
    const wrapper = mount(DeleteDialog, {
      props: { entityName: 'Category', visible: true, warningText: 'All products in this category will be unlinked.' },
      global: { stubs: { Dialog: { template: '<div v-if="visible"><slot /><slot name="footer" /></div>', props: ['visible'] }, Button: true } },
    })
    expect(wrapper.text()).toContain('All products in this category will be unlinked.')
  })

  it('emits confirm when delete button clicked', async () => {
    const wrapper = mount(DeleteDialog, {
      props: { entityName: 'item', visible: true },
      global: { stubs: { Dialog: { template: '<div v-if="visible"><slot /><slot name="footer" /></div>', props: ['visible'] }, Button: { template: '<button @click="$emit(\'click\')">{{ label }}</button>', props: ['label'] } } },
    })

    const buttons = wrapper.findAll('button')
    const deleteBtn = buttons.find(b => b.text().includes('Delete'))
    expect(deleteBtn).toBeDefined()
    if (deleteBtn) {
      await deleteBtn.trigger('click')
      expect(wrapper.emitted('confirm')).toBeTruthy()
    }
  })

  it('emits cancel when cancel button clicked', async () => {
    const wrapper = mount(DeleteDialog, {
      props: { entityName: 'item', visible: true },
      global: { stubs: { Dialog: { template: '<div v-if="visible"><slot /><slot name="footer" /></div>', props: ['visible'] }, Button: { template: '<button @click="$emit(\'click\')">{{ label }}</button>', props: ['label'] } } },
    })

    const buttons = wrapper.findAll('button')
    const cancelBtn = buttons.find(b => b.text().includes('Cancel'))
    expect(cancelBtn).toBeDefined()
    if (cancelBtn) {
      await cancelBtn.trigger('click')
      expect(wrapper.emitted('cancel')).toBeTruthy()
    }
  })

  it('shows loading state on delete button', () => {
    const wrapper = mount(DeleteDialog, {
      props: { entityName: 'item', visible: true, loading: true },
      global: { stubs: { Dialog: { template: '<div v-if="visible"><slot /><slot name="footer" /></div>', props: ['visible'] }, Button: { template: '<span>{{ label }}</span>', props: ['label'] } } },
    })
    expect(wrapper.text()).toContain('Deleting')
  })
})
