import type { PaymentMethodResponse } from '../../types/response'

export class MockPaymentMethodRepository {
  private methods: PaymentMethodResponse[] = [
    { id: 'pm-1', name: 'Credit Card', code: 'card', description: 'Pay with your credit card', providerKey: 'stripe_card' },
    { id: 'pm-2', name: 'Credit Card', code: 'card', description: 'Pay with your credit card', providerKey: 'stripe_card' },
    { id: 'pm-3', name: 'PayPal', code: 'paypal', description: 'Pay with PayPal', providerKey: 'paypal' },
  ]

  async getAll(): Promise<{ isSuccess: boolean; isFailure: boolean; statusCode: number; data?: PaymentMethodResponse[] }> {
    return { isSuccess: true, isFailure: false, statusCode: 200, data: this.methods }
  }

  async getById(id: string): Promise<{ isSuccess: boolean; isFailure: boolean; statusCode: number; data?: PaymentMethodResponse; message?: string }> {
    const method = this.methods.find(m => m.id === id)
    if (!method) return { isSuccess: false, isFailure: true, statusCode: 404, message: 'Payment method not found' }
    return { isSuccess: true, isFailure: false, statusCode: 200, data: method }
  }
}

export const mockPaymentMethodRepository = new MockPaymentMethodRepository()