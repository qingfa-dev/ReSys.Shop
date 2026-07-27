import type { VariantPriceForm } from '../schemas'
import type { VariantPriceRequest } from '../types'

export class VariantPriceFormMapper {
  static toCreate(form: VariantPriceForm): VariantPriceRequest {
    return {
      amount: form.amount ?? null,
      currency: form.currency,
      compareAtAmount: form.compareAtAmount ?? null,
      countryIso: form.countryIso ?? null,
    }
  }

  static toUpdate(form: VariantPriceForm): VariantPriceRequest {
    return VariantPriceFormMapper.toCreate(form)
  }
}
