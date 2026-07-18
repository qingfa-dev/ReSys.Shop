import { describe, it, expect, vi, beforeEach } from 'vitest'
import { mount } from '@vue/test-utils'

const requireMock = vi.fn()

vi.mock('primevue/useconfirm', () => ({
  useConfirm: () => ({ require: requireMock }),
}))

import ConfirmButton from '../ConfirmButton.Component.vue'

describe('ConfirmButton', () => {
  beforeEach(() => {
    requireMock.mockClear()
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
    expect(requireMock).toHaveBeenCalledOnce()
  })

  it('passes message and header to confirm.require', async () => {
    const wrapper = mount(ConfirmButton, {
      props: { header: 'Delete item', message: 'This action cannot be undone.' },
      global: { stubs: { Button: { template: '<button><slot /></button>' } } },
    })
    await wrapper.find('button').trigger('click')
    expect(requireMock).toHaveBeenCalledWith(
      expect.objectContaining({
        header: 'Delete item',
        message: 'This action cannot be undone.',
      }),
    )
  })

  it('emits confirm when accept callback fires', async () => {
    const wrapper = mount(ConfirmButton, {
      props: { header: 'Delete', message: 'Sure?' },
      global: { stubs: { Button: { template: '<button><slot /></button>' } } },
    })

    await wrapper.find('button').trigger('click')

    const callArgs = requireMock.mock.calls[0]?.[0] as Record<string, unknown>
    const acceptFn = callArgs?.accept as () => void
    expect(acceptFn).toBeDefined()
  })
})
