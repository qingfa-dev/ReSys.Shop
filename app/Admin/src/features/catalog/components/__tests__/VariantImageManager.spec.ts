import { describe, it, expect, vi, beforeEach } from 'vitest'
import { mount, flushPromises } from '@vue/test-utils'
import { createTestingPinia } from '@pinia/testing'
import VariantImageManager from '../VariantImageManager.vue'

const mockApiUpload = vi.hoisted(() => vi.fn())
const mockApiDelete = vi.hoisted(() => vi.fn())
const mockApiReorder = vi.hoisted(() => vi.fn())

vi.mock('../../api', () => ({
  VariantImageApi: {
    upload: mockApiUpload,
    delete: mockApiDelete,
    reorder: mockApiReorder,
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
    warn: vi.fn(),
    info: vi.fn(),
    showToast: vi.fn(),
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

const ButtonStub = {
  template: '<button :disabled="disabled" type="button" @click="$emit(\'click\')">{{ label }}<slot /></button>',
  props: ['label', 'icon', 'severity', 'size', 'loading', 'disabled', 'outlined', 'text', 'rounded'],
  emits: ['click'],
}

const mockImages = [
  { id: 'img1', url: '/img/1.jpg', fileName: 'photo1.jpg', type: 'primary' as const },
  { id: 'img2', url: '/img/2.jpg', fileName: 'photo2.jpg', type: 'gallery' as const },
]

function mountComponent(images = mockImages) {
  return mount(VariantImageManager, {
    props: {
      variantId: 'v1',
      images,
    },
    global: {
      plugins: [createTestingPinia({ stubActions: false, createSpy: vi.fn })],
      stubs: {
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

beforeEach(() => {
  vi.clearAllMocks()
  mockApiUpload.mockResolvedValue(successResult({ id: 'img3', url: '/img/3.jpg', fileName: 'photo3.jpg', type: 'gallery' }))
  mockApiDelete.mockResolvedValue(successResult({}))
  mockApiReorder.mockResolvedValue(successResult({}))
})

describe('VariantImageManager', () => {
  describe('renders items', () => {
    it('renders image thumbnails for each item', () => {
      const wrapper = mountComponent()

      const imgs = wrapper.findAll('img')
      expect(imgs).toHaveLength(2)
      expect(imgs[0]!.attributes('src')).toBe('/img/1.jpg')
      expect(imgs[1]!.attributes('src')).toBe('/img/2.jpg')
    })

    it('shows empty state when no images', () => {
      const wrapper = mountComponent([])

      expect(wrapper.text()).toContain('catalog.variants.images.empty')
      expect(wrapper.find('img').exists()).toBe(false)
    })

    it('shows upload button', () => {
      const wrapper = mountComponent()
      const buttons = wrapper.findAllComponents(ButtonStub)
      const uploadBtn = buttons.find(b => b.props('label') === 'catalog.variants.images.upload')
      expect(uploadBtn).toBeDefined()
    })

    it('disables upload button when max images reached', () => {
      const tenImages = Array.from({ length: 10 }, (_, i) => ({
        id: `img${i}`,
        url: `/img/${i}.jpg`,
        fileName: `photo${i}.jpg`,
        type: 'gallery' as const,
      }))
      const wrapper = mountComponent(tenImages)
      const buttons = wrapper.findAllComponents(ButtonStub)
      const uploadBtn = buttons.find(b => b.props('label') === 'catalog.variants.images.upload')
      expect(uploadBtn?.props('disabled')).toBe(true)
    })
  })

  describe('delete image', () => {
    it('confirms before deleting', async () => {
      const wrapper = mountComponent()
      const deleteBtn = wrapper.findAllComponents(ButtonStub).find(b => b.props('icon') === 'pi pi-times')

      await deleteBtn?.trigger('click')

      expect(mockConfirmDelete).toHaveBeenCalled()
      const confirmCall = mockConfirmDelete.mock.calls[0]![0]!
      expect(confirmCall.target).toContain('photo1.jpg')
      expect(typeof confirmCall.onAccept).toBe('function')
    })

    it('emits update:images after successful delete', async () => {
      let acceptFn: () => void = () => {}
      mockConfirmDelete.mockImplementation(({ onAccept }: { onAccept: () => void }) => {
        acceptFn = onAccept
      })

      const wrapper = mountComponent()
      const deleteBtn = wrapper.findAllComponents(ButtonStub).find(b => b.props('icon') === 'pi pi-times')

      await deleteBtn?.trigger('click')
      acceptFn()
      await flushPromises()

      expect(mockApiDelete).toHaveBeenCalledWith('img1')
      expect(toastSuccess).toHaveBeenCalledWith('catalog.variants.images.delete_success')
      const emitted = wrapper.emitted('update:images')
      expect(emitted).toBeTruthy()
      expect(emitted![0]![0]).toHaveLength(1)
    })

    it('shows error toast when delete fails', async () => {
      mockApiDelete.mockResolvedValue(errorResult('Delete failed'))
      let acceptFn: () => void = () => {}
      mockConfirmDelete.mockImplementation(({ onAccept }: { onAccept: () => void }) => {
        acceptFn = onAccept
      })

      const wrapper = mountComponent()
      const deleteBtn = wrapper.findAllComponents(ButtonStub).find(b => b.props('icon') === 'pi pi-times')

      await deleteBtn?.trigger('click')
      acceptFn()
      await flushPromises()

      expect(toastError).toHaveBeenCalledWith('catalog.variants.images.delete_failed')
    })
  })

  describe('drag and drop reorder', () => {
    it('reorder emits update:images with reordered array on drop', async () => {
      mockApiReorder.mockResolvedValue(successResult({}))
      const wrapper = mountComponent()

      const images = wrapper.findAll('img')
      expect(images).toHaveLength(2)

      const firstImgContainer = images[0]!.element.parentElement!
      const secondImgContainer = images[1]!.element.parentElement!

      expect(firstImgContainer?.getAttribute('draggable')).toBe('true')

      const dataTransfer = {
        effectAllowed: '',
        dropEffect: '',
        setData: vi.fn(),
        getData: vi.fn().mockReturnValue('0'),
      }

      const dragEvent = new Event('dragstart', { bubbles: true }) as Event & { dataTransfer: typeof dataTransfer }
      Object.defineProperty(dragEvent, 'dataTransfer', { value: dataTransfer, writable: false, configurable: true })
      firstImgContainer.dispatchEvent(dragEvent)

      expect(dataTransfer.setData).toHaveBeenCalledWith('text/plain', '0')

      const dropEvent = new Event('drop', { bubbles: true }) as Event & { dataTransfer: typeof dataTransfer }
      Object.defineProperty(dropEvent, 'dataTransfer', { value: dataTransfer, writable: false, configurable: true })
      Object.defineProperty(dropEvent, 'preventDefault', { value: vi.fn(), configurable: true })

      secondImgContainer.dispatchEvent(dropEvent)

      expect(dataTransfer.getData).toHaveBeenCalledWith('text/plain')
      const emitted = wrapper.emitted('update:images')
      expect(emitted).toBeTruthy()
      if (emitted) {
        expect(emitted[0]![0]).toHaveLength(2)
      }
      expect(mockApiReorder).toHaveBeenCalled()
    })
  })

  describe('file upload', () => {
    it('shows error when max limit exceeded', async () => {
      const nineImages = Array.from({ length: 9 }, (_, i) => ({
        id: `img${i}`,
        url: `/img/${i}.jpg`,
        fileName: `photo${i}.jpg`,
        type: 'gallery' as const,
      }))
      const wrapper = mountComponent(nineImages)

      const fileInput = wrapper.find('input[type="file"]')
      const file = new File([''], 'test.jpg', { type: 'image/jpeg' })
      Object.defineProperty(fileInput.element, 'files', {
        value: [file, file],
      })
      await fileInput.trigger('change')
      await wrapper.vm.$nextTick()

      expect(toastError).toHaveBeenCalledWith('catalog.variants.images.max_limit')
    })
  })
})
