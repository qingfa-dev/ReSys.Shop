import { z } from 'zod'
import type { TFunction } from './profile.fields'
import { AddressFields } from './address.fields'

export class AddressForms {
  private f: AddressFields
  constructor(private t: TFunction) { this.f = new AddressFields(t) }
  create() { return z.object({ firstName: this.f.firstName(), lastName: this.f.lastName(), address1: this.f.address1(), address2: this.f.address2(), city: this.f.city(), state: this.f.state(), postalCode: this.f.postalCode(), country: this.f.country(), phone: this.f.phone(), isDefault: this.f.isDefault() }) }
  update() { return this.create() }
}
export type CreateAddressForm = z.input<ReturnType<AddressForms['create']>>
export type UpdateAddressForm = z.input<ReturnType<AddressForms['update']>>
