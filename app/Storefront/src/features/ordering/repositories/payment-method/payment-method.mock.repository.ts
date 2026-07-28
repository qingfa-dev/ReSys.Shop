import type { PaymentMethodResponse } from '../../types/response'

export class MockPaymentMethodRepository {
  private methods: PaymentMethodResponse[] = [
    { id: 'pm-1', type: 'credit_card', name: 'Credit Card', last4: '4242', brand: 'Visa' },
    { id: 'pm-2', type: 'credit_card', name: 'Credit Card', last4: '5555', brand: 'Mastercard' },
    { id: 'pm-3', type: 'paypal', name: 'PayPal' },
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