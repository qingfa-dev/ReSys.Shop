import { z } from 'zod'
import type { TFunction } from './shipping-method.fields'
import { ShippingMethodFields } from './shipping-method.fields'

export class ShippingMethodForms {
  private f: ShippingMethodFields
  constructor(private t: TFunction) { this.f = new ShippingMethodFields(t) }
  create() { return z.object({ name: this.f.name(), code: this.f.code(), description: this.f.description(), isActive: this.f.isActive(), displayOrder: this.f.displayOrder(), estimatedDeliveryMin: this.f.estimatedDeliveryMin(), estimatedDeliveryMax: this.f.estimatedDeliveryMax() }) }
  update() { return this.create() }
}
export type CreateShippingMethodForm = z.input<ReturnType<ShippingMethodForms['create']>>
export type UpdateShippingMethodForm = z.input<ReturnType<ShippingMethodForms['update']>>
