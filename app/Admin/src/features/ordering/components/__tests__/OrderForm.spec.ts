import { describe, it, expect, vi, beforeEach } from 'vitest'
import { mount, flushPromises } from '@vue/test-utils'
import { createTestingPinia } from '@pinia/testing'
import OrderForm from '../OrderForm.vue'

vi.mock('vee-validate', async (importOriginal) => {
  const actual = await importOriginal<typeof import('vee-validate')>()
  return {
    ...actual,
    useForm: (opts?: Record<string, unknown>) => {
      const result = actual.useForm(opts ?? {})
      const origHandleSubmit = result.handleSubmit
      result.handleSubmit = (cb: (...args: unknown[]) => unknown) => {
        return () => cb(result.values)
      }
      return result
    },
  }
})

const mockApiGet = vi.hoisted(() => vi.fn())
const mockApiCreate = vi.hoisted(() => vi.fn())
const mockApiUpdate = vi.hoisted(() => vi.fn())
const mockApiApprove = vi.hoisted(() => vi.fn())
const mockApiComplete = vi.hoisted(() => vi.fn())
const mockApiCancel = vi.hoisted(() => vi.fn())
const mockApiResume = vi.hoisted(() => vi.fn())

vi.mock('../../api', () => ({
  OrderApi: {
    get: mockApiGet,
    create: mockApiCreate,
    update: mockApiUpdate,
    approve: mockApiApprove,
    complete: mockApiComplete,
    cancel: mockApiCancel,
    resume: mockApiResume,
  },
}))

