import type { Result, PagedResult } from '../models/result'

export interface ResultExtensions<T> {
  map<TResult>(fn: (value: T) => TResult): Result<TResult>
  flatMap<TResult>(fn: (value: T) => Result<TResult>): Result<TResult>
  then<TResult>(fn: (value: T) => Result<TResult>): Result<TResult>
  match<TResult>(onSuccess: (value: T) => TResult, onFailure: (errors: Result<T>['errors']) => TResult): TResult
  tap(fn: (value: T) => void): Result<T>
  recover(fn: (errors: Result<T>['errors']) => T): Result<T>
}

export interface AsyncResultExtensions<T> {
  mapAsync<TResult>(fn: (value: T) => Promise<TResult>): Promise<Result<TResult>>
  flatMapAsync<TResult>(fn: (value: T) => Promise<Result<TResult>>): Promise<Result<TResult>>
  thenAsync<TResult>(fn: (value: T) => Promise<Result<TResult>>): Promise<Result<TResult>>
  matchAsync<TResult>(onSuccess: (value: T) => Promise<TResult>, onFailure: (errors: Result<T>['errors']) => Promise<TResult>): Promise<TResult>
  tapAsync(fn: (value: T) => Promise<void>): Promise<Result<T>>
}

export function withResultExtensions<T>(result: Result<T>): ResultExtensions<T> & Result<T> {
  return {
    ...result,
    map<TResult>(fn: (value: T) => TResult): Result<TResult> {
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
    },
    flatMap<TResult>(fn: (value: T) => Result<TResult>): Result<TResult> {
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
    },
    then<TResult>(fn: (value: T) => Result<TResult>): Result<TResult> {
      return withResultExtensions(result).flatMap(fn)
    },
    match<TResult>(onSuccess: (value: T) => TResult, onFailure: (errors: Result<T>['errors']) => TResult): TResult {
      if (result.isSuccess && result.data !== undefined) {
        return onSuccess(result.data)
      }
      return onFailure(result.errors)
    },
    tap(fn: (value: T) => void): Result<T> {
      if (result.isSuccess && result.data !== undefined) {
        fn(result.data)
      }
      return result
    },
    recover(fn: (errors: Result<T>['errors']) => T): Result<T> {
      if (result.isFailure) {
        return {
          isSuccess: true,
          isFailure: false,
          statusCode: result.statusCode,
          data: fn(result.errors),
        }
      }
      return result
    },
  }
}

export async function withAsyncResultExtensions<T>(result: Promise<Result<T>>): Promise<AsyncResultExtensions<T> & Result<T>> {
  const resolved = await result
  return {
    ...resolved,
    async mapAsync<TResult>(fn: (value: T) => Promise<TResult>): Promise<Result<TResult>> {
      if (resolved.isFailure) {
        return {
          isSuccess: false,
          isFailure: true,
          statusCode: resolved.statusCode,
          message: resolved.message,
          errors: resolved.errors,
        }
      }
      return {
        isSuccess: true,
        isFailure: false,
        statusCode: resolved.statusCode,
        data: await fn(resolved.data!),
      }
    },
    async flatMapAsync<TResult>(fn: (value: T) => Promise<Result<TResult>>): Promise<Result<TResult>> {
      if (resolved.isFailure) {
        return {
          isSuccess: false,
          isFailure: true,
          statusCode: resolved.statusCode,
          message: resolved.message,
          errors: resolved.errors,
        }
      }
      return fn(resolved.data!)
    },
    async thenAsync<TResult>(fn: (value: T) => Promise<Result<TResult>>): Promise<Result<TResult>> {
      if (resolved.isFailure) {
        return {
          isSuccess: false,
          isFailure: true,
          statusCode: resolved.statusCode,
          message: resolved.message,
          errors: resolved.errors,
        }
      }
      return fn(resolved.data!)
    },
    async matchAsync<TResult>(onSuccess: (value: T) => Promise<TResult>, onFailure: (errors: Result<T>['errors']) => Promise<TResult>): Promise<TResult> {
      if (resolved.isSuccess && resolved.data !== undefined) {
        return onSuccess(resolved.data)
      }
      return onFailure(resolved.errors)
    },
    async tapAsync(fn: (value: T) => Promise<void>): Promise<Result<T>> {
      if (resolved.isSuccess && resolved.data !== undefined) {
        await fn(resolved.data)
      }
      return resolved
    },
  }
}

export function mapResult<T, TResult>(result: Result<T>, fn: (value: T) => TResult): Result<TResult> {
  return withResultExtensions(result).map(fn)
}

export function flatMapResult<T, TResult>(result: Result<T>, fn: (value: T) => Result<TResult>): Result<TResult> {
  return withResultExtensions(result).flatMap(fn)
}

export function matchResult<T, TResult>(result: Result<T>, onSuccess: (value: T) => TResult, onFailure: (errors: Result<T>['errors']) => TResult): TResult {
  return withResultExtensions(result).match(onSuccess, onFailure)
}

export function tapResult<T>(result: Result<T>, fn: (value: T) => void): Result<T> {
  return withResultExtensions(result).tap(fn)
}

export function recoverResult<T>(result: Result<T>, fn: (errors: Result<T>['errors']) => T): Result<T> {
  return withResultExtensions(result).recover(fn)
}

export async function mapResultAsync<T, TResult>(result: Result<T>, fn: (value: T) => Promise<TResult>): Promise<Result<TResult>> {
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
    data: await fn(result.data!),
  }
}

export async function flatMapResultAsync<T, TResult>(result: Result<T>, fn: (value: T) => Promise<Result<TResult>>): Promise<Result<TResult>> {
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

export async function matchResultAsync<T, TResult>(result: Result<T>, onSuccess: (value: T) => Promise<TResult>, onFailure: (errors: Result<T>['errors']) => Promise<TResult>): Promise<TResult> {
  if (result.isSuccess && result.data !== undefined) {
    return onSuccess(result.data)
  }
  return onFailure(result.errors)
}