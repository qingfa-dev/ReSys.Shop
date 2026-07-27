import type { CreateShippingRateForm, UpdateShippingRateForm } from '../schemas'
import type { CreateShippingRateRequest, UpdateShippingRateRequest } from '../types'

export class ShippingRateFormMapper {
  static toCreate(form: CreateShippingRateForm): CreateShippingRateRequest { return form }
  static toUpdate(form: UpdateShippingRateForm): UpdateShippingRateRequest { return form }
}
