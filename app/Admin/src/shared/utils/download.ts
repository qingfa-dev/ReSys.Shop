export function downloadFile(source: string | Blob, filename = 'download'): void {
  const url = typeof source === 'string' ? source : URL.createObjectURL(source)
  const anchor = document.createElement('a')
  anchor.href = url
  anchor.download = filename
  anchor.style.display = 'none'
  document.body.appendChild(anchor)
  anchor.click()
  document.body.removeChild(anchor)
  if (typeof source !== 'string') URL.revokeObjectURL(url)
}
