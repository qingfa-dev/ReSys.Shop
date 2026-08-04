import type { QueryingParameters } from '@/shared/types/querying'
import { queryingModelToParams, queryingParamsToModel } from '@/shared/types/querying'
import type { PagedResult } from '@/shared/types/result'
import { pagedFailure } from '@/shared/types/result'
import { get, HttpError } from './client'
import type { ApiError } from '@/shared/types/error'

export interface PagedRequestOptions {
  allowedFilterFields?: string[]
  allowedSortFields?: string[]
  allowedSearchFields?: string[]
  signal?: AbortSignal
  headers?: Record<string, string>
}

function buildSearchParams(params: QueryingParameters): URLSearchParams {
  const sp = new URLSearchParams()
  if (params.filter) sp.set('filter', params.filter)
  if (params.search) sp.set('search', params.search)
  if (params.searchFields?.length) sp.set('searchFields', params.searchFields.join(','))
  if (params.searchMode) sp.set('searchMode', params.searchMode)
  if (params.sort?.length) params.sort.forEach(s => sp.append('sort', s))
  if (params.pageNumber != null) sp.set('page', String(params.pageNumber))
  if (params.pageSize != null) sp.set('pageSize', String(params.pageSize))
  return sp
}

export async function getPaged<T>(
  url: string,
  params: QueryingParameters,
  options?: PagedRequestOptions,
): Promise<PagedResult<T>> {
  const parsed = queryingParamsToModel(
    params,
    options?.allowedFilterFields ?? null,
    options?.allowedSortFields ?? null,
    options?.allowedSearchFields ?? null,
  )

  if (!parsed.isSuccess) {
    return pagedFailure<T>(parsed.errors, parsed.statusCode)
  }

  const qp = queryingModelToParams(parsed.value)
  const searchParams = buildSearchParams(qp)
  const qs = searchParams.toString()

  try {
    const sep = url.includes('?') ? '&' : '?'
    const fullUrl = qs ? `${url}${sep}${qs}` : url
    return await get<PagedResult<T>>(fullUrl, { signal: options?.signal, headers: options?.headers })
  } catch (e) {
    if (e instanceof HttpError) {
      return pagedFailure<T>(e.errors, e.statusCode)
    }
    const apiError: ApiError = {
      code: 'NetworkError',
      message: e instanceof Error ? e.message : 'Network request failed.',
      type: 500,
    }
    return pagedFailure<T>([apiError])
  }
}
