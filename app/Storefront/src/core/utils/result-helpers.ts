import type { Result, PagedResult } from '../models/result'

export function resultMap<T, TResult>(result: Result<T>, fn: (value: T) => TResult): Result<TResult> {
  if (result.isFailure) {
    return {
      isSuccess: false,
      isFailure: true,
      statusCode: result.statusCode,
      message: result.message,
      errors: result.errors,
    }
  }
  return {
    isSuccess: true,
    isFailure: false,
    statusCode: result.statusCode,
    data: fn(result.data!),
  }
}

export function resultFlatMap<T, TResult>(result: Result<T>, fn: (value: T) => Result<TResult>): Result<TResult> {
  if (result.isFailure) {
    return {
      isSuccess: false,
      isFailure: true,
      statusCode: result.statusCode,
      message: result.message,
      errors: result.errors,
    }
  }
  return fn(result.data!)
}

export function resultTraverse<T, TResult>(items: T[], fn: (item: T) => Result<TResult>): Result<TResult[]> {
  const results: TResult[] = []
  for (const item of items) {
    const result = fn(item)
    if (result.isFailure) {
      return {
        isSuccess: false,
        isFailure: true,
        statusCode: result.statusCode,
        message: result.message,
        errors: result.errors,
      }
    }
    results.push(result.data!)
  }
  return {
    isSuccess: true,
    isFailure: false,
    statusCode: 200,
    data: results,
  }
}

export function fromNullable<T>(value: T | null | undefined, errorMessage = 'Value is null or undefined', statusCode = 404): Result<T> {
  if (value === null || value === undefined) {
    return {
      isSuccess: false,
      isFailure: true,
      statusCode,
      message: errorMessage,
      errors: [{ code: 'NULL_VALUE', description: errorMessage }],
    }
  }
  return {
    isSuccess: true,
    isFailure: false,
    statusCode: 200,
    data: value,
  }
}

export function fromPromise<T>(promise: Promise<T>, errorMessage = 'Operation failed', statusCode = 500): Promise<Result<T>> {
  return promise
    .then((value) => ({
      isSuccess: true,
      isFailure: false,
      statusCode: 200,
      data: value,
    }))
    .catch((error) => ({
      isSuccess: false,
      isFailure: true,
      statusCode,
      message: errorMessage,
      errors: [{ code: 'UNKNOWN', description: error instanceof Error ? error.message : errorMessage }],
    }))
}

export function succeed<T>(data: T, statusCode = 200): Result<T> {
  return {
    isSuccess: true,
    isFailure: false,
    statusCode,
    data,
  }
}

export function fail<T>(message: string, statusCode = 400, errors?: Result<T>['errors']): Result<T> {
  return {
    isSuccess: false,
    isFailure: true,
    statusCode,
    message,
    errors: errors ?? [{ code: 'ERROR', description: message }],
  }
}

export function resultAll<T extends Result<unknown>[]>(...results: T): Result<{ [K in keyof T]: T[K] extends Result<infer U> ? U : never }> {
  const data: unknown[] = []
  for (const result of results) {
    if (result.isFailure) {
      return {
        isSuccess: false,
        isFailure: true,
        statusCode: result.statusCode,
        message: result.message,
        errors: result.errors,
      }
    }
    data.push(result.data)
  }
  return {
    isSuccess: true,
    isFailure: false,
    statusCode: 200,
    data: data as { [K in keyof T]: T[K] extends Result<infer U> ? U : never },
  }
}

export function resultPartition<T>(items: T[], predicate: (item: T) => boolean): [T[], T[]] {
  const pass: T[] = []
  const fail: T[] = []
  for (const item of items) {
    if (predicate(item)) {
      pass.push(item)
    } else {
      fail.push(item)
    }
  }
  return [pass, fail]
}

export function resultFilterMap<T, TResult>(items: T[], fn: (item: T) => Result<TResult>): Result<TResult[]> {
  const results: TResult[] = []
  for (const item of items) {
    const result = fn(item)
    if (result.isFailure) {
      return {
        isSuccess: false,
        isFailure: true,
        statusCode: result.statusCode,
        message: result.message,
        errors: result.errors,
      }
    }
    if (result.data !== undefined) {
      results.push(result.data)
    }
  }
  return {
    isSuccess: true,
    isFailure: false,
    statusCode: 200,
    data: results,
  }
}