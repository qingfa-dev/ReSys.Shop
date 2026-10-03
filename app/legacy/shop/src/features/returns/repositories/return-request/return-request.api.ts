import { BaseRepository } from '@/core/repositories'
import type { Result } from '@/core/models/result'
import type { ReturnRequestResponse, CreateReturnRequestResponse } from '../../types/response'
import type { IReturnRequestRepository } from './return-request.repository.interface'

export class ReturnRequestApiRepository extends BaseRepository implements IReturnRequestRepository {
  async getAll(orderId: string): Promise<Result<ReturnRequestResponse[]>> {
    return this.get<ReturnRequestResponse[]>(`/returns`, { filter: `orderId:${orderId}` })
  }

  async getById<T = ReturnRequestResponse>(id: string): Promise<Result<T>> {
    return this.get<T>(`/returns/${id}`)
  }

  async create(request: CreateReturnRequestResponse): Promise<Result<ReturnRequestResponse>> {
    return this.post<ReturnRequestResponse>('/returns', request)
  }

  async cancel(id: string): Promise<Result<void>> {
    return this.post<void>(`/returns/${id}/cancel`)
  }

  async getLabels(returnId: string): Promise<Result<string>> {
    return this.get<string>(`/returns/${returnId}/labels`)
  }
}

export const returnRequestApiRepository = new ReturnRequestApiRepository()