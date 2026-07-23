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
import OptionTypeListPage from '../OptionTypeListPage.vue'

const mockGetOptionTypes = vi.fn<(...args: unknown[]) => unknown>()
const mockDeleteOptionType = vi.fn<(...args: unknown[]) => unknown>()

vi.mock('../../api', () => ({
  OptionTypeApi: {
    getMany: (...args: unknown[]) => mockGetOptionTypes(...args),
    delete: (...args: unknown[]) => mockDeleteOptionType(...args),
  },
}))

const router = createRouter({
  history: createWebHistory(),
  routes: [
    { path: '/', component: { template: '<div />' } },
    { path: '/catalog/option-types', name: ROUTE.OPTION_TYPES.LIST, component: { template: '<div />' } },
    { path: '/catalog/option-types/new', name: ROUTE.OPTION_TYPES.CREATE, component: { template: '<div />' } },
    { path: '/catalog/option-types/:id', name: ROUTE.OPTION_TYPES.VIEW, component: { template: '<div />' } },
    { path: '/catalog/option-types/:id/edit', name: ROUTE.OPTION_TYPES.EDIT, component: { template: '<div />' } },
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
      isSuccess: true,
      items: [],
      page: 1, pageSize: 20, totalCount: 0,
    })
    const wrapper = mount(OptionTypeListPage, {
      global: { plugins: createTestPlugins() },
    })
    await flushPromises()
    expect(wrapper.text()).toContain('Option Types')
  })

  it('displays empty state when no option types', async () => {
    mockGetOptionTypes.mockResolvedValue({
      isSuccess: true,
      items: [],
      page: 1, pageSize: 20, totalCount: 0,
    })
    const wrapper = mount(OptionTypeListPage, {
      global: { plugins: createTestPlugins() },
    })
    await flushPromises()
    expect(wrapper.text()).toContain('No option types')
  })

  it('displays option types in table when data exists', async () => {
    mockGetOptionTypes.mockResolvedValue({
      isSuccess: true,
      items: [
        { id: '1', name: 'Color', presentation: 'Color', position: 1, filterable: true, createdAt: '2026-01-01', updatedAt: '2026-01-01' },
      ],
      page: 1, pageSize: 20, totalCount: 1,
    })
    const wrapper = mount(OptionTypeListPage, {
      global: { plugins: createTestPlugins() },
    })
    await flushPromises()
    expect(wrapper.text()).toContain('Color')
  })

  it('displays error state on API failure', async () => {
    mockGetOptionTypes.mockResolvedValue({
      isSuccess: false,
      message: 'Server error', statusCode: 500, errors: [{ code: 'ERR', message: 'Server error' }],
    })
    const wrapper = mount(OptionTypeListPage, {
      global: { plugins: createTestPlugins() },
    })
    await flushPromises()
    expect(wrapper.text()).toContain('Server error')
  })

  it('has a create button', async () => {
    mockGetOptionTypes.mockResolvedValue({
      isSuccess: true,
      items: [],
      page: 1, pageSize: 20, totalCount: 0,
    })
    const wrapper = mount(OptionTypeListPage, {
      global: { plugins: createTestPlugins() },
    })
    await flushPromises()
    expect(wrapper.text()).toContain('Add Type')
  })
})
