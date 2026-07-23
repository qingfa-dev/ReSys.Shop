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
import OptionTypeDetailPage from '../OptionTypeDetailPage.vue'

const mockGetOptionType = vi.fn<(...args: unknown[]) => unknown>()
const mockCreateOptionType = vi.fn<(...args: unknown[]) => unknown>()
const mockUpdateOptionType = vi.fn<(...args: unknown[]) => unknown>()
const mockGetOptionValues = vi.fn<(...args: unknown[]) => unknown>()
const mockCreateOptionValue = vi.fn<(...args: unknown[]) => unknown>()
const mockUpdateOptionValue = vi.fn<(...args: unknown[]) => unknown>()
const mockDeleteOptionValue = vi.fn<(...args: unknown[]) => unknown>()

vi.mock('vue-i18n', () => ({
  useI18n: () => ({
    t: (key: string) => {
      const map: Record<string, string> = {
        'catalog.option_types.titles.create': 'Create Option Type',
        'catalog.option_types.actions.edit': 'Edit',
        'catalog.option_types.titles.edit': 'Edit Option Type',
        'catalog.option_types.actions.save_create': 'Create Option Type',
        'catalog.option_types.actions.save_edit': 'Update Option Type',
        'catalog.option_types.actions.cancel': 'Cancel',
        'catalog.option_types.labels.name': 'Internal Name',
        'catalog.option_types.labels.presentation': 'Display Name',
        'catalog.option_types.labels.filterable': 'Filterable',
        'catalog.option_types.descriptions.values': 'Add and manage specific values for this option type.',
        'catalog.option_values.titles.list': 'Option Values',
        'catalog.option_values.titles.create': 'Create Option Value',
        'catalog.option_values.titles.edit': 'Edit Option Value',
        'catalog.option_values.actions.add_value': 'Add Option Value',
        'catalog.option_values.actions.cancel': 'Cancel',
        'catalog.option_values.messages.update_success': 'Option value updated',
        'catalog.option_values.messages.create_success': 'Option value created',
        'catalog.option_values.messages.delete_success': 'Option value deleted',
        'catalog.option_values.messages.empty_list': 'No option values',
        'catalog.option_values.labels.name': 'Value Name',
        'catalog.option_values.labels.presentation': 'Display Label',
        'catalog.option_values.labels.position': 'Position',
      }
      return map[key] ?? key
    },
  }),
}))
vi.mock('../../api', () => ({
  OptionTypeApi: {
    get: (...args: unknown[]) => mockGetOptionType(...args),
    create: (...args: unknown[]) => mockCreateOptionType(...args),
    update: (...args: unknown[]) => mockUpdateOptionType(...args),
    getValues: (...args: unknown[]) => mockGetOptionValues(...args),
    createValue: (...args: unknown[]) => mockCreateOptionValue(...args),
    updateValue: (...args: unknown[]) => mockUpdateOptionValue(...args),
    deleteValue: (...args: unknown[]) => mockDeleteOptionValue(...args),
  },
}))

function makeRouter(initialRoute: string) {
  const router = createRouter({
    history: createWebHistory(),
    routes: [
      { path: '/catalog/option-types/new', name: ROUTE.OPTION_TYPES.CREATE, component: OptionTypeDetailPage },
      { path: '/catalog/option-types/:id', name: ROUTE.OPTION_TYPES.VIEW, component: OptionTypeDetailPage },
      { path: '/catalog/option-types/:id/edit', name: ROUTE.OPTION_TYPES.EDIT, component: OptionTypeDetailPage },
      { path: '/catalog/option-types', name: ROUTE.OPTION_TYPES.LIST, component: { template: '<div />' } },
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
      isSuccess: true, value: { id: 'abc', name: 'Color', presentation: 'Color Family', position: 1, filterable: true, createdAt: '', updatedAt: '' },
    })
    mockGetOptionValues.mockResolvedValue({
      isSuccess: true, value: [
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
      isSuccess: true, value: { id: 'abc', name: 'Size', presentation: null, position: 0, filterable: false, createdAt: '', updatedAt: '' },
    })
    mockGetOptionValues.mockResolvedValue({ isSuccess: true, value: [] })
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
      isSuccess: true, value: { id: 'abc', name: 'Empty', presentation: null, position: 0, filterable: false, createdAt: '', updatedAt: '' },
    })
    mockGetOptionValues.mockResolvedValue({ isSuccess: true, value: [] })
    const router = makeRouter('/catalog/option-types/abc')
    await router.isReady()
    const wrapper = mount(OptionTypeDetailPage, { global: { plugins: [...plugins, router] } })
    await flushPromises()
    await flushPromises()
    expect(wrapper.text()).toContain('No option values')
  })
})
