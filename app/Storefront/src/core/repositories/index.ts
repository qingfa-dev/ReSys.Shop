import type { AxiosInstance } from 'axios'
import type { Result, PagedResult } from '../models/result'
import type { PagingParams, FilterParams, SearchParams, SortParams } from '../models'
import type { BulkActionRequest, BulkActionResponse } from '../models/bulk-action.model'
import type { FileUploadResponse } from '../models/file-upload.model'
import type { CustomActionResponse } from '../models/custom-action.model'
import { httpClient } from '@/core/http'
import type { IRepository } from './IRepository'

export * from './IRepository'

export type { Result, PagedResult }
export type { PagingParams, FilterParams, SearchParams, SortParams }
export type { BulkActionRequest, BulkActionResponse }
export type { FileUploadResponse }
export type { CustomActionResponse }

export class BaseRepository implements IRepository {
  constructor(
    protected readonly client: AxiosInstance = httpClient,
    protected readonly baseUrl: string = ''
  ) { }

  async get<T>(url: string, params?: FilterParams): Promise<Result<T>> {
    try {
      const response = await this.client.get<Result<T>>(url, { params })
      return response.data
    } catch (error) {
      return this.handleError(error)
    }
  }

  async getPaged<T>(
    url: string,
    paging?: PagingParams,
    filter?: FilterParams,
    search?: SearchParams,
    sort?: SortParams
  ): Promise<PagedResult<T>> {
    try {
      const params: Record<string, unknown> = {}

      if (paging) {
        params.page = paging.page ?? 1
        params.pageSize = paging.pageSize ?? 10
      }

      if (filter?.filter) {
        params.filter = filter.filter
      }

      if (search?.search) {
        params.search = search.search
        if (search.searchFields?.length) {
          params.searchFields = search.searchFields.join(',')
        }
      }

      if (sort?.sortBy) {
        params.sortBy = sort.sortBy
        params.sortOrder = sort.sortOrder ?? 'asc'
      }

      const response = await this.client.get<PagedResult<T>>(url, { params })
      return response.data
    } catch (error) {
      return this.handlePagedError(error)
    }
  }

  async post<T>(url: string, data?: unknown): Promise<Result<T>> {
    try {
      const response = await this.client.post<Result<T>>(url, data)
      return response.data
    } catch (error) {
      return this.handleError(error)
    }
  }

  async put<T>(url: string, data?: unknown): Promise<Result<T>> {
    try {
      const response = await this.client.put<Result<T>>(url, data)
      return response.data
    } catch (error) {
      return this.handleError(error)
    }
  }

  async patch<T>(url: string, data?: unknown): Promise<Result<T>> {
    try {
      const response = await this.client.patch<Result<T>>(url, data)
      return response.data
    } catch (error) {
      return this.handleError(error)
    }
  }

  async delete<T>(url: string): Promise<Result<T>> {
    try {
      const response = await this.client.delete<Result<T>>(url)
      return response.data
    } catch (error) {
      return this.handleError(error)
    }
  }

  async getById<T>(url: string, id: string): Promise<Result<T>> {
    return this.get<T>(`${url}/${id}`)
  }

  async bulkAction<TPayload = unknown>(
    url: string,
    request: BulkActionRequest<TPayload>
  ): Promise<Result<BulkActionResponse>> {
    try {
      const response = await this.client.post<Result<BulkActionResponse>>(`${url}/bulk/${request.action}`, request)
      return response.data
    } catch (error) {
      return this.handleError(error)
    }
  }

  async bulkDelete(url: string, ids: string[]): Promise<Result<BulkActionResponse>> {
    return this.bulkAction(url, { ids, action: 'delete' })
  }

  async bulkPatch<T>(url: string, ids: string[], updates: Partial<T>): Promise<Result<BulkActionResponse>> {
    return this.bulkAction(url, { ids, action: 'patch', payload: updates })
  }

  async patchPartial<T>(url: string, id: string, fieldsToUpdate: Partial<T>): Promise<Result<T>> {
    try {
      const response = await this.client.patch<Result<T>>(`${url}/${id}`, fieldsToUpdate)
      return response.data
    } catch (error) {
      return this.handleError(error)
    }
  }

  async uploadFile(
    url: string,
    id: string,
    file: File,
    fieldName: string,
    metadata?: Record<string, unknown>
  ): Promise<Result<FileUploadResponse>> {
    try {
      const formData = new FormData()
      formData.append(fieldName, file)

      if (metadata) {
        formData.append('metadata', JSON.stringify(metadata))
      }

      const response = await this.client.post<Result<FileUploadResponse>>(
        `${url}/${id}/upload`,
        formData,
        { headers: { 'Content-Type': 'multipart/form-data' } }
      )
      return response.data
    } catch (error) {
      return this.handleError(error)
    }
  }

  async executeAction<TPayload = unknown, TResult = unknown>(
    url: string,
    id: string,
    action: string,
    payload?: TPayload
  ): Promise<Result<CustomActionResponse<TResult>>> {
    try {
      const response = await this.client.post<Result<CustomActionResponse<TResult>>>(
        `${url}/${id}/actions/${action}`,
        payload
      )
      return response.data
    } catch (error) {
      return this.handleError(error)
    }
  }

  protected handleError(error: unknown): Result<never> {
    const axiosError = error as { response?: { status: number; data?: { message?: string } }; message?: string }

    return {
      isSuccess: false,
      isFailure: true,
      statusCode: axiosError.response?.status || 500,
      message: axiosError.response?.data?.message || axiosError.message || 'An unexpected error occurred',
    }
  }

  protected handlePagedError(error: unknown): PagedResult<never> {
    const axiosError = error as { response?: { status: number; data?: { message?: string } }; message?: string }

    return {
      isSuccess: false,
      isFailure: true,
      statusCode: axiosError.response?.status || 500,
      items: [],
      page: 1,
      pageSize: 10,
      totalCount: 0,
      totalPages: 0,
      hasNextPage: false,
      hasPreviousPage: false,
    }
  }
}

export abstract class BasePagedRepository<TKey, TEntity, TQuery = unknown> {
  constructor(
    protected readonly httpClient: AxiosInstance,
    protected readonly endpoint: string
  ) { }

  async getPaged(_query?: TQuery): Promise<PagedResult<TEntity>> {
    throw new Error('Method not implemented.')
  }

  async getById(_id: TKey): Promise<Result<TEntity>> {
    throw new Error('Method not implemented.')
  }

  protected handleError(error: unknown): Result<never> {
    const axiosError = error as { response?: { status: number; data?: { message?: string } }; message?: string }

    return {
      isSuccess: false,
      isFailure: true,
      statusCode: axiosError.response?.status || 500,
      message: axiosError.response?.data?.message || axiosError.message || 'An unexpected error occurred',
    }
  }

  protected handlePagedError(error: unknown): PagedResult<TEntity> {
    const axiosError = error as { response?: { status: number; data?: { message?: string } }; message?: string }

    return {
      isSuccess: false,
      isFailure: true,
      statusCode: axiosError.response?.status || 500,
      items: [],
      page: 1,
      pageSize: 10,
      totalCount: 0,
      totalPages: 0,
      hasNextPage: false,
      hasPreviousPage: false,
    }
  }
}
