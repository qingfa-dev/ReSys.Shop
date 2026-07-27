import { describe, it, expect, vi, beforeEach } from 'vitest'
import { mount, flushPromises } from '@vue/test-utils'
import { createTestingPinia } from '@pinia/testing'
import RolePermissionManager from '../RolePermissionManager.vue'

const mockApiGet = vi.hoisted(() => vi.fn())
const mockApiSync = vi.hoisted(() => vi.fn())

vi.mock('../../api', () => ({
  RolePermissionApi: {
    get: mockApiGet,
    sync: mockApiSync,
  },
}))

const toastSuccess = vi.hoisted(() => vi.fn())
const toastError = vi.hoisted(() => vi.fn())
vi.mock('@/shared/composables/useToast', () => ({
  useToast: () => ({
    success: toastSuccess,
    error: toastError,
    warn: vi.fn(),
    info: vi.fn(),
    showToast: vi.fn(),
  }),
}))

vi.mock('primevue/usetoast', () => ({
  useToast: () => ({ add: vi.fn() }),
}))

const CheckboxStub = {
  template: '<input type="checkbox" :id="inputId" :checked="modelValue" @change="$emit(\'update:modelValue\', !modelValue)" />',
  props: ['modelValue', 'binary', 'inputId'],
  emits: ['update:modelValue'],
}

const ButtonStub = {
  template: '<button :disabled="loading" class="save-perm-btn" type="button" @click="$emit(\'click\')">{{ label }}<slot /></button>',
  props: ['label', 'icon', 'severity', 'size', 'loading', 'disabled', 'outlined', 'text'],
  emits: ['click'],
}

function mountComponent() {
  return mount(RolePermissionManager, {
    props: { roleId: 'r1' },
    global: {
      plugins: [createTestingPinia({ stubActions: false, createSpy: vi.fn })],
      stubs: {
        Checkbox: CheckboxStub,
        Button: ButtonStub,
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

const mockItems = [
  { permissionId: 'p1', name: 'catalog.products.read', isAssigned: true },
  { permissionId: 'p2', name: 'catalog.products.write', isAssigned: false },
  { permissionId: 'p3', name: 'ordering.orders.read', isAssigned: true },
  { permissionId: 'p4', name: 'ordering.orders.write', isAssigned: false },
]

beforeEach(() => {
  vi.clearAllMocks()
  mockApiGet.mockResolvedValue(successResult({ items: mockItems.map(i => ({ ...i })) }))
  mockApiSync.mockResolvedValue(successResult({}))
})

describe('RolePermissionManager', () => {
  describe('renders items', () => {
    it('shows loading state initially', async () => {
      let resolveGet: (v: unknown) => void
      mockApiGet.mockReturnValue(new Promise((r) => { resolveGet = r }))

      const wrapper = mountComponent()
      await wrapper.vm.$nextTick()

      expect(wrapper.text()).toContain('Loading permissions...')

      resolveGet!(successResult({ items: [] }))
    })

    it('renders permission items grouped by prefix', async () => {
      const wrapper = mountComponent()
      await flushPromises()
      await wrapper.vm.$nextTick()

      assertGrouped(wrapper, ['CATALOG', 'ORDERING'])
    })

    it('shows assigned state correctly', async () => {
      const wrapper = mountComponent()
      await flushPromises()
      await wrapper.vm.$nextTick()

      const checkboxes = wrapper.findAll('input[type="checkbox"]')
      expect(checkboxes.length).toBe(4)
      expect((checkboxes[0]!.element as HTMLInputElement).checked).toBe(true)
      expect((checkboxes[1]!.element as HTMLInputElement).checked).toBe(false)
      expect((checkboxes[2]!.element as HTMLInputElement).checked).toBe(true)
      expect((checkboxes[3]!.element as HTMLInputElement).checked).toBe(false)
    })
  })

  describe('assign/revoke flow', () => {
    it('toggles permission assignment on checkbox click', async () => {
      const wrapper = mountComponent()
      await flushPromises()
      await wrapper.vm.$nextTick()

      const checkboxes = wrapper.findAll('input[type="checkbox"]')
      await checkboxes[0]!.trigger('change')
      expect((checkboxes[0]!.element as HTMLInputElement).checked).toBe(false)

      await checkboxes[0]!.trigger('change')
      expect((checkboxes[0]!.element as HTMLInputElement).checked).toBe(true)
    })

    it('saves only assigned permissions', async () => {
      const wrapper = mountComponent()
      await flushPromises()
      await wrapper.vm.$nextTick()

      const saveBtn = wrapper.find('.save-perm-btn')
      await saveBtn.trigger('click')
      await wrapper.vm.$nextTick()

      expect(mockApiSync).toHaveBeenCalledWith('r1', {
        items: [
          { permissionId: 'p1' },
          { permissionId: 'p3' },
        ],
      })
      expect(toastSuccess).toHaveBeenCalledWith('Permissions updated successfully')
    })

    it('shows loading state on save button during sync', async () => {
      let resolveSync: (v: unknown) => void
      mockApiSync.mockReturnValue(new Promise((r) => { resolveSync = r }))

      const wrapper = mountComponent()
      await flushPromises()
      await wrapper.vm.$nextTick()

      const saveBtn = wrapper.find('.save-perm-btn')
      expect((saveBtn.element as HTMLButtonElement).disabled).toBe(false)

      await saveBtn.trigger('click')
      await wrapper.vm.$nextTick()

      expect((saveBtn.element as HTMLButtonElement).disabled).toBe(true)

      resolveSync!(successResult({}))
      await wrapper.vm.$nextTick()
    })
  })

  describe('error state', () => {
    it('shows error toast when get fails', async () => {
      mockApiGet.mockResolvedValue(errorResult('Failed to load permissions'))

      const wrapper = mountComponent()
      await flushPromises()
      await wrapper.vm.$nextTick()

      expect(toastError).toHaveBeenCalledWith('Failed to load permissions')
    })

    it('shows error toast when sync fails', async () => {
      mockApiSync.mockResolvedValue(errorResult('Failed to update permissions'))

      const wrapper = mountComponent()
      await flushPromises()
      await wrapper.vm.$nextTick()

      const saveBtn = wrapper.find('.save-perm-btn')
      await saveBtn.trigger('click')
      await wrapper.vm.$nextTick()

      expect(toastError).toHaveBeenCalledWith('Failed to update permissions')
    })

    it('handles items with no dot for group', async () => {
      mockApiGet.mockResolvedValue(successResult({
        items: [{
          permissionId: 'p99',
          name: 'dashboard',
          isAssigned: false,
        }],
      }))

      const wrapper = mountComponent()
      await flushPromises()
      await wrapper.vm.$nextTick()

      assertGrouped(wrapper, ['DASHBOARD'])
    })
  })
})

function assertGrouped(wrapper: ReturnType<typeof mountComponent>, expectedGroups: string[]) {
  const headings = wrapper.findAll('h4')
  const labels = headings.map(h => h.text())
  for (const g of expectedGroups) {
    expect(labels.some(l => l.toUpperCase() === g.toUpperCase())).toBe(true)
  }
}
