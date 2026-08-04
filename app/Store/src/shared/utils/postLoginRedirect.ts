export function validateRedirect(path: string | null): string {
  if (!path) return '/'
  // Only allow same-origin relative paths (no //, no http:)
  if (path.startsWith('//') || path.includes('://')) return '/'
  return path.startsWith('/') ? path : '/'
}
