import { describe, it, expect, vi, beforeEach } from 'vitest'
import { mount, flushPromises } from '@vue/test-utils'
import { createTestingPinia } from '@pinia/testing'
import ShippingRateForm from '../ShippingRateForm.vue'

vi.mock('vee-validate', async (importOriginal) => {
  const actual = await importOriginal<typeof import('vee-validate')>()
  return {
    ...actual,
    useForm: (opts?: Record<string, unknown>) => {
      const result = actual.useForm(opts ?? {})
      return {
        ...result,
        handleSubmit: ((cb: any) => {
          return () => cb(result.values)
        }) as typeof result.handleSubmit,
      }
    },
  }
})

const mockApiGet = vi.hoisted(() => vi.fn())
const mockApiCreate = vi.hoisted(() => vi.fn())
const mockApiUpdate = vi.hoisted(() => vi.fn())

vi.mock('../../api', () => ({
  ShippingRateApi: {
    get: mockApiGet,
    create: mockApiCreate,
    update: mockApiUpdate,
  },
}))

const mockRoute: { params: Record<string, string>; name: string; meta: Record<string, unknown> } = vi.hoisted(() => ({
  params: {} as Record<string, string>,
  name: 'shipping.rates.create',
  meta: {} as Record<string, unknown>,
}))

vi.mock('vue-router', () => ({
  useRoute: () => mockRoute,
  useRouter: () => ({
    push: vi.fn(),
    replace: vi.fn(),
  }),
}))

vi.mock('vue-i18n', () => ({
  useI18n: () => ({
    t: (key: string) => key,
  }),
}))

vi.mock('@/shared/composables/useToast', () => ({
  useToast: () => ({
    success: vi.fn(),
    error: vi.fn(),
    warn: vi.fn(),
    info: vi.fn(),
    showToast: vi.fn(),
  }),
}))

vi.mock('primevue/usetoast', () => ({
  useToast: () => ({ add: vi.fn() }),
}))

const PageHeaderStub = {
  template: '<div class="page-header-stub"><slot /><slot name="actions" /></div>',
  props: ['title', 'subtitle', 'icon'],
}

const FormFieldStub = {
  template: '<div class="form-field-stub"><slot /></div>',
  props: ['label', 'hint', 'error', 'required', 'name'],
}

const FormActionsStub = {
  template: '<div class="form-actions-stub"><button class="save-btn" :disabled="loading" type="button" @click="$emit(\'save\')">{{ saveLabel }}</button><button class="cancel-btn" :disabled="loading" type="button" @click="$emit(\'cancel\')">{{ cancelLabel }}</button></div>',
  props: ['saveLabel', 'cancelLabel', 'loading'],
  emits: ['save', 'cancel'],
}

const AppCardStub = {
  template: '<div class="app-card-stub"><slot /></div>',
}

const LoadingSkeletonStub = {
  template: '<div class="loading-skeleton-stub" />',
  props: ['rows', 'columns'],
}

const ErrorStateStub = {
  template: '<div class="error-state-stub"><span class="error-title">{{ title }}</span><button class="retry-btn" type="button" @click="$emit(\'retry\')">Retry</button></div>',
  props: ['title', 'description', 'retryable'],
  emits: ['retry'],
}

const ButtonStub = {
  template: '<button :disabled="disabled" type="button" @click="$emit(\'click\')">{{ label }}<slot /></button>',
  props: ['label', 'icon', 'severity', 'size', 'loading', 'disabled', 'outlined', 'text'],
  emits: ['click'],
}

function mountForm() {
  return mount(ShippingRateForm, {
    global: {
      plugins: [createTestingPinia({ stubActions: false, createSpy: vi.fn })],
      stubs: {
        PageHeader: PageHeaderStub,
        FormField: FormFieldStub,
        FormActions: FormActionsStub,
        AppCard: AppCardStub,
        LoadingSkeleton: LoadingSkeletonStub,
        ErrorState: ErrorStateStub,
        Button: ButtonStub,
        Select: { template: '<select class="p-select-stub"><slot /></select>', props: ['options', 'optionValue', 'optionLabel', 'loading', 'disabled', 'placeholder', 'modelValue'] },
      },
    },
  })
}

function successResult(value: unknown) {
  return { isSuccess: true, statusCode: 200, value, errors: [], message: null, metadata: null }
}

function errorResult(message?: string) {
  return { isSuccess: false, statusCode: 400, value: null, errors: [], message: message ?? 'Failed', metadata: null }
}

