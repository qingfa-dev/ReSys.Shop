export interface BulkActionRequest<TPayload = unknown> {
  ids: string[]
  action: string
  payload?: TPayload
}

export interface BulkActionResponse {
  success: boolean
  processedCount: number
  failedCount: number
  errors: { id: string; message: string }[]
}

export function createBulkDeleteRequest(ids: string[]): BulkActionRequest {
  return { ids, action: 'delete' }
}

export function createBulkActionRequest<TPayload>(ids: string[], action: string, payload?: TPayload): BulkActionRequest<TPayload> {
  return { ids, action, payload }
}