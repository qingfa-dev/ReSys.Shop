import type { ServerResult, ServerPagedResult } from '@/common/api/types/result.types'

export function mapValue<T, R>(result: ServerResult<T>, fn: (dto: T) => R): ServerResult<R> {
  return result.isSuccess && result.value != null
    ? { ...result, value: fn(result.value) }
    : result as unknown as ServerResult<R>
}

export function mapItems<T, R>(result: ServerPagedResult<T>, fn: (dto: T) => R): ServerPagedResult<R> {
  return result.isSuccess && result.items
    ? { ...result, items: result.items.map(fn) }
    : result as unknown as ServerPagedResult<R>
}
