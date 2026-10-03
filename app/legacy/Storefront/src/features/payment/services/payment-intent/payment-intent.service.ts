import { paymentIntentApiRepository } from '../../repositories/payment-intent/payment-intent.api'
import type { IPaymentIntentService, CreatePaymentIntentParams } from './payment-intent.service.interface'
import type { PaymentIntent } from '../../types'
import type { Result } from '@/core/models/result'
import { toPaymentIntent } from '../../mapping'
import { resultMap } from '@/core/utils/result-helpers'

export class PaymentIntentService implements IPaymentIntentService {
  private readonly paymentIntentRepository = paymentIntentApiRepository

  async createPaymentIntent(params: CreatePaymentIntentParams): Promise<Result<PaymentIntent>> {
    const response = await this.paymentIntentRepository.create(params)
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
}

export const paymentIntentService = new PaymentIntentService()