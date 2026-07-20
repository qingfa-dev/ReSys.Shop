import { paymentMethodApiRepository } from '../../repositories/payment-method/payment-method.api'
import { mockPaymentMethodRepository } from '../../repositories/payment-method/payment-method.mock.repository'
import type { IPaymentMethodService } from './payment-method.service.interface'
import type { PaymentMethod } from '../../types'
import type { Result } from '@/core/models/result'
import { succeed, fail } from '@/core/utils/result-helpers'

const USE_MOCK = true

export class PaymentMethodService implements IPaymentMethodService {
  private paymentRepo = USE_MOCK ? mockPaymentMethodRepository : paymentMethodApiRepository

  async getPaymentMethods(): Promise<Result<PaymentMethod[]>> {
    const response = await this.paymentRepo.getAll() as Result<PaymentMethod[]>
    if (response.isFailure) {
      return fail(response.message ?? 'Failed to get payment methods', response.statusCode, response.errors)
    }
    return succeed(response.data!.map(pm => ({
      id: pm.id,
      name: pm.name,
      type: pm.type as 'card' | 'paypal' | 'bank',
      last4: pm.lastFour ?? '',
      brand: pm.name,
      expiryMonth: 0,
      expiryYear: 0,
    })), response.statusCode)
  }
}

export const paymentMethodService = new PaymentMethodService()