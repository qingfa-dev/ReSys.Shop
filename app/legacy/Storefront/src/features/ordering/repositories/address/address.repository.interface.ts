import type { Result } from '@/core/models/result'
import type { AddressResponse } from '../../types/response'

export interface IAddressRepository {
  getAll(): Promise<Result<AddressResponse[]>>
  getById<T = AddressResponse>(id: string): Promise<Result<T>>
  create(address: Omit<AddressResponse, 'id'>): Promise<Result<AddressResponse>>
  update(id: string, address: Partial<AddressResponse>): Promise<Result<AddressResponse>>
  setDefault(id: string): Promise<Result<AddressResponse>>
}