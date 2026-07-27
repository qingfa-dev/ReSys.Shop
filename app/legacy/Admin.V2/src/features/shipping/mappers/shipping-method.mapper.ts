import type { CreateShippingMethodForm, UpdateShippingMethodForm } from '../schemas'
import type { CreateShippingMethodRequest, UpdateShippingMethodRequest } from '../types'

export class ShippingMethodFormMapper {
  static toCreate(form: CreateShippingMethodForm): CreateShippingMethodRequest { return form }
  static toUpdate(form: UpdateShippingMethodForm): UpdateShippingMethodRequest { return form }
}
