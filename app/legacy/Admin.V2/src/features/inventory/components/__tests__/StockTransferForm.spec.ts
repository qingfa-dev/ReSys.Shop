import { describe, it, expect, vi, beforeEach } from 'vitest'
import { mount, flushPromises } from '@vue/test-utils'
import { createTestingPinia } from '@pinia/testing'
import StockTransferForm from '../StockTransferForm.vue'

const mockApiGet = vi.hoisted(() => vi.fn())
const mockApiCreate = vi.hoisted(() => vi.fn())
const mockApiTransfer = vi.hoisted(() => vi.fn())
const mockApiReceive = vi.hoisted(() => vi.fn())
const mockApiCancel = vi.hoisted(() => vi.fn())

vi.mock('../../api', () => ({
  StockTransferApi: {
    get: mockApiGet,
    create: mockApiCreate,
    transfer: mockApiTransfer,
    receive: mockApiReceive,
    cancel: mockApiCancel,
  },
}))

const mockRoute: { params: Record<string, string>; name: string; meta: Record<string, unknown> } = vi.hoisted(() => ({
  params: {} as Record<string, string>,
  name: 'inventory.transfers.create',
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

const TagStub = {
  template: '<span class="tag-stub">{{ value }}</span>',
  props: ['value', 'severity'],
}

function mountForm() {
  return mount(StockTransferForm, {
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
        Tag: TagStub,
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

const mockTransferResponse = {
  id: 'tr-1',
  reference: 'TR-001',
  sourceLocationId: 'src-1',
  sourceLocationName: 'Warehouse A',
  destinationLocationId: 'dst-1',
  destinationLocationName: 'Store B',
  status: 'Pending',
  lineItems: [
    { variantId: 'VAR-1', variantSku: 'SKU-1', quantity: 10, receivedQuantity: 0 },
  ],
  notes: 'Urgent',
  createdAt: '2026-01-01',
  updatedAt: '2026-01-01',
}

beforeEach(() => {
  vi.clearAllMocks()
  mockRoute.params = {}
  mockRoute.name = 'inventory.transfers.create'
  mockRoute.meta = {}
  mockApiGet.mockResolvedValue(successResult({}))
  mockApiCreate.mockResolvedValue(successResult({ id: 'new-tr' }))
})

describe('StockTransferForm', () => {
  describe('create mode', () => {
    it('renders source, destination, notes, line items, and actions', () => {
      const wrapper = mountForm()

      expect(wrapper.find('.app-card-stub').exists()).toBe(true)
      expect(wrapper.find('.form-actions-stub').exists()).toBe(true)
      expect(wrapper.find('.save-btn').exists()).toBe(true)
      expect(wrapper.find('.cancel-btn').exists()).toBe(true)

      const inputs = wrapper.findAll('.form-field-stub input')
      expect(inputs.length).toBeGreaterThanOrEqual(3)
      expect(wrapper.find('textarea').exists()).toBe(true)
    })

    it('creates transfer with filled fields', async () => {
      const wrapper = mountForm()
      const inputs = wrapper.findAll('.form-field-stub input')
      await inputs[0]!.setValue('src-1')
      await inputs[1]!.setValue('dst-1')
      await inputs[2]!.setValue('VAR-001')
      await inputs[3]!.setValue('5')
      await wrapper.vm.$nextTick()

      await wrapper.find('.save-btn').trigger('click')

      expect(mockApiCreate).toHaveBeenCalledWith({
        sourceLocationId: 'src-1',
        destinationLocationId: 'dst-1',
        lineItems: [{ variantId: 'VAR-001', quantity: 5 }],
        notes: null,
      })
    })

    it('disables save button during saving', async () => {
      let resolveCreate: (v: unknown) => void
      mockApiCreate.mockReturnValue(new Promise((r) => { resolveCreate = r }))

      const wrapper = mountForm()
      const inputs = wrapper.findAll('.form-field-stub input')
      await inputs[0]!.setValue('src-1')
      await inputs[1]!.setValue('dst-1')
      await inputs[2]!.setValue('VAR-001')
      await inputs[3]!.setValue('5')
      await wrapper.vm.$nextTick()

      const saveBtn = wrapper.find('.save-btn')
      saveBtn.trigger('click')
      await wrapper.vm.$nextTick()

      expect(saveBtn.attributes('disabled')).toBeDefined()

      resolveCreate!(successResult({ id: 'new-tr' }))
      await flushPromises()
      await wrapper.vm.$nextTick()

      expect(saveBtn.attributes('disabled')).toBeUndefined()
    })
  })

  describe('view mode', () => {
    beforeEach(() => {
      mockRoute.params = { id: 'tr-1' }
      mockRoute.name = 'inventory.transfers.view'
      mockApiGet.mockResolvedValue(successResult({ ...mockTransferResponse }))
    })

    it('shows transfer details with status tag', async () => {
      const wrapper = mountForm()
      await flushPromises()
      await wrapper.vm.$nextTick()

      expect(wrapper.find('.tag-stub').exists()).toBe(true)
      expect(wrapper.find('.tag-stub').text()).toBe('Pending')
      expect(wrapper.text()).toContain('TR-001')
      expect(wrapper.text()).toContain('Warehouse A')
      expect(wrapper.text()).toContain('Store B')
    })

    it('shows transfer and cancel buttons for Pending status', async () => {
      const wrapper = mountForm()
      await flushPromises()
      await wrapper.vm.$nextTick()

      await vi.waitFor(() => {
        const headerButtons = wrapper.findAll('.page-header-stub button')
        return headerButtons.some(b => b.text().includes('inventory.transfers.actions.transfer'))
      })

      const headerButtons = wrapper.findAll('.page-header-stub button')
      const hasTransfer = headerButtons.some(b => b.text().includes('inventory.transfers.actions.transfer'))
      const hasCancel = headerButtons.some(b => b.text().includes('inventory.transfers.actions.cancel'))

      expect(hasTransfer).toBe(true)
      expect(hasCancel).toBe(true)
    })

    it('shows receive button for InTransit status', async () => {
      mockApiGet.mockResolvedValue(successResult({ ...mockTransferResponse, status: 'InTransit' }))

      const wrapper = mountForm()
      await flushPromises()
      await wrapper.vm.$nextTick()

      await vi.waitFor(() => {
        const headerButtons = wrapper.findAll('.page-header-stub button')
        return headerButtons.some(b => b.text().includes('inventory.transfers.actions.receive'))
      })

      const headerButtons = wrapper.findAll('.page-header-stub button')
      const hasReceive = headerButtons.some(b => b.text().includes('inventory.transfers.actions.receive'))
      const hasCancel = headerButtons.some(b => b.text().includes('inventory.transfers.actions.cancel'))

      expect(hasReceive).toBe(true)
      expect(hasCancel).toBe(true)
    })

    it('disables action buttons during transfer', async () => {
      let resolveTransfer: (v: unknown) => void
      mockApiTransfer.mockReturnValue(new Promise((r) => { resolveTransfer = r }))

      const wrapper = mountForm()
      await flushPromises()
      await wrapper.vm.$nextTick()

      await vi.waitFor(() => {
        const headerButtons = wrapper.findAll('.page-header-stub button')
        return headerButtons.some(b => b.text().includes('inventory.transfers.actions.transfer'))
      })

      const headerButtons = wrapper.findAll('.page-header-stub button')
      const transferBtn = headerButtons.find(b => b.text().includes('inventory.transfers.actions.transfer'))!

      transferBtn.trigger('click')
      await wrapper.vm.$nextTick()

      expect(transferBtn.attributes('disabled')).toBeDefined()

      resolveTransfer!(successResult({ ...mockTransferResponse, status: 'InTransit' }))
      await flushPromises()
      await wrapper.vm.$nextTick()

      // After successful transfer, the transfer button disappears (status changed to InTransit)
      const postButtons = wrapper.findAll('.page-header-stub button')
      const postTransferBtn = postButtons.find(b => b.text().includes('inventory.transfers.actions.transfer'))
      expect(postTransferBtn).toBeUndefined()
    })
  })

  describe('loading and error states', () => {
    it('shows loading skeleton while fetching transfer', async () => {
      let resolveGet: (v: unknown) => void
      mockApiGet.mockReturnValue(new Promise((r) => { resolveGet = r }))
      mockRoute.params = { id: 'tr-1' }
      mockRoute.name = 'inventory.transfers.view'

      const wrapper = mountForm()
      await wrapper.vm.$nextTick()

      expect(wrapper.find('.loading-skeleton-stub').exists()).toBe(true)

      resolveGet!(successResult({ ...mockTransferResponse }))
      await flushPromises()
      await wrapper.vm.$nextTick()

      expect(wrapper.find('.loading-skeleton-stub').exists()).toBe(false)
    })

    it('shows error state when api.get fails', async () => {
      mockRoute.params = { id: 'tr-1' }
      mockRoute.name = 'inventory.transfers.view'
      mockApiGet.mockResolvedValue(errorResult('Transfer not found'))

      const wrapper = mountForm()
      await flushPromises()
      await wrapper.vm.$nextTick()

      expect(wrapper.find('.error-state-stub').exists()).toBe(true)
      expect(wrapper.find('.error-title').text()).toBe('Transfer not found')
      expect(wrapper.find('.retry-btn').exists()).toBe(true)
    })
  })
})
