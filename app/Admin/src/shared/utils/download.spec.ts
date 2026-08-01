import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest'
import { downloadFile } from './download'

describe('downloadFile', () => {
  let anchor: HTMLAnchorElement

  beforeEach(() => {
    anchor = { href: '', download: '', click: vi.fn<(...args: unknown[]) => unknown>(), style: {} } as unknown as HTMLAnchorElement
    vi.spyOn(document, 'createElement').mockReturnValue(anchor)
    vi.spyOn(document.body, 'appendChild').mockReturnValue(anchor)
    vi.spyOn(document.body, 'removeChild').mockReturnValue(anchor)
  })

  afterEach(() => {
    vi.restoreAllMocks()
  })

  it('creates an anchor and clicks it for a URL string', () => {
    downloadFile('https://example.com/file.pdf', 'report.pdf')
    expect(anchor.href).toBe('https://example.com/file.pdf')
    expect(anchor.download).toBe('report.pdf')
    expect(anchor.click).toHaveBeenCalledOnce()
  })

  it('creates an anchor and clicks it for a Blob', () => {
    const blob = new Blob(['test'], { type: 'text/plain' })
    downloadFile(blob, 'notes.txt')
    expect(anchor.download).toBe('notes.txt')
    expect(anchor.click).toHaveBeenCalledOnce()
  })
})
