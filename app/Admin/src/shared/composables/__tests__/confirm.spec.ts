import { describe, it, expect, vi, beforeEach } from 'vitest'
import { useConfirm } from '../useConfirm'

const mockRequire = vi.fn()
vi.mock('primevue/useconfirm', () => ({
  useConfirm: () => ({ require: mockRequire }),
}))

describe('useConfirm', () => {
  beforeEach(() => { vi.clearAllMocks() })

  it('confirmDelete calls require with delete options', () => {
    const { confirmDelete } = useConfirm()
    const onAccept = vi.fn()
    confirmDelete({ onAccept })
    expect(mockRequire).toHaveBeenCalledWith(
      expect.objectContaining({ header: 'Delete confirmation' })
    )
    const callArgs = mockRequire.mock.calls[0]![0]
    callArgs.accept()
    expect(onAccept).toHaveBeenCalled()
  })

  it('confirmAction calls require with confirm options', () => {
    const { confirmAction } = useConfirm()
    const onAccept = vi.fn()
    confirmAction({ onAccept })
    expect(mockRequire).toHaveBeenCalledWith(
      expect.objectContaining({ header: 'Please confirm' })
    )
  })

  it('confirmDelete uses custom target in message', () => {
    const { confirmDelete } = useConfirm()
    confirmDelete({ target: 'the product', onAccept: vi.fn() })
    expect(mockRequire).toHaveBeenCalledWith(
      expect.objectContaining({ message: expect.stringContaining('the product') })
    )
  })
})
