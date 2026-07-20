import { describe, it, expect, vi, beforeEach } from 'vitest'
import { mount } from '@vue/test-utils'
import PrimeVue from 'primevue/config'
import { useConfirm } from 'primevue/useconfirm'

vi.mock('primevue/useconfirm')

import ConfirmButton from '../ConfirmDialog.vue'

describe('ConfirmDialog', () => {
  beforeEach(() => {
    vi.clearAllMocks()
    vi.mocked(useConfirm).mockReturnValue({
      require: vi.fn(),
    } as any)
  })

  it('renders trigger button', () => {
    const wrapper = mount(ConfirmButton, {
      props: { header: 'Confirm', message: 'Are you sure?' },
      global: { stubs: { Button: true } },
    })
    expect(wrapper.findComponent({ name: 'Button' }).exists()).toBe(true)
  })

  it('calls confirm.require on click', async () => {
    const wrapper = mount(ConfirmButton, {
      props: { header: 'Delete', message: 'Sure?' },
      global: { stubs: { Button: { template: '<button><slot /></button>' } } },
    })
    await wrapper.find('button').trigger('click')
    expect(vi.mocked(useConfirm)().require).toHaveBeenCalledOnce()
  })

  it('passes message and header to confirm.require', async () => {
    const wrapper = mount(ConfirmButton, {
      props: { header: 'Delete item', message: 'This action cannot be undone.' },
      global: { stubs: { Button: { template: '<button><slot /></button>' } } },
    })
    await wrapper.find('button').trigger('click')
    expect(vi.mocked(useConfirm)().require).toHaveBeenCalledWith(
      expect.objectContaining({
        header: 'Delete item',
        message: 'This action cannot be undone.',
      }),
    )
  })

  it('emits confirm when accept callback fires', async () => {
    let acceptFn: (() => void) | null = null
    vi.mocked(useConfirm).mockReturnValue({
      require: vi.fn((opts: any) => {
        acceptFn = opts.accept
      }),
    } as any)

    const wrapper = mount(ConfirmButton, {
      props: { header: 'Delete', message: 'Are you sure?', severity: 'danger' },
      global: { plugins: [PrimeVue] },
    })

    await wrapper.find('button').trigger('click')
    expect(acceptFn).toBeDefined()

    acceptFn!()

    expect(wrapper.emitted('confirm')).toBeTruthy()
  })
})
