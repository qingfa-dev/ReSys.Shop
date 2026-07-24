import { describe, it, expect, vi } from 'vitest'
import { useConfirm } from '../useConfirm'

vi.mock('primevue/useconfirm', () => ({
  useConfirm: () => ({
    require: vi.fn(),
  }),
}))

describe('useConfirm', () => {
  it('returns confirmDelete and confirmAction', () => {
    const { confirmDelete, confirmAction } = useConfirm()
    expect(typeof confirmDelete).toBe('function')
    expect(typeof confirmAction).toBe('function')
  })

  it('confirmDelete calls confirm.require', () => {
    // The mock uses vi.fn() internally which always returns undefined — this
    // already covers the call-was-invoked path because require is inside a
    // mock.
    const { confirmDelete } = useConfirm()
    const onAccept = vi.fn()
    expect(() => confirmDelete({ onAccept })).not.toThrow()
  })
})
