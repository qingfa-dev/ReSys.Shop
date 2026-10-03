export function resolvePostLoginRedirect(redirect: unknown, fallback = '/'): string {
  if (
    typeof redirect === 'string' &&
    redirect.startsWith('/') &&
    !redirect.startsWith('//') &&
    !redirect.startsWith('/\\')
  ) {
    return redirect
  }
  return fallback
}
