import type { ServerResult, ServerPagedResult } from '@/shared/api/types/result.types'

export function createMockResult<T>(value: T, overrides?: Partial<ServerResult<T>>): ServerResult<T> {
  return {
    isSuccess: true,
    statusCode: 200,
    errors: [],
    message: null,
    metadata: null,
    value,
    ...overrides,
  }
}

export function createMockPagedResult<T>(items: T[], overrides?: Partial<ServerPagedResult<T>>): ServerPagedResult<T> {
  return {
    isSuccess: true,
    statusCode: 200,
    errors: [],
    message: null,
    metadata: null,
    items,
    page: 1,
    pageSize: items.length,
    totalCount: items.length,
    ...overrides,
  }
}

export function createMockErrorResult<T = never>(overrides?: Partial<ServerResult<T>>): ServerResult<T> {
  return {
    isSuccess: false,
    statusCode: 500,
    errors: [{ code: 'ERROR', message: 'Mock error', type: 0, metadata: null }],
    message: 'Mock error',
    metadata: null,
    value: null as T,
    ...overrides,
  }
}
