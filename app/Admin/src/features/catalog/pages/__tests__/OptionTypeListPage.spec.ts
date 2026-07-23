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
import OptionTypeListPage from '../OptionTypeListPage.vue'

const mockGetOptionTypes = vi.fn<(...args: unknown[]) => unknown>()
const mockDeleteOptionType = vi.fn<(...args: unknown[]) => unknown>()

vi.mock('../../api/optionTypes', () => ({
  getOptionTypes: (...args: unknown[]) => mockGetOptionTypes(...args),
  deleteOptionType: (...args: unknown[]) => mockDeleteOptionType(...args),
}))

const router = createRouter({
  history: createWebHistory(),
  routes: [
    { path: '/', component: { template: '<div />' } },
    { path: '/catalog/option-types', name: ROUTE_CATALOG.OPTION_TYPES.LIST, component: { template: '<div />' } },
    { path: '/catalog/option-types/new', name: ROUTE_CATALOG.OPTION_TYPES.CREATE, component: { template: '<div />' } },
    { path: '/catalog/option-types/:id', name: ROUTE_CATALOG.OPTION_TYPES.VIEW, component: { template: '<div />' } },
    { path: '/catalog/option-types/:id/edit', name: ROUTE_CATALOG.OPTION_TYPES.EDIT, component: { template: '<div />' } },
  ],
})

function createTestPlugins() {
  return [PrimeVue, ConfirmationService, ToastService, router]
}

describe('OptionTypeListPage', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  it('renders page header', async () => {
    mockGetOptionTypes.mockResolvedValue({
      success: true,
      data: [],
      meta: { page: 1, pageSize: 20, totalCount: 0, totalPages: 0 },
    })
    const wrapper = mount(OptionTypeListPage, {
      global: { plugins: createTestPlugins() },
    })
    await flushPromises()
    expect(wrapper.text()).toContain('Option Types')
  })

  it('displays empty state when no option types', async () => {
    mockGetOptionTypes.mockResolvedValue({
      success: true,
      data: [],
      meta: { page: 1, pageSize: 20, totalCount: 0, totalPages: 0 },
    })
    const wrapper = mount(OptionTypeListPage, {
      global: { plugins: createTestPlugins() },
    })
    await flushPromises()
    expect(wrapper.text()).toContain('No option types')
  })

  it('displays option types in table when data exists', async () => {
    mockGetOptionTypes.mockResolvedValue({
      success: true,
      data: [
        { id: '1', name: 'Color', presentation: 'Color', position: 1, filterable: true, createdAt: '2026-01-01', updatedAt: '2026-01-01' },
      ],
      meta: { page: 1, pageSize: 20, totalCount: 1, totalPages: 1 },
    })
    const wrapper = mount(OptionTypeListPage, {
      global: { plugins: createTestPlugins() },
    })
    await flushPromises()
    expect(wrapper.text()).toContain('Color')
  })

  it('displays error state on API failure', async () => {
    mockGetOptionTypes.mockResolvedValue({
      success: false,
      error: { message: 'Server error', statusCode: 500, title: 'Error', detail: null, errors: {}, errorCode: 'ERR' },
    })
    const wrapper = mount(OptionTypeListPage, {
      global: { plugins: createTestPlugins() },
    })
    await flushPromises()
    expect(wrapper.text()).toContain('Server error')
  })

  it('has a create button', async () => {
    mockGetOptionTypes.mockResolvedValue({
      success: true,
      data: [],
      meta: { page: 1, pageSize: 20, totalCount: 0, totalPages: 0 },
    })
    const wrapper = mount(OptionTypeListPage, {
      global: { plugins: createTestPlugins() },
    })
    await flushPromises()
    expect(wrapper.text()).toContain('Add Type')
  })
})
