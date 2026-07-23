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
import { ROUTE } from '../../routes'
import ProductListPage from '../ProductListPage.vue'

const mockGetProducts = vi.fn<(...args: unknown[]) => unknown>()
const mockDeleteProduct = vi.fn<(...args: unknown[]) => unknown>()

vi.mock('../../api', () => ({
  ProductApi: {
    getMany: (...args: unknown[]) => mockGetProducts(...args),
    delete: (...args: unknown[]) => mockDeleteProduct(...args),
  },
}))

const router = createRouter({
  history: createWebHistory(),
  routes: [
    { path: '/', component: { template: '<div />' } },
    { path: '/catalog/products', name: ROUTE.PRODUCTS.LIST, component: { template: '<div />' } },
    { path: '/catalog/products/new', name: ROUTE.PRODUCTS.CREATE, component: { template: '<div />' } },
    { path: '/catalog/products/:id', name: ROUTE.PRODUCTS.VIEW, component: { template: '<div />' } },
    { path: '/catalog/products/:id/edit', name: ROUTE.PRODUCTS.EDIT, component: { template: '<div />' } },
  ],
})

function createTestPlugins() {
  return [PrimeVue, ConfirmationService, ToastService, router]
}

describe('ProductListPage', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  it('renders page header', async () => {
    mockGetProducts.mockResolvedValue({
      isSuccess: true,
      items: [],
      page: 1, pageSize: 20, totalCount: 0,
    })
    const wrapper = mount(ProductListPage, {
      global: { plugins: createTestPlugins() },
    })
    await flushPromises()
    expect(wrapper.text()).toContain('Products')
  })

  it('displays empty state when no products', async () => {
    mockGetProducts.mockResolvedValue({
      isSuccess: true,
      items: [],
      page: 1, pageSize: 20, totalCount: 0,
    })
    const wrapper = mount(ProductListPage, {
      global: { plugins: createTestPlugins() },
    })
    await flushPromises()
    expect(wrapper.text()).toContain('No products')
  })

  it('displays products in table when data exists', async () => {
    mockGetProducts.mockResolvedValue({
      isSuccess: true,
      items: [
        { id: '1', name: 'Test Product', slug: 'test', status: 'Draft', department: null, createdAt: '2026-01-01', updatedAt: '2026-01-01' },
      ],
      page: 1, pageSize: 20, totalCount: 1,
    })
    const wrapper = mount(ProductListPage, {
      global: { plugins: createTestPlugins() },
    })
    await flushPromises()
    expect(wrapper.text()).toContain('Test Product')
  })

  it('displays error state on API failure', async () => {
    mockGetProducts.mockResolvedValue({
      isSuccess: false,
      message: 'Server error', statusCode: 500, errors: [{ code: 'ERR', message: 'Server error' }],
    })
    const wrapper = mount(ProductListPage, {
      global: { plugins: createTestPlugins() },
    })
    await flushPromises()
    expect(wrapper.text()).toContain('Server error')
  })

  it('has a create button', async () => {
    mockGetProducts.mockResolvedValue({
      isSuccess: true,
      items: [],
      page: 1, pageSize: 20, totalCount: 0,
    })
    const wrapper = mount(ProductListPage, {
      global: { plugins: createTestPlugins() },
    })
    await flushPromises()
    expect(wrapper.text()).toContain('Add Product')
  })
})
