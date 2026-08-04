import type { PaymentIntentResponse } from '../../types/response'
import type { IPaymentIntentRepository } from './payment-intent.repository.interface'
import type { CreatePaymentIntentParams } from '../../services/payment-intent/payment-intent.service.interface'
import type { Result } from '@/core/models/result'

let mockPaymentIntents: PaymentIntentResponse[] = []

export class MockPaymentIntentRepository implements IPaymentIntentRepository {
  static reset() {
    mockPaymentIntents = []
  }

  async create(params: CreatePaymentIntentParams): Promise<Result<PaymentIntentResponse>> {
    const intent: PaymentIntentResponse = {
      id: `pi-${Date.now()}`,
      amount: params.amount,
      currency: params.currency,
      status: 'pending',
    }
    mockPaymentIntents.push(intent)
    return { isSuccess: true, isFailure: false, statusCode: 201, data: intent }
  }

  async getById<T = PaymentIntentResponse>(id: string): Promise<Result<T>> {
    const intent = mockPaymentIntents.find(i => i.id === id)
    if (!intent) {
      return { isSuccess: false, isFailure: true, statusCode: 404, message: 'Payment intent not found' }
    }
    return { isSuccess: true, isFailure: false, statusCode: 200, data: intent as T }
  }

  async confirm(paymentIntentId: string, _paymentMethodId: string): Promise<Result<PaymentIntentResponse>> {
    const intent = mockPaymentIntents.find(i => i.id === paymentIntentId)
    if (!intent) {
      return { isSuccess: false, isFailure: true, statusCode: 404, message: 'Payment intent not found' }
    }
    intent.status = 'succeeded'
    return { isSuccess: true, isFailure: false, statusCode: 200, data: intent }
  }
}

export const mockPaymentIntentRepository = new MockPaymentIntentRepository()