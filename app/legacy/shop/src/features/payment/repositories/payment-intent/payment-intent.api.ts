import { BaseRepository } from '@/core/repositories'
import type { Result } from '@/core/models/result'
import type { PaymentIntentResponse } from '../../types/response'
import type { IPaymentIntentRepository } from './payment-intent.repository.interface'

export class PaymentIntentApiRepository extends BaseRepository implements IPaymentIntentRepository {
  async create(amount: number, currency = 'USD'): Promise<Result<PaymentIntentResponse>> {
    return this.post<PaymentIntentResponse>('/payment/intents', { amount, currency })
  }

  async getById<T = PaymentIntentResponse>(id: string): Promise<Result<T>> {
    return this.get<T>(`/payment/intents/${id}`)
  }

  async confirm(paymentIntentId: string, paymentMethodId: string): Promise<Result<PaymentIntentResponse>> {
    return this.post<PaymentIntentResponse>(`/payment/intents/${paymentIntentId}/confirm`, { paymentMethodId })
  }

  async cancel(paymentIntentId: string): Promise<Result<void>> {
    return this.post<void>(`/payment/intents/${paymentIntentId}/cancel`)
  }
}

export const paymentIntentApiRepository = new PaymentIntentApiRepository()