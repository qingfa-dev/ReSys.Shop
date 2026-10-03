import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest'

// useVisualSearch() calls onUnmounted() at setup time; there is no active
// component instance in a unit test, so stub the lifecycle hook out.
vi.mock('vue', async (importOriginal) => {
  const actual = await importOriginal<typeof import('vue')>()
  return {
    ...actual,
    onUnmounted: vi.fn<(...args: unknown[]) => unknown>(),
  }
})

import { useVisualSearch } from '../useVisualSearch'

const MB = 1024 * 1024

function fileOf(type: string, size: number, name = 'image.bin'): File {
  return new File([new Uint8Array(size)], name, { type })
}

describe('useVisualSearch validateFile', () => {
  const createObjectURL = vi.fn<(...args: unknown[]) => unknown>(() => 'blob:mock-preview')
  const revokeObjectURL = vi.fn<(...args: unknown[]) => unknown>()

  beforeEach(() => {
    vi.clearAllMocks()
    vi.stubGlobal('URL', { ...URL, createObjectURL, revokeObjectURL })
  })

  afterEach(() => {
    vi.unstubAllGlobals()
  })

  it('accepts a valid JPEG', () => {
    const { validateFile } = useVisualSearch()
    expect(validateFile(fileOf('image/jpeg', 1024, 'photo.jpg'))).toBe(true)
  })

  it('accepts a valid PNG', () => {
    const { validateFile } = useVisualSearch()
    expect(validateFile(fileOf('image/png', 1024, 'photo.png'))).toBe(true)
  })

  it('accepts a valid WebP', () => {
    const { validateFile } = useVisualSearch()
    expect(validateFile(fileOf('image/webp', 1024, 'photo.webp'))).toBe(true)
  })

  it('rejects a non-image MIME type', () => {
    const vs = useVisualSearch()
    const result = vs.validateFile(fileOf('text/plain', 1024, 'notes.txt'))
    expect(result).toBe(false)
    expect(vs.validationError).toContain('JPEG, PNG, or WebP')
  })

  it('rejects an oversized file over 10 MB', () => {
    const vs = useVisualSearch()
    const result = vs.validateFile(fileOf('image/jpeg', 10 * MB + 1, 'big.jpg'))
    expect(result).toBe(false)
    expect(vs.validationError).toContain('10 MB')
  })

  it('accepts a file exactly at the 10 MB boundary', () => {
    const { validateFile } = useVisualSearch()
    expect(validateFile(fileOf('image/jpeg', 10 * MB, 'exact.jpg'))).toBe(true)
  })

  it('selectFile records a validation error and keeps state for a bad file', () => {
    const vs = useVisualSearch()
    vs.selectFile(fileOf('text/plain', 1024, 'notes.txt'))

    expect(vs.validationError).toContain('JPEG, PNG, or WebP')
    expect(vs.selectedFile).toBeNull()
    expect(vs.state).toBe('empty')
    expect(createObjectURL).not.toHaveBeenCalled()
  })

  it('selectFile sets the file, preview URL, and upload state for a valid file', () => {
    const vs = useVisualSearch()
    const file = fileOf('image/jpeg', 1024, 'photo.jpg')

    vs.selectFile(file)

    expect(vs.validationError).toBeNull()
    expect(vs.selectedFile).toBe(file)
    expect(vs.previewUrl).toBe('blob:mock-preview')
    expect(vs.state).toBe('upload')
    expect(createObjectURL).toHaveBeenCalledWith(file)
  })

  it('reset returns the composable to the empty state', () => {
    const vs = useVisualSearch()
    vs.selectFile(fileOf('image/jpeg', 1024, 'photo.jpg'))
    expect(vs.state).toBe('upload')

    vs.reset()

    expect(vs.state).toBe('empty')
    expect(vs.selectedFile).toBeNull()
    expect(vs.previewUrl).toBeNull()
    expect(vs.validationError).toBeNull()
    expect(revokeObjectURL).toHaveBeenCalled()
  })
})
