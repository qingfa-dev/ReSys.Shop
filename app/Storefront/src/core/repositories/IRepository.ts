/**
 * Unified Repository interface defining standard CRUD operations + advanced features.
 * Abstract layer for data access operations - matches admin module pattern.
 *
 * Advanced features include:
 * - Bulk actions (delete, patch, custom)
 * - Partial updates (PATCH)
 * - File uploads
 * - Custom domain-specific actions
 */
import type { Result, PagedResult } from '../models/result'
import type { PagingParams, FilterParams, SearchParams, SortParams } from '../models'
import type { BulkActionRequest, BulkActionResponse } from '../models/bulk-action.model'
import type { FileUploadResponse } from '../models/file-upload.model'
import type { CustomActionResponse } from '../models/custom-action.model'

export interface IRepository {
  // Standard CRUD
  get<T>(url: string, params?: FilterParams): Promise<Result<T>>
  getPaged<T>(url: string, paging?: PagingParams, filter?: FilterParams, search?: SearchParams, sort?: SortParams): Promise<PagedResult<T>>
  post<T>(url: string, data?: unknown): Promise<Result<T>>
  put<T>(url: string, data?: unknown): Promise<Result<T>>
  patch<T>(url: string, data?: unknown): Promise<Result<T>>
  delete<T>(url: string): Promise<Result<T>>

  // Convenience methods
  getById<T>(url: string, id: string): Promise<Result<T>>

  // Advanced: Bulk Operations
  bulkAction<TPayload = unknown>(url: string, request: BulkActionRequest<TPayload>): Promise<Result<BulkActionResponse>>
  bulkDelete(url: string, ids: string[]): Promise<Result<BulkActionResponse>>
  bulkPatch<T>(url: string, ids: string[], updates: Partial<T>): Promise<Result<BulkActionResponse>>

  // Advanced: Partial Updates
  patchPartial<T>(url: string, id: string, fieldsToUpdate: Partial<T>): Promise<Result<T>>

  // Advanced: File Operations
  uploadFile(url: string, id: string, file: File, fieldName: string, metadata?: Record<string, unknown>): Promise<Result<FileUploadResponse>>

  // Advanced: Custom Actions
  executeAction<TPayload = unknown, TResult = unknown>(url: string, id: string, action: string, payload?: TPayload): Promise<Result<CustomActionResponse<TResult>>>
}