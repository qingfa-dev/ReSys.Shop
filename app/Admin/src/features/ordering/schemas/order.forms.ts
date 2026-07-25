import { z } from 'zod'
import type { TFunction } from './order.fields'
import { OrderFields } from './order.fields'

export class OrderForms {
  private f: OrderFields
  constructor(private t: TFunction) { this.f = new OrderFields(t) }
  create() {
    return z.object({
      customerId: this.f.customerId(),
      notes: this.f.notes(),
      lineItems: this.f.lineItems(),
    })
  }
  update() {
    return z.object({
      notes: this.f.notes(),
    })
  }
  address() {
    return z.object({
      firstName: this.f.firstName(),
      lastName: this.f.lastName(),
      address1: this.f.address1(),
      address2: this.f.address2(),
      city: this.f.city(),
      state: this.f.state(),
      postalCode: this.f.postalCode(),
      country: this.f.country(),
      phone: this.f.phone(),
    })
  }
}
export type CreateOrderForm = z.input<ReturnType<OrderForms['create']>>
export type UpdateOrderForm = z.input<ReturnType<OrderForms['update']>>
export type AddressForm = z.input<ReturnType<OrderForms['address']>>
