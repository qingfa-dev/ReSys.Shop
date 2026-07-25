import type { CreateOrderForm, UpdateOrderForm } from '../schemas'
import type { CreateOrderRequest } from '../types'

export class OrderFormMapper {
  static toCreate(form: CreateOrderForm): CreateOrderRequest {
    return {
      customerId: form.customerId,
      notes: form.notes,
      lineItems: form.lineItems,
    }
  }
  static toUpdate(form: UpdateOrderForm): { notes?: string | null } {
    return { notes: form.notes }
  }
}
