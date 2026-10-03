import type { Result } from '@/core/models/result'
import type { ReturnRequest, CreateReturnRequest } from '../../types'

export interface IReturnRequestService {
  createReturnRequest(request: CreateReturnRequest): Promise<Result<ReturnRequest>>
  getReturnRequest(id: string): Promise<Result<ReturnRequest>>
  getReturnRequestsByOrder(orderId: string): Promise<Result<ReturnRequest[]>>
  cancelReturnRequest(id: string): Promise<Result<void>>
  getReturnLabels(returnId: string): Promise<Result<string>>
}