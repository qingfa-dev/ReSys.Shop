import type { AddressParameters } from '../schemas/Address.Schema'
export type CreateAddressRequest = AddressParameters & { userId: string }
export type UpdateAddressRequest = Partial<CreateAddressRequest>
