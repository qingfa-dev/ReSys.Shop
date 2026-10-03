import type { ReturnRequestResponse, CreateReturnRequestResponse } from '../../types/response'
import type { IReturnRequestRepository } from './return-request.repository.interface'
import type { Result } from '@/core/models/result'

const initialReturnRequests: ReturnRequestResponse[] = [
  { id: 'return-1', orderId: 'order-1', items: [{ orderItemId: 'item-1', quantity: 1, reason: 'defective' }], status: 'approved', refundAmount: 50, refundMethod: 'original', createdAt: '2024-01-01T00:00:00Z', updatedAt: '2024-01-02T00:00:00Z' },
  { id: 'return-2', orderId: 'order-2', items: [{ orderItemId: 'item-2', quantity: 2, reason: 'wrong-size' }], status: 'pending', refundAmount: 0, refundMethod: 'original', createdAt: '2024-01-03T00:00:00Z', updatedAt: '2024-01-03T00:00:00Z' },
]

const mockReturnRequests: ReturnRequestResponse[] = JSON.parse(JSON.stringify(initialReturnRequests))

export class MockReturnRequestRepository implements IReturnRequestRepository {
  static reset() {
    mockReturnRequests.length = 0
    initialReturnRequests.forEach(r => mockReturnRequests.push({ ...r }))
  }

  async getAll(orderId: string): Promise<Result<ReturnRequestResponse[]>> {
    const returns = mockReturnRequests.filter(r => r.orderId === orderId)
    return { isSuccess: true, isFailure: false, statusCode: 200, data: returns }
  }

  async getById<T = ReturnRequestResponse>(id: string): Promise<Result<T>> {
    const returnRequest = mockReturnRequests.find(r => r.id === id)
    if (!returnRequest) {
      return { isSuccess: false, isFailure: true, statusCode: 404, message: 'Return request not found' }
    }
    return { isSuccess: true, isFailure: false, statusCode: 200, data: returnRequest as T }
  }

  async create(request: CreateReturnRequestResponse): Promise<Result<ReturnRequestResponse>> {
    const newReturn: ReturnRequestResponse = {
      id: `return-${Date.now()}`,
      orderId: request.orderId,
      items: request.items,
      status: 'pending',
      refundAmount: 0,
      refundMethod: request.refundMethod || 'original',
      createdAt: new Date().toISOString(),
      updatedAt: new Date().toISOString(),
    }
    mockReturnRequests.push(newReturn)
    return { isSuccess: true, isFailure: false, statusCode: 201, data: newReturn }
  }

  async cancel(id: string): Promise<Result<void>> {
    const returnRequest = mockReturnRequests.find(r => r.id === id)
    if (!returnRequest) {
      return { isSuccess: false, isFailure: true, statusCode: 404, message: 'Return request not found' }
    }
    return { isSuccess: true, isFailure: false, statusCode: 200, data: undefined }
  }

  async getLabels(returnId: string): Promise<Result<string>> {
    const returnRequest = mockReturnRequests.find(r => r.id === returnId)
    if (!returnRequest) {
      return { isSuccess: false, isFailure: true, statusCode: 404, message: 'Return request not found' }
    }
    return { isSuccess: true, isFailure: false, statusCode: 200, data: `https://returns.example.com/label/${returnId}` }
  }
}

export const mockReturnRequestRepository = new MockReturnRequestRepository()