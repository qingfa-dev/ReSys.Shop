import type { CreateAddressForm, UpdateAddressForm } from '../schemas'
import type { CreateAddressRequest, UpdateAddressRequest } from '../types'

export class AddressFormMapper {
  static toCreate(form: CreateAddressForm): CreateAddressRequest { return form }
  static toUpdate(form: UpdateAddressForm): UpdateAddressRequest { return form }
}
