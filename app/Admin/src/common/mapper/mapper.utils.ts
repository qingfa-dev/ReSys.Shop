export function toCamelCase(str: string): string {
  return str.replace(/_([a-z])/g, (_, c) => c.toUpperCase())
}

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
