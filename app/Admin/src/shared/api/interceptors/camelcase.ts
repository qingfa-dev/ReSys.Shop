function toCamelCase(str: string): string {
  return str.replace(/_([a-z])/g, (_, c) => c.toUpperCase())
}

function transformKeys(obj: unknown): unknown {
  if (Array.isArray(obj)) {
    return obj.map(transformKeys)
  }
  if (obj !== null && typeof obj === 'object') {
    const result: Record<string, unknown> = {}
    for (const [key, value] of Object.entries(obj as Record<string, unknown>)) {
      result[toCamelCase(key)] = transformKeys(value)
    }
    return result
  }
  return obj
}

export function camelCaseInterceptor(response: { data: unknown; status?: number; statusText?: string; headers?: unknown; config?: unknown }): { data: unknown; status?: number; statusText?: string; headers?: unknown; config?: unknown } {
  if (response.data !== null && response.data !== undefined) {
    response.data = transformKeys(response.data)
  }
  return response
}
