import { BaseRepository } from '@/core/repositories'
import type { Result } from '@/core/models/result'
import type { AddressResponse } from '../../types/response'
import type { IAddressRepository } from './address.repository.interface'

export class AddressApiRepository extends BaseRepository implements IAddressRepository {
  async getAll(): Promise<Result<AddressResponse[]>> {
    return this.get<AddressResponse[]>('/api/store/profiles/addresses')
  }

  getById<T = AddressResponse>(id: string): Promise<Result<T>> {
    return super.getById<T>('/api/store/profiles/addresses', id)
  }

  async create(address: Omit<AddressResponse, 'id'>): Promise<Result<AddressResponse>> {
    return this.post<AddressResponse>('/api/store/profiles/addresses', address)
  }

  async update(id: string, address: Partial<AddressResponse>): Promise<Result<AddressResponse>> {
    return this.patch<AddressResponse>(`/api/store/profiles/addresses/${id}`, address)
  }

  async setDefault(id: string): Promise<Result<AddressResponse>> {
    return this.post<AddressResponse>(`/api/store/profiles/addresses/${id}/default`)
  }
}

export const addressApiRepository = new AddressApiRepository()