import { describe, it, expect, vi, beforeEach } from 'vitest'
import { mount } from '@vue/test-utils'
import { createTestingPinia } from '@pinia/testing'
import FulfillmentWorkflow from '../FulfillmentWorkflow.vue'

const mockApiApprove = vi.hoisted(() => vi.fn())
const mockApiComplete = vi.hoisted(() => vi.fn())
const mockApiCancel = vi.hoisted(() => vi.fn())
const mockApiResume = vi.hoisted(() => vi.fn())

vi.mock('../../api', () => ({
  OrderApi: {
    approve: mockApiApprove,
    complete: mockApiComplete,
    cancel: mockApiCancel,
    resume: mockApiResume,
  },
}))

const mockConfirmDelete = vi.hoisted(() => vi.fn())
vi.mock('@/shared/composables/useConfirm', () => ({
  useConfirm: () => ({
    confirmDelete: mockConfirmDelete,
  }),
}))

const toastSuccess = vi.hoisted(() => vi.fn())
const toastError = vi.hoisted(() => vi.fn())
vi.mock('@/shared/composables/useToast', () => ({
  useToast: () => ({
    success: toastSuccess,
    error: toastError,
  }),
}))

vi.mock('vue-i18n', () => ({
  useI18n: () => ({
    t: (key: string, params?: Record<string, unknown>) => {
      if (params) {
        let result = key
        for (const [k, v] of Object.entries(params)) {
          result = result.replace(`{${k}}`, String(v))
        }
        return result
      }
      return key
    },
  }),
}))

vi.mock('primevue/usetoast', () => ({
  useToast: () => ({ add: vi.fn() }),
}))

const StepsStub = {
  template: '<div class="steps-stub"><div class="step" v-for="(s, i) in model" :key="s.value" :class="{ active: i <= activeStep }">{{ s.label }}</div></div>',
  props: ['model', 'activeStep'],
}

const ButtonStub = {
  template: '<button :disabled="disabled || loading" :class="[\'wf-btn\', severity]" type="button" @click="$emit(\'click\')">{{ label }}<slot /></button>',
  props: ['label', 'icon', 'severity', 'size', 'loading', 'disabled', 'outlined', 'text'],
  emits: ['click'],
}

function mountComponent(props?: Record<string, unknown>) {
  return mount(FulfillmentWorkflow, {
    props: {
      orderId: 'o1',
      status: props?.status ?? 'pending',
      ...props,
    },
    global: {
      plugins: [createTestingPinia({ stubActions: false, createSpy: vi.fn })],
      stubs: {
        Steps: StepsStub,
        Button: ButtonStub,
      },
    },
  })
}

function successResult(value?: unknown) {
  return { isSuccess: true, statusCode: 200, value: value ?? {}, errors: [], message: null, metadata: null }
}

function errorResult(message?: string) {
  return { isSuccess: false, statusCode: 400, value: null, errors: [], message: message ?? 'Failed', metadata: null }
}

beforeEach(() => {
  vi.clearAllMocks()
  mockApiApprove.mockResolvedValue(successResult())
  mockApiComplete.mockResolvedValue(successResult())
  mockApiCancel.mockResolvedValue(successResult())
  mockApiResume.mockResolvedValue(successResult())
})

