import type { ShippingMethodResponse } from '../../types/response'

export class MockShippingMethodRepository {
  private methods: ShippingMethodResponse[] = [
    { id: 'ship-1', name: 'Standard Shipping', description: '5-7 business days', price: 5.99, estimatedDays: 7 },
    { id: 'ship-2', name: 'Express Shipping', description: '2-3 business days', price: 12.99, estimatedDays: 3 },
    { id: 'ship-3', name: 'Next Day Delivery', description: 'Next business day', price: 24.99, estimatedDays: 1 },
  ]

  async getAll(): Promise<{ isSuccess: boolean; isFailure: boolean; statusCode: number; data?: ShippingMethodResponse[] }> {
    return { isSuccess: true, isFailure: false, statusCode: 200, data: this.methods }
  }

  async getById(id: string): Promise<{ isSuccess: boolean; isFailure: boolean; statusCode: number; data?: ShippingMethodResponse; message?: string }> {
    const method = this.methods.find(m => m.id === id)
    if (!method) return { isSuccess: false, isFailure: true, statusCode: 404, message: 'Shipping method not found' }
    return { isSuccess: true, isFailure: false, statusCode: 200, data: method }
  }
}

export const mockShippingMethodRepository = new MockShippingMethodRepository()