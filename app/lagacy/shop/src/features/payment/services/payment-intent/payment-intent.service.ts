import { paymentIntentApiRepository } from '../../repositories/payment-intent/payment-intent.api'
import { mockPaymentIntentRepository } from '../../repositories/payment-intent/payment-intent.mock.repository'
import type { IPaymentIntentService } from './payment-intent.service.interface'
import type { PaymentIntent } from '../../types'
import type { Result } from '@/core/models/result'
import { toPaymentIntent } from '../../mapping'
import { resultMap, succeed, fail } from '@/core/utils/result-helpers'

const USE_MOCK = true

export class PaymentIntentService implements IPaymentIntentService {
  private readonly paymentIntentRepository = USE_MOCK ? mockPaymentIntentRepository : paymentIntentApiRepository

  async createPaymentIntent(amount: number, currency: string): Promise<Result<PaymentIntent>> {
    const response = await this.paymentIntentRepository.create(amount, currency)
    return resultMap(response, toPaymentIntent)
  }

  async getPaymentIntent(id: string): Promise<Result<PaymentIntent>> {
    const response = await this.paymentIntentRepository.getById(id)
    return resultMap(response, toPaymentIntent)
  }

  async confirmPayment(paymentIntentId: string, paymentMethodId: string): Promise<Result<PaymentIntent>> {
    const response = await this.paymentIntentRepository.confirm(paymentIntentId, paymentMethodId)
    return resultMap(response, toPaymentIntent)
  }

  async cancelPayment(paymentIntentId: string): Promise<Result<void>> {
    const response = await this.paymentIntentRepository.cancel(paymentIntentId)
    if (response.isFailure) {
      return fail(response.message ?? 'Failed to cancel payment', response.statusCode, response.errors)
    }
    return succeed(undefined, response.statusCode)
  }
}

export const paymentIntentService = new PaymentIntentService()