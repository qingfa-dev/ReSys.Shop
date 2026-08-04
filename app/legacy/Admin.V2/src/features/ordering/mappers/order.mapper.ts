import type { CreateOrderForm, UpdateOrderForm } from '../schemas'
import type { CreateOrderRequest } from '../types'

export class OrderFormMapper {
  static toCreate(form: CreateOrderForm): CreateOrderRequest {
    return {
      customerId: form.customerId,
      notes: form.notes ?? undefined,
      lineItems: (form as unknown as { lineItems?: CreateOrderRequest['lineItems'] }).lineItems ?? [],
    }
  }
  static toUpdate(form: UpdateOrderForm): { customerId?: string; notes?: string | null } {
    return { customerId: form.customerId, notes: form.notes }
  }
}
