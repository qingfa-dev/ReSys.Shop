import { describe, it, expect, vi, beforeEach } from 'vitest'
import { useToast } from '../useToast'

const mockAdd = vi.fn()
vi.mock('primevue/usetoast', () => ({
  useToast: () => ({ add: mockAdd }),
}))

describe('useToast', () => {
  beforeEach(() => { vi.clearAllMocks() })

  it('showToast calls toast.add with severity, summary, detail', () => {
    const { showToast } = useToast()
    showToast('success', 'Done', 'Operation completed')
    expect(mockAdd).toHaveBeenCalledWith(
      expect.objectContaining({ severity: 'success', summary: 'Done', detail: 'Operation completed' })
    )
  })

  it('success() calls add with success severity', () => {
    const { success } = useToast()
    success('Saved')
    expect(mockAdd).toHaveBeenCalledWith(
      expect.objectContaining({ severity: 'success', detail: 'Saved' })
    )
  })

  it('error() calls add with error severity and 5000 life', () => {
    const { error } = useToast()
    error('Failed')
    expect(mockAdd).toHaveBeenCalledWith(
      expect.objectContaining({ severity: 'error', detail: 'Failed', life: 5000 })
    )
  })

  it('warn() calls add with warn severity and 4000 life', () => {
    const { warn } = useToast()
    warn('Caution')
    expect(mockAdd).toHaveBeenCalledWith(
      expect.objectContaining({ severity: 'warn', detail: 'Caution', life: 4000 })
    )
  })

  it('info() calls add with info severity', () => {
    const { info } = useToast()
    info('Note')
    expect(mockAdd).toHaveBeenCalledWith(
      expect.objectContaining({ severity: 'info', detail: 'Note' })
    )
  })
})
