import { z } from 'zod'
import type { TFunction } from './shipping-rate.fields'
import { ShippingRateFields } from './shipping-rate.fields'

export class ShippingRateForms {
  private f: ShippingRateFields
  constructor(private t: TFunction) { this.f = new ShippingRateFields(t) }
  create() { return z.object({ shippingMethodId: this.f.shippingMethodId(), name: this.f.name(), rate: this.f.rate(), currency: this.f.currency(), minOrderAmount: this.f.minOrderAmount(), maxOrderAmount: this.f.maxOrderAmount(), minWeight: this.f.minWeight(), maxWeight: this.f.maxWeight() }) }
  update() { return this.create() }
}
export type CreateShippingRateForm = z.input<ReturnType<ShippingRateForms['create']>>
export type UpdateShippingRateForm = z.input<ReturnType<ShippingRateForms['update']>>
