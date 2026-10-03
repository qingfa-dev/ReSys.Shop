import type { Result } from '@/core/models/result'
import type { PaymentMethod } from '../../types'

export interface IPaymentMethodService {
  getPaymentMethods(): Promise<Result<PaymentMethod[]>>
}