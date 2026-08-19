import { get, post, patch, del } from '@/shared/api/client'
import { AddressSchema } from '../validations/address'
import { PagedResultSchema } from '@/shared/validations/result'
import type { Result, PagedResult } from '@/shared/types'
import type { Address, AddressInput } from '../types'

// Validate: Paged list schema for address collection endpoint
const addressList = PagedResultSchema(AddressSchema)

export class AddressApi {
  // Call: Fetch all addresses for the authenticated user
  static async getAddresses(): Promise<PagedResult<Address>> {
    const result = await get<PagedResult<Address>>('/api/storefront/customer/addresses')
    if (!result.isSuccess) return result
    // Transform: Parse paged result with address schema
    const parsed = addressList.parse({ ...result, items: result.items })
    return parsed as PagedResult<Address>
  }

  // Call: Fetch the single default address
  static async getDefaultAddress(): Promise<Result<Address>> {
    const result = await get<Result<Address>>('/api/storefront/customer/addresses/default')
    if (!result.isSuccess) return result
    result.value = AddressSchema.parse(result.value)
    return result
  }

  // Call: Create a new address entry
  static async createAddress(req: AddressInput): Promise<Result<Address>> {
    const result = await post<Result<Address>>('/api/storefront/customer/addresses', req)
    if (!result.isSuccess) return result
    result.value = AddressSchema.parse(result.value)
    return result
  }

  // Call: Update an existing address by id
  static async updateAddress(id: string, req: AddressInput): Promise<Result<Address>> {
    const result = await patch<Result<Address>>(`/api/storefront/customer/addresses/${id}`, req)
    if (!result.isSuccess) return result
    result.value = AddressSchema.parse(result.value)
    return result
  }

  // Call: Delete an address by id
  static async deleteAddress(id: string): Promise<Result<void>> {
    return await del<Result<void>>(`/api/storefront/customer/addresses/${id}`)
  }
}
