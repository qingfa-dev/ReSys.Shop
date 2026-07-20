import { toCamelCase } from './string.transforms'

export function mapKeys<T extends Record<string, unknown>>(obj: T, transform: (key: string) => string): Record<string, unknown> {
  const result: Record<string, unknown> = {}
  for (const key of Object.keys(obj)) {
    result[transform(key)] = obj[key]
  }
  return result
}

export function toCamelCaseKeys<T extends Record<string, unknown>>(obj: T): Record<string, unknown> {
  return mapKeys(obj, toCamelCase)
}
