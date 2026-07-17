export function toCamelCase(str: string): string {
  return str.replace(/_([a-z])/g, (_, c) => c.toUpperCase())
}

export function toSnakeCase(str: string): string {
  return str.replace(/([A-Z])/g, '_$1').toLowerCase().replace(/^_/, '')
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

export function toSnakeCaseKeys<T extends Record<string, unknown>>(obj: T): Record<string, unknown> {
  return mapKeys(obj, toSnakeCase)
}

export function mapDto<T extends Record<string, unknown>, R>(dto: T, mapping: Record<keyof R, keyof T>): R {
  const result = {} as R
  for (const [targetKey, sourceKey] of Object.entries(mapping)) {
    ;(result as Record<string, unknown>)[targetKey] = dto[sourceKey as string]
  }
  return result
}
