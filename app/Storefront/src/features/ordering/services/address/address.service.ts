import { addressApiRepository } from '../../repositories/address/address.api'
import type { IAddressService } from './address.service.interface'
import type { Address } from '../../types'
import type { Result } from '@/core/models/result'
import { mapAddressResponseToEntity } from '../../mapping'
import { resultMap, succeed, fail } from '@/core/utils/result-helpers'

export class AddressService implements IAddressService {
  private addressRepo = addressApiRepository

  async getAddresses(): Promise<Result<Address[]>> {
    const response = await this.addressRepo.getAll()
    if (response.isFailure) {
      return fail(response.message ?? 'Failed to get addresses', response.statusCode, response.errors)
    }
    return succeed(response.data!.map(mapAddressResponseToEntity), response.statusCode)
  }

  async createAddress(address: Omit<Address, 'id'>): Promise<Result<Address>> {
    const response = await this.addressRepo.create(address as never)
    return resultMap(response, mapAddressResponseToEntity)
  }

  async updateAddress(id: string, address: Partial<Address>): Promise<Result<Address>> {
    const response = await this.addressRepo.update(id, address as never)
    return resultMap(response, mapAddressResponseToEntity)
  }

  async deleteAddress(id: string): Promise<Result<void>> {
    const response = await this.addressRepo.delete(id) as Result<void>
    if (response.isFailure) {
      return fail(response.message ?? 'Failed to delete address', response.statusCode, response.errors)
    }
    return succeed(undefined, response.statusCode)
  }
}

export const addressService = new AddressService()