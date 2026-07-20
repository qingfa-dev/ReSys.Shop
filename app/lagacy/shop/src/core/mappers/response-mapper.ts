import type { Result, PagedResult } from '../models/result'

export type ResponseToEntityMapper<TResponse, TEntity> = (response: TResponse) => TEntity

export function mapResponseToEntity<TResponse, TEntity>(
  response: TResponse,
  mapper: ResponseToEntityMapper<TResponse, TEntity>
): TEntity {
  return mapper(response)
}

export function mapResponseToResult<TResponse, TEntity>(
  response: TResponse,
  mapper: ResponseToEntityMapper<TResponse, TEntity>
): Result<TEntity> {
  try {
    return {
      isSuccess: true,
      isFailure: false,
      statusCode: 200,
      data: mapper(response),
    }
  } catch (error) {
    return {
      isSuccess: false,
      isFailure: true,
      statusCode: 500,
      message: 'Mapping failed',
      errors: [{ code: 'MAPPING_ERROR', description: error instanceof Error ? error.message : 'Unknown error' }],
    }
  }
}

export function mapResponseListToEntityList<TResponse, TEntity>(
  responses: TResponse[],
  mapper: ResponseToEntityMapper<TResponse, TEntity>
): TEntity[] {
  return responses.map(mapper)
}

export function mapResponseListToResult<TResponse, TEntity>(
  responses: TResponse[],
  mapper: ResponseToEntityMapper<TResponse, TEntity>
): Result<TEntity[]> {
  try {
    return {
      isSuccess: true,
      isFailure: false,
      statusCode: 200,
      data: responses.map(mapper),
    }
  } catch (error) {
    return {
      isSuccess: false,
      isFailure: true,
      statusCode: 500,
      message: 'Mapping failed',
      errors: [{ code: 'MAPPING_ERROR', description: error instanceof Error ? error.message : 'Unknown error' }],
    }
  }
}

export function mapPagedResponseToEntity<TResponse, TEntity>(
  response: TResponse & { items: TResponse[] },
  mapper: ResponseToEntityMapper<TResponse, TEntity>
): PagedResult<TEntity> {
  return {
    isSuccess: true,
    isFailure: false,
    statusCode: 200,
    items: response.items.map(mapper),
    page: (response as unknown as { page: number }).page ?? 1,
    pageSize: (response as unknown as { pageSize: number }).pageSize ?? 10,
    totalCount: (response as unknown as { totalCount: number }).totalCount ?? response.items.length,
    totalPages: (response as unknown as { totalPages: number }).totalPages ?? 1,
    hasNextPage: (response as unknown as { hasNextPage: boolean }).hasNextPage ?? false,
    hasPreviousPage: (response as unknown as { hasPreviousPage: boolean }).hasPreviousPage ?? false,
  }
}

export function mapPagedResponseToResult<TResponse, TEntity>(
  response: TResponse & { items: TResponse[] },
  mapper: ResponseToEntityMapper<TResponse, TEntity>
): Result<PagedResult<TEntity>> {
  try {
    return {
      isSuccess: true,
      isFailure: false,
      statusCode: 200,
      data: mapPagedResponseToEntity(response, mapper),
    }
  } catch (error) {
    return {
      isSuccess: false,
      isFailure: true,
      statusCode: 500,
      message: 'Mapping failed',
      errors: [{ code: 'MAPPING_ERROR', description: error instanceof Error ? error.message : 'Unknown error' }],
    }
  }
}

export function createMapper<TResponse, TEntity>(mapFn: (response: TResponse) => TEntity): ResponseToEntityMapper<TResponse, TEntity> {
  return mapFn
}

export function composeMappers<T1, T2, T3>(first: (r: T1) => T2, second: (r: T2) => T3): (r: T1) => T3 {
  return (response: T1) => second(first(response))
}