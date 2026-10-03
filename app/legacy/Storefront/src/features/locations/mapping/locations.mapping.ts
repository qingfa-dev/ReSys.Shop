import type { Address, AddressSchemaType } from '../types'

export function toAddress(schema: AddressSchemaType): Address {
  return {
    id: schema.id,
    firstName: schema.firstName,
    lastName: schema.lastName,
    address1: schema.address1,
    address2: schema.address2,
    city: schema.city,
    state: schema.state,
    postalCode: schema.postalCode,
    country: schema.country,
    phone: schema.phone,
    isDefault: schema.isDefault,
  }
}

export function fromAddress(address: Address): AddressSchemaType {
  return AddressSchema.parse(address)
}

export function formatAddress(address: Address): string {
  const parts = [
    address.address1,
    address.address2,
    `${address.city}, ${address.state} ${address.postalCode}`,
    address.country,
  ].filter(Boolean)
  return parts.join(', ')
}

export function getFullName(address: Address): string {
  return `${address.firstName} ${address.lastName}`
}

import { AddressSchema } from '../types/schemas'
