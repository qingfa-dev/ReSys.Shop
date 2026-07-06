let tokenAccessor: () => string | null = () => null

export function setAuthTokenAccessor(fn: () => string | null): void {
  tokenAccessor = fn
}

export function getToken(): string | null {
  return tokenAccessor()
}
