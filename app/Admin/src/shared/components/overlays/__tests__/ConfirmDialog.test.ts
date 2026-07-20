import { describe, it, expect, vi } from 'vitest'
import { mount } from '@vue/test-utils'
import ConfirmDialog from '../ConfirmDialog.vue'
import ConfirmDialogPrime from 'primevue/confirmdialog'
import { useConfirm } from 'primevue/useconfirm'

vi.mock('primevue/useconfirm', () => ({
  useConfirm: vi.fn(() => ({
    require: vi.fn((opts: any) => {
      // last call will be used for assertions
      ;(useConfirm as any)._lastOpts = opts
      // call accept callback to simulate confirmation
      opts.accept?.()
    }),
  })),
}))
;(useConfirm as any)._lastOpts = null

describe('ConfirmDialog', () => {
  it('renders default slot as trigger', () => {
    const wrapper = mount(ConfirmDialog, {
      global: { stubs: { ConfirmDialogPrime: true, Button: { template: '<button><slot /></button>' } } },
      props: { header: 'Delete', message: 'Sure?' },
      slots: { default: '<button class="my-trigger">Delete</button>' },
    })
    expect(wrapper.html()).toContain('my-trigger')
  })

  it('emits confirm when accept clicked', async () => {
    const wrapper = mount(ConfirmDialog, {
      global: { stubs: { ConfirmDialogPrime: true, Button: { template: '<button><slot /></button>' } } },
      props: { header: 'Delete', message: 'Sure?' },
      slots: { default: '<button>X</button>' },
    })
    await wrapper.find('button').trigger('click')
    await wrapper.vm.$nextTick()
    expect((useConfirm as any)._lastOpts.header).toBe('Delete')
    expect(wrapper.emitted('confirm')).toBeTruthy()
  })

  it('uses default severity and icon when not provided', () => {
    const wrapper = mount(ConfirmDialog, {
      global: { stubs: { ConfirmDialogPrime: true, Button: { template: '<button><slot /></button>' } } },
      props: { header: 'Title', message: 'Msg' },
      slots: { default: '<button>X</button>' },
    })
    // defaults: severity='danger', icon='pi pi-trash', acceptLabel='Confirm'
    expect(wrapper.props('severity')).toBe('danger')
  })
})
