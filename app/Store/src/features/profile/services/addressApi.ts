import { get, post, put, del } from '@/shared/api/client'
import { PROFILES } from '@/shared/constants/api'
import { AddressSchema } from '../validations/address'
import { PagedResultSchema } from '@/shared/validations/result'
import type { Result, PagedResult } from '@/shared/types'
import type { Address, AddressInput } from '../types'

const addressList = PagedResultSchema(AddressSchema)

export class AddressApi {
  private static readonly BASE = `${PROFILES}/addresses`

  static async getAddresses(): Promise<PagedResult<Address>> {
    const result = await get<PagedResult<Address>>(this.BASE)
    if (!result.isSuccess) return result
    const parsed = addressList.parse({ ...result, items: result.items })
    return parsed as PagedResult<Address>
  }

  static async getDefaultAddress(): Promise<Result<Address>> {
    const result = await get<Result<Address>>(`${this.BASE}/default`)
    if (!result.isSuccess) return result
    result.value = AddressSchema.parse(result.value)
    return result
  }

  static async createAddress(req: AddressInput): Promise<Result<Address>> {
    const result = await post<Result<Address>>(this.BASE, req)
    if (!result.isSuccess) return result
    result.value = AddressSchema.parse(result.value)
    return result
  }

  static async updateAddress(id: string, req: AddressInput): Promise<Result<Address>> {
    const result = await put<Result<Address>>(`${this.BASE}/${id}`, req)
    if (!result.isSuccess) return result
    result.value = AddressSchema.parse(result.value)
    return result
  }

  static async deleteAddress(id: string): Promise<Result<void>> {
    return await del<Result<void>>(`${this.BASE}/${id}`)
  }
}
