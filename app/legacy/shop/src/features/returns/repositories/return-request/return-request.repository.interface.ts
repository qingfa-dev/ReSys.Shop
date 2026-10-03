import type { Result } from '@/core/models/result'
import type { ReturnRequestResponse, CreateReturnRequestResponse } from '../../types/response'

export interface IReturnRequestRepository {
  getAll(orderId: string): Promise<Result<ReturnRequestResponse[]>>
  getById<T = ReturnRequestResponse>(id: string): Promise<Result<T>>
  create(request: CreateReturnRequestResponse): Promise<Result<ReturnRequestResponse>>
  cancel(id: string): Promise<Result<void>>
  getLabels(returnId: string): Promise<Result<string>>
}