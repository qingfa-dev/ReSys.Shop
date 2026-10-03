import type { ShippingMethodResponse } from '../../types/response'

export class MockShippingMethodRepository {
  private methods: ShippingMethodResponse[] = [
    { id: 'ship-1', name: 'Standard Shipping', adminName: 'Standard', code: 'standard', calculatorType: 'flat', position: 1 },
    { id: 'ship-2', name: 'Express Shipping', adminName: 'Express', code: 'express', calculatorType: 'flat', position: 2 },
    { id: 'ship-3', name: 'Next Day Delivery', adminName: 'Next Day', code: 'next-day', calculatorType: 'flat', position: 3 },
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