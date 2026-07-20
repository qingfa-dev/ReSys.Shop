import { describe, it, expect, vi } from 'vitest'
import { mount } from '@vue/test-utils'
import PageHeader from '../layout/PageHeader.vue'

const pushMock = vi.fn()
const backMock = vi.fn()

vi.mock('vue-router', () => ({
  useRouter: () => ({ push: pushMock, back: backMock }),
}))

describe('PageHeader', () => {
  it('renders title', () => {
    const wrapper = mount(PageHeader, {
      props: { title: 'Products' },
      global: { stubs: { Button: true } },
    })
    expect(wrapper.text()).toContain('Products')
  })

  it('renders description', () => {
    const wrapper = mount(PageHeader, {
      props: { title: 'Products', description: 'Manage your catalog' },
      global: { stubs: { Button: true } },
    })
    expect(wrapper.text()).toContain('Manage your catalog')
  })

  it('renders actions slot', () => {
    const wrapper = mount(PageHeader, {
      props: { title: 'Products' },
      slots: { actions: '<button class="add-btn">Add New</button>' },
      global: { stubs: { Button: true } },
    })
    expect(wrapper.find('.add-btn').exists()).toBe(true)
    expect(wrapper.find('.add-btn').text()).toBe('Add New')
  })

  it('renders back button when backTo is set', () => {
    const wrapper = mount(PageHeader, {
      props: { title: 'Detail', backTo: '/list' },
      global: { stubs: { Button: true } },
    })
    const backBtn = wrapper.findComponent({ name: 'Button' })
    expect(backBtn.exists()).toBe(true)
  })

  it('navigates to backTo when back button clicked', async () => {
    const wrapper = mount(PageHeader, {
      props: { title: 'Detail', backTo: '/products' },
      global: { stubs: { Button: { template: '<button @click="$emit(\'click\')"><slot /></button>' } } },
    })
    await wrapper.find('button').trigger('click')
    expect(pushMock).toHaveBeenCalledWith('/products')
  })

  it('calls router.back when backTo is empty string and back button clicked', async () => {
    const wrapper = mount(PageHeader, {
      props: { title: 'Detail', backTo: '' },
      global: { stubs: { Button: { template: '<button @click="$emit(\'click\')"><slot /></button>' } } },
    })
    await wrapper.find('button').trigger('click')
    expect(backMock).toHaveBeenCalled()
  })

  it('renders default slot content below title', () => {
    const wrapper = mount(PageHeader, {
      props: { title: 'Order' },
      slots: { default: '<span class="badge">#1234</span>' },
      global: { stubs: { Button: true } },
    })
    expect(wrapper.find('.badge').exists()).toBe(true)
  })

  it('does not render back button when backTo is undefined', () => {
    const wrapper = mount(PageHeader, {
      props: { title: 'Dashboard' },
      global: { stubs: { Button: true } },
    })
    expect(wrapper.findComponent({ name: 'Button' }).exists()).toBe(false)
  })
})
