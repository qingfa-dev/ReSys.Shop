import { describe, it, expect, vi } from 'vitest'
import { mount } from '@vue/test-utils'
import EmptyState from '../feedback/EmptyState.vue'

const pushMock = vi.fn()

vi.mock('vue-router', () => ({
  useRouter: () => ({ push: pushMock }),
}))

describe('EmptyState', () => {
  it('renders title', () => {
    const wrapper = mount(EmptyState, {
      props: { title: 'No items found' },
      global: { stubs: { Button: true } },
    })
    expect(wrapper.text()).toContain('No items found')
  })

  it('renders default icon', () => {
    const wrapper = mount(EmptyState, {
      props: { title: 'Empty' },
      global: { stubs: { Button: true } },
    })
    expect(wrapper.find('i.pi-inbox').exists()).toBe(true)
  })

  it('renders custom icon', () => {
    const wrapper = mount(EmptyState, {
      props: { title: 'Empty', icon: 'pi pi-search' },
      global: { stubs: { Button: true } },
    })
    expect(wrapper.find('i.pi-search').exists()).toBe(true)
  })

  it('renders description when provided', () => {
    const wrapper = mount(EmptyState, {
      props: { title: 'Empty', description: 'Try adding a new item' },
      global: { stubs: { Button: true } },
    })
    expect(wrapper.text()).toContain('Try adding a new item')
  })

  it('renders action button when actionLabel and actionTo provided', () => {
    const wrapper = mount(EmptyState, {
      props: { title: 'Empty', actionLabel: 'Add', actionTo: '/new' },
      global: { stubs: { RouterLink: { template: '<a><slot /></a>' }, Button: true } },
    })
    expect(wrapper.findComponent({ name: 'Button' }).exists()).toBe(true)
  })

  it('emits action when button clicked and no actionTo', async () => {
    const wrapper = mount(EmptyState, {
      props: { title: 'Empty', actionLabel: 'Add' },
      global: { stubs: { Button: { template: '<button @click="$emit(\'click\')"><slot /></button>' } } },
    })
    await wrapper.find('button').trigger('click')
    expect(wrapper.emitted('action')).toBeTruthy()
  })

  it('does not render action button without actionLabel', () => {
    const wrapper = mount(EmptyState, {
      props: { title: 'Empty' },
      global: { stubs: { Button: true } },
    })
    expect(wrapper.findComponent({ name: 'Button' }).exists()).toBe(false)
  })
})
