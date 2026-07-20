import type { Result } from '@/core/models/result'
import type { Address } from '../../types'

export interface IAddressService {
  getAddresses(): Promise<Result<Address[]>>
  createAddress(address: Omit<Address, 'id'>): Promise<Result<Address>>
  updateAddress(id: string, address: Partial<Address>): Promise<Result<Address>>
  deleteAddress(id: string): Promise<Result<void>>
}