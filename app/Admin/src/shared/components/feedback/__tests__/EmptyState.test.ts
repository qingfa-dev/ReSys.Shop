import { describe, it, expect, vi } from 'vitest'
import { mount } from '@vue/test-utils'
import EmptyState from '../EmptyState.vue'

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

  it('renders action button when actionLabel and actionRoute provided', () => {
    const wrapper = mount(EmptyState, {
      props: { title: 'Empty', actionLabel: 'Add', actionRoute: '/new' },
      global: { stubs: { Button: true } },
    })
    expect(wrapper.findComponent({ name: 'Button' }).exists()).toBe(true)
  })

  it('does not render action button without actionRoute', () => {
    const wrapper = mount(EmptyState, {
      props: { title: 'Empty', actionLabel: 'Add' },
      global: { stubs: { Button: true } },
    })
    expect(wrapper.findComponent({ name: 'Button' }).exists()).toBe(false)
  })
})
