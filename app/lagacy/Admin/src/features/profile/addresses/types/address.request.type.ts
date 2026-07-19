import type { AddressParameters } from '../schemas/address.schema'
export type CreateAddressRequest = AddressParameters & { userId: string }
export type UpdateAddressRequest = Partial<CreateAddressRequest>
