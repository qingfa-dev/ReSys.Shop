export interface CustomActionRequest<TPayload = unknown> {
  action: string
  payload?: TPayload
}

export interface CustomActionResponse<TResult = unknown> {
  success: boolean
  result?: TResult
  message?: string
}

export function createCustomActionRequest<TPayload>(action: string, payload?: TPayload): CustomActionRequest<TPayload> {
  return { action, payload }
}

export function isCustomActionSuccess(response: CustomActionResponse): boolean {
  return response.success === true
}