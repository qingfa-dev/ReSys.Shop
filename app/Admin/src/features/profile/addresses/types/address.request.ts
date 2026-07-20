import type { AddressParameters } from '../types/address.field'
export type CreateAddressRequest = AddressParameters & { userId: string }
export type UpdateAddressRequest = Partial<CreateAddressRequest>
