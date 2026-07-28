import { BaseRepository } from '@/core/repositories'
import type { Result } from '@/core/models/result'
import type { PaymentIntentResponse } from '../../types/response'
import type { IPaymentIntentRepository } from './payment-intent.repository.interface'

export class PaymentIntentApiRepository extends BaseRepository implements IPaymentIntentRepository {
  async create(amount: number, currency = 'USD'): Promise<Result<PaymentIntentResponse>> {
    return this.post<PaymentIntentResponse>('/api/storefront/payment/create-intent', { amount, currency })
  }

  async getById<T = PaymentIntentResponse>(_id: string): Promise<Result<T>> {
    return { isSuccess: false, isFailure: true, statusCode: 501, message: 'Payment intent lookup not available via API' } as Result<T>
  }

  async confirm(paymentIntentId: string, paymentMethodId: string): Promise<Result<PaymentIntentResponse>> {
    return this.post<PaymentIntentResponse>(`/api/storefront/payment/confirm/${paymentIntentId}`, { paymentMethodId })
  }

}

export const paymentIntentApiRepository = new PaymentIntentApiRepository()