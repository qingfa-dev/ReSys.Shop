import { paymentMethodApiRepository } from '../../repositories/payment-method/payment-method.api'
import type { IPaymentMethodService } from './payment-method.service.interface'
import type { PaymentMethod } from '../../types'
import type { Result } from '@/core/models/result'
import { succeed, fail } from '@/core/utils/result-helpers'

export class PaymentMethodService implements IPaymentMethodService {
  private paymentRepo = paymentMethodApiRepository

  async getPaymentMethods(): Promise<Result<PaymentMethod[]>> {
    const response = await this.paymentRepo.getAll()
    if (response.isFailure) {
      return fail(response.message ?? 'Failed to get payment methods', response.statusCode, response.errors)
    }
    return succeed(response.data!.map(pm => ({
      id: pm.id,
      name: pm.name,
      type: pm.providerKey,          // backend has no type enum — providerKey carries the method kind
      description: pm.description,
      providerKey: pm.providerKey,
      code: pm.code,
    })), response.statusCode)
  }
}

export const paymentMethodService = new PaymentMethodService()