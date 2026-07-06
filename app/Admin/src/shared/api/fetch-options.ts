let tokenAccessor: () => string | null = () => null

export function setAuthTokenAccessor(fn: () => string | null): void {
  tokenAccessor = fn
}

export function buildHeaders(extra?: HeadersInit): HeadersInit {
  const headers: Record<string, string> = {
    'Content-Type': 'application/json',
    'X-Request-Id': crypto.randomUUID(),
  }
  const token = tokenAccessor()
  if (token) {
    headers['Authorization'] = `Bearer ${token}`
  }
  return { ...headers, ...(extra as Record<string, string> | undefined) }
}
