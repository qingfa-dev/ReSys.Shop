import { BaseRepository } from '@/core/repositories'
import type { Result } from '@/core/models/result'
import type { AddressResponse } from '../../types/response'
import type { IAddressRepository } from './address.repository.interface'

export class AddressApiRepository extends BaseRepository implements IAddressRepository {
  async getAddresses(): Promise<Result<AddressResponse[]>> {
    return this.get<AddressResponse[]>('/api/store/profiles/addresses')
  }

  async getById<T = AddressResponse>(id: string): Promise<Result<T>> {
    return this.get<T>(`/api/store/profiles/addresses/${id}`)
  }

  async getDefault(): Promise<Result<AddressResponse>> {
    return this.get<AddressResponse>('/api/store/profiles/addresses/default')
  }

  async create(address: Omit<AddressResponse, 'id'>): Promise<Result<AddressResponse>> {
    return this.post<AddressResponse>('/api/store/profiles/addresses', address)
  }

  async update(id: string, address: Partial<AddressResponse>): Promise<Result<AddressResponse>> {
    return this.patchPartial<AddressResponse>('/api/store/profiles/addresses', id, address)
  }

  async setDefault(id: string): Promise<Result<void>> {
    return this.post<void>(`/api/store/profiles/addresses/${id}/set-default`)
  }
}

export const addressApiRepository = new AddressApiRepository()
