export type IsoDateString = string

export function nowIso(): IsoDateString {
  return new Date().toISOString()
}
