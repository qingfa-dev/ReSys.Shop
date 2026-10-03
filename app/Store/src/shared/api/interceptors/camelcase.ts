// Normalize: Convert snake_case key to camelCase for frontend consistency
function toCamelCase(str: string): string {
  return str.replace(/_([a-z])/g, (_, c) => c.toUpperCase())
}

// Transform: Recursively convert all object keys from snake_case to camelCase
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

import type { AxiosResponse } from 'axios'

// Intercept: Auto-convert snake_case API responses to camelCase — skip Blob downloads
export function camelCaseInterceptor(response: AxiosResponse): AxiosResponse {
  if (response.data instanceof Blob) {
    return response
  }
  if (response.data !== null && response.data !== undefined) {
    response.data = transformKeys(response.data)
  }
  return response
}