const mockShippingRate = {
  id: 'sr1',
  name: 'Standard Domestic',
  shippingMethodId: 'sm1',
  rate: 9.99,
  currency: 'USD',
  minOrderAmount: 0,
  maxOrderAmount: 100,
  minWeight: 0,
  maxWeight: 10,
}

beforeEach(() => {
  vi.clearAllMocks()
  mockRoute.params = {}
  mockRoute.name = 'shipping.rates.create'
  mockRoute.meta = {}
  mockApiGet.mockResolvedValue(successResult({}))
  mockApiCreate.mockResolvedValue(successResult({ id: 'new-id' }))
  mockApiUpdate.mockResolvedValue(successResult({ id: 'updated-id' }))
})

describe('ShippingRateForm', () => {
  describe('create mode', () => {
    it('renders form fields and save/cancel actions', () => {
      const wrapper = mountForm()

      expect(wrapper.find('.app-card-stub').exists()).toBe(true)
      expect(wrapper.find('.form-actions-stub').exists()).toBe(true)
      expect(wrapper.find('.save-btn').exists()).toBe(true)
      expect(wrapper.find('.cancel-btn').exists()).toBe(true)

      const inputs = wrapper.findAll('.form-field-stub input')
      expect(inputs.length).toBeGreaterThanOrEqual(6)
    })

    it('renders name, shipping method, rate, currency fields', () => {
      const wrapper = mountForm()

      const formFields = wrapper.findAll('.form-field-stub')
      expect(formFields.length).toBeGreaterThanOrEqual(6)
    })
  })

  describe('view mode', () => {
    beforeEach(() => {
      mockRoute.params = { id: 'sr1' }
      mockRoute.name = 'shipping.rates.view'
      mockApiGet.mockResolvedValue(successResult({ ...mockShippingRate }))
    })

    it('shows disabled fields after loading', async () => {
      const wrapper = mountForm()
      await flushPromises()
      await wrapper.vm.$nextTick()

      expect(wrapper.find('.app-card-stub').exists()).toBe(true)
      expect(wrapper.find('.form-actions-stub').exists()).toBe(false)

      const inputs = wrapper.findAll('.form-field-stub input')
      expect(inputs.length).toBeGreaterThanOrEqual(6)
      for (const input of inputs) {
        expect(input.attributes('disabled')).toBeDefined()
      }
    })
  })

  describe('edit mode', () => {
    beforeEach(() => {
      mockRoute.params = { id: 'sr1' }
      mockRoute.name = 'shipping.rates.edit'
      mockApiGet.mockResolvedValue(successResult({ ...mockShippingRate }))
    })

    it('shows editable fields with weight boundary fields', async () => {
      const wrapper = mountForm()
      await flushPromises()
      await wrapper.vm.$nextTick()

      expect(wrapper.find('.app-card-stub').exists()).toBe(true)
      expect(wrapper.find('.form-actions-stub').exists()).toBe(true)

      const formFields = wrapper.findAll('.form-field-stub')
      expect(formFields.length).toBeGreaterThanOrEqual(6)

      const inputs = wrapper.findAll('.form-field-stub input')
      expect(inputs[0]!.attributes('disabled')).toBeUndefined()
    })
  })

  describe('loading state', () => {
    it('shows loading skeleton in view mode', async () => {
      let resolveGet: (v: unknown) => void
      mockApiGet.mockReturnValue(new Promise((r) => { resolveGet = r }))
      mockRoute.params = { id: 'sr1' }
      mockRoute.name = 'shipping.rates.view'

      const wrapper = mountForm()
      await wrapper.vm.$nextTick()

      expect(wrapper.find('.loading-skeleton-stub').exists()).toBe(true)

      resolveGet!(successResult({ ...mockShippingRate }))
      await flushPromises()
      await wrapper.vm.$nextTick()

      expect(wrapper.find('.loading-skeleton-stub').exists()).toBe(false)
    })

    it('does not show loading skeleton in create mode', () => {
      const wrapper = mountForm()
      expect(wrapper.find('.loading-skeleton-stub').exists()).toBe(false)
    })
  })

  describe('error state', () => {
    it('shows error state when api.get fails', async () => {
      mockRoute.params = { id: 'sr1' }
      mockRoute.name = 'shipping.rates.view'
      mockApiGet.mockResolvedValue(errorResult('Shipping rate not found'))

      const wrapper = mountForm()
      await flushPromises()
      await wrapper.vm.$nextTick()

      expect(wrapper.find('.error-state-stub').exists()).toBe(true)
      expect(wrapper.find('.error-title').text()).toBe('Shipping rate not found')
    })
  })
})
