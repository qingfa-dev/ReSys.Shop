import { z } from 'zod'

export type TFunction = (key: string) => string

export class VariantPriceFields {
  constructor(private t: TFunction) {}

  amount() { return z.coerce.number().optional() }
  currency() { return z.string().min(1, this.t('catalog.validation.currency.required')) }
  compareAtAmount() { return z.coerce.number().optional() }
  countryIso() { return z.string().optional() }
}
