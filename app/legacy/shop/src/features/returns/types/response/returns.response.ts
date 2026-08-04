import type { Result } from '@/core/models/result'
import type { ReturnRequestSchemaType, CreateReturnRequestSchemaType } from '../schemas'

export interface ReturnRequestResponse extends ReturnRequestSchemaType {}
export interface CreateReturnRequestResponse extends CreateReturnRequestSchemaType {}

export interface CreateReturnResponse {
  returnRequest: ReturnRequestResponse
  createdAt: string
}

export interface GetReturnByIdResponse {
  returnRequest: ReturnRequestResponse
}

export interface GetReturnRequestsByOrderResponse {
  returnRequests: ReturnRequestResponse[]
  totalCount: number
}

export interface CancelReturnResponse {
  success: boolean
  returnRequest: ReturnRequestResponse
  cancelledAt: string
}

export interface GetReturnLabelsResponse {
  returnId: string
  labelUrl: string
}

export type ReturnSingleResponse = Result<ReturnRequestResponse>
export type ReturnListResponse = Result<ReturnRequestResponse[]>