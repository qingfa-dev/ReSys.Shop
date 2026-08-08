import { describe, it, expect, vi, beforeEach } from 'vitest'
import { mount } from '@vue/test-utils'
import { createRouter, createMemoryHistory } from 'vue-router'
import CartDrawer from '../CartDrawer.vue'
import { useCartStore } from '../../stores/cartStore'

vi.mock('../../stores/cartStore', () => ({
  useCartStore: vi.fn(),
}))

const stubs = {
  Button: { template: '<button><slot /><template v-if="label">{{ label }}</template></button>', props: ['icon', 'label', 'text', 'rounded', 'size', 'as', 'to'] },
  Skeleton: { template: '<div class="skeleton" />', props: ['height'] },
  Teleport: { template: '<slot />' },
}

function createTestRouter() {
  return createRouter({
    history: createMemoryHistory(),
    routes: [
      { path: '/', component: { template: '<div />' } },
      { path: '/shop', component: { template: '<div />' } },
      { path: '/checkout', component: { template: '<div />' } },
    ],
  })
}

describe('CartDrawer', () => {
  beforeEach(() => {
    vi.clearAllMocks()
    vi.mocked(useCartStore).mockReturnValue({
      itemCount: 0,
      items: [],
      loading: false,
      isEmpty: true,
      subtotal: 0,
      fetchCart: vi.fn(),
      updateQuantity: vi.fn(),
    } as never)
  })

  it('shows Cart text when visible', async () => {
    const router = createTestRouter()
    await router.push('/')
    await router.isReady()

    const wrapper = mount(CartDrawer, {
      props: { visible: true },
      global: { plugins: [router], stubs },
    })

    expect(wrapper.text()).toContain('Cart')
  })

  it('shows empty state when cart is empty', async () => {
    const router = createTestRouter()
    await router.push('/')
    await router.isReady()

    const wrapper = mount(CartDrawer, {
      props: { visible: true },
      global: { plugins: [router], stubs },
    })

    expect(wrapper.text()).toContain('Your cart is empty')
  })

  it('does not render when visible is false', async () => {
    const router = createTestRouter()
    await router.push('/')
    await router.isReady()

    const wrapper = mount(CartDrawer, {
      props: { visible: false },
      global: { plugins: [router], stubs },
    })

    expect(wrapper.text()).not.toContain('Cart')
    expect(wrapper.text()).not.toContain('Your cart is empty')
  })
})