const mockRoute: { params: Record<string, string>; name: string; meta: Record<string, unknown> } = vi.hoisted(() => ({
  params: {} as Record<string, string>,
  name: 'ordering.orders.create',
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

const StatusTagStub = {
  template: '<span class="status-tag-stub">{{ status }}</span>',
  props: ['status'],
}

const ButtonStub = {
  template: '<button :disabled="disabled" :class="\'btn-\' + severity" type="button" @click="$emit(\'click\')">{{ label }}<slot /></button>',
  props: ['label', 'icon', 'severity', 'size', 'loading', 'disabled', 'outlined', 'text'],
  emits: ['click'],
}

const OrderLineItemManagerStub = {
  template: '<div class="line-item-mgr-stub" />',
  props: ['orderId', 'lineItems', 'readonly'],
}

function mountForm() {
  return mount(OrderForm, {
    global: {
      plugins: [createTestingPinia({ stubActions: false, createSpy: vi.fn })],
      stubs: {
        PageHeader: PageHeaderStub,
        FormField: FormFieldStub,
        FormActions: FormActionsStub,
        AppCard: AppCardStub,
        LoadingSkeleton: LoadingSkeletonStub,
        ErrorState: ErrorStateStub,
        StatusTag: StatusTagStub,
        Button: ButtonStub,
        OrderLineItemManager: OrderLineItemManagerStub,
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

const mockOrder = {
  id: 'o1',
  orderNumber: 'ORD-001',
  customerId: 'cust-1',
  customerName: 'John Doe',
  customerEmail: 'john@example.com',
  status: 'pending',
  subtotal: 100,
  taxTotal: 10,
  shippingTotal: 15,
  total: 125,
  notes: 'Test notes',
  lineItems: [],
}

beforeEach(() => {
  vi.clearAllMocks()
  mockRoute.params = {}
  mockRoute.name = 'ordering.orders.create'
  mockRoute.meta = {}
  mockApiGet.mockResolvedValue(successResult({}))
  mockApiCreate.mockResolvedValue(successResult({ id: 'new-id' }))
  mockApiUpdate.mockResolvedValue(successResult({ id: 'updated-id' }))
  mockApiApprove.mockResolvedValue(successResult({}))
  mockApiComplete.mockResolvedValue(successResult({}))
  mockApiCancel.mockResolvedValue(successResult({}))
  mockApiResume.mockResolvedValue(successResult({}))
})

describe('OrderForm', () => {
  describe('create mode', () => {
    it('renders form fields and save/cancel actions', () => {
      const wrapper = mountForm()

      expect(wrapper.find('.app-card-stub').exists()).toBe(true)
      expect(wrapper.find('.form-actions-stub').exists()).toBe(true)
      expect(wrapper.find('.save-btn').exists()).toBe(true)
      expect(wrapper.find('.cancel-btn').exists()).toBe(true)

      const inputs = wrapper.findAll('.form-field-stub input')
      expect(inputs.length).toBeGreaterThanOrEqual(1)
    })
  })

  describe('view mode', () => {
    beforeEach(() => {
      mockRoute.params = { id: 'o1' }
      mockRoute.name = 'ordering.orders.view'
      mockApiGet.mockResolvedValue(successResult({ ...mockOrder }))
    })

    it('shows order details with status tag', async () => {
      const wrapper = mountForm()
      await flushPromises()
      await wrapper.vm.$nextTick()

      expect(wrapper.find('.status-tag-stub').exists()).toBe(true)
      expect(wrapper.find('.status-tag-stub').text()).toBe('pending')
      const pageHeader = wrapper.find('.page-header-stub')
      expect(pageHeader.exists()).toBe(true)
    })

    it('shows approve button for pending orders', async () => {
      const wrapper = mountForm()
      await flushPromises()
      await wrapper.vm.$nextTick()

      const buttons = wrapper.findAll('.page-header-stub button')
      const hasApprove = buttons.some(b => b.text().includes('ordering.orders.actions.approve'))
      expect(hasApprove).toBe(true)
    })

    it('shows complete button for approved orders', async () => {
      mockApiGet.mockResolvedValue(successResult({ ...mockOrder, status: 'approved' }))
      const wrapper = mountForm()
      await flushPromises()
      await wrapper.vm.$nextTick()

      const buttons = wrapper.findAll('.page-header-stub button')
      const hasComplete = buttons.some(b => b.text().includes('ordering.orders.actions.complete'))
      expect(hasComplete).toBe(true)
    })

    it('shows cancel button for non-terminal orders', async () => {
      const wrapper = mountForm()
      await flushPromises()
      await wrapper.vm.$nextTick()

      const buttons = wrapper.findAll('.page-header-stub button')
      const hasCancel = buttons.some(b => b.text().includes('ordering.orders.actions.cancel_action'))
      expect(hasCancel).toBe(true)
    })

    it('shows resume button for cancelled orders', async () => {
      mockApiGet.mockResolvedValue(successResult({ ...mockOrder, status: 'cancelled' }))
      const wrapper = mountForm()
      await flushPromises()
      await wrapper.vm.$nextTick()

      const buttons = wrapper.findAll('.page-header-stub button')
      const hasResume = buttons.some(b => b.text().includes('ordering.orders.actions.resume'))
      expect(hasResume).toBe(true)
    })
  })

  describe('edit mode', () => {
    beforeEach(() => {
      mockRoute.params = { id: 'o1' }
      mockRoute.name = 'ordering.orders.edit'
      mockApiGet.mockResolvedValue(successResult({ ...mockOrder }))
    })

    it('shows editable fields with notes', async () => {
      const wrapper = mountForm()
      await flushPromises()
      await wrapper.vm.$nextTick()

      expect(wrapper.find('.app-card-stub').exists()).toBe(true)
      expect(wrapper.find('.form-actions-stub').exists()).toBe(true)
      expect(wrapper.find('textarea').exists()).toBe(true)
    })
  })

  describe('loading state', () => {
    it('shows loading skeleton while fetching', async () => {
      let resolveGet: (v: unknown) => void
      mockApiGet.mockReturnValue(new Promise((r) => { resolveGet = r }))
      mockRoute.params = { id: 'o1' }
      mockRoute.name = 'ordering.orders.view'

      const wrapper = mountForm()
      await wrapper.vm.$nextTick()

      expect(wrapper.find('.loading-skeleton-stub').exists()).toBe(true)

      resolveGet!(successResult({ ...mockOrder }))
      await flushPromises()
      await wrapper.vm.$nextTick()

      expect(wrapper.find('.loading-skeleton-stub').exists()).toBe(false)
    })
  })

  describe('error state', () => {
    it('shows error state when api.get fails', async () => {
      mockRoute.params = { id: 'o1' }
      mockRoute.name = 'ordering.orders.view'
      mockApiGet.mockResolvedValue(errorResult('Order not found'))

      const wrapper = mountForm()
      await flushPromises()
      await wrapper.vm.$nextTick()

      expect(wrapper.find('.error-state-stub').exists()).toBe(true)
      expect(wrapper.find('.error-title').text()).toBe('Order not found')
    })
  })
})
