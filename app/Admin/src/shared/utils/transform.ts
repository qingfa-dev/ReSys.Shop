export function toCamelCase(str: string): string {
  return str.charAt(0).toLowerCase() + str.slice(1).replace(/_([a-z])/g, (_, c) => c.toUpperCase())
}

export function mapKeys<T extends Record<string, unknown>>(obj: T, transform: (key: string) => string): Record<string, unknown> {
  const result: Record<string, unknown> = {}
  for (const key of Object.keys(obj)) {
    result[transform(key)] = obj[key]
  }
  return result
}

export function toCamelCaseKeys<T extends Record<string, unknown>>(obj: T): Record<string, unknown> {
  const result: Record<string, unknown> = {}
  for (const key of Object.keys(obj)) {
    const newKey = toCamelCase(key)
    const val = obj[key]
    if (val !== null && typeof val === 'object' && !Array.isArray(val)) {
      result[newKey] = toCamelCaseKeys(val as Record<string, unknown>)
    } else if (Array.isArray(val)) {
      result[newKey] = val.map(item =>
        item !== null && typeof item === 'object' && !Array.isArray(item)
          ? toCamelCaseKeys(item as Record<string, unknown>)
          : item
      )
    } else {
      result[newKey] = val
    }
  }
  return result
}
