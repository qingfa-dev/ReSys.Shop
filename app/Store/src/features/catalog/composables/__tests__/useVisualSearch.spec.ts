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
    expect(validateFile(fileOf('image/jpeg', 1024, 'photo.jpg'))).toBeNull()
  })

  it('accepts a valid PNG', () => {
    const { validateFile } = useVisualSearch()
    expect(validateFile(fileOf('image/png', 1024, 'photo.png'))).toBeNull()
  })

  it('accepts a valid WebP', () => {
    const { validateFile } = useVisualSearch()
    expect(validateFile(fileOf('image/webp', 1024, 'photo.webp'))).toBeNull()
  })

  it('rejects a non-image MIME type', () => {
    const { validateFile } = useVisualSearch()
    const err = validateFile(fileOf('text/plain', 1024, 'notes.txt'))
    expect(err?.type).toBe('type')
    expect(err?.message).toContain('JPEG, PNG, or WebP')
  })

  it('rejects an oversized file over 10 MB', () => {
    const { validateFile } = useVisualSearch()
    const err = validateFile(fileOf('image/jpeg', 10 * MB + 1, 'big.jpg'))
    expect(err?.type).toBe('size')
    expect(err?.message).toContain('under 10 MB')
  })

  it('accepts a file exactly at the 10 MB boundary', () => {
    const { validateFile } = useVisualSearch()
    expect(validateFile(fileOf('image/jpeg', 10 * MB, 'exact.jpg'))).toBeNull()
  })

  it('selectFile records a validation error and keeps state for a bad file', () => {
    const vs = useVisualSearch()
    vs.selectFile(fileOf('text/plain', 1024, 'notes.txt'))

    expect(vs.validationError.value?.type).toBe('type')
    expect(vs.selectedFile.value).toBeNull()
    expect(vs.state.value).toBe('empty')
    expect(createObjectURL).not.toHaveBeenCalled()
  })

  it('selectFile sets the file, preview URL, and upload state for a valid file', () => {
    const vs = useVisualSearch()
    const file = fileOf('image/jpeg', 1024, 'photo.jpg')

    vs.selectFile(file)

    expect(vs.validationError.value).toBeNull()
    expect(vs.selectedFile.value).toBe(file)
    expect(vs.previewUrl.value).toBe('blob:mock-preview')
    expect(vs.state.value).toBe('upload')
    expect(createObjectURL).toHaveBeenCalledWith(file)
  })

  it('reset returns the composable to the empty state', () => {
    const vs = useVisualSearch()
    vs.selectFile(fileOf('image/jpeg', 1024, 'photo.jpg'))
    expect(vs.state.value).toBe('upload')

    vs.reset()

    expect(vs.state.value).toBe('empty')
    expect(vs.selectedFile.value).toBeNull()
    expect(vs.previewUrl.value).toBeNull()
    expect(vs.validationError.value).toBeNull()
    expect(revokeObjectURL).toHaveBeenCalled()
  })
})
