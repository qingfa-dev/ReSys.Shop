const EMAIL_RE = /^[^\s@]+@[^\s@]+\.[^\s@]+$/
const URL_RE = /^https?:\/\/[^\s/$.?#].[^\s]*$/i
const GUID_RE = /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i

export function isEmail(s: string): boolean {
  return EMAIL_RE.test(s)
}

export function isUrl(s: string): boolean {
  return URL_RE.test(s)
}

export function isGuid(s: string): boolean {
  return GUID_RE.test(s)
}