describe('FulfillmentWorkflow', () => {
  describe('step highlighting', () => {
    it('highlights the first step for pending status', () => {
      const wrapper = mountComponent({ status: 'pending' })
      const steps = wrapper.findAll('.step')
      expect(steps.length).toBeGreaterThanOrEqual(1)
      expect(steps[0]!.classes()).toContain('active')
    })

    it('highlights up to approved step for approved status', () => {
      const wrapper = mountComponent({ status: 'approved' })
      const steps = wrapper.findAll('.step')
      expect(steps[0]!.classes()).toContain('active')
      expect(steps[1]!.classes()).toContain('active')
    })

    it('highlights all steps up to delivered', () => {
      const wrapper = mountComponent({ status: 'delivered' })
      const steps = wrapper.findAll('.step')
      expect(steps.length).toBeGreaterThanOrEqual(5)
      for (const step of steps) {
        expect(step.classes()).toContain('active')
      }
    })
  })

  describe('action buttons per status', () => {
    it('shows approve and cancel for pending status', () => {
      const wrapper = mountComponent({ status: 'pending' })
      const buttons = wrapper.findAll('.wf-btn')

      const labels = buttons.map(b => b.text())
      expect(labels).toContain('ordering.workflow.confirm_order')
      expect(labels).toContain('ordering.workflow.cancel_order')
    })

    it('shows only complete and cancel for approved status', () => {
      const wrapper = mountComponent({ status: 'approved' })
      const buttons = wrapper.findAll('.wf-btn')

      const labels = buttons.map(b => b.text())
      expect(labels).not.toContain('ordering.workflow.confirm_order')
      expect(labels).toContain('ordering.workflow.mark_complete')
      expect(labels).toContain('ordering.workflow.cancel_order')
    })

    it('shows only complete and cancel for processing status', () => {
      const wrapper = mountComponent({ status: 'processing' })
      const buttons = wrapper.findAll('.wf-btn')

      const labels = buttons.map(b => b.text())
      expect(labels).not.toContain('ordering.workflow.confirm_order')
      expect(labels).toContain('ordering.workflow.mark_complete')
      expect(labels).toContain('ordering.workflow.cancel_order')
    })

    it('shows no action buttons for terminal status (completed)', () => {
      const wrapper = mountComponent({ status: 'completed' })
      const buttons = wrapper.findAll('.wf-btn')
      expect(buttons.length).toBe(0)
      expect(wrapper.text()).toContain('ordering.workflow.terminal_note')
    })

    it('shows no action buttons for terminal status (cancelled)', () => {
      const wrapper = mountComponent({ status: 'cancelled' })
      const buttons = wrapper.findAll('.wf-btn')
      expect(buttons.length).toBe(0)
      expect(wrapper.text()).toContain('ordering.workflow.terminal_note')
    })
  })

  describe('transition actions', () => {
    it('disables buttons during transition', async () => {
      let resolveApprove: (v: unknown) => void
      mockApiApprove.mockReturnValue(new Promise((r) => { resolveApprove = r }))

      const wrapper = mountComponent({ status: 'pending' })
      const approveBtn = wrapper.findAll('.wf-btn').find(b => b.text() === 'ordering.workflow.confirm_order')!

      await approveBtn.trigger('click')
      await wrapper.vm.$nextTick()

      expect((approveBtn.element as HTMLButtonElement).disabled).toBe(true)

      resolveApprove!(successResult())
      await wrapper.vm.$nextTick()
    })

    it('calls approve API and emits status-changed', async () => {
      const wrapper = mountComponent({ status: 'pending' })
      const approveBtn = wrapper.findAll('.wf-btn').find(b => b.text() === 'ordering.workflow.confirm_order')!

      await approveBtn.trigger('click')
      await wrapper.vm.$nextTick()

      expect(mockApiApprove).toHaveBeenCalledWith('o1')
      expect(toastSuccess).toHaveBeenCalledWith('ordering.orders.messages.approve_success')
      expect(wrapper.emitted('status-changed')).toBeTruthy()
    })

    it('calls complete API and shows success toast', async () => {
      const wrapper = mountComponent({ status: 'approved' })
      const completeBtn = wrapper.findAll('.wf-btn').find(b => b.text() === 'ordering.workflow.mark_complete')!

      await completeBtn.trigger('click')
      await wrapper.vm.$nextTick()

      expect(mockApiComplete).toHaveBeenCalledWith('o1')
      expect(toastSuccess).toHaveBeenCalledWith('ordering.orders.messages.complete_success')
    })

    it('uses confirm dialog for cancel action', async () => {
      const wrapper = mountComponent({ status: 'pending' })
      const cancelBtn = wrapper.findAll('.wf-btn').find(b => b.text() === 'ordering.workflow.cancel_order')!

      await cancelBtn.trigger('click')

      expect(mockConfirmDelete).toHaveBeenCalled()
      const confirmCall = mockConfirmDelete.mock.calls[0]![0]!
      expect(confirmCall.target).toBe('ordering.workflow.confirm.cancel')
      expect(typeof confirmCall.onAccept).toBe('function')
    })

    it('shows error toast when API fails', async () => {
      mockApiApprove.mockResolvedValue(errorResult('Approval failed'))

      const wrapper = mountComponent({ status: 'pending' })
      const approveBtn = wrapper.findAll('.wf-btn').find(b => b.text() === 'ordering.workflow.confirm_order')!

      await approveBtn.trigger('click')
      await wrapper.vm.$nextTick()

      expect(toastError).toHaveBeenCalledWith('Approval failed')
    })
  })
})
