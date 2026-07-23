import { describe, it, expect, vi, beforeEach, beforeAll } from 'vitest'

beforeAll(() => {
  Object.defineProperty(window, 'matchMedia', {
    writable: true,
    value: (query: string) => ({
      matches: false,
      media: query,
      onchange: null,
      addListener: () => {},
      removeListener: () => {},
      addEventListener: () => {},
      removeEventListener: () => {},
      dispatchEvent: () => false,
    }),
  })
})
import { mount, flushPromises } from '@vue/test-utils'
import { createRouter, createWebHistory } from 'vue-router'
import PrimeVue from 'primevue/config'
import ConfirmationService from 'primevue/confirmationservice'
import ToastService from 'primevue/toastservice'
import { ROUTE_CATALOG } from '../../routers/route-names'
import ProductDetailPage from '../ProductDetailPage.vue'

const mockGet = vi.fn<(...args: unknown[]) => unknown>()
const mockCreate = vi.fn<(...args: unknown[]) => unknown>()
const mockUpdate = vi.fn<(...args: unknown[]) => unknown>()

vi.mock('../../api/products', () => ({
  getProduct: (...args: unknown[]) => mockGet(...args),
  createProduct: (...args: unknown[]) => mockCreate(...args),
  updateProduct: (...args: unknown[]) => mockUpdate(...args),
}))

function makeRouter(_initialRoute: string) {
  return createRouter({
    history: createWebHistory(),
    routes: [
      { path: '/catalog/products/new', name: ROUTE_CATALOG.PRODUCTS.CREATE, component: ProductDetailPage },
      { path: '/catalog/products/:id', name: ROUTE_CATALOG.PRODUCTS.VIEW, component: ProductDetailPage },
      { path: '/catalog/products/:id/edit', name: ROUTE_CATALOG.PRODUCTS.EDIT, component: ProductDetailPage },
      { path: '/catalog/products', name: ROUTE_CATALOG.PRODUCTS.LIST, component: { template: '<div />' } },
    ],
  })
}

describe('ProductDetailPage', () => {
  beforeEach(() => { vi.clearAllMocks() })

  it('renders in create mode when no :id param', async () => {
    const router = makeRouter('/catalog/products/new')
    router.push('/catalog/products/new')
    await router.isReady()
    const wrapper = mount(ProductDetailPage, { global: { plugins: [PrimeVue, ConfirmationService, ToastService, router] } })
    await flushPromises()
    expect(wrapper.text()).toContain('Create Product')
  })

  it('renders in view mode when :id present and route not .edit', async () => {
    mockGet.mockResolvedValue({
      success: true,
      data: { id: 'abc', name: 'Test Product', slug: 'test', status: 'Draft', department: null, createdAt: '', updatedAt: '' },
    })
    const router = makeRouter('/catalog/products/abc')
    router.push('/catalog/products/abc')
    await router.isReady()
    const wrapper = mount(ProductDetailPage, { global: { plugins: [PrimeVue, ConfirmationService, ToastService, router] } })
    await flushPromises()
    expect(wrapper.text()).toContain('Test Product')
  })

  it('renders in edit mode when route ends with .edit', async () => {
    mockGet.mockResolvedValue({
      success: true,
      data: { id: 'abc', name: 'Edit Me', slug: 'edit-me', status: 'Draft', department: null, createdAt: '', updatedAt: '' },
    })
    const router = makeRouter('/catalog/products/abc/edit')
    router.push('/catalog/products/abc/edit')
    await router.isReady()
    const wrapper = mount(ProductDetailPage, { global: { plugins: [PrimeVue, ConfirmationService, ToastService, router] } })
    await flushPromises()
    expect(wrapper.text()).toContain('Edit: Edit Me')
  })

  it('displays error state on load failure', async () => {
    mockGet.mockResolvedValue({ success: false, error: { message: 'Not found', statusCode: 404, title: '', detail: null, errors: {}, errorCode: 'ERR' } })
    const router = makeRouter('/catalog/products/missing')
    router.push('/catalog/products/missing')
    await router.isReady()
    const wrapper = mount(ProductDetailPage, { global: { plugins: [PrimeVue, ConfirmationService, ToastService, router] } })
    await flushPromises()
    expect(wrapper.text()).toContain('Not found')
  })

  it('shows save and cancel buttons in edit mode', async () => {
    mockGet.mockResolvedValue({
      success: true,
      data: { id: 'abc', name: 'Test', slug: 'test', status: 'Draft', department: null, createdAt: '', updatedAt: '' },
    })
    const router = makeRouter('/catalog/products/abc/edit')
    router.push('/catalog/products/abc/edit')
    await router.isReady()
    const wrapper = mount(ProductDetailPage, { global: { plugins: [PrimeVue, ConfirmationService, ToastService, router] } })
    await flushPromises()
    expect(wrapper.text()).toContain('Save Changes')
    expect(wrapper.text()).toContain('Cancel')
  })
})
