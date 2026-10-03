import { returnRequestApiRepository } from '../../repositories/return-request/return-request.api'
import { mockReturnRequestRepository } from '../../repositories/return-request/return-request.mock.repository'
import type { IReturnRequestService } from './return-request.service.interface'
import type { ReturnRequest, CreateReturnRequest, ReturnRequestResponse, CreateReturnRequestResponse } from '../../types'
import type { Result } from '@/core/models/result'
import { toReturnRequest } from '../../mapping'
import { resultMap } from '@/core/utils/result-helpers'

const USE_MOCK = true

export class ReturnRequestService implements IReturnRequestService {
  private readonly repository = USE_MOCK ? mockReturnRequestRepository : returnRequestApiRepository

  async createReturnRequest(request: CreateReturnRequest): Promise<Result<ReturnRequest>> {
    const response = await this.repository.create(request as CreateReturnRequestResponse)
    return resultMap(response, toReturnRequest)
  }

  async getReturnRequest(id: string): Promise<Result<ReturnRequest>> {
    const response = await this.repository.getById(id)
    return resultMap(response, toReturnRequest)
  }

  async getReturnRequestsByOrder(orderId: string): Promise<Result<ReturnRequest[]>> {
    const response = await this.repository.getAll(orderId)
    if (response.isFailure) {
      return response as unknown as Result<ReturnRequest[]>
    }
    return resultMap(response, (data) => data.map(toReturnRequest))
  }

  async cancelReturnRequest(id: string): Promise<Result<void>> {
    return (await this.repository.cancel(id)) as unknown as Result<void>
  }

  async getReturnLabels(returnId: string): Promise<Result<string>> {
    return (await this.repository.getLabels(returnId)) as unknown as Result<string>
  }
}

export const returnRequestService = new ReturnRequestService()
