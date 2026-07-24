import { z } from 'zod'
import { VariantPriceFields } from './variant-price.fields'
import type { TFunction } from './variant-price.fields'

export class VariantPriceForms {
  private f: VariantPriceFields
  constructor(private t: TFunction) { this.f = new VariantPriceFields(t) }

  create() {
    return z.object({
      amount: this.f.amount(),
      currency: this.f.currency(),
      compareAtAmount: this.f.compareAtAmount(),
      countryIso: this.f.countryIso(),
    })
  }

  update() {
    return this.create()
  }
}

export type VariantPriceForm = z.input<ReturnType<VariantPriceForms['create']>>
