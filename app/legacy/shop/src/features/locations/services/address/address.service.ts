import { addressApiRepository } from '../../repositories/address/address.api'
import { mockAddressRepository } from '../../repositories/address/address.mock.repository'
import type { IAddressService } from './address.service.interface'
import type { Address, AddressResponse } from '../../types'
import type { Result } from '@/core/models/result'
import { toAddress } from '../../mapping'
import { resultMap } from '@/core/utils/result-helpers'

const USE_MOCK = true

export class AddressService implements IAddressService {
  private readonly addressRepository = USE_MOCK ? mockAddressRepository : addressApiRepository

  async getAddresses(): Promise<Result<Address[]>> {
    const response = await this.addressRepository.getAddresses()
    if (response.isFailure) {
      return response as unknown as Result<Address[]>
    }
    return resultMap(response, (data) => data.map(toAddress))
  }

  async getDefaultAddress(): Promise<Result<Address>> {
    const response = await this.addressRepository.getDefault()
    return resultMap(response, toAddress)
  }

  async createAddress(address: Omit<Address, 'id'>): Promise<Result<Address>> {
    const response = await this.addressRepository.create(address as Omit<AddressResponse, 'id'>)
    return resultMap(response, toAddress)
  }

  async updateAddress(id: string, address: Partial<Address>): Promise<Result<Address>> {
    const response = await this.addressRepository.update(id, address as Partial<AddressResponse>)
    return resultMap(response, toAddress)
  }

  async deleteAddress(id: string): Promise<Result<void>> {
    return (await this.addressRepository.delete(id)) as unknown as Result<void>
  }

  async setDefaultAddress(id: string): Promise<Result<void>> {
    return (await this.addressRepository.setDefault(id)) as unknown as Result<void>
  }
}

export const addressService = new AddressService()
