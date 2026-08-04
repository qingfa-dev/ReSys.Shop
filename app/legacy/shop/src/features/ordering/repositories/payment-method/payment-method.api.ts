import { BaseRepository } from '@/core/repositories'
import type { Result } from '@/core/models/result'
import type { PaymentMethodResponse } from '../../types/response'
import type { IPaymentMethodRepository } from './payment-method.repository.interface'

export class PaymentMethodApiRepository extends BaseRepository implements IPaymentMethodRepository {
  async getAll(): Promise<Result<PaymentMethodResponse[]>> {
    return this.get<PaymentMethodResponse[]>('/ordering/payment-methods')
  }

  getById<T = PaymentMethodResponse>(id: string): Promise<Result<T>> {
    return super.getById<T>('/ordering/payment-methods', id)
  }
}

export const paymentMethodApiRepository = new PaymentMethodApiRepository()