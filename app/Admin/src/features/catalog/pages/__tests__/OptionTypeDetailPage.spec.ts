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
import OptionTypeDetailPage from '../OptionTypeDetailPage.vue'

const mockGetOptionType = vi.fn<(...args: unknown[]) => unknown>()
const mockGetOptionValues = vi.fn<(...args: unknown[]) => unknown>()

vi.mock('../../api/optionTypes', () => ({
  getOptionType: (...args: unknown[]) => mockGetOptionType(...args),
  createOptionType: vi.fn<(...args: unknown[]) => unknown>(),
  updateOptionType: vi.fn<(...args: unknown[]) => unknown>(),
  getOptionValues: (...args: unknown[]) => mockGetOptionValues(...args),
  createOptionValue: vi.fn<(...args: unknown[]) => unknown>(),
  updateOptionValue: vi.fn<(...args: unknown[]) => unknown>(),
  deleteOptionValue: vi.fn<(...args: unknown[]) => unknown>(),
}))

function makeRouter(initialRoute: string) {
  const router = createRouter({
    history: createWebHistory(),
    routes: [
      { path: '/catalog/option-types/new', name: ROUTE_CATALOG.OPTION_TYPES.CREATE, component: OptionTypeDetailPage },
      { path: '/catalog/option-types/:id', name: ROUTE_CATALOG.OPTION_TYPES.VIEW, component: OptionTypeDetailPage },
      { path: '/catalog/option-types/:id/edit', name: ROUTE_CATALOG.OPTION_TYPES.EDIT, component: OptionTypeDetailPage },
      { path: '/catalog/option-types', name: ROUTE_CATALOG.OPTION_TYPES.LIST, component: { template: '<div />' } },
    ],
  })
  router.push(initialRoute)
  return router
}

const plugins = [PrimeVue, ConfirmationService, ToastService]

describe('OptionTypeDetailPage', () => {
  beforeEach(() => { vi.clearAllMocks() })

  it('renders in view mode and loads option values', async () => {
    mockGetOptionType.mockResolvedValue({
      success: true, data: { id: 'abc', name: 'Color', presentation: 'Color Family', position: 1, filterable: true, createdAt: '', updatedAt: '' },
    })
    mockGetOptionValues.mockResolvedValue({
      success: true, data: [
        { id: '1', name: 'Red', presentation: 'Red', position: 1, optionTypeId: 'abc' },
        { id: '2', name: 'Blue', presentation: 'Blue', position: 2, optionTypeId: 'abc' },
      ],
    })
    const router = makeRouter('/catalog/option-types/abc')
    await router.isReady()
    const wrapper = mount(OptionTypeDetailPage, { global: { plugins: [...plugins, router] } })
    await flushPromises()
    await flushPromises()
    expect(wrapper.text()).toContain('Color')
    expect(wrapper.text()).toContain('Option Values')
    expect(wrapper.text()).toContain('Red')
    expect(wrapper.text()).toContain('Blue')
  })

  it('shows Add Option Value button', async () => {
    mockGetOptionType.mockResolvedValue({
      success: true, data: { id: 'abc', name: 'Size', presentation: null, position: 0, filterable: false, createdAt: '', updatedAt: '' },
    })
    mockGetOptionValues.mockResolvedValue({ success: true, data: [] })
    const router = makeRouter('/catalog/option-types/abc')
    await router.isReady()
    const wrapper = mount(OptionTypeDetailPage, { global: { plugins: [...plugins, router] } })
    await flushPromises()
    await flushPromises()
    expect(wrapper.text()).toContain('Add Option Value')
  })

  it('renders in create mode', async () => {
    const router = makeRouter('/catalog/option-types/new')
    await router.isReady()
    const wrapper = mount(OptionTypeDetailPage, { global: { plugins: [...plugins, router] } })
    await flushPromises()
    expect(wrapper.text()).toContain('Create Option Type')
  })

  it('shows empty state for option values', async () => {
    mockGetOptionType.mockResolvedValue({
      success: true, data: { id: 'abc', name: 'Empty', presentation: null, position: 0, filterable: false, createdAt: '', updatedAt: '' },
    })
    mockGetOptionValues.mockResolvedValue({ success: true, data: [] })
    const router = makeRouter('/catalog/option-types/abc')
    await router.isReady()
    const wrapper = mount(OptionTypeDetailPage, { global: { plugins: [...plugins, router] } })
    await flushPromises()
    await flushPromises()
    expect(wrapper.text()).toContain('No option values')
  })
})
