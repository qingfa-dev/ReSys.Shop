// Guard: Validate redirect path is same-origin relative — prevents open redirect attacks
export function validateRedirect(path: string | null): string {
  if (!path) return '/'
  // Guard: Reject protocol-relative URLs and absolute URLs
  if (path.startsWith('//') || path.includes('://')) return '/'
  return path.startsWith('/') ? path : '/'
}
